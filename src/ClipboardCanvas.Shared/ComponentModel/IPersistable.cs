using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Shared.ComponentModel
{
    /// <summary>
    /// Allows for data to be saved into a persistence store.
    /// </summary>
    public interface IPersistable
    {
        /// <summary>
        /// Asynchronously saves data stored in memory.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task SaveAsync(CancellationToken cancellationToken = default);
    }
}
