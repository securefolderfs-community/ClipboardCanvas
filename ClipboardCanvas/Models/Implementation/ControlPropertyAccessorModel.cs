using System;

namespace ClipboardCanvas.Models.Implementation
{
    public class ControlPropertyAccessorModel<TProperty> : IControlPropertyAccessorModel<TProperty>
    {
        public event EventHandler<TProperty> OnPropertyValueUpdatedEvent;

        public void PropertyValueUpdated(TProperty newValue)
        {
            OnPropertyValueUpdatedEvent?.Invoke(this, newValue);
        }
    }
}
