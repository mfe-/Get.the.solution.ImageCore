using Get.the.solution.Image.Contract;
using Prism.Mvvm;
using System;

namespace Get.the.solution.Image.Manipulation.ViewModel
{
    public class HelpPageViewModel : BindableBase
    {
        protected readonly ILoggerService _LoggerService;
        public HelpPageViewModel(ILoggerService loggerService)
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
