using NVUpdateManager.NotificationService.Data;
using static NVUpdateManager.EmailHandler.EmailHandler;

namespace NVUpdateManager.NotificationService
{
    public class Program
    {
        private static string Usage =
            @"
                Run with no arguments to start the service normally

                Usage:
                
                    -EncryptEndpoint: Encrypt Azure Logic App endpoint

                    Example: NVUpdateManager.NotificationService.exe -EncryptEndpoint ""your-endpoint-here""
            ";

        public static void Main(string[] args)
        {
            if(args.Length > 0) 
            {
                ParseArguments(args);
                return;
            }
            IHost host = Host.CreateDefaultBuilder(args)
                .UseWindowsService(options =>
                {
                    options.ServiceName = $"{nameof(NVUpdateManager)}.{nameof(NotificationService)}";
                })
                .ConfigureServices(( hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration;

                    services.Configure<EmailConfiguration>(configuration.GetSection(nameof(EmailConfiguration)));

                    var sectionName = $"{nameof(SupportedDriver)}s";

                    var supportedDrivers= new List<SupportedDriver>();

                    configuration.GetSection(sectionName).Bind(supportedDrivers);

                    foreach (var supportedDriver in supportedDrivers)
                    {
                        services.AddSingleton(supportedDriver);
                    }

                    services.Configure<HostOptions>(hostOptions =>
                    {
                        hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
                    });

                    services.AddHostedService<NotificationWorker>();
                })
                .Build();

            host.Run();
        }

        private static void ParseArguments(string[] args)
        {
            if (args[0].ToLower().Equals("-encryptendpoint"))
            {
                EncodeLogicAppEndpoint(args[1]);
            }
            else
            {
                ShowUsage();
            }
        }

        private static void ShowUsage()
        {
            Console.WriteLine(Usage);
        }
    }
}
