using System.Threading.Tasks;

using ClipboardCanvas.Models.Autopaste;
using ClipboardCanvas.ViewModels.UserControls.Autopaste;

namespace ClipboardCanvas.Services.Implementation
{
    public class AutopasteService : IAutopasteService
    {
        public AutopasteControlViewModel AutopasteControlViewModel { get; set; }

        public async Task InitializeAutopaste()
        {
            if (AutopasteControlViewModel != null)
            {
                await AutopasteControlViewModel.Initialize();
            }
        }

        public bool IsAutopasteTarget(IAutopasteTarget autopasteTarget)
        {
            return AutopasteControlViewModel.AutopasteTarget == autopasteTarget;
        }

        public void UpdateAutopasteTarget(IAutopasteTarget autopasteTarget)
        {
            AutopasteControlViewModel?.UpdateTarget(autopasteTarget);
        }
    }
}
