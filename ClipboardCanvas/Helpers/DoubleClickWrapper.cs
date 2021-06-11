using System;
using System.Diagnostics;
using Windows.UI.Xaml;

namespace ClipboardCanvas.Helpers
{
    /// <summary>
    /// A wrapper for click event to implement double click
    /// </summary>
    public class DoubleClickWrapper : IDisposable
    {
        private readonly TimeSpan _timeBetweenClicks;

        private readonly DispatcherTimer _timer;

        private Action _doubleClickAction;

        private bool _canExecute;

        private bool _isDoubleClick;

        public DoubleClickWrapper(Action doubleClickAction, TimeSpan timeBetweenClicks)
        {
            this._doubleClickAction = doubleClickAction;
            this._timeBetweenClicks = timeBetweenClicks;

            this._timer = new DispatcherTimer();
            this._timer.Interval = _timeBetweenClicks;
            this._timer.Tick += Timer_Tick;
            this._timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            _canExecute = false;
            _timer.Stop();
        }

        /// <summary>
        /// The click action to signalize the click to the wrapper
        /// </summary>
        public void Click()
        {
            _timer.Start();

            if (_canExecute)
            {
                _isDoubleClick = true;
                _doubleClickAction?.Invoke();
            }

            if (!_isDoubleClick)
            {
                _canExecute = true;
            }
            else
            {
                _canExecute = false;
            }
        }

        public void Dispose()
        {
            this._doubleClickAction = null;

            this._timer.Stop();
            this._timer.Tick -= Timer_Tick;
        }
    }
}
