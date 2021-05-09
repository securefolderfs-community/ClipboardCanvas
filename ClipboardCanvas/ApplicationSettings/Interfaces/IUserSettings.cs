namespace ClipboardCanvas.ApplicationSettings.Interfaces
{
    public interface IUserSettings
    {
        /// <summary>
        /// Determines whether detailed logging is enabled or disabled
        /// <br/><br/>
        /// Detailed logging helps further investigate an issue by logging exceptions caught by <see cref="ClipboardCanvas.Helpers.SafetyHelpers.SafeWrapperRoutines"/>
        /// </summary>
        bool EnableDetailedLogging { get; set; }

        bool OpenNewCanvasOnPaste { get; set; }

        bool AlwaysOpenNewPageWhenSelectingCollection { get; set; }

        /// <summary>
        /// Determines whether items are copied as reference or directly to collection
        /// <br/><br/>
        /// If <see cref="AlwaysPasteFilesAsReference"/> is true, then item on paste will be copied as reference to original item location.
        /// <br/>
        /// Otherwise, items are copied directly to collection.
        /// </summary>
        public bool AlwaysPasteFilesAsReference { get; set; } // TODO: Add a setting so user can also select which items to paste as reference
    }
}
