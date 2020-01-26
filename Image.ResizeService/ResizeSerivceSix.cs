using Get.the.solution.Image.Contract;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;

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
        public MemoryStream Resize(Stream inputStream, int width, int height, string suggestedFileName = null, int quality = 75)
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
                    _loggerService.LogEvent(
                        $"{nameof(SixLabors.ImageSharp.Image.DetectFormat)} is null. Using image extension", extension);
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
                        if (quality != 100)
                        {
                            image.SaveAsJpeg(output, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder() { Quality = quality });
                        }
                        else
                        {
                            image.SaveAsJpeg(output);
                        }

                    }
                }
                else
                {
                    _loggerService.LogEvent(
                        $"{nameof(SixLabors.ImageSharp.Image.DetectFormat)} is", format?.Name);
                    image.Save(output, format);
                }
                return output;
            }
        }
    }

}
