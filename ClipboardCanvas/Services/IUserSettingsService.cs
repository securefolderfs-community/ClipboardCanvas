using System;
using ClipboardCanvas.EventArguments;

namespace ClipboardCanvas.Services
{
    public interface IUserSettingsService
    {
        event EventHandler<SettingChangedEventArgs> OnSettingChangedEvent;

        /// <summary>
        /// Determines whether to push a notification when app crashes
        /// </summary>
        bool PushErrorNotification { get; set; }

        /// <summary>
        /// Determines whether to show Timeline widget on homepage
        /// </summary>
        bool ShowTimelineOnHomepage { get; set; }

        /// <summary>
        /// Determines whether to enable permanent deletion option as default
        /// </summary>
        bool DeletePermanentlyAsDefault { get; set; }
        
        /// <summary>
        /// Determines whether to open new canvas on paste
        /// </summary>
        bool OpenNewCanvasOnPaste { get; set; }

        /// <summary>
        /// Determines whether items are copied as reference or directly to collection
        /// <br/><br/>
        /// If <see cref="AlwaysPasteFilesAsReference"/> is true, then item on paste will be copied as reference to original item location.
        /// <br/>
        /// Otherwise, items are copied directly to collection.
        /// </summary>
        bool AlwaysPasteFilesAsReference { get; set; } // TODO: Add a setting so user can also select which items to paste as reference

        /// <summary>
        /// Determines whether to favor markdown or .txt files when pasting items
        /// </summary>
        bool PrioritizeMarkdownOverText { get; set; }

        /// <summary>
        /// Determines whether to show delete confirmation dialog when deleting canvases
        /// </summary>
        bool ShowDeleteConfirmationDialog { get; set; }

        /// <summary>
        /// Determines whether to use Infinite Canvas as default when opening new Canvas
        /// </summary>
        bool UseInfiniteCanvasAsDefault { get; set; }

        /// <summary>
        /// Determines whether to use autopasting functionality
        /// </summary>
        bool IsAutopasteEnabled { get; set; }
    }
}
