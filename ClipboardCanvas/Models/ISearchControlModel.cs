using System;
using ClipboardCanvas.EventArguments;
using ClipboardCanvas.Interfaces.Search;

namespace ClipboardCanvas.Models
{
    public interface ISearchControlModel
    {
        event EventHandler<SearchCloseRequestedEventArgs> OnSearchCloseRequestedEvent;

        ISearchItems SearchItems { get; set; }

        bool IsKeyboardAcceleratorEnabled { get; set; }

        void FindNext();

        void FindPrevious();

        void OnSearchShown();

        void OnSearchHidden();

        void ResetIndex();
    }
}
