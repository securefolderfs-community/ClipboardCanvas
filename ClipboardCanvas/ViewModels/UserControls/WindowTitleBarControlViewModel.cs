using Microsoft.Toolkit.Mvvm.ComponentModel;
using ClipboardCanvas.Models;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;
using System;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class WindowTitleBarControlViewModel : ObservableObject, IWindowTitleBarControlModel
    {
        #region Public Properties

        private bool _DefaultTitleBarTextLoad;
        public bool DefaultTitleBarTextLoad
        {
            get => _DefaultTitleBarTextLoad;
            private set => SetProperty(ref _DefaultTitleBarTextLoad, value);
        }

        private bool _CollectionsTitleBarTextLoad;
        public bool CollectionsTitleBarTextLoad
        {
            get => _CollectionsTitleBarTextLoad;
            set => SetProperty(ref _CollectionsTitleBarTextLoad, value);
        }

        private bool _CanvasTitleBarTextLoad;
        public bool CanvasTitleBarTextLoad
        {
            get => _CanvasTitleBarTextLoad;
            private set => SetProperty(ref _CanvasTitleBarTextLoad, value);
        }

        private string _CurrentCollectionText;
        public string CurrentCollectionText
        {
            get => _CurrentCollectionText;
            private set => SetProperty(ref _CurrentCollectionText, value);
        }

        #endregion

        #region Events

        public event EventHandler OnSwitchApplicationViewRequestedEvent;

        #endregion

        #region Commands

        public ICommand SwitchApplicationViewCommand { get; private set; }

        #endregion

        #region Constructor

        public WindowTitleBarControlViewModel()
        {
            // Create commands
            SwitchApplicationViewCommand = new RelayCommand(SwitchApplicationView);
        }

        #endregion

        #region Command Implementation

        private void SwitchApplicationView()
        {
            OnSwitchApplicationViewRequestedEvent?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region IWindowTitleBarControlModel

        public void SetTitleBarForDefaultView()
        {
            DefaultTitleBarTextLoad = true;
            CollectionsTitleBarTextLoad = false;
            CanvasTitleBarTextLoad = false;
        }

        public void SetTitleBarForCollectionsView()
        {
            DefaultTitleBarTextLoad = false;
            CollectionsTitleBarTextLoad = true;
            CanvasTitleBarTextLoad = false;
        }

        public void SetTitleBarForCanvasView(string collectionName)
        {
            DefaultTitleBarTextLoad = false;
            CollectionsTitleBarTextLoad = false;
            CanvasTitleBarTextLoad = true;
            CurrentCollectionText = collectionName;
        }

        #endregion
    }
}
