using System.IO;
using System.Threading.Tasks;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface IFileService
    {
        Task<bool> HasGlobalWriteAccessAsync();
        Task<DirectoryInfo> PickDirectoryAsync();
        Task CleanUpStorageItemTokensAsync(int removeOlderThanDays = 14);
        /// <summary>
        /// Deletes the overgiven file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<bool> DeleteFileAsync(string path);
    }
}
