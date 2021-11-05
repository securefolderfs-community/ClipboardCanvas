using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using Windows.UI.Xaml;

namespace ClipboardCanvas.AttachedProperties
{
    public abstract class BaseObservableCollectionAttachedProperty<TParent, TCollectionProperty, TTarget> : BaseAttachedProperty<TParent, ObservableCollection<TCollectionProperty>, TTarget>
        where TParent : new()
        where TTarget : DependencyObject
    {
        private ObservableCollection<TCollectionProperty> _collection;

        private TTarget _sender;

        protected override void OnValueChanged(TTarget sender, ObservableCollection<TCollectionProperty> newValue)
        {
            this._collection = newValue;
            this._sender = sender;
        }

        public override void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is INotifyCollectionChanged notifyCollectionChangedOld)
            {
                notifyCollectionChangedOld.CollectionChanged -= ObservableCollection_CollectionChangedInternal;
            }

            if (e.NewValue is INotifyCollectionChanged notifyCollectionChangedNew)
            {
                notifyCollectionChangedNew.CollectionChanged -= ObservableCollection_CollectionChangedInternal;
                notifyCollectionChangedNew.CollectionChanged += ObservableCollection_CollectionChangedInternal;
            }

            base.OnValueChanged(sender, e);
        }

        private void ObservableCollection_CollectionChangedInternal(object sender, NotifyCollectionChangedEventArgs e)
        {
            ObservableCollection_CollectionChanged(this._sender, this._collection, e);
        }

        protected abstract void ObservableCollection_CollectionChanged(object sender, ObservableCollection<TCollectionProperty> collection, NotifyCollectionChangedEventArgs e);
    }
}
