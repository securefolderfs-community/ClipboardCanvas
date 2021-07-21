using System;
using Windows.UI.Xaml;

namespace ClipboardCanvas.AttachedProperties
{
    /// <summary>
    /// A control tied <see cref="BaseAttachedProperty{TParent, TProperty, TTarget}"/> wrapper for <see cref="DependencyProperty"/> to attach properties
    /// </summary>
    /// <typeparam name="TParent">The parent class to be the attached property</typeparam>
    /// <typeparam name="TProperty">The type of this attached property</typeparam>
    /// <typeparam name="TTarget">The type that this property can be attached to</typeparam>
    public abstract class BaseAttachedProperty<TParent, TProperty, TTarget>
        where TParent : new()
        where TTarget : DependencyObject
    {
        #region Public Events

        /// <summary>
        /// Fired when the value changes
        /// </summary>
        public event Action<DependencyObject, DependencyPropertyChangedEventArgs> ValueChanged = (sender, e) => { };

        #endregion

        #region Public Properties

        /// <summary>
        /// A singleton instance of our parent class
        /// </summary>
        public static TParent Instance { get; private set; } = new TParent();

        #endregion

        #region Attached Property Definitions

        /// <summary>
        /// The attached property for this class
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached(
            "Value",
            typeof(TProperty),
            typeof(TTarget),
            new PropertyMetadata(
                default(TProperty),
                new PropertyChangedCallback(OnValuePropertyChanged)));

        /// <summary>
        /// The callback event when the <see cref="ValueProperty"/> is changed
        /// </summary>
        /// <param name="d">The UI element that had it's property changed</param>
        /// <param name="e">The arguments for the event</param>
        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Call the parent function
            (Instance as BaseAttachedProperty<TParent, TProperty, TTarget>)?.OnValueChanged(d, e);

            // Call event listeners
            (Instance as BaseAttachedProperty<TParent, TProperty, TTarget>)?.ValueChanged(d, e);
        }

        /// <summary>
        /// Gets the attached property
        /// </summary>
        /// <param name="d">The element to get the property from</param>
        /// <returns></returns>
        public static TProperty GetValue(DependencyObject d) => (TProperty)d.GetValue(ValueProperty);

        /// <summary>
        /// Sets the attached property
        /// </summary>
        /// <param name="d">The element to get the property from</param>
        /// <param name="value">The value to set the property to</param>
        public static void SetValue(DependencyObject d, TProperty value) => d.SetValue(ValueProperty, value);

        #endregion

        #region Event Methods

        /// <summary>
        /// The method that is called when any attached property of this type is changed
        /// </summary>
        /// <param name="sender">The UI element that this property was changed for</param>
        /// <param name="e">The arguments for this event</param>
        public virtual void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) 
        {
            this.OnValueChanged((TTarget)sender, (TProperty)e.NewValue);
        }

        public abstract void OnValueChanged(TTarget sender, TProperty newValue);

        #endregion
    }
}
