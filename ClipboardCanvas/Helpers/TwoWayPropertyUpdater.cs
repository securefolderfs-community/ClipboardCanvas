using System;

namespace ClipboardCanvas.Helpers
{
    public sealed class TwoWayPropertyUpdater<TProperty>
    {
        public event EventHandler<TProperty> OnPropertyValueUpdatedEvent;

        public void NotifyPropertyValueUpdated(TProperty newValue)
        {
            OnPropertyValueUpdatedEvent?.Invoke(this, newValue);
        }
    }
}
