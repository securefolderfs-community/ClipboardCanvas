using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using System.Collections.Specialized;

using ClipboardCanvas.DataModels;
using ClipboardCanvas.DataModels.Navigation;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Models.Autopaste;
using ClipboardCanvas.Services;
using ClipboardCanvas.ViewModels.UserControls.Autopaste.Rules;
using ClipboardCanvas.ViewModels.UserControls.Collections;
using System.Diagnostics;

namespace ClipboardCanvas.ViewModels.UserControls.Autopaste
{
    public class AutopasteControlViewModel : ObservableObject, IRuleActions, IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly List<DataPackageView> _autopasteDataQueue = new List<DataPackageView>();

        private int _internalCollectionCount;

        private bool _itemAddedInternally;

        private bool _autopasteRoutineStarted = false;

        private INavigationService NavigationService { get; } = Ioc.Default.GetService<INavigationService>();

        private IAutopasteSettingsService AutopasteSettingsService { get; } = Ioc.Default.GetService<IAutopasteSettingsService>();

        private ITimelineService TimelineService { get; } = Ioc.Default.GetService<ITimelineService>();

        private IUserSettingsService UserSettingsService { get; } = Ioc.Default.GetService<IUserSettingsService>();

        private IApplicationService ApplicationService { get; } = Ioc.Default.GetService<IApplicationService>();

        public RangeObservableCollection<BaseAutopasteRuleViewModel> AutopasteRules { get; } = new RangeObservableCollection<BaseAutopasteRuleViewModel>();

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

        private bool _IsInfoBarOpen;
        public bool IsInfoBarOpen
        {
            get => _IsInfoBarOpen;
            set => SetProperty(ref _IsInfoBarOpen, value);
        }

        #region Commands

        public ICommand ChangeTargetCommand { get; private set; }

        public ICommand AddFileSizeRuleCommand { get; private set; }

        public ICommand AddTypeFilterCommand { get; private set; }

        public ICommand CloseInfoBarCommand { get; private set; }

        #endregion

        public AutopasteControlViewModel()
        {
            ChangeTargetCommand = new RelayCommand(ChangeTarget);
            AddFileSizeRuleCommand = new RelayCommand(AddFileSizeRule);
            AddTypeFilterCommand = new RelayCommand(AddTypeFilter);
            CloseInfoBarCommand = new RelayCommand(CloseInfoBar);

            AutopasteSettingsService.SavedRules ??= new List<BaseAutopasteRuleViewModel>();

            AutopasteRules.CollectionChanged += AutopasteRules_CollectionChanged;
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
            AddRuleToRuleset(new FileSizeRuleViewModel(this));

            if (!AutopasteSettingsService.FileSizeRuleDoesNotApplyToFoldersWarningDismissed)
            {
                IsInfoBarOpen = true;
            }
        }

        private void AddTypeFilter()
        {
            AddRuleToRuleset(new TypeFilterRuleViewModel(this));
        }

        private void CloseInfoBar()
        {
            AutopasteSettingsService.FileSizeRuleDoesNotApplyToFoldersWarningDismissed = true;
        }

        #endregion

        #region Helpers

        public Task Initialize()
        {
            if (!string.IsNullOrEmpty(AutopasteSettingsService.AutopastePath))
            {
                IAutopasteTarget autopasteTarget = CollectionsWidgetViewModel.FindCollection(AutopasteSettingsService.AutopastePath);
                UpdateTarget(autopasteTarget);
            }
            
            // Get rules
            if (!AutopasteSettingsService.SavedRules.IsEmpty())
            {
                _itemAddedInternally = true;
                AutopasteRules.AddRange(AutopasteSettingsService.SavedRules);
                foreach (var item in AutopasteRules)
                {
                    item.RuleActions = this; // Initialize Rule Actions when deserializing
                    if (!IsInfoBarOpen && item is FileSizeRuleViewModel && !AutopasteSettingsService.FileSizeRuleDoesNotApplyToFoldersWarningDismissed)
                    {
                        IsInfoBarOpen = true;
                    }
                }
                _itemAddedInternally = false;
            }

            return Task.CompletedTask;
        }

        private void AddRuleToRuleset(BaseAutopasteRuleViewModel ruleViewModel)
        {
            _itemAddedInternally = true;
            AutopasteRules.Add(ruleViewModel);
            AutopasteSettingsService.GuaranteeAddToList(AutopasteSettingsService.SavedRules, ruleViewModel, nameof(AutopasteSettingsService.SavedRules));
            _itemAddedInternally = false;
        }

        public void UpdateTarget(IAutopasteTarget autopasteTarget)
        {
            this.AutopasteTarget = autopasteTarget;
            OnPropertyChanged(nameof(AutopasteTargetName));

            AutopasteSettingsService.AutopastePath = autopasteTarget?.TargetPath;
        }

        public void SerializeRules()
        {
            AutopasteSettingsService.SavedRules = AutopasteRules.ToList();
            AutopasteSettingsService.FlushSettings();
        }

        public void RemoveRule(BaseAutopasteRuleViewModel ruleViewModel)
        {
            AutopasteRules.Remove(ruleViewModel);
            AutopasteSettingsService.GuaranteeRemoveFromList(AutopasteSettingsService.SavedRules, ruleViewModel, nameof(AutopasteSettingsService.SavedRules));

            if (IsInfoBarOpen && !AutopasteRules.Any((item) => item is FileSizeRuleViewModel))
            {
                IsInfoBarOpen = false;
            }
        }

        #endregion

        #region Event Handlers

        private void AutopasteRules_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_internalCollectionCount < AutopasteRules.Count && !_itemAddedInternally)
            {
                // Serialize rules when collection items have been reordered
                SerializeRules();
            }

            _internalCollectionCount = AutopasteRules.Count;
        }

        private async void Clipboard_ContentChanged(object sender, object e)
        {
            if (AutopasteTarget != null && !ApplicationService.IsWindowActivated)
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
                        Debug.WriteLine(pasteResult.Message);
                        Debug.WriteLine(pasteResult.Result);
                        Debug.WriteLine(pasteResult.Exception);
                        Debug.WriteLine(pasteResult.ErrorCode);
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
                    }
                    else
                    {
                        // Show notification if failed
                    }
                }
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Clipboard.ContentChanged -= Clipboard_ContentChanged;
            if (AutopasteRules != null)
            {
                AutopasteRules.CollectionChanged -= AutopasteRules_CollectionChanged;
            }
        }

        #endregion
    }
}
