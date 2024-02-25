using System.ComponentModel;

namespace ClipboardCanvas.Shared.ComponentModel
{
    /// <summary>
    /// Represents a generic viewable display.
    /// </summary>
    public interface IViewable : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the title of this view.
        /// </summary>
        string? Title { get; }
    }
}
