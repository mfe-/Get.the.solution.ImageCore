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
        void WriteBytesAsync(ImageFile file, byte[] buffer);
    }
}
