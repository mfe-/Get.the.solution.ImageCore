using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface IFileService
    {
        /// <summary>
        /// Loads the stream of the overgiven file path
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <returns>The loaded stream</returns>
        Task<Stream> LoadStreamFromFileAsync(string path);
        /// <summary>
        /// Loads the stream of the overgiven file object. For UWP storageFile is expected. Other platforms may have its own file object type.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        Task<Stream> LoadStreamFromFileAsync(object file);
        Task<bool> HasGlobalWriteAccessAsync();
        Task<DirectoryInfo> PickDirectoryAsync();
        /// <summary>
        /// Opens the Save As Dialog and adds the file to the Future Access List
        /// </summary>
        /// <param name="preferredSaveLocation"></param>
        /// <param name="suggestedFileName"></param>
        /// <param name="fileTypeChoicesFilter"></param>
        /// <returns></returns>
        Task<Stream> PickSaveFileStreamAsync(String preferredSaveLocation, String suggestedFileName, IList<string> fileTypeChoicesFilter);
        Task CleanUpStorageItemTokensAsync(int removeOlderThanDays = 14);
        /// <summary>
        /// Deletes the overgiven file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<bool> DeleteFileAsync(string path);
    }
}
