using System.Diagnostics;

namespace Kurisu.Scan.PE
{
    [DebuggerDisplay("{Name}")]
    public class Import
    {
        public string Name { get; set; }
        public ushort Ordinal { get; set; }
    }
}
