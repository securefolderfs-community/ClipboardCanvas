using OwlCore.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Views.Browser
{
    public class FileViewModel(IFile file) : BrowserItemViewModel(file)
    {
        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
            // TODO: Load thumbnail
        }
    }
}
