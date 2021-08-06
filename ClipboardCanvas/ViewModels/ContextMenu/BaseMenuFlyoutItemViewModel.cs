using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace ClipboardCanvas.ViewModels.ContextMenu
{
    public abstract class BaseMenuFlyoutItemViewModel : ObservableObject
    {
        public object Tag { get; set; }

        public bool IsShown { get; set; } = true;

        public bool IsEnabled { get; set; } = true;
    }
}
