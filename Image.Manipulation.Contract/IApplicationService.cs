using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface IApplicationService
    {
        bool IsAlwaysOnTop { get; }
        Task<bool> ToggleAlwaysOnTopAsync();
        void SetAppTitlebar(string titleText);
        bool TrySetWindowSize();
        bool TrySetWindowSize(double width, double height);
        void SetActivatedEventArgs(String args);
        string ActivatedEventArgs { get; }
        string UriDefinitionOpen { get; }
        string UriDefinitionResize { get; }
        string UriFilePathParamName { get; }
        string GetDeviceFormFactorType();
        String GetAppVersion();
        String GetTemporaryFolderPath();
        String GetCulture();
        bool CtrlPressed(object param);
        /// <summary>
        /// starts the process for imagefile
        /// </summary>
        /// <param name="imageFile"></param>
        /// <param name="openWith"></param>
        /// <returns></returns>
        Task<bool> LaunchFileAsync(ImageFile imageFile, bool openWith = false);
        /// <summary>
        /// Starts protocol based process
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<bool> LaunchProtocolFileAsync(string protocol, IDictionary<string, object> parameters);
        /// <summary>
        /// Starts the file explorer 
        /// </summary>
        Task<bool> LaunchFileExplorerAsync(string protocol);
        /// <summary>
        /// Starts on the platform a process which is associated with the overgiven uri
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        Task<bool> LaunchUriAsync(string protocol);
        void AddToClipboard(String content);
        Task<Stream> GetClipboardAsync();
        void Exit();
    }
}
