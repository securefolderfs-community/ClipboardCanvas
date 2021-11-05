using ClipboardCanvas.Models.Autopaste;

namespace ClipboardCanvas.Services
{
    public interface IAutopasteService
    {
        void UpdateAutopasteTarget(IAutopasteTarget autopasteTarget);
    }
}
