using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface IShareService : INotifyPropertyChanged
    {
        /// <summary>
        /// Initiates the share process
        /// </summary>
        /// <param name="packageTitle">Under which title or name the share packages should be displayed</param>
        /// <param name="imageFiles">The images which should be shared</param>
        /// <param name="viewModelReiszeImageFunc">A function which should be called, before sharing (sample resizing the image before sharing)</param>
        /// <param name="shareCompleteAction">Callback when share process is complete</param>
        /// <returns></returns>
        Task StartShareAsync(string packageTitle, IList<ImageFile> imageFiles, Func<Action<ImageFile, String>, Task<bool>> viewModelReiszeImageFunc = null, Action shareCompleteAction = null);

        bool SharingProcess { get; set; }

        void StartShareTargetOperation(object shareOperation);
        void EndShareTargetOperation();
    }
}
