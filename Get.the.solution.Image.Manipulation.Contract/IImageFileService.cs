using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface IImageFileService
    {
        IList<String> FileTypeFilter { get; set; }
        Task<IReadOnlyList<ImageFile>> PickMultipleFilesAsync();
        Task<ImageFile> PickSaveFileAsync(String preferredSaveLocation, String SuggestedFileName);
        Task WriteBytesAsync(ImageFile file, byte[] buffer);
        Task WriteBytesAsync(string folderPath, string suggestedFileName, ImageFile file, byte[] buffer);
        Task<ImageFile> FileToImageFileConverter(object storageFile);

        Task<IList<ImageFile>> GetFilesFromFolderAsync(string folderPath);
        Task<ImageFile> LoadImageFileAsync(string filepath);
        string GenerateResizedFileName(ImageFile storeage);
    }
}
