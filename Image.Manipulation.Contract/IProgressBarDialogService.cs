using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface IProgressBarDialogService : INotifyPropertyChanged
    {
        IProgressBarDialogService ProgressBarDialogFactory();
        Task StartAsync(int amountItems);
        String CurrentItem { get; set; }
        int ProcessedItems { get; set; }
        void Stop();
        bool AbortedClicked();

    }
}
