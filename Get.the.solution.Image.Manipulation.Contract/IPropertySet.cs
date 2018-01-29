using System;
using System.Collections.Generic;
using System.Text;

namespace Get.the.solution.Image.Manipulation.Contract
{
    //
    // Summary:
    //     Represents a collection of key-value pairs, correlating several other collection
    //     interfaces.
    public interface IPropertySet :  IDictionary<string, object>, IEnumerable<KeyValuePair<string, object>>
    {
    }
}
