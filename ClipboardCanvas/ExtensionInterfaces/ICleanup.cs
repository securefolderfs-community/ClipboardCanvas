using System;

namespace ClipboardCanvas.ExtensionInterfaces
{
    /// <summary>
    /// Defines a common interface for classes that should be cleaned up,
    /// but without the implications that <see cref="IDisposable"/> presupposes. An instance
    /// implementing <see cref="ICleanup"/> can be cleaned up without being
    /// disposed and garbage collected.
    /// </summary>
    public interface ICleanup
    {
        /// <summary>
        /// Cleans up the instance, for example by saving its state,
        /// removing resources, etc...
        /// </summary>
        void Cleanup();
    }
}
