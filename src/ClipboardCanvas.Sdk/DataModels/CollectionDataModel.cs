using System;

namespace ClipboardCanvas.Sdk.DataModels
{
    [Serializable]
    public sealed record class CollectionDataModel(string? Id, string? Name);
}
