using ClipboardCanvas.Models.Autopaste;
using System.Threading.Tasks;

namespace ClipboardCanvas.Services
{
    public interface IAutopasteService
    {
        void UpdateAutopasteTarget(IAutopasteTarget autopasteTarget);

        bool IsAutopasteTarget(IAutopasteTarget autopasteTarget);

        Task InitializeAutopaste();
    }
}
