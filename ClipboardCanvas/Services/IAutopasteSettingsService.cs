using System.Collections.Generic;

using ClipboardCanvas.ViewModels.UserControls.Autopaste.Rules;

namespace ClipboardCanvas.Services
{
    public interface IAutopasteSettingsService
    {
        bool GuaranteeAddToList<T>(IList<T> list, T item, string listSettingName);

        bool GuaranteeRemoveFromList<T>(IList<T> list, T item, string listSettingName);

        bool FlushSettings();

        string AutopastePath { get; set; }

        List<BaseAutopasteRuleViewModel> SavedRules { get; set; }

        bool FileSizeRuleDoesNotApplyToFoldersWarningDismissed { get; set; }
    }
}
