using System.Threading.Tasks;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface IApplicationService
    {
        bool CtrlPressed(object param);
        Task LaunchFileAsync(ImageFile imageFile);
        void Exit();
    }
}
