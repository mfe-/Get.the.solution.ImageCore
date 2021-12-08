using Get.the.solution.Image.Contract;
using Get.the.solution.Image.Manipulation.Contract;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Threading.Tasks;

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
        public async Task<MemoryStream> ResizeAsync(Stream inputStream, int width, int height, string suggestedFileName = null, int quality = 75)
        {
            if (inputStream.Length == inputStream.Position)
            {
                inputStream.Position = 0;
            }
            try
            {
                (SixLabors.ImageSharp.Image Image, IImageFormat Format) imf = await SixLabors.ImageSharp.Image.LoadWithFormatAsync(inputStream);
                {
                    var output = new MemoryStream();
                    imf.Image.Mutate((x) => x.AutoOrient().Resize(width, height));

                    if (imf.Format == null)
                    {
                        string extension = new FileInfo(suggestedFileName).Extension.ToLowerInvariant();
                        _loggerService.LogEvent(
                            $"{nameof(SixLabors.ImageSharp.Image.DetectFormat)} is null. Using image extension", extension);
                        if (".gif".Equals(extension))
                        {
                            imf.Format = SixLabors.ImageSharp.Formats.Gif.GifFormat.Instance;
                        }
                        else if (".bmp".Equals(extension))
                        {
                            imf.Format = SixLabors.ImageSharp.Formats.Bmp.BmpFormat.Instance;
                        }
                        else if (".png".Equals(extension))
                        {
                            imf.Format = SixLabors.ImageSharp.Formats.Png.PngFormat.Instance;
                        }
                        else
                        {
                            imf.Format = SixLabors.ImageSharp.Formats.Jpeg.JpegFormat.Instance;
                        }
                    }

                    _loggerService.LogEvent(
                        $"{nameof(SixLabors.ImageSharp.Image.DetectFormat)} is", imf.Format?.Name);

                    if (imf.Format is SixLabors.ImageSharp.Formats.Jpeg.JpegFormat)
                    {
                        imf.Image.Save(output,
                            new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder() { Quality = quality });
                    }
                    else
                    {
                        imf.Image.Save(output, imf.Format);
                    }
                    imf.Image?.Dispose();

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
