
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Get.the.solution.Image.Manipulation
{
    /// <summary>
    /// https://blogs.msdn.microsoft.com/dotnet/2017/01/19/net-core-image-processing/
    /// </summary>
    public class ImageService
    {
        public static MemoryStream Resize(Stream inputStream, int width, int height)
        {
            const int quality = 75;

            //////Configuration.Default.AddImageFormat(new JpegFormat());
            //if (inputStream.Length == inputStream.Position)
            //{
            //    inputStream.Position = 0;
            //}
            //using (Image<Rgba32> image = SixLabors.ImageSharp.Image.Load(inputStream))
            //{
            //    var format = SixLabors.ImageSharp.Image.DetectFormat(inputStream);
            //    var output = new MemoryStream();
            //    image.Mutate(x => x.Resize(width, height));
            //    image.Save(output, format);
            //    return output;
            //}
            return null;
        }
    }
}
