using ClipboardCanvas.ViewModels.UserControls.Autopaste.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ClipboardCanvas.TemplateSelectors
{
    public class AutopasteTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FileSizeDataTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch (item)
            {
                case FileSizeRuleViewModel:
                    {
                        return FileSizeDataTemplate;
                    }

                default:
                    return null;
            }
        }
    }
}
