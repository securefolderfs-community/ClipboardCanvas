using ClipboardCanvas.ViewModels.UserControls;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.ViewModels.Dialogs
{
    public class UpdateChangeLogDialogViewModel : ObservableObject
    {
        #region Public Properties

        public ObservableCollection<UpdateChangeLogItemViewModel> Items { get; private set; }

        public bool IsLoadingData { get; private set; }

        #endregion

        #region Constructor

        public UpdateChangeLogDialogViewModel()
        {
            Items = new ObservableCollection<UpdateChangeLogItemViewModel>();
        }

        #endregion

        #region Public Helpers

        public async Task LoadUpdateDataFromGitHub()
        {
            IsLoadingData = true;



            IsLoadingData = false;
        }

        #endregion
    }
}
