using System.Xml.Linq;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NVUpdateManager.Core.Data;
using NVUpdateManager.Notifications.Channels;
using NVUpdateManager.Notifications.Data;

namespace NVUpdateManager.Notifications.Tests
{
    [TestClass]
    public class SmtpNotificationChannelTests
    {
        private static SmtpNotificationChannel ChannelFor(SmtpOptions smtp)
        {
            return new SmtpNotificationChannel(
                Options.Create(new NotificationOptions { Smtp = smtp }),
                new UnusedTransport());
        }

        private sealed class UnusedTransport : ISmtpTransport
        {
            public Task SendAsync(SmtpEnvelope envelope, SmtpOptions options, CancellationToken cancellationToken = default)
                => throw new AssertFailedException("The transport should not be reached by these tests.");
        }

        [TestMethod]
        public void IsNotConfiguredWithoutAHost()
        {
            var channel = ChannelFor(new SmtpOptions { Username = "me@example.com" });

            Assert.IsFalse(channel.IsConfigured);
        }

        [TestMethod]
        public void IsConfiguredOnceAHostAndAccountAreKnown()
        {
            var channel = ChannelFor(new SmtpOptions { Host = "smtp.example.com", Username = "me@example.com" });

            Assert.IsTrue(channel.IsConfigured);
        }

        [TestMethod]
        public void SendsToYourselfWhenNoRecipientIsGiven()
        {
            // The common case is notifying your own address, so it should not need saying twice.
            var channel = ChannelFor(new SmtpOptions { Host = "smtp.example.com", Username = "me@example.com" });

            var envelope = channel.BuildEnvelope(SampleMessage());

            Assert.AreEqual("me@example.com", envelope.FromAddress);
            Assert.AreEqual("me@example.com", envelope.ToAddress);
        }

        [TestMethod]
        public void AnExplicitSenderAndRecipientAreHonoured()
        {
            var channel = ChannelFor(new SmtpOptions
            {
                Host = "smtp.example.com",
                Username = "robot@example.com",
                From = "alerts@example.com",
                To = "someone@example.com",
                FromName = "GPU Watch"
            });

            var envelope = channel.BuildEnvelope(SampleMessage());

            Assert.AreEqual("alerts@example.com", envelope.FromAddress);
            Assert.AreEqual("someone@example.com", envelope.ToAddress);
            Assert.AreEqual("GPU Watch", envelope.FromName);
        }

        [TestMethod]
        public void TheEmailCarriesTheFullReleaseNotes()
        {
            var channel = ChannelFor(new SmtpOptions { Host = "smtp.example.com", Username = "me@example.com" });

            var envelope = channel.BuildEnvelope(SampleMessage());

            Assert.AreEqual("New driver", envelope.Subject);
            StringAssert.Contains(envelope.HtmlBody, "release notes");
        }

        private static NotificationMessage SampleMessage()
        {
            return new NotificationMessage("New driver", "610.88 available", "<p>release notes</p>", "https://example.invalid/d.exe");
        }
    }

    [TestClass]
    public class ToastPayloadTests
    {
        [TestMethod]
        public void BuildsAToastWindowsCanParse()
        {
            var xml = ToastPayload.Build(
                new NotificationMessage("New driver", "610.88 available", "<p>notes</p>", "https://example.invalid"));

            var document = XDocument.Parse(xml);

            Assert.AreEqual("toast", document.Root!.Name.LocalName);

            var texts = document.Descendants("text").Select(t => t.Value).ToArray();

            CollectionAssert.AreEqual(new[] { "New driver", "610.88 available" }, texts);
        }

        [TestMethod]
        public void EscapesTextThatWouldOtherwiseBreakTheDocument()
        {
            /* Driver names and release notes are full of characters that mean something in XML;
             * NVIDIA's own notes arrive as HTML.
             */

            var message = new NotificationMessage(
                subject: "Ampere & \"Ada\" <b>drivers</b>",
                summary: "5 < 10 & 'quoted'",
                htmlBody: "<p>notes</p>",
                downloadLink: "https://example.invalid");

            var document = XDocument.Parse(ToastPayload.Build(message));

            var texts = document.Descendants("text").Select(t => t.Value).ToArray();

            Assert.AreEqual("Ampere & \"Ada\" <b>drivers</b>", texts[0]);
            Assert.AreEqual("5 < 10 & 'quoted'", texts[1]);
        }
    }

    [TestClass]
    public class NotificationMessageTests
    {
        [TestMethod]
        public void DescribesTheUpdateAndTheCardItIsFor()
        {
            var update = new UpdateInfo("610.88", "Tue Jul 28, 2026", "<p>notes</p>",
                "https://example.invalid/d.exe", "GeForce Game Ready Driver");

            var message = NotificationMessage.ForUpdate(update, "NVIDIA GeForce RTX 4060");

            StringAssert.Contains(message.Subject, "GeForce Game Ready Driver");
            StringAssert.Contains(message.Summary, "610.88");
            StringAssert.Contains(message.Summary, "NVIDIA GeForce RTX 4060");
            Assert.AreEqual("https://example.invalid/d.exe", message.DownloadLink);
        }

        [TestMethod]
        public void StillReadsSensiblyWhenNvidiaGivesNoDriverName()
        {
            var update = new UpdateInfo("610.88", "Tue Jul 28, 2026", "<p>notes</p>", "https://example.invalid/d.exe");

            var message = NotificationMessage.ForUpdate(update, "NVIDIA GeForce RTX 4060");

            StringAssert.Contains(message.Subject, "driver");
        }
    }
}
