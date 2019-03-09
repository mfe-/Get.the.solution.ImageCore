using System.Threading.Tasks;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface IFileSystemPermissionDialogService
    {
        Task ShowFileSystemAccessDialogAsync();
    }
}
