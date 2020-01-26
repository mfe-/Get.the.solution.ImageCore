using System.IO;

namespace Get.the.solution.Image.Contract
{
    public interface IResizeService
    {
        MemoryStream Resize(Stream inputStream, int width, int height, string suggestedFileName = null, int quality = 75);
    }
}
