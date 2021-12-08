using System;
using Microsoft.UI.Xaml;

namespace ClipboardCanvas.AttachedProperties
{
    /// <summary>
    /// Generic control tied <see cref="BaseGenericAttachedProperty{TParent, TProperty}"/> wrapper for <see cref="DependencyProperty"/> to attach properties
    /// </summary>
    /// <typeparam name="TParent">The parent class to be the attached property</typeparam>
    /// <typeparam name="TProperty">The type of this attached property</typeparam>
    public abstract class BaseGenericAttachedProperty<TParent, TProperty> : BaseAttachedProperty<TParent, TProperty, DependencyObject>
        where TParent : new()
    {
    }
}
