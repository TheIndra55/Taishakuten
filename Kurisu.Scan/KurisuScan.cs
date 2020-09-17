using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kurisu.Scan.PE;
using Kurisu.VirusScan;

namespace Kurisu.Scan
{
    public class KurisuScan : IScan
    {
        public string Name { get; set; } = "Kurisu.Scan";
        public bool ThirdParty { get; set; } = false;

        public string[] SuspiciousIndicators =
        {
            "NtRaiseHardError",
            "RtlAdjustPrivilege",
            "NtLoadDriver",
            "ZwLoadDriver"
        };

        public async Task<ScanResult> ScanAsync(Stream file, string hash)
        {
            var result = new ScanResult();

            var pe = new PEReader(file);
            try
            {
                pe.Parse();
            }
            catch(Exception)
            {
                // if any of the parsing goes wrong just stop the scan
                throw new NoScanResultException();
            }

            var imports = SuspiciousIndicators.Where(indicator => pe.Imports.Any(x => x.Imports.Any(y => indicator == y.Name)));

            result.Score = imports.Count();
            result.Detection = "Suspicious imports";
            result.Extra = $"Suspicious imports: {string.Join(", ", imports)}";

            return result;
        }
    }
}
