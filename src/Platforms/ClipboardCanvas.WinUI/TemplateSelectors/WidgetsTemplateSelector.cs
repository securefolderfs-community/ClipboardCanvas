using ClipboardCanvas.Sdk.ViewModels.Widgets;
using Microsoft.UI.Xaml;

namespace ClipboardCanvas.WinUI.TemplateSelectors
{
    internal sealed class WidgetsTemplateSelector : BaseTemplateSelector<BaseWidgetViewModel>
    {
        public DataTemplate? CollectionsWidgetDataTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(BaseWidgetViewModel? item, DependencyObject container)
        {
            return item switch
            {
                CollectionsWidgetViewModel => CollectionsWidgetDataTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
