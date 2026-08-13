using System.Collections.Specialized;
using System.Net;
using System.Web;

namespace NVUpdateManager.Web.Tests
{
    /// <summary>
    /// Serves a miniature copy of NVIDIA's lookup service so the catalogue can be exercised
    /// without touching the network. The shape of the XML matches what the real endpoint
    /// returns, including the "NVIDIA" prefix appearing only on recent products.
    /// </summary>
    internal sealed class StubLookupHandler : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;

            var query = HttpUtility.ParseQueryString(request.RequestUri!.Query);

            var typeId = query["TypeID"];
            var parentId = query["ParentID"];

            var body = (typeId, parentId) switch
            {
                ("1", _) => Xml(("GeForce", 1)),

                ("2", "1") => Xml(
                    ("GeForce RTX 40 Series", 127),
                    ("GeForce RTX 40 Series (Notebooks)", 129),
                    ("GeForce 10 Series", 101),
                    ("GeForce 10 Series (Notebooks)", 102)),

                ("3", "127") => Xml(
                    ("NVIDIA GeForce RTX 4090", 995),
                    ("NVIDIA GeForce RTX 4070", 1015),
                    ("NVIDIA GeForce RTX 4060", 1023)),

                ("3", "129") => Xml(
                    ("GeForce RTX 4090 Laptop GPU", 1004),
                    ("GeForce RTX 4070 Laptop GPU", 1006)),

                ("3", "101") => Xml(("GeForce GTX 1080", 815)),

                ("3", "102") => Xml(("GeForce GTX 1080", 819)),

                ("4", _) => Xml(
                    ("Windows 10 64-bit", 57),
                    ("Windows 11", 135),
                    ("Linux 64-bit", 12)),

                _ => Xml()
            };

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body)
            });
        }

        private static string Xml(params (string Name, int Value)[] values)
        {
            var entries = values.Select(v =>
                $"<LookupValue><Name>{v.Name}</Name>\n<Value>{v.Value}</Value></LookupValue>");

            return "<?xml version=\"1.0\" encoding=\"UTF-8\"?><LookupValueSearch><LookupValues>"
                + string.Concat(entries)
                + "</LookupValues></LookupValueSearch>";
        }
    }

    /// <summary>
    /// Hands out clients backed by a single stub handler.
    /// </summary>
    internal sealed class StubHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;

        public StubHttpClientFactory(HttpMessageHandler handler)
        {
            _handler = handler;
        }

        public HttpClient CreateClient(string name)
        {
            // disposeHandler: false so the shared handler survives each client being disposed.
            return new HttpClient(_handler, disposeHandler: false);
        }
    }
}
