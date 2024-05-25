using ClipboardCanvas.Sdk.ViewModels.Views.Browser;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ClipboardCanvas.WinUI.TemplateSelectors
{
    internal sealed class ExplorerTemplateSelector : BaseTemplateSelector<BrowserItemViewModel>
    {
        public DataTemplate? FileTemplate { get; set; }

        public DataTemplate? FolderTemplate { get; set; }

        /// <inheritdoc/>
        protected override DataTemplate? SelectTemplateCore(BrowserItemViewModel? item, DependencyObject container)
        {
            if (container is TreeViewItem tvi)
                tvi.IsExpanded = true;

            return item switch
            {
                FileViewModel => FileTemplate,
                FolderViewModel => FolderTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
