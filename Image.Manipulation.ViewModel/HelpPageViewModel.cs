using Get.the.solution.Image.Manipulation.Contract;
using System;

namespace Get.the.solution.Image.Manipulation.ViewModel.ResizeImage
{
    public class HelpPageViewModel : NotifyPropertyChanged
    {
        protected readonly ILoggerService _LoggerService;
        public HelpPageViewModel(ILoggerService loggerService) : base(loggerService)
        {
            try
            {
                _LoggerService = loggerService;
                _LoggerService?.LogEvent(nameof(HelpPageViewModel));
            }
            catch (Exception e)
            {
                _LoggerService?.LogException(nameof(HelpPageViewModel), e);
            }

        }

    }
}
