using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Views.Overlays;
using ClipboardCanvas.Shared.ComponentModel;
using ClipboardCanvas.UI.ServiceImplementation;
using ClipboardCanvas.UI.Utils;
using ClipboardCanvas.WinUI.Dialogs;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Foundation.Metadata;

namespace ClipboardCanvas.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IOverlayService"/>
    internal sealed class DialogService : BaseOverlayService
    {
        /// <inheritdoc/>
        protected override IOverlayControl GetOverlay(IViewable viewable)
        {
            IOverlayControl overlay = viewable switch
            {
                SettingsOverlayViewModel => new SettingsDialog(),
                SaveConfirmationOverlayViewModel => new SaveConfirmationDialog(),
                // TODO: Implement location choosing save dialog

                _ => throw new ArgumentException("Unknown viewable type.", nameof(viewable))
            };

#if WINDOWS
            if (overlay is ContentDialog contentDialog && ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
                contentDialog.XamlRoot = MainWindow.Instance?.Content.XamlRoot;
#endif

            return overlay;
        }
    }
}
