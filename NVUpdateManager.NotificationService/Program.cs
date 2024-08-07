using NVUpdateManager.NotificationService.Data;
using NVUpdateManager.NotificationService.Services;
using static NVUpdateManager.EmailHandler.EmailHandler;
using NVUpdateManager.Core.Extensions;
using NVUpdateManager.Web.Extensions;
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

                    services.Configure<EmailConfiguration>(configuration.GetSection(nameof(EmailConfiguration)));

                    var sectionName = $"{nameof(SupportedDriver)}s";

                    var supportedDrivers = new List<SupportedDriver>();

                    configuration.GetSection(sectionName).Bind(supportedDrivers);

                    foreach (var supportedDriver in supportedDrivers)
                    {
                        services.AddSingleton(supportedDriver);
                    }

                    services.Configure<HostOptions>(hostOptions =>
                    {
                        hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
                    });

                    services.AddDriverManager();

                    services.AddUpdateFinder();

                    services.TryAddSingleton<INotificationService, NVNotificationService>();
                })
                .Build();

            var ns = ActivatorUtilities.GetServiceOrCreateInstance<NVNotificationService>(host.Services);

            await ns.Run();
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
