using ImageSharp;
using ImageSharp.Formats;
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

            Configuration.Default.AddImageFormat(new JpegFormat());

            using (var input = inputStream)
            {
                var output = new MemoryStream();
                {
                    var image = new ImageSharp.Image(input)
                        .Resize(new ResizeOptions
                        {
                            Size = new Size(width, height),
                            Mode = ResizeMode.Max
                        });
                    //image.ExifProfile = null;
                    //image.Quality = quality;
                    image.Save(output);
                    return output;
                }
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
