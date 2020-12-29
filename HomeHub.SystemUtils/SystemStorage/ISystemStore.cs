using System.Collections.ObjectModel;
using System.Threading.Tasks;
using HomeHub.SystemUtils.Models;

namespace HomeHub.SystemUtils.SystemStorage
{
    public interface ISystemStore
    {
         Task<Collection<StorageResult>> GetAllStorageSpaceAsync();
         Task<Collection<StorageResult>> GetAllStorageSpaceRAWAsync();
         Task<Collection<StorageResult>> GetStorageOfDrive(string drive);
    }
}