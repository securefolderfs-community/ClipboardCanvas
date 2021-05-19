using System;

namespace ClipboardCanvas.CanvasExtensions
{
    /// <summary>
    /// Interface to mark <see cref="ClipboardCanvas.ViewModels.UserControls.CanvasDisplay.BasePasteCanvasViewModel"/> to implement draggable content
    /// </summary>
    public interface ICanvasContentDraggableExtension
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
