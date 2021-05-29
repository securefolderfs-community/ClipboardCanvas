using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Helpers.SafetyHelpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClipboardCanvas.ReferenceItems
{
    /// <summary>
    /// A file containing data and reference to an actual file
    /// </summary>
    public sealed class ReferenceFile
    {
        private readonly StorageFile _innerFile;

        public SafeWrapperResult LastError { get; private set; } = SafeWrapperResult.S_SUCCESS;

        public StorageFile ReferencedFile { get; private set; }

        public ReferenceFileData ReferenceFileData { get; private set; }

        private ReferenceFile(StorageFile innerFile, StorageFile referencedFile)
        {
            this._innerFile = innerFile;
            this.ReferencedFile = referencedFile;
        }

        public async Task UpdateReferenceFile(ReferenceFileData referenceFileData)
        {
            string serialized = JsonConvert.SerializeObject(referenceFileData, Formatting.Indented);
            await FileIO.WriteTextAsync(_innerFile, serialized);
        }

        internal static async Task<ReferenceFileData> ReadData(StorageFile referenceFile)
        {
            string data = await FileIO.ReadTextAsync(referenceFile);
            ReferenceFileData referenceFileData = JsonConvert.DeserializeObject<ReferenceFileData>(data);

            return referenceFileData;
        }

        public static async Task<ReferenceFile> GetFile(StorageFile referenceFile)
        {
            if (!IsReferenceFile(referenceFile))
            {
                return null;
            }

            ReferenceFileData referenceFileData = await ReadData(referenceFile);

            return await GetFile(referenceFile, referenceFileData);
        }

        public static async Task<ReferenceFile> GetFile(StorageFile referenceFile, ReferenceFileData referenceFileData)
        {
            if (referenceFileData == null || string.IsNullOrEmpty(referenceFileData.path))
            {
                return new ReferenceFile(referenceFile, null);
            }

            SafeWrapper<StorageFile> file = await StorageItemHelpers.ToStorageItemWithError<StorageFile>(referenceFileData.path);

            if (!file)
            {
                return new ReferenceFile(referenceFile, null)
                {
                    LastError = (SafeWrapperResult)file
                };
            }

            return new ReferenceFile(referenceFile, file);
        }

        public static bool IsReferenceFile(StorageFile file)
        {
            return file?.Path.EndsWith(Constants.FileSystem.REFERENCE_FILE_EXTENSION) ?? false;
        }
    }
}
