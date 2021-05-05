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
    /// A file containing data and references to actual files
    /// </summary>
    public sealed class ReferenceFolder
    {
        private readonly StorageFile _innerFile;

        private List<ReferenceFileData> _referencedFiles; // TODO: Maybe change to Dictionary<StorageItem, ReferenceFileData>

        private ReferenceFolder(StorageFile innerFile, List<ReferenceFileData> referencedFiles)
        {
            this._innerFile = innerFile;
            this._referencedFiles = referencedFiles;
        }

        private void UpdateReferencedFiles(List<ReferenceFileData> referencedFiles)
        {
            for (int i = 0; i < _referencedFiles.Count; i++)
            {
                var currentItem = _referencedFiles[i];

                if (!referencedFiles.Contains(currentItem))
                {
                    // item in referencedFiles is not present, remove from _referencedFiles
                    _referencedFiles.Remove(currentItem);
                }
            }

            for(int i = 0; i < referencedFiles.Count; i++)
            {
                var currentItem = referencedFiles[i];

                if (!_referencedFiles.Contains(currentItem))
                {
                    // item in _referencedFiles is not present, add it to _referencedFiles
                    _referencedFiles.Add(currentItem);
                }
            }
        }

        public IReadOnlyList<ReferenceFileData> GetReferencedFiles()
        {
            return _referencedFiles;
        }

        public async Task<bool> AddReference(ReferenceFileData referenceFileData)
        {
            string data = await FileIO.ReadTextAsync(_innerFile);

            List<ReferenceFileData> references = JsonConvert.DeserializeObject<List<ReferenceFileData>>(data);
            references.Add(referenceFileData);

            UpdateReferencedFiles(references);

            string serialized = JsonConvert.SerializeObject(references, Formatting.Indented);
            await FileIO.WriteTextAsync(_innerFile, serialized);

            return true;
        }

        public async Task<bool> RemoveReference(ReferenceFileData referenceFileData)
        {
            string data = await FileIO.ReadTextAsync(_innerFile);

            List<ReferenceFileData> references = JsonConvert.DeserializeObject<List<ReferenceFileData>>(data);
            references.Remove(referenceFileData);

            UpdateReferencedFiles(references);

            string serialized = JsonConvert.SerializeObject(references, Formatting.Indented);
            await FileIO.WriteTextAsync(_innerFile, serialized);

            return true;
        }

        internal static async Task<List<ReferenceFileData>> ReadData(StorageFile referenceFile)
        {
            string data = await FileIO.ReadTextAsync(referenceFile);
            List<ReferenceFileData> referenceFileData = JsonConvert.DeserializeObject<List<ReferenceFileData>>(data);

            return referenceFileData;
        }

        public static async Task<ReferenceFolder> GetFolder(StorageFile referenceFile)
        {
            if (!IsReferenceFolder(referenceFile))
            {
                return null;
            }

            List<ReferenceFileData> referenceFileData = await ReadData(referenceFile);

            return GetFolder(referenceFile, referenceFileData);
        }

        public static ReferenceFolder GetFolder(StorageFile referenceFile, List<ReferenceFileData> referenceFileData)
        {
            return new ReferenceFolder(referenceFile, referenceFileData);
        }

        public static bool IsReferenceFolder(StorageFile file)
        {
            return file.Path.EndsWith(Constants.FileSystem.REFERENCE_FILELIST_EXTENSION);
        }
    }
}
