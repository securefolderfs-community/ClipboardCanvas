using OwlCore.Storage;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClipboardCanvas.WinUI.Storage
{
    /// <inheritdoc cref="IStorableChild"/>
    internal abstract class WindowsStorable<TStorage> : IStorableChild
        where TStorage : class, IStorageItem
    {
        internal readonly TStorage storage;

        /// <inheritdoc/>
        public string Name { get; protected set; }

        /// <inheritdoc/>
        public virtual string Id { get; }

        protected WindowsStorable(TStorage storage)
        {
            this.storage = storage;
            this.Id = storage.Path;
            this.Name = storage.Name;
        }

        /// <inheritdoc/>
        public abstract Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default);
    }
}
