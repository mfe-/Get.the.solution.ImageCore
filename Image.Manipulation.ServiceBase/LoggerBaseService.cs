﻿using Get.the.solution.Image.Manipulation.Contract;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Get.the.solution.Image.Manipulation.ServiceBase
{
    public abstract class LoggerBaseService : ILoggerService
    {
        protected readonly Queue<Action> _queueLogs = new Queue<Action>();
        /// <summary>
        /// Get or sets whether collection data is enabled
        /// </summary>
        public bool EnableTracking { get; set; } = true;

        public virtual Task SendBufferAsync()
        {
            return Task.CompletedTask;
        }
        public virtual void LogException(Exception e)
        {
            LogException(nameof(Exception), e, null);
        }
        public virtual void LogException(string methodname, Exception e, IDictionary<string, string> data = null)
        {
            try
            {
                if (data == null)
                {
                    data = new Dictionary<String, String>();
                }
                if (!data.ContainsKey(nameof(methodname)))
                {
                    data.Add(nameof(methodname), methodname);
                }

                data.Add(nameof(Exception.Data), $"{e?.Data}");
                data.Add(nameof(Exception.HResult), $"{e?.HResult}");
                data.Add(nameof(Exception.InnerException), $"{e?.InnerException}");
                data.Add(nameof(Exception.Message), $"{e?.Message}");
                data.Add(nameof(Exception.Source), $"{e?.Source}");
                data.Add(nameof(Exception.StackTrace), $"{e?.StackTrace}");

                LogEvent(nameof(Exception.Message), data);
            }
            catch (Exception)
            {
                //cant handle this exception
            }

        }
        public abstract void LogEvent(string eventName);
        public abstract void LogEvent(string eventName, IDictionary<string, string> data);
        public void LogEvent(string eventName, string value)
        {
            LogEvent(eventName, new Dictionary<string, string>() { { eventName, value } });
        }
    }
    public static class LoggeHelper
    {
        public static int RoundTo(this int number, int step = 5)
        {
            if(number<=step)
            {
                return number;
            }
            return (number / step) * step;
        }
    }

}
