using System;

namespace ClipboardCanvas.ReferenceItems
{
    [Serializable]
    public sealed class ReferenceFileData
    {
        public readonly string path;

        public readonly long fileId;

        public readonly bool isRepairable;

        public ReferenceFileData(string path, long fileId, bool isRepairable)
        {
            this.path = path;
            this.fileId = fileId;
            this.isRepairable = isRepairable;
        }
    }
}
