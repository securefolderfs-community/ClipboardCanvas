using System.Collections.Generic;

namespace ClipboardCanvas.ViewModels.ContextMenu
{
    public sealed class MenuFlyoutSubItemViewModel : MenuFlyoutItemViewModel
    {
        public List<BaseMenuFlyoutItemViewModel> SubItems { get; set; } = new List<BaseMenuFlyoutItemViewModel>();
    }
}
