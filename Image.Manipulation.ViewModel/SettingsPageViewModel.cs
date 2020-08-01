using Get.the.solution.Image.Manipulation.Contract;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows.Input;

namespace Get.the.solution.Image.Manipulation.ViewModel
{
    public class SettingsPageViewModel : BindableBase
    {
        private readonly ILoggerService _LoggerService;
        private readonly ILocalSettings<ResizeSettings> _localSettings;
        private readonly IFileService _fileService;

        public SettingsPageViewModel(ILoggerService loggerService, ILocalSettings<ResizeSettings> localSettings, IFileService fileService)
        {
            try
            {
                _LoggerService = loggerService;
                _localSettings = localSettings;
                _fileService = fileService;
                _LoggerService?.LogEvent(nameof(SettingsPageViewModel));
            }
            catch (Exception e)
            {
                _LoggerService?.LogException(nameof(AboutPageViewModel), e);
            }
        }
        public ILocalSettings<ResizeSettings> LocalSettings => _localSettings;

        private ICommand _ResetDefaultSaveAsTargetFolderCommand;
        public ICommand ResetDefaultSaveAsTargetFolderCommand => _ResetDefaultSaveAsTargetFolderCommand ?? (_ResetDefaultSaveAsTargetFolderCommand = new DelegateCommand(OnResetDefaultSaveAsTargetFolderCommand));

        protected void OnResetDefaultSaveAsTargetFolderCommand()
        {
            LocalSettings.Settings.DefaultSaveAsTargetFolder = String.Empty;
        }


        private ICommand _SetDefaultSaveAsTargetFolderCommand;
        public ICommand SetDefaultSaveAsTargetFolderCommand => _SetDefaultSaveAsTargetFolderCommand ?? (_SetDefaultSaveAsTargetFolderCommand = new DelegateCommand(OnSetDefaultSaveAsTargetFolderCommandAsync));

        protected async void OnSetDefaultSaveAsTargetFolderCommandAsync()
        {
            try
            {
                var dir = await _fileService.PickDirectoryAsync();
                if (dir != null)
                {
                    LocalSettings.Settings.DefaultSaveAsTargetFolder = dir.FullName;
                }
            }
            catch (Exception e)
            {
                _LoggerService?.LogException(e);
            }

        }
    }
}
