using System.IO;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface IResizeService
    {
        MemoryStream Resize(Stream inputStream, int width, int height);
    }
}
