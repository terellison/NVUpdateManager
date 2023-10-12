using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace NVUpdateManager.EmailHandler
{
    public class EmailInfo
    {
        public string Subject { get; set; }
        public string EmailBody { get; set; }

        public string To { get; set; }

        public string From { get; set; }

        public string Priority { get; set; }
    }

    public enum SendPriority
    {
        Low,
        Normal,
        High,
    }


    public static class EmailHandler
    {
        private static string Entropy;
        private static string SecureEndpoint;
        private static string NotificationAddress;

        public static void ConfigureLogicAppEndpoint(string entropy, string secureEndpoint)
        {
            Entropy = entropy;
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

            Console.WriteLine("entropy=\"{0}\"\nencrypted endpoint=\"{1}\"",
                Convert.ToBase64String(entropy),
                Convert.ToBase64String(encryptedData));
        }

        public static string DecodeSecureEndpoint(string secureEndpoint, string entropy)
        {
            byte[] endpointBytes = Convert.FromBase64String(secureEndpoint);

            if (string.IsNullOrEmpty(entropy))
            {
                return Encoding.ASCII.GetString(ProtectedData.Unprotect(endpointBytes, null, DataProtectionScope.LocalMachine));
            }
            else
            {
                byte[] ivBytes = Convert.FromBase64String(entropy);
                return Encoding.ASCII.GetString(ProtectedData.Unprotect(endpointBytes, ivBytes, DataProtectionScope.LocalMachine));
            }
        }

        private static void SendEmail(string to, string subject, string body)
        {
            try
            {
                var emailObj = new EmailInfo();
                emailObj.To = to;
                emailObj.Subject = subject;
                emailObj.EmailBody = body;
                emailObj.From = "unused2";
                emailObj.Priority = "Normal";


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

        public static void SendNotificationEmail(string subject, string messageBody)
        {
            SendEmail(NotificationAddress, subject, messageBody);
        }


        private static string GetRelayEndpoint()
        {
            return DecodeSecureEndpoint(SecureEndpoint, Entropy);
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
                    !string.IsNullOrEmpty(Entropy) &&
                    !string.IsNullOrEmpty(SecureEndpoint) &&
                    !string.Equals(NotificationAddress, null)
                    ;
            }
        }

    }
}
