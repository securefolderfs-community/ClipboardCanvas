using System.Windows.Input;

namespace ClipboardCanvas.ViewModels.ContextMenu
{
    public class MenuFlyoutItemViewModel : BaseMenuFlyoutItemViewModel
    {
        #region Public Properties

        public ICommand Command { get; set; }

        public object CommandParameter { get; set; }
        
        public string IconGlyph { get; set; }

        public string Text { get; set; }

        #endregion
    }
}
