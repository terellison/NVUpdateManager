using NVUpdateManager.NotificationService.Data;
using static NVUpdateManager.EmailHandler.EmailHandler;
using CliWrap;

namespace NVUpdateManager.NotificationService
{
    public class Program
    {
        private const string ServiceName = $"{nameof(NVUpdateManager)}.{nameof(NotificationService)}";
        private static string Usage =
            @"
                Run with no arguments to start the service normally

                Usage:
                
                    /EncryptEndpoint: Encrypt Azure Logic App endpoint

                    Example: NVUpdateManager.NotificationService.exe /EncryptEndpoint ""your-endpoint-here""

                    /Install: install as a service

                    /Uninstall: Uinstall existing service
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
                    options.ServiceName = ServiceName;
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
            switch(args[0].ToLower())
            {
                case "/encryptendpoint":
                    EncodeLogicAppEndpoint(args[1]);
                    break;
                case "/install":
                    _ = Install();
                    break;
                case "/uninstall":
                    _ = Uninstall();
                    break;
                default:
                    ShowUsage();
                    break;
            }
        }

        private static async Task Install()
        {
            var executablePath = Path.Combine(AppContext.BaseDirectory, $"{ServiceName}.exe");

            await Cli.Wrap("sc")
                .WithArguments(new[] { "create", ServiceName, $"binPath={executablePath}", "start=auto" })
                .ExecuteAsync();
        }

        private static async Task Uninstall()
        {
            await Cli.Wrap("sc")
                .WithArguments(new[] { "stop", ServiceName })
                .ExecuteAsync();

            await Cli.Wrap("sc")
                .WithArguments(new[] { "delete", ServiceName })
                .ExecuteAsync();
        }

        private static void ShowUsage()
        {
            Console.WriteLine(Usage);
        }
    }
}
