using NVUpdateManager.NotificationService.Data;
using NVUpdateManager.NotificationService.Services;
using static NVUpdateManager.EmailHandler.EmailHandler;
using NVUpdateManager.Core.Extensions;
using NVUpdateManager.Web.Extensions;
using NVUpdateManager.Notifications.Data;
using NVUpdateManager.Notifications.Extensions;
using NVUpdateManager.Core.Data;
using NVUpdateManager.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace NVUpdateManager.NotificationService
{
    public class Program
    {
        private static string Usage =
            @"
                Usage:

                    /TestNotification: Send a sample notification through the configured
                                       channels, to check delivery works

                    /EncryptEndpoint:  Encrypt an Azure Logic App endpoint

                    Examples:

                        NVUpdateManager.NotificationService.exe /TestNotification

                        NVUpdateManager.NotificationService.exe /EncryptEndpoint ""your-endpoint-here""
            ";

        public static async Task Main(string[] args)
        {
            var command = args.Length > 0 ? args[0].ToLower() : string.Empty;

            if (command == "/encryptendpoint")
            {
                if (args.Length < 2)
                {
                    ShowUsage();
                    return;
                }

                EncodeLogicAppEndpoint(args[1]);
                return;
            }

            if (command.Length > 0 && command != "/testnotification")
            {
                ShowUsage();
                return;
            }

            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(( hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration;

                    services.Configure<DriverSearchConfiguration>(configuration.GetSection(nameof(DriverSearchConfiguration)));

                    services.Configure<NotificationOptions>(options =>
                    {
                        configuration.GetSection("Notifications").Bind(options);

                        ApplyLegacyEmailConfiguration(configuration, options);
                    });

                    services.Configure<HostOptions>(hostOptions =>
                    {
                        hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
                    });

                    services.AddDriverManager();

                    services.AddUpdateFinder();

                    services.AddNotifications();

                    services.TryAddSingleton<INotificationService, NVNotificationService>();
                })
                .Build();

            if (command == "/testnotification")
            {
                await SendTestNotification(host.Services);
                return;
            }

            var ns = ActivatorUtilities.GetServiceOrCreateInstance<NVNotificationService>(host.Services);

            await ns.Run();
        }

        /// <summary>
        /// Sends a sample notification through whichever channels are configured, so that the
        /// delivery can be tested without waiting for NVIDIA to publish a driver newer than the
        /// one installed.
        ///
        /// Deliberately goes through the real dispatcher rather than poking a channel directly,
        /// so that channel selection is exercised too: what this sends is what a real update
        /// would send.
        /// </summary>
        private static async Task SendTestNotification(IServiceProvider services)
        {
            var dispatcher = services.GetRequiredService<INotificationDispatcher>();

            var message = new NotificationMessage(
                subject: "NVUpdateManager test notification",
                summary: "Notifications are working. This is a test, not a real driver update.",
                htmlBody:
                    "<p>This is a test notification from NVUpdateManager.</p>"
                    + "<p>A real one looks like this:</p>"
                    + "<p>Version: 610.88</p>"
                    + "<p>Release Date: Tue Jul 28, 2026</p>"
                    + "<p>Download Link: https://www.nvidia.com/en-us/drivers/</p>",
                downloadLink: "https://www.nvidia.com/en-us/drivers/");

            var delivered = await dispatcher.SendAsync(message);

            if (delivered.Count == 0)
            {
                Console.WriteLine(
                    "No notification was delivered. Check the log above: either no channel is "
                    + "configured, or the ones that are could not deliver.");

                return;
            }

            Console.WriteLine($"Test notification delivered via: {string.Join(", ", delivered)}");
        }

        /// <summary>
        /// Installations predating the notification channels configured the Logic App relay under
        /// an EmailConfiguration section. Honour it so that upgrading does not quietly stop the
        /// mail an enterprise install depends on.
        /// </summary>
        private static void ApplyLegacyEmailConfiguration(IConfiguration configuration, NotificationOptions options)
        {
            var legacy = configuration.GetSection(nameof(EmailConfiguration)).Get<EmailConfiguration>();

            if (legacy == null)
            {
                return;
            }

            options.LogicApp.EncryptedAzLogicAppEndpoint ??= legacy.EncryptedAzLogicAppEndpoint;
            options.LogicApp.Entropy ??= legacy.Entropy;
            options.LogicApp.NotificationAddress ??= legacy.NotificationAddress;
        }

        private static void ShowUsage()
        {
            Console.WriteLine(Usage);
        }
    }
}
