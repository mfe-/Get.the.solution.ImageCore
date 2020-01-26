using Get.the.solution.Image.Contract;
using Get.the.solution.Image.Manipulation.ServiceBase;
using System;
using System.Collections.Generic;
using Windows.Storage;

namespace Get.the.solution.Image.Manipulation.Service.UWP
{
    public class LocalSettingsService : LocalSettingsBaseService
    {

        public ApplicationDataContainer LocalSettings { get; }

        public override IDictionary<string, object> Values => LocalSettings.Values;

        public LocalSettingsService(ILoggerService loggerService) : base(loggerService)
        {
            LocalSettings = ApplicationData.Current.LocalSettings;
            EnabledImageViewer = Values?[nameof(EnabledImageViewer)] == null ? false : bool.Parse(Values[nameof(EnabledImageViewer)].ToString());
            EnabledOpenSingleFileAfterResize = Values?[nameof(EnabledOpenSingleFileAfterResize)] == null ? false : Boolean.Parse(Values[nameof(EnabledOpenSingleFileAfterResize)].ToString());
            ShowSuccessMessage = Values?[nameof(ShowSuccessMessage)] == null ? false : bool.Parse(Values[nameof(ShowSuccessMessage)].ToString());
            ImageQuality = Values?[nameof(ImageQuality)] == null ? 75 : int.Parse(Values[nameof(ImageQuality)].ToString());
            ClearImageListAfterSuccess = Values?[nameof(ClearImageListAfterSuccess)] == null ? true : bool.Parse(Values[nameof(ClearImageListAfterSuccess)].ToString());
        }
    }
}
