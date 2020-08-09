using System;
using System.Threading.Tasks;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface IPageDialogService
    {
        Task ShowAsync(String content);
        Task<bool> ShowConfirmationAsync(string content, string yesButton, string noButton);
    }
}
