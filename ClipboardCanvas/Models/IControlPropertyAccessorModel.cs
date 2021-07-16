using System;

namespace ClipboardCanvas.Models
{
    public interface IControlPropertyAccessorModel<TProperty>
    {
        event EventHandler<TProperty> OnPropertyValueUpdatedEvent;

        void PropertyValueUpdated(TProperty newValue);
    }
}
