using System.Threading.Tasks;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface IFileSystemPermissionDialogService
    {
        /// <summary>
        /// Shows the user a dialog that the app needs file access permissions. 
        /// </summary>
        Task ShowFileSystemAccessDialogAsync();
    }
}
