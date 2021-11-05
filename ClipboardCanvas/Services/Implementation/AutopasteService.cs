using ClipboardCanvas.Models.Autopaste;
using ClipboardCanvas.ViewModels.UserControls.Autopaste;

namespace ClipboardCanvas.Services.Implementation
{
    public class AutopasteService : IAutopasteService
    {
        public AutopasteControlViewModel AutopasteControlViewModel { get; set; }

        public void UpdateAutopasteTarget(IAutopasteTarget autopasteTarget)
        {
            AutopasteControlViewModel.AutopasteTarget = autopasteTarget;
        }
    }
}
