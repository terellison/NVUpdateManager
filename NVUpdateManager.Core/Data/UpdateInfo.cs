using System.Text;

namespace NVUpdateManager.Core.Data
{
    public class UpdateInfo
    {
        public UpdateInfo(string versionNumber, string releaseDate, string details, string downloadLink, string name = null)
        {
            VersionNumber = versionNumber;
            ReleaseDate = releaseDate;
            Details = details;
            DownloadLink = downloadLink;
            Name = name;
        }

        /// <summary>
        /// The driver's name, for example "GeForce Game Ready Driver" or "NVIDIA Studio Driver".
        /// </summary>
        public string Name { get; }

        public string VersionNumber { get; }
        public string ReleaseDate { get; }
        public string Details { get; }
        public string DownloadLink { get; }

        public override string ToString()
        {
            var returnValue = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(Name))
            {
                returnValue.Append($"<p>Driver: {Name}</p>");
            }

            returnValue.Append($"<p>Version: {VersionNumber}</p>");
            returnValue.Append($"<p>Release Date: {ReleaseDate}</p>");
            returnValue.Append($"<p>Download Link: {DownloadLink}</p>");
            returnValue.Append($"<p>Details:\n\n{Details}</p>");

            return returnValue.ToString();
        }
    }
}
