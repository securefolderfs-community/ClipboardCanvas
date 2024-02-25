using ClipboardCanvas.Shared.ComponentModel;
using System.Threading;

namespace ClipboardCanvas.Shared.Extensions
{
    public static class InitializationExtensions
    {
        public static T WithInitAsync<T>(this T asyncInitialize, CancellationToken cancellationToken = default)
            where T : IAsyncInitialize
        {
            _ = asyncInitialize.InitAsync(cancellationToken);
            return asyncInitialize;
        }
    }
}
