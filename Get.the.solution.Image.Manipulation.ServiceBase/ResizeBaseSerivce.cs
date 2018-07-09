using Get.the.solution.Image.Manipulation.Contract;
using System;
using System.Collections.Generic;
using System.IO;

namespace Get.the.solution.Image.Manipulation.ServiceBase
{
    public abstract class ResizeBaseSerivce : IResizeService
    {
        protected ILoggerService _loggerService;
        public ResizeBaseSerivce(ILoggerService loggerService)
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
                    //String suggestedfilename = $"{storeage.Name.Replace(storeage.FileType, String.Empty)}-{Width}x{Height}{storeage.FileType}";
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

        public abstract MemoryStream Resize(Stream inputStream, int width, int height);

    }
}
