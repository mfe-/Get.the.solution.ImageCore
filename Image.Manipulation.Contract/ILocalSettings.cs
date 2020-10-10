using System.Collections.Generic;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface ILocalSettings
    {
        IDictionary<string, object> Values { get; }
    }
}
