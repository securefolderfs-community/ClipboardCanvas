using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;

using ClipboardCanvas.Models.Autopaste;

namespace ClipboardCanvas.ViewModels.UserControls.Autopaste.Rules
{
    public abstract class BaseAutopasteRuleViewModel : ObservableObject
    {
        [JsonIgnore]
        public IRuleActions RuleActions { get; set; }

        [JsonIgnore]
        protected string ruleName;
        [JsonIgnore]
        public string RuleName
        {
            get => ruleName;
            set => SetProperty(ref ruleName, value);
        }

        [JsonIgnore]
        protected string ruleFontIconGlyph;
        [JsonIgnore]
        public string RuleFontIconGlyph
        {
            get => ruleFontIconGlyph;
            set => SetProperty(ref ruleFontIconGlyph, value);
        }

        [JsonIgnore]
        public ICommand RemoveRuleCommand { get; private set; }

        public BaseAutopasteRuleViewModel(IRuleActions ruleActions)
            : this()
        {
            this.RuleActions = ruleActions;
        }

        [JsonConstructor]
        private BaseAutopasteRuleViewModel()
        {
            this.RemoveRuleCommand = new RelayCommand(() => this.RuleActions.RemoveRule(this));
        }

        public abstract Task<bool> PassesRule(DataPackageView dataPackage);
    }
}
