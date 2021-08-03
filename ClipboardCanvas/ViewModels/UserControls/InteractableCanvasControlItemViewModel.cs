using ClipboardCanvas.ModelViews;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class InteractableCanvasControlItemViewModel : ObservableObject, IDisposable
    {
        #region Private Members

        private IInteractableCanvasControlView _view;

        #endregion

        #region Properties

        private Vector2 ItemPosition
        {
            get => _view.GetItemPosition(this);
            set => _view.SetItemPosition(this, value);
        }

        public string TestText { get; set; }

        public ICommand UpdatePosCommand { get; set; }

        /// <summary>
        /// The horizontal position
        /// </summary>
        public float XPos
        {
            get => ItemPosition.X;
            set => ItemPosition = new Vector2(value, ItemPosition.Y);
        }

        /// <summary>
        /// The vertical position
        /// </summary>
        public float YPos
        {
            get => ItemPosition.Y;
            set => ItemPosition = new Vector2(ItemPosition.X, value);
        }

        #endregion

        #region Constructor

        public InteractableCanvasControlItemViewModel(IInteractableCanvasControlView view)
        {
            this._view = view;

            this.UpdatePosCommand = new RelayCommand(() =>
            {
                Debug.WriteLine($"X: {XPos} | Y: {YPos}");
            });
        }

        #endregion

        #region Public Helpers

        public void NotifyCanvasLoaded()
        {
            this.XPos = 100;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _view = null;
        }

        #endregion
    }
}
