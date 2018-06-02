using System;
using System.Threading.Tasks;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface IApplicationService
    {
        String GetAppVersion();
        String GetLocalCacheFolder();
        String GetCulture();
        bool CtrlPressed(object param);
        Task LaunchFileAsync(ImageFile imageFile);
        void Exit();
    }
}
