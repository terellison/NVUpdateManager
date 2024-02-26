using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using NVUpdateManager.Core.Data;
using NVUpdateManager.Core.Interfaces;
using static NVUpdateManager.Web.Data.NvidiaDriverLookupInfo;

namespace NVUpdateManager.Web
{
    public class UpdateFinder : IUpdateFinder
    {
        private readonly HttpClient _httpClient;

        public UpdateFinder(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UpdateInfo> FindLatestUpdate(string gpuSeries, string gpuName, string driverType)
        {
            var productSeriesId = GetProductSeriesSearchValue()[gpuSeries];

            var productFamilyId = GetProductFamilySearchValue()[gpuName];

            var initialURI = $"https://www.nvidia.com/Download/processFind.aspx?psid={productSeriesId}&pfid={productFamilyId}&osid=57&lid=1&whql=&lang=en-us&ctk=0&qnfslb=00&dtcid=1";

            var driverListResponse = await _httpClient.GetAsync(initialURI);

            var latestUpdateLink = ParseLinkToUpdate(await driverListResponse.Content.ReadAsStringAsync());

            string updateHtml = await (await _httpClient.GetAsync(latestUpdateLink)).Content.ReadAsStringAsync();

            return await ParseUpdateInfo(updateHtml);
        }

        private string ParseLinkToUpdate(string html)
        {
            var parser = new HtmlParser();

            var updateTable = parser.ParseDocument(html);

            var latestDriver = updateTable.All.First(
                x => x.Id == "driverList"
                && x.QuerySelector("a").TextContent.Contains("Game Ready Driver"));

            var result = "https:";
            result += latestDriver.QuerySelector("a").GetAttribute("href");

            return result;
        }

        private async Task<UpdateInfo> ParseUpdateInfo(string html)
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

        private async Task<string> GetActualDownloadLink(string url)
        {
            HttpResponseMessage response;

            response = await _httpClient.GetAsync(url);
            
            var html = await response.Content.ReadAsStringAsync();

            var downloadPage = new HtmlParser().ParseDocument(html);

            var result = "https:";

            return result + downloadPage.QuerySelector("btn_drvr_lnk_txt").ParentElement.GetAttribute("href");
        }

        public async Task<string> DownloadUpdate(string updateLink)
        {
            var downloadLocation = Path.GetRandomFileName();

            var response = await _httpClient.GetAsync(updateLink);

            using(var fs = new FileStream(downloadLocation, FileMode.CreateNew))
            {
                await response.Content.CopyToAsync(fs);
            }

            var newLocation = Path.ChangeExtension(downloadLocation, ".exe");

            File.Move(downloadLocation, newLocation);

            return Path.GetFullPath(newLocation);
        }
    }
}
