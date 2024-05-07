using ClipboardCanvas.Sdk.Services;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.UI.ServiceImplementation
{
    /// <inheritdoc cref="IStorageService"/>
    public sealed class SystemStorageService : IStorageService
    {
        /// <inheritdoc/>
        public Task<IChildFile> GetFileAsync(string id, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(id))
                throw new FileNotFoundException();

            return Task.FromResult<IChildFile>(new SystemFile(id));
        }

        /// <inheritdoc/>
        public Task<IChildFolder> GetFolderAsync(string id, CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(id))
                throw new DirectoryNotFoundException();

            return Task.FromResult<IChildFolder>(new SystemFolder(id));
        }
    }
}
