#if WINDOWS

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using NVUpdateManager.Core.Data;
using NVUpdateManager.Core.Interfaces;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace NVUpdateManager.Notifications.Channels
{
    /// <summary>
    /// Shows a Windows desktop notification.
    ///
    /// This is the channel that needs no setup, and so the one that makes the application useful
    /// the moment it is installed. It only reaches the desktop when the process runs in the
    /// signed-in user's session, which is how the scheduled task runs it - a Windows service in
    /// session 0 cannot show anything to anybody.
    ///
    /// Windows attributes a notification to an application identity, so an unpackaged program has
    /// to declare one before it can post. The registration below is best effort: if Windows
    /// declines the toast, the dispatcher reports it and moves on to the next channel rather than
    /// failing the update check.
    ///
    /// Like the WMI adapter, this deliberately holds no logic worth testing - the payload it
    /// sends is built by <see cref="ToastPayload"/>, which is tested.
    /// </summary>
    internal sealed class ToastNotificationChannel : INotificationChannel
    {
        private const string AppUserModelId = "NVUpdateManager.NotificationService";
        private const string DisplayName = "NVIDIA Update Manager";

        public string Name => "Toast";

        /// <summary>Nothing to configure; that is the entire point of this channel.</summary>
        public bool IsConfigured => true;

        public Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
        {
            RegisterApplicationIdentity();

            var document = new XmlDocument();
            document.LoadXml(ToastPayload.Build(message));

            ToastNotificationManager.CreateToastNotifier(AppUserModelId)
                .Show(new ToastNotification(document));

            return Task.CompletedTask;
        }

        /// <summary>
        /// Declares the application identity the notification is posted under, so the toast is
        /// attributed to this application rather than rejected as coming from nowhere.
        /// </summary>
        private static void RegisterApplicationIdentity()
        {
            using (var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\AppUserModelId\{AppUserModelId}"))
            {
                key?.SetValue("DisplayName", DisplayName);
            }
        }
    }
}

#endif
