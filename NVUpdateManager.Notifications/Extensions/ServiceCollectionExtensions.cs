using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NVUpdateManager.Core.Interfaces;
using NVUpdateManager.Notifications.Channels;

namespace NVUpdateManager.Notifications.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers every notification channel available on this platform, plus the dispatcher
        /// that chooses between them. Channels missing their settings report themselves as
        /// unconfigured and are skipped, so registering them all is harmless.
        /// </summary>
        public static IServiceCollection AddNotifications(this IServiceCollection services)
        {
#if WINDOWS
            // First, so it is the one that answers on a machine with nothing configured.
            services.AddSingleton<INotificationChannel, ToastNotificationChannel>();
#endif

            services.TryAddSingleton<ISmtpTransport, MailKitSmtpTransport>();
            services.AddSingleton<INotificationChannel, SmtpNotificationChannel>();
            services.AddSingleton<INotificationChannel, LogicAppNotificationChannel>();

            services.TryAddSingleton<INotificationDispatcher, NotificationDispatcher>();

            return services;
        }
    }
}
