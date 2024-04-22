using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Shared.ComponentModel;
using OwlCore.Storage;

namespace ClipboardCanvas.Sdk.Models
{
    public interface IPasteModel : IAsyncInitialize, IDisposable
    {
        IDataSourceModel SourceModel { get; }

        //Task<Tuple> PasteAsync(IClipboardData clipboardData, CancellationToken cancellationToken);
    }
}
