using ClipboardCanvas.DataModels;
using ClipboardCanvas.DataModels.Navigation;
using ClipboardCanvas.Enums;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Models;
using ClipboardCanvas.Models.Autopaste;
using ClipboardCanvas.Services;
using ClipboardCanvas.ViewModels.UserControls.Autopaste.Rules;
using ClipboardCanvas.ViewModels.UserControls.Collections;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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

        private bool _autopasteRoutineStarted = false;

        private List<DataPackageView> _autopasteDataQueue = new List<DataPackageView>();

        private INavigationService NavigationService { get; } = Ioc.Default.GetService<INavigationService>();

        private IAutopasteSettingsService AutopasteSettingsService { get; } = Ioc.Default.GetService<IAutopasteSettingsService>();

        private ITimelineService TimelineService { get; } = Ioc.Default.GetService<ITimelineService>();

        private IUserSettingsService UserSettingsService { get; } = Ioc.Default.GetService<IUserSettingsService>();

        public ObservableCollection<BaseAutopasteRuleViewModel> AutopasteRules { get; } = new ObservableCollection<BaseAutopasteRuleViewModel>();

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

        #region Commands

        public ICommand ChangeTargetCommand { get; private set; }

        public ICommand AddFileSizeRuleCommand { get; private set; }

        #endregion

        public AutopasteControlViewModel()
        {
            ChangeTargetCommand = new RelayCommand(ChangeTarget);
            AddFileSizeRuleCommand = new RelayCommand(AddFileSizeRule);

            Clipboard.ContentChanged -= Clipboard_ContentChanged;
            Clipboard.ContentChanged += Clipboard_ContentChanged;
        }

        #region Command Implementation

        private void ChangeTarget()
        {
            NavigationService.OpenHomepage(new FromAutopasteHomepageNavigationParameterModel(
                CanvasHelpers.GetDefaultCanvasType()));
        }

        private void AddFileSizeRule()
        {
            AutopasteRules.Add(new FileSizeRuleViewModel());
        }

        #endregion

        public Task Initialize()
        {
            if (UserSettingsService.IsAutopasteEnabled && !string.IsNullOrEmpty(AutopasteSettingsService.AutopastePath))
            {
                IAutopasteTarget autopasteTarget = CollectionsWidgetViewModel.FindCollection(AutopasteSettingsService.AutopastePath);
                UpdateTarget(autopasteTarget);
            }

            return Task.CompletedTask;
        }

        private async void Clipboard_ContentChanged(object sender, object e)
        {
            if (AutopasteTarget != null)
            {
                SafeWrapper<DataPackageView> clipboardData = ClipboardHelpers.GetClipboardData();
                if (clipboardData)
                {
                    SafeWrapper<CanvasItem> pasteResult = (null, SafeWrapperResult.UNKNOWN_FAIL);

                    try
                    {
                        if (_autopasteRoutineStarted && _autopasteDataQueue.Any((item) => item.Properties.SequenceEqual(clipboardData.Result.Properties)))
                        {
                            return; // Avoid duplicates where the event is called twice
                        }

                        // Start and add the operation to queue
                        _autopasteRoutineStarted = true;
                        _autopasteDataQueue.Add(clipboardData);

                        // Check filters
                        foreach (var item in AutopasteRules)
                        {
                            if (!await item.PassesRule(clipboardData))
                            {
                                return;
                            }
                        }

                        pasteResult = await AutopasteTarget.PasteData(clipboardData, _cancellationTokenSource.Token);
                    }
                    catch { }
                    finally
                    {
                        // Remove the completed operation from queue
                        _autopasteDataQueue.Remove(clipboardData);
                        _autopasteRoutineStarted = _autopasteDataQueue.IsEmpty();
                    }

                    if (pasteResult)
                    {
                        var todaySection = await TimelineService.GetOrCreateTodaySection();
                        var item = await TimelineService.AddItemForSection(todaySection, AutopasteTarget.CollectionModel, pasteResult);
                    }
                    else
                    {
                        // Show notification if failed
                    }
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
