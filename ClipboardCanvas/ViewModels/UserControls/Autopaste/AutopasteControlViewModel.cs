using ClipboardCanvas.DataModels.Navigation;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Models;
using ClipboardCanvas.Models.Autopaste;
using ClipboardCanvas.Services;
using ClipboardCanvas.ViewModels.UserControls.Collections;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace ClipboardCanvas.ViewModels.UserControls.Autopaste
{
    public class AutopasteControlViewModel : ObservableObject
    {
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private INavigationService NavigationService { get; } = Ioc.Default.GetService<INavigationService>();

        private IAutopasteSettingsService AutopasteSettingsService { get; } = Ioc.Default.GetService<IAutopasteSettingsService>();

        private IUserSettingsService UserSettingsService { get; } = Ioc.Default.GetService<IUserSettingsService>();

        public IAutopasteTarget AutopasteTarget { get; private set; }

        public string AutopasteTargetName
        {
            get => AutopasteTarget?.DisplayName ?? "No target";
        }

        public bool EnableAutopaste
        {
            get => UserSettingsService.IsAutopasteEnabled;
            set
            {
                if (value != UserSettingsService.IsAutopasteEnabled)
                {
                    UserSettingsService.IsAutopasteEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand ChangeTargetCommand { get; private set; }

        public ICommand AddAllowedTypeCommand { get; private set; }

        public AutopasteControlViewModel()
        {
            ChangeTargetCommand = new RelayCommand(ChangeTarget);
            AddAllowedTypeCommand = new RelayCommand(AddAllowedType);

            Clipboard.ContentChanged += Clipboard_ContentChanged;
        }

        private void ChangeTarget()
        {
            NavigationService.OpenHomepage(new FromAutopasteHomepageNavigationParameterModel(
                CanvasHelpers.GetDefaultCanvasType()));
        }

        public Task Initialize()
        {
            if (UserSettingsService.IsAutopasteEnabled && !string.IsNullOrEmpty(AutopasteSettingsService.AutopastePath))
            {
                IAutopasteTarget autopasteTarget = CollectionsWidgetViewModel.FindCollection(AutopasteSettingsService.AutopastePath);
                UpdateTarget(autopasteTarget);
            }

            return Task.CompletedTask;
        }

        private void AddAllowedType()
        {

        }
        
        private async void Clipboard_ContentChanged(object sender, object e)
        {
            if (AutopasteTarget != null)
            {
                SafeWrapper<DataPackageView> clipboardData = ClipboardHelpers.GetClipboardData();
                if (clipboardData)
                {
                    SafeWrapperResult result = await AutopasteTarget.PasteData(clipboardData, _cancellationTokenSource.Token);

                    // Show notification depending on the result
                }
            }
        }

        public void UpdateTarget(IAutopasteTarget autopasteTarget)
        {
            this.AutopasteTarget = autopasteTarget;
            OnPropertyChanged(nameof(AutopasteTargetName));

            AutopasteSettingsService.AutopastePath = autopasteTarget?.TargetPath;
        }
    }
}
