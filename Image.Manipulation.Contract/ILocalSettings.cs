using System.Collections.Generic;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface ILocalSettings
    {
        IDictionary<string, object> Values { get; }
        bool EnabledImageViewer { get; set; }
        bool EnabledOpenSingleFileAfterResize { get; set; }
        bool EnableAddImageToGallery { get; set; }
        bool ShowSuccessMessage { get; set; }
    }
}
