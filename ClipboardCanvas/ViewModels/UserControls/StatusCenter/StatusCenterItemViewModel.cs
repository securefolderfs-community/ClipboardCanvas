using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading;
using System.Windows.Input;
using ClipboardCanvas.GlobalizationExtensions;

using ClipboardCanvas.Contexts.Operations;
using ClipboardCanvas.Enums;
using ClipboardCanvas.EventArguments;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Services;

namespace ClipboardCanvas.ViewModels.UserControls.StatusCenter
{
    public class StatusCenterItemViewModel : ObservableObject, IOperationContext
    {
        private Progress<double> _operationProgress;

        private CancellationTokenSource _cancellationTokenSource;

        private StatusCenterOperationType _operationType;

        private SafeWrapperResult _result;

        #region Properties

        private IStatusCenterService StatusCenterService { get; } = Ioc.Default.GetService<IStatusCenterService>();

        public IProgress<double> OperationProgress
        {
            get => _operationProgress;
        }

        private bool _IsProgressBarVisible;
        public bool IsProgressBarVisible
        {
            get => _IsProgressBarVisible;
            private set => SetProperty(ref _IsProgressBarVisible, value);
        }

        private string _OperationTitle;
        public string OperationTitle
        {
            get => _OperationTitle;
            private set => SetProperty(ref _OperationTitle, value);
        }

        private string _OperationDescription;
        public string OperationDescription
        {
            get => _OperationDescription;
            set => SetProperty(ref _OperationDescription, value);
        }

        private string _IconGlyph;
        public string IconGlyph
        {
            get => _IconGlyph;
            private set => SetProperty(ref _IconGlyph, value);
        }

        private double _ProgressBarValue;
        public double ProgressBarValue
        {
            get => _ProgressBarValue;
            set => SetProperty(ref _ProgressBarValue, value);
        }

        private bool _IsIndeterminate = true;
        public bool IsIndeterminate
        {
            get => _IsIndeterminate;
            set => SetProperty(ref _IsIndeterminate, value);
        }

        private bool _ProgressPaused;
        public bool ProgressPaused
        {
            get => _ProgressPaused;
            set
            {   
                if (SetProperty(ref _ProgressPaused, value))
                {
                    IsIndeterminate = true;
                    ProgressError = false;
                }
            }
        }

        private bool _ProgressError;
        public bool ProgressError
        {
            get => _ProgressError;
            set
            {
                if (SetProperty(ref _ProgressError, value))
                {
                    IsIndeterminate = true;
                    ProgressPaused = false;
                }
            }
        }

        public bool IsOperationStarted { get; private set; }

        public bool IsOperationFinished { get; private set; }

        public CancellationToken CancellationToken
        {
            get => _cancellationTokenSource.Token;
        }

        #endregion

        public event EventHandler<OperationFinishedEventArgs> OnOperationFinishedEvent;

        public ICommand CancelOrDismissCommand { get; private set; }

        #region Constructor

        private StatusCenterItemViewModel(string operationName, StatusCenterOperationType operationType, CancellationTokenSource cancellationTokenSource)
        {
            this.OperationTitle = operationName;
            this._operationType = operationType;
            this.IsProgressBarVisible = operationType != StatusCenterOperationType.Info;
            this._cancellationTokenSource = cancellationTokenSource;

            this.IconGlyph = GetIconGlyphFromOperationType(operationType);
            this._operationProgress = new Progress<double>(UpdateProgress);

            // Create commands
            CancelOrDismissCommand = new RelayCommand(CancelOrDismiss);
        }

        #endregion

        private void CancelOrDismiss()
        {
            if (_operationType != StatusCenterOperationType.Info)
            {
                _result = SafeWrapperResult.CANCEL;
                PostCancelBanner();
            }
            else
            {
                RemoveBanner();
            }
        }

        private void UpdateProgress(double value)
        {
            this.IsProgressBarVisible = true;

            if (value > 0.0d)
            {
                OperationDescription = string.Format("StatusCenterPercentCompleted".GetLocalized(), value.ToString("0.00"));
                IsIndeterminate = false;
                ProgressBarValue = value;
            }
            else
            {
                OperationDescription = "StatusCenterProcessing".GetLocalized();
                IsIndeterminate = true;
            }
        }

        public static StatusCenterItemViewModel ConstructOperationBanner(string operationName, StatusCenterOperationType operationType, CancellationTokenSource cancellationTokenSource)
        {
            StatusCenterItemViewModel item = new StatusCenterItemViewModel(operationName, operationType, cancellationTokenSource)
            {
                OperationDescription = "StatusCenterStarting".GetLocalized()
            };

            return item;
        }

        public static StatusCenterItemViewModel ConstructInfoBanner(string infoTitle, string infoDescription, SafeWrapperResult result)
        {
            StatusCenterItemViewModel item = new StatusCenterItemViewModel(infoTitle, StatusCenterOperationType.Info, null)
            {
                _result = result,
                OperationDescription = infoDescription
            };

            return item;
        }

        private void PostCancelBanner()
        {
            _cancellationTokenSource?.Cancel();
            RemoveBanner();

            string operationDescription;
            switch (_operationType)
            {
                case StatusCenterOperationType.Paste:
                    {
                        operationDescription = "StatusCenterPastingCanceled".GetLocalized();
                        break;
                    }

                case StatusCenterOperationType.OverrideReference:
                    {
                        operationDescription = "StatusCenterOverwritingReferenceCanceled".GetLocalized();
                        break;
                    }

                default:
                    {
                        operationDescription = "StatusCenterOperationCanceled".GetLocalized();
                        break;
                    }
            }

            var item = StatusCenterService.AppendInfoBanner("StatusCenterOperationCanceledTitle".GetLocalized(), operationDescription, _result);
            item.ProgressPaused = true;
            item.IsProgressBarVisible = true;
        }

        private void PostSuccessBanner()
        {
            RemoveBanner();

            string operationDescription;
            switch (_operationType)
            {
                case StatusCenterOperationType.Paste:
                    {
                        operationDescription = "StatusCenterPastingComplete".GetLocalized();
                        break;
                    }

                case StatusCenterOperationType.OverrideReference:
                    {
                        operationDescription = "StatusCenterOverwritingReferenceComplete".GetLocalized();
                        break;
                    }

                default:
                    {
                        operationDescription = "StatusCenterOperationComplete".GetLocalized();
                        break;
                    }
            }

            var item = StatusCenterService.AppendInfoBanner("StatusCenterOperationCompleteTitle".GetLocalized(), operationDescription, _result);
            item.IconGlyph = "\uE73E";
        }

        private string GetIconGlyphFromOperationType(StatusCenterOperationType operationType)
        {
            switch (operationType)
            {
                case StatusCenterOperationType.Info:
                    return "\uE946";

                case StatusCenterOperationType.Paste:
                    return "\uE77F";

                case StatusCenterOperationType.OverrideReference:
                    return "\uE71B";

                default:
                    return "\uE9CE"; // Unknown
            }
        }

        public void StartOperation()
        {
            IsIndeterminate = false;
            OperationDescription = "StatusCenterCompleted".GetLocalized();

            if (IsOperationFinished)
            {
                IsOperationStarted = false;
                return;
            }

            IsOperationStarted = true;
        }

        public void FinishOperation(SafeWrapperResult result)
        {
            IsOperationFinished = true;
            IsOperationStarted = false;
            _result = result;

            if (_result)
            {
                PostSuccessBanner();
            }

            OnOperationFinishedEvent?.Invoke(this, new OperationFinishedEventArgs(_result));
        }

        public void RemoveBanner()
        {
            StatusCenterService.RemoveBanner(this);
        }
    }
}
