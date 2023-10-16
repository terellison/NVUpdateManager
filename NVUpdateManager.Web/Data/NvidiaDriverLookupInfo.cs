using System.Collections.Generic;

namespace NVUpdateManager.Web.Data
{
    internal static class NvidiaDriverLookupInfo
    {
        internal static Dictionary<string, int> GetProductSeriesSearchValue()
        {
            return new Dictionary<string, int>
            {
                { "GeForce RTX 30 Series", 120 },
                { "GeForce 16 Series", 112 }
            };
        }

        internal static Dictionary<string, int> GetProductFamilySearchValue()
        {
            return new Dictionary<string, int>
            {
                { "GeForce RTX 3080", 929 },
                { "GeForce GTX 1660 SUPER", 910 }
            };
        }

        internal static int OperatingSystemSearchValue = 135;

        internal static int LanguageSearchValue = 1;
    }
}
