using ClipboardCanvas.Shared.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Shared.Extensions
{
    /// <summary>
    /// Contains common extensions for <see cref="IPersistable"/> and <see cref="IAsyncInitialize"/>.
    /// </summary>
    public static class PersistableExtensions
    {
        /// <summary>
        /// Tries to asynchronously initialize <see cref="IAsyncInitialize"/>
        /// </summary>
        /// <param name="asyncInitialize">The <see cref="IAsyncInitialize"/> instance to use.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns true; otherwise false.</returns>
        public static async Task<bool> TryInitAsync(this IAsyncInitialize asyncInitialize, CancellationToken cancellationToken = default)
        {
            try
            {
                await asyncInitialize.InitAsync(cancellationToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Tries to asynchronously save data stored in memory.
        /// </summary>
        /// <param name="persistable">The <see cref="IPersistable"/> instance to use.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns true; otherwise false.</returns>
        public static async Task<bool> TrySaveAsync(this IPersistable persistable, CancellationToken cancellationToken = default)
        {
            try
            {
                await persistable.SaveAsync(cancellationToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static T WithInitAsync<T>(this T asyncInitialize, CancellationToken cancellationToken = default)
            where T : IAsyncInitialize
        {
            _ = asyncInitialize.InitAsync(cancellationToken);
            return asyncInitialize;
        }
    }
}
