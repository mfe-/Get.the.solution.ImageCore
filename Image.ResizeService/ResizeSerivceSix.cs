using Get.the.solution.Image.Contract;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Linq;

namespace Get.the.solution.Image.Manipulation.ResizeService
{
    /// <summary>
    /// https://blogs.msdn.microsoft.com/dotnet/2017/01/19/net-core-image-processing/
    /// </summary>
    public class ResizeSerivceSix : IResizeService
    {
        protected readonly ILoggerService _loggerService;
        protected readonly Configuration _configuration;
        public ResizeSerivceSix(ILoggerService loggerService)
        {
            _loggerService = loggerService;
            //for some reason DetectFormat returns tga for images - so we use our own configuration
            _configuration = new Configuration(
                  new SixLabors.ImageSharp.Formats.Bmp.BmpConfigurationModule()
                , new SixLabors.ImageSharp.Formats.Gif.GifConfigurationModule()
                , new SixLabors.ImageSharp.Formats.Png.PngConfigurationModule()
                , new SixLabors.ImageSharp.Formats.Jpeg.JpegConfigurationModule()
                );
        }
        public MemoryStream Resize(Stream inputStream, int width, int height, string suggestedFileName = null)
        {
            if (inputStream.Length == inputStream.Position)
            {
                inputStream.Position = 0;
            }

            using (var image = SixLabors.ImageSharp.Image.Load(_configuration, inputStream))
            {
                var format = SixLabors.ImageSharp.Image.DetectFormat(_configuration, inputStream);

                var output = new MemoryStream();
                image.Mutate((x) => x.AutoOrient().Resize(width, height));
                if (format == null)
                {
                    string extension = new FileInfo(suggestedFileName).Extension.ToLowerInvariant();
                    if (".gif".Equals(extension))
                    {
                        image.SaveAsGif(output);
                    }
                    else if (".bmp".Equals(extension))
                    {
                        image.SaveAsBmp(output);
                    }
                    else if (".png".Equals(extension))
                    {
                        image.SaveAsPng(output);
                    }
                    else
                    {
                        image.SaveAsJpeg(output);
                    }
                }
                else
                {
                    image.Save(output, format);
                }
                return output;
            }
        }
    }

}
