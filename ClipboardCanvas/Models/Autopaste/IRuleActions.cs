using ClipboardCanvas.ViewModels.UserControls.Autopaste.Rules;

namespace ClipboardCanvas.Models.Autopaste
{
    public interface IRuleActions
    {
        void SerializeRules();

        void RemoveRule(BaseAutopasteRuleViewModel ruleViewModel);
    }
}
