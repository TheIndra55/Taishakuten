using System.IO;
using System.Threading.Tasks;

namespace Kurisu.VirusScan
{
    interface IScan
    {
        /// <summary>
        /// Gets or sets the name of this scan service
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets whether or not this scan is powered by a third part and should be displayed in "Powered by ..."
        /// </summary>
        public bool ThirdParty { get; set; }

        /// <summary>
        /// Scans a file or looks up a SHA1 hash for any viruses or malware
        /// </summary>
        /// <param name="file">The stream containing the file</param>
        /// <param name="hash">The SHA1 hash of the file</param>
        public Task<ScanResult> ScanAsync(Stream file, string hash);
    }
}
