using ClipboardCanvas.Models.Autopaste;
using ClipboardCanvas.ViewModels.UserControls.Autopaste;
using System.Threading.Tasks;

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

        public void UpdateAutopasteTarget(IAutopasteTarget autopasteTarget)
        {
            AutopasteControlViewModel?.UpdateTarget(autopasteTarget);
        }
    }
}
