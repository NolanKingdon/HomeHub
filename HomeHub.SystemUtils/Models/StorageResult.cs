using HomeHub.SystemUtils.SystemStorage;

namespace HomeHub.SystemUtils.Models
{
    public class StorageResult
    {
        public string FileSystem { get; set; }
        public double TotalSpace { get; set; }
        public StorageUnit Unit { get; set; }
        public double UsedSpacePercent { get; set; }
        public string Mount { get; set; }
    }
}
