using NVUpdateManager.NotificationService.Data;
namespace NVUpdateManager.NotificationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
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
    }
}
