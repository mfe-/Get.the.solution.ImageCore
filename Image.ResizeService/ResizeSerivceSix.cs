using Get.the.solution.Image.Contract;
using Get.the.solution.Image.Manipulation.Contract;
using SixLabors.ImageSharp;
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
        public ResizeSerivceSix(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }
        public MemoryStream Resize(Stream inputStream, int width, int height, string suggestedFileName = null, int quality = 75)
        {
            if (inputStream.Length == inputStream.Position)
            {
                inputStream.Position = 0;
            }
            try
            {
                using (var image = SixLabors.ImageSharp.Image.Load(inputStream))
                {
                    var output = new MemoryStream();
                    image.Mutate((x) => x.AutoOrient().Resize(width, height));

                    string extension = new FileInfo(suggestedFileName).Extension.ToLowerInvariant();
                    var format = SixLabors.ImageSharp.Image.DetectFormat(inputStream);
                    if (format == null)
                    {
                        _loggerService.LogEvent(
                            $"{nameof(SixLabors.ImageSharp.Image.DetectFormat)} is null. Using image extension", extension);
                        if (".gif".Equals(extension))
                        {
                            format = SixLabors.ImageSharp.Formats.Gif.GifFormat.Instance;
                        }
                        else if (".bmp".Equals(extension))
                        {
                            format = SixLabors.ImageSharp.Formats.Bmp.BmpFormat.Instance;
                        }
                        else if (".png".Equals(extension))
                        {
                            format = SixLabors.ImageSharp.Formats.Png.PngFormat.Instance;
                        }
                        else
                        {
                            format = SixLabors.ImageSharp.Formats.Jpeg.JpegFormat.Instance;
                        }
                    }

                    _loggerService.LogEvent(
                        $"{nameof(SixLabors.ImageSharp.Image.DetectFormat)} is", format?.Name);

                    if(format is SixLabors.ImageSharp.Formats.Jpeg.JpegFormat)
                    {
                        image.Save(output,
                            new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder() { Quality = quality });
                    }
                    else
                    {
                        image.Save(output, format);
                    }


                    return output;
                }
            }
            catch (UnknownImageFormatException ui)
            {
                throw new Image.Contract.Exceptions.UnknownImageFormatException(ui.Message, ui);
            }
        }
    }

}
