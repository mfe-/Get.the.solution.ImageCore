using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface IShareService : INotifyPropertyChanged
    {
        Task StartShareAsync(IList<ImageFile> imageFiles, Func<Action<ImageFile, String>, Task<bool>> viewModelReiszeImageFunc, Action shareCompleteAction = null);

        bool SharingProcess { get; set; }

        void StartShareTargetOperation(object shareOperation);
        void EndShareTargetOperation();
    }
}
