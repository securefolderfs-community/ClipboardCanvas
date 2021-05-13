using System;

namespace ClipboardCanvas.Models
{
    /// <summary>
    /// Interface to mark <see cref="BasePasteCanvasViewModel"/> to implement draggable content
    /// </summary>
    public interface ICanvasContentDraggable
    {
        event EventHandler OnDragStartedEvent;

        /// <summary>
        /// Determines whether the implementing class can use drag
        /// <br/><br/>
        /// Attention!
        /// <br/>
        /// The implementing class must NOT modify value of the property in any way
        /// </summary>
        bool IsDragAvailable { get; set; }
    }
}
