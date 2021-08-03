using ClipboardCanvas.ModelViews;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class InteractableCanvasControlViewModel : ObservableObject
    {
        #region Private Members

        private IInteractableCanvasControlView _view;

        #endregion

        #region Public Properties

        public ObservableCollection<InteractableCanvasControlItemViewModel> Items { get; private set; }

        #endregion

        #region Constructor

        public InteractableCanvasControlViewModel(IInteractableCanvasControlView view)
        {
            this._view = view;

            Items = new ObservableCollection<InteractableCanvasControlItemViewModel>()
            {
                new InteractableCanvasControlItemViewModel(_view)
                {
                    TestText = "Hi 1"
                },
                new InteractableCanvasControlItemViewModel(_view)
                {
                    TestText = "Hi 2"
                }
            };
        }

        #endregion

        #region Public Helpers

        public void NotifyCanvasLoaded()
        {
            foreach (var item in Items)
            {
                item.NotifyCanvasLoaded();
            }
        }

        #endregion
    }
}
