using OwlCore.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.Services
{
    public interface IApplicationService
    {
        Task LaunchHandlerAsync(IFile file, CancellationToken cancellationToken);
    }
}
