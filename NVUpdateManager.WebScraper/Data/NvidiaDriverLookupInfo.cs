using System;
using System.Collections.Generic;
using System.Text;

namespace NVUpdateManager.WebScraper.Data
{
    internal static class NvidiaDriverLookupInfo
    {
        internal static Dictionary<string, int> GetProductSeriesSearchValue()
        {
            return new Dictionary<string, int>
            {
                { "GeForce", 120 }
            };
        }

        internal static Dictionary<string, int> GetProductFamilySearchValue()
        {
            return new Dictionary<string, int>
            {
                { "GeForce RTX 3080", 929 }
            };
        }

        internal static int OperatingSystemSearchValue = 135;

        internal static int LanguageSearchValue = 1;
    }
}
