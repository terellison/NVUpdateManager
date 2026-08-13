using NVUpdateManager.NotificationService.Data;
using NVUpdateManager.NotificationService.Services;
using static NVUpdateManager.EmailHandler.EmailHandler;
using NVUpdateManager.Core.Extensions;
using NVUpdateManager.Web.Extensions;
using NVUpdateManager.Notifications.Data;
using NVUpdateManager.Notifications.Extensions;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace NVUpdateManager.NotificationService
{
    public class Program
    {
        private static string Usage =
            @"
                Usage:

                    /EncryptEndpoint: Encrypt Azure Logic App endpoint

                    Example: NVUpdateManager.NotificationService.exe /EncryptEndpoint ""your-endpoint-here""
            ";

        public static async Task Main(string[] args)
        {
            if(args.Length > 0)
            {
                ParseArguments(args);
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

            var ns = ActivatorUtilities.GetServiceOrCreateInstance<NVNotificationService>(host.Services);

            await ns.Run();
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

        private static void ParseArguments(string[] args)
        {
            switch(args[0].ToLower())
            {
                case "/encryptendpoint":
                    EncodeLogicAppEndpoint(args[1]);
                    break;
                default:
                    ShowUsage();
                    break;
            }
        }

        private static void ShowUsage()
        {
            Console.WriteLine(Usage);
        }
    }
}
