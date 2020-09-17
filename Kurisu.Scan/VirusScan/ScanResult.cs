using System;
using System.Collections.Generic;
using System.Text;

namespace Kurisu.VirusScan
{
    public class ScanResult
    {
        /// <summary>
        /// Gets or sets the score returned by the antivirus scan
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Gets or sets the detection method
        /// </summary>
        public string Detection { get; set; }

        /// <summary>
        /// Gets or sets extra information returned by this scan, leave empty for not being present
        /// </summary>
        public string Extra { get; set; }

        /// <summary>
        /// Gets or sets the image associated with this scan
        /// </summary>
        public string Image { get; set; }
    }
}
