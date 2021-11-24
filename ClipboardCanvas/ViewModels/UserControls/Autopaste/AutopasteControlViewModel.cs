﻿using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;

using ClipboardCanvas.DataModels;
using ClipboardCanvas.DataModels.Navigation;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Models.Autopaste;
using ClipboardCanvas.Services;
using ClipboardCanvas.ViewModels.UserControls.Autopaste.Rules;
using ClipboardCanvas.ViewModels.UserControls.Collections;

namespace ClipboardCanvas.ViewModels.UserControls.Autopaste
{
    public class AutopasteControlViewModel : ObservableObject, IRuleActions
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly List<DataPackageView> _autopasteDataQueue = new List<DataPackageView>();

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

        #region Commands

        public ICommand ChangeTargetCommand { get; private set; }

        public ICommand AddFileSizeRuleCommand { get; private set; }

        #endregion

        public AutopasteControlViewModel()
        {
            ChangeTargetCommand = new RelayCommand(ChangeTarget);
            AddFileSizeRuleCommand = new RelayCommand(AddFileSizeRule);

            AutopasteSettingsService.SavedRules ??= new List<BaseAutopasteRuleViewModel>();

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
            AddRuleToRuleset(new FileSizeRuleViewModel(this));
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
                AutopasteRules.AddRange(AutopasteSettingsService.SavedRules);
                foreach (var item in AutopasteRules)
                {
                    item.RuleActions = this; // Initialize Rule Actions when deserializing
                }
            }

            return Task.CompletedTask;
        }

        private void AddRuleToRuleset(BaseAutopasteRuleViewModel ruleViewModel)
        {
            AutopasteRules.Add(ruleViewModel);
            AutopasteSettingsService.GuaranteeAddToList(AutopasteSettingsService.SavedRules, ruleViewModel, nameof(AutopasteSettingsService.SavedRules));
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

        public void SerializeRules()
        {
            AutopasteSettingsService.SavedRules = AutopasteRules.ToList();
            AutopasteSettingsService.FlushSettings();
        }

        public void RemoveRule(BaseAutopasteRuleViewModel ruleViewModel)
        {
            AutopasteRules.Remove(ruleViewModel);
            AutopasteSettingsService.GuaranteeRemoveFromList(AutopasteSettingsService.SavedRules, ruleViewModel, nameof(AutopasteSettingsService.SavedRules));
        }

        #endregion
    }
}
