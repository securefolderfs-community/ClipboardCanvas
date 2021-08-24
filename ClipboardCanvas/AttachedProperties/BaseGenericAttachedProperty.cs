using System;
using Windows.UI.Xaml;

namespace ClipboardCanvas.AttachedProperties
{
    /// <summary>
    /// Generic control tied <see cref="BaseGenericAttachedProperty{TParent, TProperty}"/> wrapper for <see cref="DependencyProperty"/> to attach properties
    /// </summary>
    /// <typeparam name="TParent">The parent class to be the attached property</typeparam>
    /// <typeparam name="TProperty">The type of this attached property</typeparam>
    public abstract class BaseGenericAttachedProperty<TParent, TProperty>
        where TParent : new()
    {
        #region Public Events

        /// <summary>
        /// Fired when the value changes
        /// </summary>
        public event Action<DependencyObject, DependencyPropertyChangedEventArgs> ValueChanged;

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
            typeof(BaseGenericAttachedProperty<TParent, TProperty>),
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
            (Instance as BaseGenericAttachedProperty<TParent, TProperty>)?.OnValueChanged(d, e);

            // Call event listeners
            (Instance as BaseGenericAttachedProperty<TParent, TProperty>)?.ValueChanged?.Invoke(d, e);
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
        public virtual void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) { }

        #endregion
    }
}
