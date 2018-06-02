using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface ILoggerService
    {
        void LogException(String methodname, Exception e);
        void LogException(String methodname, Exception e, IDictionary<String, String> data);
        void LogEvent(string eventName);
        void LogEvent(string eventName, IDictionary<string, string> data);
        void LogEvent(string eventName, string value);
    }
}
