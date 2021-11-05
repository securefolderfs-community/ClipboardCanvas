using ClipboardCanvas.DataModels.Navigation;
using ClipboardCanvas.Helpers;
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClipboardCanvas.ViewModels.UserControls.Autopaste
{
    public class AutopasteControlViewModel : ObservableObject
    {
        private INavigationService NavigationService { get; } = Ioc.Default.GetService<INavigationService>();

        public IAutopasteTarget AutopasteTarget { get; set; }

        private bool _EnableAutopaste;
        public bool EnableAutopaste
        {
            get => _EnableAutopaste;
            set => SetProperty(ref _EnableAutopaste, value);
        }

        public ICommand ChangeTargetCommand { get; private set; }

        public ICommand AddAllowedTypeCommand { get; private set; }

        public AutopasteControlViewModel()
        {
            ChangeTargetCommand = new RelayCommand(ChangeTarget);
            AddAllowedTypeCommand = new RelayCommand(AddAllowedType);
        }

        private void ChangeTarget()
        {
            NavigationService.OpenHomepage(new FromAutopasteHomepageNavigationParameterModel(
                CanvasHelpers.GetDefaultCanvasType()));
        }

        private void AddAllowedType()
        {

        }
    }
}
