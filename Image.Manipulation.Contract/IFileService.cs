using System.IO;
using System.Threading.Tasks;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface IFileService
    {
        Task<bool> HasGlobalWriteAccessAsync();
        Task<DirectoryInfo> PickDirectoryAsync();
        Task CleanUpStorageItemTokensAsync(int removeOlderThanDays = 14);
    }
}
