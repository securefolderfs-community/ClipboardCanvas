using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Shared.ComponentModel;
using ClipboardCanvas.UI.Utils;
using System.Threading.Tasks;

namespace ClipboardCanvas.UI.ServiceImplementation
{
    /// <inheritdoc cref="IOverlayService"/>
    public abstract class BaseOverlayService : IOverlayService
    {
        /// <inheritdoc/>
        public virtual async Task<IResult> ShowAsync(IViewable viewable)
        {
            var overlay = GetOverlay(viewable);
            overlay.SetView(viewable);

            // Show overlay
            return await overlay.ShowAsync();
        }

        protected abstract IOverlayControl GetOverlay(IViewable viewable);
    }
}
