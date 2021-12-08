using Get.the.solution.Image.Contract;
using Get.the.solution.Image.Manipulation.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Get.the.solution.Image.Manipulation.ServiceBase
{
    public abstract class ResizeBaseSerivce : IResizeService
    {
        protected ILoggerService _loggerService;
        protected ResizeBaseSerivce(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }
        public string GenerateResizedFileName(ImageFile storeage)
        {
            try
            {
                if (storeage != null)
                {
                    string suggestedfilename = $"{storeage.FileInfo.Name.Replace(storeage.FileInfo.Extension, String.Empty)}-{storeage.Width}x{storeage.Height}{storeage.FileInfo.Extension}";
                    _loggerService?.LogEvent(nameof(GenerateResizedFileName), new Dictionary<String, String>()
                        {
                            { nameof(suggestedfilename), $"{suggestedfilename}" },
                            { nameof(FileInfo.Extension), $"{new FileInfo(suggestedfilename)?.Extension}" },
                        });
                    return suggestedfilename;
                }
                else
                {
                    return String.Empty;
                }
            }
            catch (Exception e)
            {
                _loggerService?.LogException(nameof(GenerateResizedFileName), e);
            }
            return String.Empty;
        }

        public abstract Task<MemoryStream> ResizeAsync(Stream inputStream, int width, int height, string suggestedFileName = null, int quality = 75);

    }
}
