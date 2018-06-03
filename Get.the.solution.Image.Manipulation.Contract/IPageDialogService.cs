using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface IPageDialogService
    {
        Task ShowAsync(String content);
    }
}
