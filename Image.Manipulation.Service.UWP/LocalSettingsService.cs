using Get.the.solution.Image.Contract;
using Get.the.solution.Image.Manipulation.Contract;
using Get.the.solution.Image.Manipulation.ServiceBase;
using System;
using System.Collections.Generic;
using Windows.Storage;

namespace Get.the.solution.Image.Manipulation.Service.UWP
{
    public class LocalSettingsService : LocalSettingsBaseService
    {

        public ApplicationDataContainer LocalSettings { get; }
        public LocalSettingsService(ILoggerService loggerService) : base(loggerService)
        {
            LocalSettings = ApplicationData.Current.LocalSettings;
            EnabledImageViewer = Values?[nameof(EnabledImageViewer)] == null ? false : Boolean.Parse(Values[nameof(EnabledImageViewer)].ToString());
            EnabledOpenSingleFileAfterResize = Values?[nameof(EnabledOpenSingleFileAfterResize)] == null ? false : Boolean.Parse(Values[nameof(EnabledOpenSingleFileAfterResize)].ToString());
            ShowSuccessMessage = Values?[nameof(ShowSuccessMessage)] == null ? false : Boolean.Parse(Values[nameof(ShowSuccessMessage)].ToString());
        }
        public override IDictionary<string, object> Values => LocalSettings.Values;
    }
}
