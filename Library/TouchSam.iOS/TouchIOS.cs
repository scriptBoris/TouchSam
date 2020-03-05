using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ResolutionGroupName("TouchSam")]
[assembly: ExportEffect(typeof(TouchSam.iOS.TouchIOS), nameof(TouchSam.Touch))]
namespace TouchSam.iOS
{
    public class TouchIOS : PlatformEffect
    {
        public static void Init() { }

        public bool IsDisposed => (Container as IVisualElementRenderer)?.Element == null;
        public UIView View => Control ?? Container;
        private UIView _layer;
        private double _alpha;
        private CancellationTokenSource _cancellation;
        private bool isTaped;

        private System.Timers.Timer timer;
        private UILongPressGestureRecognizer gestureTap;
        private ICommand commandTap;
        private ICommand commandLongTap;
        private double longTapLatency;
        private bool isCanTouch;

        protected override void OnAttached()
        {
            isCanTouch = Touch.GetIsEnabled(Element);
            commandTap = Touch.GetTap(Element);
            commandLongTap = Touch.GetLongTap(Element);
            longTapLatency = Touch.GetLongTapLatency(Element);
            gestureTap = new UILongPressGestureRecognizer(OnTap);
            gestureTap.MinimumPressDuration = 0;

            if (commandLongTap != null)
                TimerInit();

            View.UserInteractionEnabled = true;
            View.AddGestureRecognizer(gestureTap);

            UpdateEffectColor();
        }

        protected override void OnDetached()
        {
            View.RemoveGestureRecognizer(gestureTap);
            gestureTap.Dispose();
            _layer?.Dispose();
            _layer = null;
            TimerDispose();
        }

        protected override void OnElementPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(e);

            if (e.PropertyName == Touch.IsEnabledProperty.PropertyName)
                isCanTouch = Touch.GetIsEnabled(Element);
            else if (e.PropertyName == Touch.ColorProperty.PropertyName)
                UpdateEffectColor();
            else if (e.PropertyName == Touch.TapProperty.PropertyName)
                commandTap = Touch.GetTap(Element);
            else if (e.PropertyName == Touch.LongTapProperty.PropertyName)
            {
                commandLongTap = Touch.GetLongTap(Element);
                if (commandLongTap == null)
                    TimerDispose();
                else
                    TimerInit();
            }
            else if (e.PropertyName == Touch.LongTapLatencyProperty.PropertyName)
            {
                longTapLatency = Touch.GetLongTapLatency(Element);
                if (timer != null)
                    timer.Interval = longTapLatency;
            }
        }

        private void OnTap(UILongPressGestureRecognizer press)
        {
            var coordinate = press.LocationInView(press.View);
            bool isInside = press.View.PointInside(coordinate, null);

            switch (press.State)
            {
                case UIGestureRecognizerState.Began:
                    if (isCanTouch)
                    {
                        isTaped = true;
                        if (timer != null)
                            timer.Start();
                        TapAnimation(0.3, 0, _alpha, false);
                    }
                    break;
                case UIGestureRecognizerState.Changed:
                    if (!isInside && isTaped)
                    {
                        isTaped = false;
                        TapAnimation(0.3, _alpha);
                    }
                    break;
                case UIGestureRecognizerState.Ended:
                    if (isInside && isTaped && isCanTouch)
                    {
                        if (timer == null)
                        {
                            TapExecute();
                        }
                        else 
                        {
                            if (timer.Enabled)
                            {
                                TapExecute();
                                timer.Stop();
                            }
                        }
                        TapAnimation(0.3, _alpha);
                    }
                    isTaped = false;
                    break;
                case UIGestureRecognizerState.Cancelled:
                case UIGestureRecognizerState.Failed:
                    if (isTaped)
                    {
                        isTaped = false;
                        TapAnimation(0.3, _alpha);
                    }
                    break;
            }
        }

        private void OnTimerElapsed(object o, System.Timers.ElapsedEventArgs e)
        {
            if (isTaped)
            {
                isTaped = false;

                Device.BeginInvokeOnMainThread(() =>
                {
                    LongTapExecute();
                    TapAnimation(0.3, _alpha);
                });
            }
        }

        private void TimerInit()
        {
            if (timer == null)
            {
                timer = new System.Timers.Timer();
                timer.Elapsed += OnTimerElapsed;
                timer.Interval = longTapLatency;
                timer.AutoReset = false;
            }
        }
        private void TimerDispose()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Elapsed -= OnTimerElapsed;
                timer.Dispose();
                timer = null;
            }
        }

        private void LongTapExecute()
        {
            if (commandLongTap != null)
            {
                var param = Touch.GetLongTapParameter(Element);
                if (commandLongTap.CanExecute(param))
                    commandLongTap.Execute(param);
            }
            else
            {
                TapExecute();
            }
        }

        private void TapExecute()
        {
            if (commandTap != null)
            {
                var param = Touch.GetTapParameter(Element);
                if (commandTap.CanExecute(param))
                    commandTap.Execute(param);
            }
        }

        #region TouchEffects
        private void UpdateEffectColor()
        {
            _layer?.Dispose();
            _layer = null;

            var color = Touch.GetColor(Element);
            if (color == Color.Default)
            {
                return;
            }
            _alpha = color.A < 1.0 ? 1 : 0.3;

            _layer = new UIView
            {
                BackgroundColor = color.ToUIColor(),
                UserInteractionEnabled = false,
            };
        }

        private async void TapAnimation(double duration, double start = 1, double end = 0, bool remove = true)
        {
            if (!IsDisposed && _layer != null)
            {
                _cancellation?.Cancel();
                _cancellation = new CancellationTokenSource();

                var token = _cancellation.Token;

                _layer.Frame = new CGRect(0, 0, Container.Bounds.Width, Container.Bounds.Height);
                Container.AddSubview(_layer);
                Container.BringSubviewToFront(_layer);
                _layer.Alpha = (float)start;
                await UIView.AnimateAsync(duration, () => {
                    if (!token.IsCancellationRequested && !IsDisposed)
                        _layer.Alpha = (float)end;
                });
                if (remove && !IsDisposed && !token.IsCancellationRequested)
                {
                    _layer?.RemoveFromSuperview();
                }
            }
        }
        #endregion
    }
}