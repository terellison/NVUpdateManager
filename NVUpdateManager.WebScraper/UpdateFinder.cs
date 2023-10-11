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

            var updateHtml = string.Empty;
            using(var client = new HttpClient())
            {
                var driverListResponse = await client.GetAsync(initialURI);

                var latestUpdateLink = ParseLinkToUpdate(await driverListResponse.Content.ReadAsStringAsync());

                updateHtml = await (await client.GetAsync(latestUpdateLink)).Content.ReadAsStringAsync();
            }

            return await ParseUpdateInfo(updateHtml);
        }

        private static string ParseLinkToUpdate(string html)
        {
            var parser = new HtmlParser();

            var updateTable = parser.ParseDocument(html);

            var latestDriver = updateTable.All.First(x => x.Id == "driverList");

            var result = "https:";
            result += latestDriver.QuerySelector("a").GetAttribute("href");

            return result;
        }

        private static async Task<UpdateInfo> ParseUpdateInfo(string html)
        {
            var parser = new HtmlParser();

            var updatePage = parser.ParseDocument(html);

            var versionNumber = updatePage.QuerySelectorAll("td")
                .FirstOrDefault(x => x.Id == "tdVersion")
                .Text()
                .Trim();

            versionNumber = versionNumber.Substring(0, versionNumber.IndexOf('W')).TrimEnd();


            var releaseDate = updatePage.All.First(x => x.Id == "tdReleaseDate").InnerHtml.Trim();
            var details = updatePage.All.First(x => x.Id == "tab1_content").InnerHtml.Trim();

            var url = "https://nvidia.com";
                
            url += updatePage.QuerySelectorAll("a")
                             .FirstOrDefault(x => x.InnerHtml.Contains("btn_drvr_lnk_txt"))
                             .GetAttribute("href");

            url = await GetActualDownloadLink(url);

            return new UpdateInfo(versionNumber, releaseDate, details, url);
        }

        private static async Task<string> GetActualDownloadLink(string url)
        {
            HttpResponseMessage response;

            using(var client  = new HttpClient())
            {
                response = await client.GetAsync(url);
            }

            var html = await response.Content.ReadAsStringAsync();

            var downloadPage = new HtmlParser().ParseDocument(html);

            var result = "https:";

            return result + downloadPage.QuerySelector("btn_drvr_lnk_txt").ParentElement.GetAttribute("href");
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
