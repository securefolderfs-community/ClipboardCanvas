using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClipboardCanvas.Storage.Base
{
    public interface IBaseStorageFolder
    {
        Task DeleteAsync(StorageDeleteOption option, bool recursive, IProgress<float> progress, CancellationToken cancellationToken);
    }
}
