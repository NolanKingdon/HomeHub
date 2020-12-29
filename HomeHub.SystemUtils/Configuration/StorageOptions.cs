using HomeHub.SystemUtils.SystemStorage;

namespace HomeHub.SystemUtils.Configuration
{
    public class StorageOptions
    {
        // The Unit (mb, kb, etc.) The size should report to by default.
        public StorageUnit Unit { get; set; }
        public string CommandInterface { get; set; }
        public string CommandArgs { get; set; }
    }
}
