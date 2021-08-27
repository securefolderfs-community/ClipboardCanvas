using System;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;

using ClipboardCanvas.Enums;
using ClipboardCanvas.ViewModels.Dialogs;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.ModelViews;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ClipboardCanvas.Dialogs
{
    public sealed partial class SettingsDialog : Windows.UI.Xaml.Controls.ContentDialog, IDialog<SettingsDialogViewModel>, ISettingsDialogView
    {
        public SettingsDialogViewModel ViewModel
        {
            get => (SettingsDialogViewModel)DataContext;
            set => DataContext = value;
        }

        public SettingsDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.View = this;
            }
        }

        private void SettingsNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = args.SelectedItem as NavigationViewItem;
            int tag = Convert.ToInt32(selectedItem.Tag);

            switch (tag)
            {
                default:
                case 0:
                    {
                        this.ViewModel.UpdateNavigation(SettingsPageType.General);
                        break;
                    }

                case 1:
                    {
                        this.ViewModel.UpdateNavigation(SettingsPageType.Pasting);
                        break;
                    }

                case 2:
                    {
                        this.ViewModel.UpdateNavigation(SettingsPageType.Notifications);
                        break;
                    }

                case 3:
                    {
                        this.ViewModel.UpdateNavigation(SettingsPageType.About);
                        break;
                    }
            }
        }

        public async new Task<DialogResult> ShowAsync() => (await base.ShowAsync()).ToDialogResult();

        public void CloseDialog()
        {
            this.Hide();
        }
    }
}
