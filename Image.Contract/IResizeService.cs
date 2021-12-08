using System.IO;
using System.Threading.Tasks;

namespace Get.the.solution.Image.Contract
{
    public interface IResizeService
    {
        Task<MemoryStream> ResizeAsync(Stream inputStream, int width, int height, string suggestedFileName = null, int quality = 75);
    }
}
