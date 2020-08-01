using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface IDragDropService
    {
        bool IsDragAndDropEnabled { get; }

        void OnDragOverCommand(object param);

        Task OnDropCommandAsync(object param, ObservableCollection<ImageFile> ImageFiles);

    }
}
