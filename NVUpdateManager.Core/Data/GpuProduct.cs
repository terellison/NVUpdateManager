namespace NVUpdateManager.Core.Data
{
    /// <summary>
    /// A GPU model as NVIDIA's driver lookup service knows it, paired with the identifiers
    /// its driver search expects. These identifiers are discovered at runtime rather than
    /// hardcoded, because NVIDIA assigns new ones every time a product launches.
    /// </summary>
    public sealed class GpuProduct
    {
        public GpuProduct(string productName, string seriesName, int productSeriesId, int productFamilyId, bool isNotebook)
        {
            ProductName = productName;
            SeriesName = seriesName;
            ProductSeriesId = productSeriesId;
            ProductFamilyId = productFamilyId;
            IsNotebook = isNotebook;
        }

        /// <summary>
        /// The product name as NVIDIA lists it, for example "GeForce RTX 3080".
        /// </summary>
        public string ProductName { get; }

        /// <summary>
        /// The series the product belongs to, for example "GeForce RTX 30 Series".
        /// </summary>
        public string SeriesName { get; }

        /// <summary>
        /// The "psid" query string value identifying the product series.
        /// </summary>
        public int ProductSeriesId { get; }

        /// <summary>
        /// The "pfid" query string value identifying the product family.
        /// </summary>
        public int ProductFamilyId { get; }

        /// <summary>
        /// True when this entry describes the notebook variant of a GPU. NVIDIA lists desktop
        /// and notebook parts separately, and before the Ampere generation both variants often
        /// share a name, so this is what tells them apart.
        /// </summary>
        public bool IsNotebook { get; }

        public override string ToString()
        {
            return $"{ProductName} ({SeriesName}; psid={ProductSeriesId}, pfid={ProductFamilyId})";
        }
    }
}
