using System;
using System.IO;
using System.Net;
using NVUpdateManager.WebScraper.Data;
using static NVUpdateManager.WebScraper.Data.NvidiaDriverLookupInfo;

namespace NVUpdateManager.WebScraper
{
    public static class UpdateFinder
    {
        public static UpdateInfo FindLatestUpdate(string gpuSeries, string gpuName, string driverType)
        {
            // TODO: Deduce arguments, make api call, parse the HTML result

            var productSeriesId = GetProductSeriesSearchValue()[gpuSeries];

            var productFamilyId = GetProductFamilySearchValue()[gpuName];


            throw new NotImplementedException();
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
