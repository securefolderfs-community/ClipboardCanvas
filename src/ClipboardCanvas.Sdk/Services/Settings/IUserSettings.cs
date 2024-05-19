using ClipboardCanvas.Shared.ComponentModel;
using System.ComponentModel;

namespace ClipboardCanvas.Sdk.Services.Settings
{
    /// <summary>
    /// An interface to manage user preferences and settings.
    /// </summary>
    public interface IUserSettings : IPersistable, IAsyncInitialize, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the value whether to resume the previous session on last canvas.
        /// </summary>
        bool UninterruptedResume { get; set; }
    }
}
