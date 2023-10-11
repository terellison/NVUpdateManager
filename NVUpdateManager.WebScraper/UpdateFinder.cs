using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using NVUpdateManager.WebScraper.Data;
using static NVUpdateManager.WebScraper.Data.NvidiaDriverLookupInfo;

namespace NVUpdateManager.WebScraper
{
    public static class UpdateFinder
    {
        public static async Task<UpdateInfo> FindLatestUpdate(string gpuSeries, string gpuName, string driverType)
        {
            // TODO: Deduce arguments, make api call, parse the HTML result

            var productSeriesId = GetProductSeriesSearchValue()[gpuSeries];

            var productFamilyId = GetProductFamilySearchValue()[gpuName];


            var initialURI = $"https://www.nvidia.com/Download/processFind.aspx?psid={productSeriesId}&pfid={productFamilyId}&osid=57&lid=1&whql=&lang=en-us&ctk=0&qnfslb=00&dtcid=1";

            using(var client = new HttpClient())
            {
                var driverListResponse = await client.GetAsync(initialURI);

                ParseLinkToUpdate(await driverListResponse.Content.ReadAsStringAsync());

            }

            throw new NotImplementedException();
        }

        private static string ParseLinkToUpdate(string html)
        {
            var parser = new HtmlParser();

            var updateTable = parser.ParseDocument(html);

            _ = updateTable.QuerySelectorAll("td")
                .Where(x => x.Id == "driverList");

            return string.Empty;
        }

        public static string DownloadUpdate(string updateLink)
        {
            var downloadLocation = Path.GetRandomFileName();

            using(var client = new WebClient())
            {
                client.DownloadFile(new Uri(updateLink), downloadLocation);
            }

            var newLocation = Path.ChangeExtension(downloadLocation, ".exe");

            File.Move(downloadLocation, newLocation);

            return Path.GetFullPath(newLocation);
        }
    }

    
}
