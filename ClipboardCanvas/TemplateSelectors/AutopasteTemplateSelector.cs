using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using ClipboardCanvas.ViewModels.UserControls.Autopaste.Rules;

namespace ClipboardCanvas.TemplateSelectors
{
    public class AutopasteTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FileSizeDataTemplate { get; set; }

        public DataTemplate TypeFilterDataTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch (item)
            {
                case FileSizeRuleViewModel:
                    {
                        return FileSizeDataTemplate;
                    }

                case TypeFilterRuleViewModel:
                    {
                        return TypeFilterDataTemplate;
                    }

                default:
                    return null;
            }
        }
    }
}
