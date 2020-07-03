using System;
using System.Collections.Generic;

namespace Get.the.solution.Image.Contract
{
    public interface ILoggerService
    {
        void LogException(Exception e);
        void LogException(String methodname, Exception e);
        void LogException(String methodname, Exception e, IDictionary<String, String> data);
        void LogEvent(string eventName);
        void LogEvent(string eventName, IDictionary<string, string> data);
        void LogEvent(string eventName, string value);
    }
}
