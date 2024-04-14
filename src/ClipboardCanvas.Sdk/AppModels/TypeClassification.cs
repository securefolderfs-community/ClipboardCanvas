namespace ClipboardCanvas.Sdk.AppModels
{
    public struct TypeClassification
    {
        /// <summary>
        /// Gets the MIME content type.
        /// </summary>
        public string MimeType { get; }

        /// <summary>
        /// Gets the content extension, if any.
        /// </summary>
        public string? Extension { get; }

        public TypeClassification(string mimeType, string? extension)
        {
            MimeType = mimeType;
            Extension = extension;
        }
    }
}
