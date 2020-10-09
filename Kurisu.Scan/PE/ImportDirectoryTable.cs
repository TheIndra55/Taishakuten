namespace Kurisu.Scan.PE
{
    public struct ImportDirectoryTable
    {
        public uint LookupTableRVA;
        public uint Timestamp;
        public uint ForwarderChain;
        public uint Name;
        public uint AddressRVA;
    }
}
