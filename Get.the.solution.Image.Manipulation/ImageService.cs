using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Transforms;
using System.IO;

namespace Get.the.solution.Image.Manipulation
{
    /// <summary>
    /// https://blogs.msdn.microsoft.com/dotnet/2017/01/19/net-core-image-processing/
    /// </summary>
    public class ImageService
    {
        public static MemoryStream Resize(Stream inputStream, int width, int height)
        {
            if (inputStream.Length == inputStream.Position)
            {
                inputStream.Position = 0;
            }
            using (Image<Rgba32> image = SixLabors.ImageSharp.Image.Load(inputStream))
            {
                var format = SixLabors.ImageSharp.Image.DetectFormat(inputStream);
                var output = new MemoryStream();
                image.Mutate(x => x.Resize(width, height));
                image.Save(output, format);
                return output;
            }
        }
    }
}
