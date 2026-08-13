using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NVUpdateManager.Core.Data;
using NVUpdateManager.Core.Interfaces;
using NVUpdateManager.Notifications.Data;

namespace NVUpdateManager.Notifications.Tests
{
    [TestClass]
    public class NotificationDispatcherTests
    {
        private static readonly NotificationMessage Message =
            new("New driver", "610.88 is available", "<p>notes</p>", "https://example.invalid/d.exe");

        private static NotificationDispatcher DispatcherFor(NotificationOptions options, params INotificationChannel[] channels)
        {
            return new NotificationDispatcher(
                channels,
                Options.Create(options),
                NullLogger<NotificationDispatcher>.Instance);
        }

        [TestMethod]
        public async Task WithNothingConfiguredItUsesEveryChannelThatIsReady()
        {
            /* This is what makes a fresh install work: the desktop notification needs no setup,
             * so it reports itself ready and gets used without anyone configuring anything.
             */

            var toast = new FakeNotificationChannel("Toast");
            var smtp = new FakeNotificationChannel("Smtp", isConfigured: false);

            var delivered = await DispatcherFor(new NotificationOptions(), toast, smtp).SendAsync(Message);

            CollectionAssert.AreEqual(new[] { "Toast" }, delivered.ToArray());
            Assert.AreEqual(1, toast.Sent.Count);
            Assert.AreEqual(0, smtp.Sent.Count);
        }

        [TestMethod]
        public async Task NamedChannelsAreUsedInsteadOfEveryReadyOne()
        {
            var toast = new FakeNotificationChannel("Toast");
            var smtp = new FakeNotificationChannel("Smtp");

            var options = new NotificationOptions { Channels = { "Smtp" } };

            var delivered = await DispatcherFor(options, toast, smtp).SendAsync(Message);

            CollectionAssert.AreEqual(new[] { "Smtp" }, delivered.ToArray());
            Assert.AreEqual(0, toast.Sent.Count);
        }

        [TestMethod]
        public async Task ChannelNamesAreMatchedWithoutRegardToCasing()
        {
            var smtp = new FakeNotificationChannel("Smtp");

            var options = new NotificationOptions { Channels = { "sMtP" } };

            var delivered = await DispatcherFor(options, smtp).SendAsync(Message);

            CollectionAssert.AreEqual(new[] { "Smtp" }, delivered.ToArray());
        }

        [TestMethod]
        public async Task AChannelNamedInConfigurationButMissingItsSettingsIsSkipped()
        {
            var smtp = new FakeNotificationChannel("Smtp", isConfigured: false);

            var options = new NotificationOptions { Channels = { "Smtp" } };

            var delivered = await DispatcherFor(options, smtp).SendAsync(Message);

            Assert.AreEqual(0, delivered.Count);
        }

        [TestMethod]
        public async Task AChannelNameThatDoesNotExistIsIgnoredRatherThanFatal()
        {
            var toast = new FakeNotificationChannel("Toast");

            var options = new NotificationOptions { Channels = { "Carrier Pigeon", "Toast" } };

            var delivered = await DispatcherFor(options, toast).SendAsync(Message);

            CollectionAssert.AreEqual(new[] { "Toast" }, delivered.ToArray());
        }

        [TestMethod]
        public async Task OneChannelFailingDoesNotStopTheOthers()
        {
            var broken = new FakeNotificationChannel("Smtp", failure: new InvalidOperationException("no route to host"));
            var working = new FakeNotificationChannel("Toast");

            var delivered = await DispatcherFor(new NotificationOptions(), broken, working).SendAsync(Message);

            CollectionAssert.AreEqual(new[] { "Toast" }, delivered.ToArray());
            Assert.AreEqual(1, working.Sent.Count);
        }

        [TestMethod]
        public async Task AFailingChannelNeverFailsTheUpdateCheck()
        {
            // Finding the update is the valuable part; failing to announce it must not lose it.
            var broken = new FakeNotificationChannel("Smtp", failure: new InvalidOperationException("no route to host"));

            var delivered = await DispatcherFor(new NotificationOptions(), broken).SendAsync(Message);

            Assert.AreEqual(0, delivered.Count);
        }

        [TestMethod]
        public async Task WithNoChannelAvailableItReportsRatherThanThrows()
        {
            var delivered = await DispatcherFor(new NotificationOptions()).SendAsync(Message);

            Assert.AreEqual(0, delivered.Count);
        }

        [TestMethod]
        public async Task EverySelectedChannelReceivesTheMessage()
        {
            var toast = new FakeNotificationChannel("Toast");
            var smtp = new FakeNotificationChannel("Smtp");

            var delivered = await DispatcherFor(new NotificationOptions(), toast, smtp).SendAsync(Message);

            Assert.AreEqual(2, delivered.Count);
            Assert.AreSame(Message, toast.Sent.Single());
            Assert.AreSame(Message, smtp.Sent.Single());
        }
    }
}
