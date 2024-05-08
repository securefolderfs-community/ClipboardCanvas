using System.IO;

namespace ClipboardCanvas.Shared.ComponentModel
{
    /// <summary>
    /// Represents an object which uses a pointer to move within a collection.
    /// </summary>
    public interface ISeekable
    {
        /// <summary>
        /// Sets the position of the pointer within the collection.
        /// </summary>
        /// <param name="offset">The offset measured in the amount of items.</param>
        /// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
        /// <returns>The new position of the pointer.</returns>
        int Seek(int offset, SeekOrigin origin);
    }
}
