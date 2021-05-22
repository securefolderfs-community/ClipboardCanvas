namespace ClipboardCanvas.Enums
{
    public enum CanvasPreviewMode
    {
        /// <summary>
        /// The canvas is only displayed as preview of content
        /// </summary>
        PreviewOnly = 0,

        /// <summary>
        /// The canvas can be previewed and interacted with
        /// </summary>
        InteractionAndPreview = 1,

        /// <summary>
        /// The canvas' content can be previewed and modified
        /// </summary>
        WriteAndPreview = 2,

        /// <summary>
        /// The canvas' content can be previewed modified and/or interacted with
        /// </summary>
        WritePreviewAndInteract = 4
    }
}
