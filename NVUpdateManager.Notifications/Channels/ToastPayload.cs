using System.Xml.Linq;
using NVUpdateManager.Core.Data;

namespace NVUpdateManager.Notifications.Channels
{
    /// <summary>
    /// Builds the XML a Windows toast is described by.
    ///
    /// Kept apart from the channel, and free of any WinRT reference, so that it compiles and can
    /// be tested on any platform. Release notes arrive as HTML full of quotes and angle brackets,
    /// so the document is built rather than concatenated and everything is escaped for us.
    /// </summary>
    internal static class ToastPayload
    {
        internal static string Build(NotificationMessage message)
        {
            var toast = new XElement("toast",
                new XElement("visual",
                    new XElement("binding",
                        new XAttribute("template", "ToastGeneric"),
                        new XElement("text", message.Subject),
                        new XElement("text", message.Summary))));

            return toast.ToString(SaveOptions.DisableFormatting);
        }
    }
}
