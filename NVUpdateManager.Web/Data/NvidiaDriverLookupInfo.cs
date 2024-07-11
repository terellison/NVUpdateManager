using System.Collections.Generic;

namespace NVUpdateManager.Web.Data
{
    internal static class NvidiaDriverLookupInfo
    {
        /// <summary>
        /// Maps a product series to a numerical value for the NVIDIA query string
        /// </summary>
        /// <returns></returns>
        internal static Dictionary<string, int> GetProductSeriesSearchValue()
        {
            return new Dictionary<string, int>
            {
                { "GeForce RTX 30 Series", 120 },
                { "GeForce RTX 40 Series", 127 },
                { "GeForce 16 Series", 112 }
            };
        }

        /// <summary>
        /// Maps a product family to a numerical value for the NVIDIA query string
        /// </summary>
        /// <returns></returns>
        internal static Dictionary<string, int> GetProductFamilySearchValue()
        {
            return new Dictionary<string, int>
            {
                { "GeForce RTX 3080", 929 },
                { "GeForce RTX 4090", 995 },
                { "GeForce GTX 1660 SUPER", 910 }
            };
        }

        // This maps to Windows
        internal static int OperatingSystemSearchValue = 135;

        // This maps to English
        internal static int LanguageSearchValue = 1;
    }
}
