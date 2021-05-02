using ClipboardCanvas.ViewModels.UserControls;
using System.Collections.Generic;

namespace ClipboardCanvas.Models
{
    public interface IAdaptiveOptionsControlModel
    {
        void SetActions(IEnumerable<AdaptiveOptionsControlItemViewModel> actions);

        void AddAction(AdaptiveOptionsControlItemViewModel action);

        void RemoveAction(AdaptiveOptionsControlItemViewModel action);

        void RemoveActionAt(int index);

        void RemoveAllActions();
    }
}
