using ImageSharp;
using ImageSharp.Processing;
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

            //Configuration.Default.AddImageFormat(new JpegFormat());
            
            using (Image<Rgba32> image = ImageSharp.Image.Load(inputStream))
            {
                var format = ImageSharp.Image.DetectFormat(inputStream);
                var output = new MemoryStream();
                var resized = image.Resize(width, height);
                resized.Save(output, format); // automatic encoder selected based on extension.
                return output;
            }

        }
        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
