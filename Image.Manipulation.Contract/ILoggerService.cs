using System;
using System.Collections.Generic;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface ILoggerService
    {
        void LogException(Exception e);
        void LogException(String methodname, Exception e, IDictionary<string, string> data = null);
        void LogEvent(string eventName);
        void LogEvent(string eventName, IDictionary<string, string> data);
        void LogEvent(string eventName, string value);
    }
}
