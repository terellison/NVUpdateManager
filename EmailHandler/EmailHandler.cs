using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace NVUpdateManager.EmailHandler
{
    public class EmailInfo
    {
        public string To { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string EmailBody { get; set; }
        public string Priority { get; set; }
    }

    public enum SendPriority
    {
        Low,
        Normal,
        High
    }

    public static class EmailHandler
    {
        private static string Iv;
        private static string SecureEndpoint;
        private static string NotificationAddress;

        public static void ConfigureLogicAppEndpoint(string iv, string secureEndpoint)
        {
            Iv = iv;
            SecureEndpoint = secureEndpoint;
        }
        public static void EncodeLogicAppEndpoint(string insecureEndpointString)
        {
            byte[] toEncrypt = Encoding.ASCII.GetBytes(insecureEndpointString);

            // Create a byte array to hold the random value.
            byte[] entropy = new byte[16];
            // Create a new instance of the RNGCryptoServiceProvider.
            // Fill the array with a random value.

            using (var rngCSP = new RNGCryptoServiceProvider())
            {
                rngCSP.GetBytes(entropy);
            }

            byte[] encryptedData = ProtectedData.Protect(toEncrypt, entropy, DataProtectionScope.LocalMachine);
            string encryptedBase64 = Convert.ToBase64String(encryptedData);


            Console.WriteLine("<emailrelay scope=\"{2}\" iv=\"{0}\">{1}</emailrelay>",
                Convert.ToBase64String(entropy),
                Convert.ToBase64String(encryptedData),
                Environment.GetEnvironmentVariable("computerName"));
        }

        public static string DecodeSecureEndpoint(string secureEndpoint, string iv)
        {
            byte[] endpointBytes = Convert.FromBase64String(secureEndpoint);

            if (string.IsNullOrEmpty(iv))
            {
                return Encoding.ASCII.GetString(ProtectedData.Unprotect(endpointBytes, null, DataProtectionScope.LocalMachine));
            }
            else
            {
                byte[] ivBytes = Convert.FromBase64String(iv);
                return Encoding.ASCII.GetString(ProtectedData.Unprotect(endpointBytes, ivBytes, DataProtectionScope.LocalMachine));
            }
        }

        private static void SendEmail(string to, string subject, string body, SendPriority priority)
        {
            try
            {
                var emailObj = new EmailInfo();
                emailObj.Priority = "Normal";
                emailObj.To = to;
                emailObj.From = "unused2";
                emailObj.Subject = subject;
                emailObj.EmailBody = body;

                if (priority == SendPriority.Low)
                {
                    emailObj.Priority = "Low";
                }
                else if (priority == SendPriority.High)
                {
                    emailObj.Priority = "High";
                }

                string json = System.Text.Json.JsonSerializer.Serialize(emailObj, typeof(EmailInfo));
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                if (IsConfigured)
                {
                    var client = new HttpClient();

                    var response = client.PostAsync(GetRelayEndpoint(), content);

                    response.Wait();

                    Console.WriteLine("Email relay response: {0}", response.Result.StatusCode);
                }
                else
                {
                    Console.WriteLine("Email not configured, unable to send mail: {0}", subject);
                    Console.WriteLine(body);
                }
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
            }
        }

        public static void SendNotificationEmail(string subject, string messageBody, SendPriority pri)
        {
            SendEmail(NotificationAddress, subject, messageBody, pri);
        }


        private static string GetRelayEndpoint()
        {
            return DecodeSecureEndpoint(SecureEndpoint, Iv);
        }

        public static void ConfigureAddresses(string notificationAddress)
        {
            NotificationAddress = notificationAddress;
        }

        public static bool IsConfigured
        {
            get
            {
                return
                    !string.IsNullOrEmpty(Iv) &&
                    !string.IsNullOrEmpty(SecureEndpoint) &&
                    !string.Equals(NotificationAddress, null)
                    ;
            }
        }

    }
}
