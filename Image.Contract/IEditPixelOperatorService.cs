using System.IO;
using System.Threading.Tasks;

namespace Get.the.solution.Image.Contract
{
    public interface IEditPixelOperatorService
    {
        Task<Stream> SoftwareBitmapToStreamAsync(object softwareBitmap);
        Task<object> StartEditPixelEditAsync(Stream stream);

        Task EditPixelAsync(object editablePixels, IEditPixelOperator editPixelOperator);
    }
}
