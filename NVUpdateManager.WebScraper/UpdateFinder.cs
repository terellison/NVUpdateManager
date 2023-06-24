using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using NVUpdateManager.WebScraper.Data;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace NVUpdateManager.WebScraper
{
    public static class UpdateFinder
    {
        private const string _searchUrl = "https://www.nvidia.com/Download/Find.aspx";
        private static IWebDriver _driver;

        public static UpdateInfo FindLatestUpdate(string gpuSeries, string gpuName, string driverType)
        {
            var options = new EdgeOptions();

            options.AddArgument("--no-sandbox");
            options.AddArguments("--disable-dev-shm-usage");

            var file = new DriverManager().SetUpDriver(new EdgeConfig(), "Latest");

            _driver = new EdgeDriver(Path.GetDirectoryName(file), options);

            UpdateInfo info;

            try
            {
                _driver.Url = _searchUrl;

                SelectOptionFromSelector("selProductSeriesType", driverType);

                SelectOptionFromSelector("selProductSeries", gpuSeries);

                SelectOptionFromSelector("selProductFamily", gpuName);

                SelectOptionFromSelector("ddWHQL", "Recommended/Certified"); // We only want recommended drivers

                var searchButton = _driver.FindElement(By.CssSelector("btn_drvr_lnk_txt"));

                searchButton.Click();

                Thread.Sleep(500); // We should use an event to know when the page is ready instead of waiting

                NavigateToLatestDriver();

                info = GetLatestDriverInfo();
            }
            finally { _driver.Quit(); }

            _driver.Dispose();


            return info;
        }

        private static UpdateInfo GetLatestDriverInfo()
        {
            var versionNumber = _driver.FindElement(By.Id("tdVersion")).Text
                                       .Split(' ')
                                       .First();
            var releaseDate = _driver.FindElement(By.Id("tdReleaseDate")).Text;
            var details = _driver.FindElement(By.Id("tab1_content"))
                                 .GetAttribute("innerHTML")
                                 .Trim();

            _driver.Url = _driver.FindElement(By.Id("lnkDwnldBtn")).GetAttribute("href");

            var downloadLink = _driver.FindElements(By.CssSelector("a"))
                                      .FirstOrDefault(x => x.GetAttribute("innerHTML").Contains("btn_drvr_lnk_txt"))
                                      .GetAttribute("href");

            return new UpdateInfo(versionNumber, releaseDate, details, downloadLink);
        }

        private static void NavigateToLatestDriver()
        {
            var latestDriver = _driver.FindElement(By.Id("driverList"))
                                      .FindElements(By.CssSelector("td"))
                                      .FirstOrDefault(x => x.GetAttribute("class") == "gridItem driverName");

            latestDriver?.FindElement(By.CssSelector("a")).Click();
        }

        private static void SelectOptionFromSelector(string selectorId, string toSelect)
        {
            var selector = new SelectElement(_driver.FindElement(By.Id(selectorId)));

            selector.SelectByText(toSelect);
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
