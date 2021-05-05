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
        /// Determines whether pasted file is pasted as content
        /// <br/><br/>
        /// Presume a case where user pastes a file which is an image file
        /// <br/>
        /// If <see cref="PastePastableFilesAsContent"/> is true, then the file is pasted and displayed as an image.
        /// <br/>
        /// This is retained with other types of which content can be displayed.
        /// <br/><br/>
        /// Otherwise, if <see cref="PastePastableFilesAsContent"/> is false, the file is pasted and not displayed as conveniently viewable content
        /// </summary>
        public bool PastePastableFilesAsContent { get; set; } // TODO: Useless?

        /// <summary>
        /// Determines whether large items are copied directly to a collection
        /// <br/><br/>
        /// If <see cref="CopyLargeItemsDirectlyToCollection"/> is true, then item on paste will be copied.
        /// <br/>
        /// Otherwise, items are copied as reference to original item location.
        /// <br/><br/>
        /// Attention!
        /// <br/>
        /// Small items like images will always be copied no matter the value of that setting
        /// </summary>
        public bool CopyLargeItemsDirectlyToCollection { get; set; }
    }
}
