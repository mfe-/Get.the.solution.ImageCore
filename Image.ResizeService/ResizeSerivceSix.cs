using Get.the.solution.Image.Contract;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.Primitives;
using System.IO;

namespace Get.the.solution.Image.Manipulation.ResizeService
{
    /// <summary>
    /// https://blogs.msdn.microsoft.com/dotnet/2017/01/19/net-core-image-processing/
    /// </summary>
    public class ResizeSerivceSix : IResizeService
    {
        protected ILoggerService _loggerService;
        public ResizeSerivceSix(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }
        public MemoryStream Resize(Stream inputStream, int width, int height)
        {
            //int usedWidth = 0;
            //int usedHeight = 0;
            //if (height < width)
            //{
            //    usedWidth = height;
            //    usedHeight = width;
            //}
            //else
            //{
            //    usedHeight = width;
            //    usedWidth = height;
            //}
            if (inputStream.Length == inputStream.Position)
            {
                inputStream.Position = 0;
            }
            using (Image<Rgba32> image = SixLabors.ImageSharp.Image.Load(inputStream))
            {
                var format = SixLabors.ImageSharp.Image.DetectFormat(inputStream);

                var output = new MemoryStream();
                image.Mutate((x) => x.AutoOrient().Resize(width, height));
                if (format == null)
                {
                    image.SaveAsJpeg(output);
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
