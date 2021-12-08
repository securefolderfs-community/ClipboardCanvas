using System;

using ClipboardCanvas.ViewModels.Widgets.Timeline;

namespace ClipboardCanvas.EventArguments
{
    public class RemoveSectionItemRequestedEventArgs : EventArgs
    {
        public readonly TimelineSectionItemViewModel itemViewModel;

        public RemoveSectionItemRequestedEventArgs(TimelineSectionItemViewModel itemViewModel)
        {
            this.itemViewModel = itemViewModel;
        }
    }

    public class RemoveSectionRequestedEventArgs : EventArgs
    {
        public readonly TimelineSectionViewModel sectionViewModel;

        public RemoveSectionRequestedEventArgs(TimelineSectionViewModel sectionViewModel)
        {
            this.sectionViewModel = sectionViewModel;
        }
    }
}
