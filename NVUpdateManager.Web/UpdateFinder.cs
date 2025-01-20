using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
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

            var updateNumber = latestUpdateLink.Split('/')
                .Where(v => int.TryParse(v, out _))
                .First();

            var downloadDetailsURL = $"https://www.nvidia.com/services/com.nvidia.services/AEMDriversContent/getDownloadDetails?{'{' + $"%22ddID%22:%22{updateNumber}%22" + '}'}";

            var downloadDetails = (await GetDownloadDetails(downloadDetailsURL))
                .GetProperty("driverDetails")
                .GetProperty("IDS")
                .EnumerateArray()
                .ElementAt(0)
                .GetProperty("downloadInfo");

            return ParseUpdateInfo(downloadDetails);
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

        private UpdateInfo ParseUpdateInfo(JsonElement root)
        {

            var versionNumber = root.GetProperty("Version")
                .GetString();

            var releaseDate = root.GetProperty("ReleaseDateTime")
                .GetString();

            var notes = root.GetProperty("ReleaseNotes")
                .GetString();

            var details = HttpUtility.UrlDecode(notes);

            var url = root.GetProperty("DownloadURL")
                .GetString();

            return new UpdateInfo(versionNumber, releaseDate, details, url);
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

        private async Task<JsonElement> GetDownloadDetails(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();

            JsonDocument doc = JsonDocument.Parse(jsonResponse);

            return doc.RootElement;
        }
    }
}
