using Get.the.solution.Image.Contract;
using Get.the.solution.Image.Manipulation.Contract;
using System;
using System.Collections.Generic;
using System.IO;

namespace Get.the.solution.Image.Manipulation.ServiceBase
{
    public abstract class ImageFileBaseService
    {
        protected ILoggerService _loggerService;
        protected ImageFileBaseService(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }
        public virtual IList<string> FileTypeFilter { get; set; } = new List<String>() { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        public virtual string GenerateResizedFileName(ImageFile storeage, int? width, int? height)
        {
            try
            {
                if (!width.HasValue)
                {
                    width = storeage.Width;
                }
                if (!height.HasValue)
                {
                    height = storeage.Height;
                }
                if (storeage != null)
                {
                    string suggestedfilename = $"{storeage.FileInfo.Name.Replace(storeage.FileInfo.Extension, String.Empty)}-{width}x{height}{storeage.FileInfo.Extension}";
                    
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

        public virtual string GenerateSuccess(ImageFile imageFile)
        {
            return imageFile.Path;
        }
    }
}
