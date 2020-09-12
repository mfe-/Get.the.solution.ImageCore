using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Get.the.solution.Image.Manipulation.Contract
{
    /// <summary>
    /// Provides methods to load, save and update image files
    /// </summary>
    public interface IImageFileService
    {
        /// <summary>
        /// Get or sets the list of suppored images by the service
        /// </summary>
        IList<String> FileTypeFilter { get; set; }
        /// <summary>
        /// Opens the file picker to open images
        /// </summary>
        /// <returns></returns>
        Task<IReadOnlyList<ImageFile>> PickMultipleFilesAsync();
        /// <summary>
        /// Opens the file picker and returns a <seealso cref="ImageFile"/>
        /// </summary>
        /// <param name="preferredSaveLocation"></param>
        /// <param name="suggestedFileName"></param>
        /// <returns></returns>
        Task<ImageFile> PickSaveFileAsync(String preferredSaveLocation, String suggestedFileName);
        /// <summary>
        /// Open the folder picker and returns a <seealso cref="ImageFile"/>
        /// </summary>
        /// <param name="preferredSaveLocation"></param>
        /// <param name="suggestedFileName"></param>
        /// <returns></returns>
        Task<ImageFile> PickSaveFolderAsync(String preferredSaveLocation, String suggestedFileName);
        /// <summary>
        /// Writes the given bytes from <paramref name="buffer"/> into <paramref name="file"/>
        /// </summary>
        /// <param name="file"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        Task WriteBytesAsync(ImageFile file, byte[] buffer);
        /// <summary>
        /// Writes the given bytes from <paramref name="folderPath"/> into the <seealso cref="ImageFile"/> which is determined by <paramref name="folderPath"/> and <paramref name="suggestedFileName"/>
        /// If the <paramref name="suggestedFileName"/> does not exist it will be created
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="suggestedFileName"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        Task<ImageFile> WriteBytesAsync(string folderPath, string suggestedFileName, byte[] buffer);
        /// <summary>
        /// Loads from the given <paramref name="storageFile"/> a <seealso cref="ImageFile"/>. 
        /// Depending on <paramref name="readStream"/> the file stream should be read and kept in memory.
        /// </summary>
        /// <param name="storageFile"></param>
        /// <param name="readStream"></param>
        /// <returns></returns>
        Task<ImageFile> FileToImageFileConverterAsync(object storageFile, bool readStream = true);
        /// <summary>
        /// Load all images from the given <paramref name="folderPath"/> into a list of <seealso cref="ImageFile"/>
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        Task<IList<ImageFile>> GetFilesFromFolderAsync(string folderPath);
        /// <summary>
        /// Load from the given image file path a <seealso cref="ImageFile"/>
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        Task<ImageFile> LoadImageFileAsync(string filepath);
        /// <summary>
        /// Generates a file name from the given parameters
        /// </summary>
        /// <param name="storeage"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        string GenerateResizedFileName(ImageFile storeage, int? width, int? height);
        /// <summary>
        /// Tries to set the given image as wallpaper on the device
        /// </summary>
        /// <param name="imageFile"></param>
        /// <returns></returns>
        Task<bool> TrySetWallpaperImageAsync(string imageFilePath);
    }
}
