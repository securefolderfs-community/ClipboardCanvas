using ClipboardCanvas.Sdk.Enums;
using ClipboardCanvas.Sdk.ViewModels.Views.Overlays;
using ClipboardCanvas.Shared.ComponentModel;
using ClipboardCanvas.UI.Utils;
using Microsoft.UI.Xaml.Controls;
using ClipboardCanvas.Sdk.Extensions;
using System;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClipboardCanvas.WinUI.Dialogs
{
    public sealed partial class SaveConfirmationDialog : ContentDialog, IOverlayControl
    {
        public SaveConfirmationOverlayViewModel ViewModel
        {
            get => (SaveConfirmationOverlayViewModel)DataContext;
            set => DataContext = value;
        }

        public SaveConfirmationDialog()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public new async Task<IResult> ShowAsync() => ((DialogOption)await base.ShowAsync()).ParseOverlayOption();

        /// <inheritdoc/>
        public void SetView(IViewable viewable) => ViewModel = (SaveConfirmationOverlayViewModel)viewable;

        /// <inheritdoc/>
        public Task HideAsync()
        {
            Hide();
            return Task.CompletedTask;
        }
    }
}
