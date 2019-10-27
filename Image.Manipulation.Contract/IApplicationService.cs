using System;
using System.Threading.Tasks;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface IApplicationService
    {
        void SetActivatedEventArgs(String args);
        string ActivatedEventArgs { get; }
        string UriDefinitionOpen { get; }
        string UriDefinitionResize { get; }
        string GetDeviceFormFactorType();
        String GetAppVersion();
        String GetLocalCacheFolder();
        String GetCulture();
        bool CtrlPressed(object param);
        Task LaunchFileAsync(ImageFile imageFile, bool openWith = false);
        Task LaunchFileAsync(string protocol, object param);
        /// <summary>
        /// Starts on the platform a process which is associated with the overgiven uri
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        Task<bool> LaunchUriAsync(string protocol);
        void AddToClipboard(String content);
        void Exit();
    }
}
