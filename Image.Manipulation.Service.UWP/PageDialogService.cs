using Get.the.solution.Image.Manipulation.Contract;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace Get.the.solution.Image.Manipulation.Service.UWP
{
    public class PageDialogService : IPageDialogService
    {
        public async Task ShowAsync(string content)
        {
            MessageDialog Dialog = new MessageDialog(content);
            await Dialog.ShowAsync();
        }

        public async Task<bool> ShowConfirmationAsync(string content,string yesButton,string noButton)
        {
            MessageDialog Dialog = new MessageDialog(content);
            Dialog.Commands.Add(new UICommand(yesButton) { Id = 0 });
            Dialog.Commands.Add(new UICommand(noButton) { Id = 1 });
            return ((int)(await Dialog.ShowAsync()).Id == 0);
        }
    }
}
