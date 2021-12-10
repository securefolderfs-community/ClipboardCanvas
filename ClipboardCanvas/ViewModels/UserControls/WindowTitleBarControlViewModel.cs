using System;
using CommunityToolkit.Mvvm.ComponentModel;
using ClipboardCanvas.GlobalizationExtensions;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

using ClipboardCanvas.Models;
using Vanara.PInvoke;
using ClipboardCanvas.Services;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class WindowTitleBarControlViewModel : ObservableObject, IWindowTitleBarControlModel
    {
        #region Public Properties

        private bool _StandardTitleBarLoad;
        public bool StandardTitleBarLoad
        {
            get => _StandardTitleBarLoad;
            set => SetProperty(ref _StandardTitleBarLoad, value);
        }

        private string _StandardTitleBarText;
        public string StandardTitleBarText
        {
            get => _StandardTitleBarText;
            set => SetProperty(ref _StandardTitleBarText, value);
        }

        private bool _TwoSideTitleBarLoad;
        public bool TwoSideTitleBarLoad
        {
            get => _TwoSideTitleBarLoad;
            private set => SetProperty(ref _TwoSideTitleBarLoad, value);
        }

        private string _TitleBarFirstSideText;
        public string TitleBarFirstSideText
        {
            get => _TitleBarFirstSideText;
            private set => SetProperty(ref _TitleBarFirstSideText, value);
        }

        private string _TitleBarSecondSideText;
        public string TitleBarSecondSideText
        {
            get => _TitleBarSecondSideText;
            set => SetProperty(ref _TitleBarSecondSideText, value);
        }

        private bool _IsInRestrictedAccess;
        public bool IsInRestrictedAccess
        {
            get => _IsInRestrictedAccess;
            set => SetProperty(ref _IsInRestrictedAccess, value);
        }

        private bool _ShowTitleUnderline;
        public bool ShowTitleUnderline
        {
            get => _ShowTitleUnderline;
            set => SetProperty(ref _ShowTitleUnderline, value);
        }

        private IApplicationService ApplicationService { get; } = Ioc.Default.GetService<IApplicationService>();

        #endregion

        #region Events

        public event EventHandler OnSwitchApplicationViewRequestedEvent;

        #endregion

        #region Commands

        public ICommand SwitchApplicationViewCommand { get; private set; }

        public ICommand MinimizeCommand { get; private set; }

        public ICommand RestoreOrMaximizeCommand { get; private set; }

        public ICommand CloseCommand { get; private set; }

        #endregion

        #region Constructor

        public WindowTitleBarControlViewModel()
        {
            // Create commands
            SwitchApplicationViewCommand = new RelayCommand(SwitchApplicationView);
            MinimizeCommand = new RelayCommand(Minimize);
            RestoreOrMaximizeCommand = new RelayCommand(RestoreOrMaximize);
            CloseCommand = new RelayCommand(Close);
        }

        #endregion

        #region Command Implementation

        private void SwitchApplicationView()
        {
            OnSwitchApplicationViewRequestedEvent?.Invoke(this, EventArgs.Empty);
        }

        private void Minimize()
        {
            IntPtr hwnd = ApplicationService.GetHwnd(MainWindow.Instance);
            User32.ShowWindow(hwnd, ShowWindowCommand.SW_MINIMIZE);
        }

        private void RestoreOrMaximize()
        {
            IntPtr hwnd = ApplicationService.GetHwnd(MainWindow.Instance);

            User32.WindowStyles style = (User32.WindowStyles)User32.GetWindowLong(hwnd, User32.WindowLongFlags.GWL_STYLE);
            if (style.HasFlag(User32.WindowStyles.WS_MAXIMIZE))
            {
                User32.ShowWindow(hwnd, ShowWindowCommand.SW_RESTORE);
            }
            else
            {
                User32.ShowWindow(hwnd, ShowWindowCommand.SW_MAXIMIZE);
            }

        }

        private void Close()
        {
            Application.Current.Exit();
        }

        #endregion

        #region IWindowTitleBarControlModel

        public void SetTitleBarForDefaultView()
        {
            StandardTitleBarText = "Clipboard Canvas";
            StandardTitleBarLoad = true;
            TwoSideTitleBarLoad = false;
        }

        public void SetTitleBarForCollectionsView()
        {
            StandardTitleBarText = "Clipboard Canvas";
            StandardTitleBarLoad = true;
            TwoSideTitleBarLoad = false;
        }

        public void SetTitleBarForCanvasView(string collectionName)
        {
            TwoSideTitleBarLoad = true;
            TitleBarFirstSideText = "CurrentCollection".GetLocalized2();
            TitleBarSecondSideText = collectionName;

            StandardTitleBarLoad = false;
        }

        public void SetTitleBarForCollectionPreview(string collectionName)
        {
            TwoSideTitleBarLoad = true;
            TitleBarFirstSideText = "CollectionPreview".GetLocalized2();
            TitleBarSecondSideText = collectionName;

            StandardTitleBarLoad = false;
        }

        #endregion
    }
}
