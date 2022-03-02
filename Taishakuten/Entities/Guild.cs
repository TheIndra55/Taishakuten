namespace Taishakuten.Entities
{
    class Guild
    {
        public ulong Id { get; set; }

        // settings
        // one to many settings should get their own entity

        public bool ScanEnabled { get; set; } = false;
    }
}
