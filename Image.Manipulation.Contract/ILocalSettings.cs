using System.Collections.Generic;
using System.Threading.Tasks;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface ILocalSettings<TSetting>
    {
        Task LoadSettingsAsync();
        TSetting Settings { get; set; }
    }
    public interface ILocalSettings
    {
        IDictionary<string, object> Values { get; }
    }
}
