using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Shared.ComponentModel
{
    public interface IDocument : IDisposable
    {
        int PageCount { get; }

        Task<IDocumentPage> GetPageAsync(int index, CancellationToken cancellationToken);

        IAsyncEnumerable<IDocumentPage> GetPagesAsync(CancellationToken cancellationToken);
    }

    public interface IDocumentPage : IDisposable
    {
        Task<IImage?> AsImageAsync(CancellationToken cancellationToken);
    }
}
