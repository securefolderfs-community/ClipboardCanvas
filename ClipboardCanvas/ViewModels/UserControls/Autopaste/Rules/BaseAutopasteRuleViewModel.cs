using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;

namespace ClipboardCanvas.ViewModels.UserControls.Autopaste.Rules
{
    public abstract class BaseAutopasteRuleViewModel : ObservableObject
    {
        public ICommand RemoveRuleCommand { get; protected set; }

        public abstract Task<bool> PassesRule(DataPackageView dataPackage);
    }
}
