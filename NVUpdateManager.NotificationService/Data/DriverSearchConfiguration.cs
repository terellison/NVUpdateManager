using NVUpdateManager.Core.Data;

namespace NVUpdateManager.NotificationService.Data
{
    /// <summary>
    /// Optional settings for the driver search. Nothing here is required: the GPU is identified
    /// from the installed driver and looked up against NVIDIA's own product catalogue.
    /// </summary>
    public class DriverSearchConfiguration
    {
        /// <summary>
        /// Which driver line to watch.
        /// </summary>
        public DriverBranch Branch { get; set; } = DriverBranch.GameReady;

        /// <summary>
        /// Overrides the GPU name used for the catalogue lookup. Only needed when Windows reports
        /// an adapter name that NVIDIA does not publish under, for example on an OEM variant.
        /// </summary>
        public string? ProductNameOverride { get; set; }

        /// <summary>
        /// Pins the NVIDIA product series id ("psid"). Set this together with
        /// <see cref="ProductFamilyId"/> to bypass catalogue lookup entirely.
        /// </summary>
        public int? ProductSeriesId { get; set; }

        /// <summary>
        /// Pins the NVIDIA product family id ("pfid"). Set this together with
        /// <see cref="ProductSeriesId"/> to bypass catalogue lookup entirely.
        /// </summary>
        public int? ProductFamilyId { get; set; }
    }
}
