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
    [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
    internal class MessageTesting : UIViewController
    {
        public static void Show(string message)
        {
            try
            {
                var alert = UIAlertController.Create(null, message, UIAlertControllerStyle.Alert);
                var alertDelay = NSTimer.CreateRepeatingScheduledTimer(3.0, 
                    (obj) => {
                        alert.DismissViewController(true, null);
                        obj.Dispose();
                    });

                UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alert, true, null);
            }
            catch (Exception)
            {
            }
        }
    }

    [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
    public class TouchIOS : PlatformEffect
    {
        [Obsolete("Use TouchSam.iOS.TouchIOS.Preserve")]
        public static void Init() { }

        public static void Preserve()
        {
            Touch.Preserve();
            TimerPlatform.Preserve();
            TouchUITapGestureRecognizerDelegate.Preserve();
        }

        public bool IsDisposed => (Container as IVisualElementRenderer)?.Element == null;
        public UIView View => Control ?? Container;

        private UIView _layer;
        private double _alpha;
        private CancellationTokenSource _cancellation;
        private bool isTaped;

        internal TimerPlatform timer;
        private UILongPressGestureRecognizer gestureTap;
        private ICommand commandStartTap;
        private ICommand commandFinishTap;
        private ICommand commandTap;
        private ICommand commandLongTap;
        internal double longTapLatency;
        internal bool isCanTouch;

        protected override void OnAttached()
        {
            isCanTouch = Touch.GetIsEnabled(Element);
            commandStartTap = Touch.GetStartTap(Element);
            commandFinishTap = Touch.GetFinishTap(Element);
            commandTap = Touch.GetTap(Element);
            commandLongTap = Touch.GetLongTap(Element);
            longTapLatency = Touch.GetLongTapLatency(Element);
            gestureTap = new UILongPressGestureRecognizer(OnTap);
            gestureTap.MinimumPressDuration = 0;
            gestureTap.Delegate = new TouchUITapGestureRecognizerDelegate();

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
            {
                isCanTouch = Touch.GetIsEnabled(Element);
            }
            else if (e.PropertyName == Touch.ColorProperty.PropertyName)
            {
                UpdateEffectColor();
            }
            else if (e.PropertyName == Touch.TapProperty.PropertyName)
            {
                commandTap = Touch.GetTap(Element);
            }
            else if (e.PropertyName == Touch.StartTapProperty.PropertyName)
            {
                commandStartTap = Touch.GetStartTap(Element);
            }
            else if (e.PropertyName == Touch.FinishTapProperty.PropertyName)
            {
                commandFinishTap = Touch.GetFinishTap(Element);
            }
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
            }
        }

        private CGPoint startPoint;
        private void OnTap(UILongPressGestureRecognizer press)
        {
            var coordinate = press.LocationInView(press.View);
            bool isInside = press.View.PointInside(coordinate, null);

            switch (press.State)
            {
                case UIGestureRecognizerState.Began:
                    if (isCanTouch)
                    {
                        startPoint = coordinate;
                        StartTapExecute();
                        isTaped = true;
                        if (timer != null)
                            timer.Start(longTapLatency);
                        TapAnimation(0.3, 0, _alpha, false);
                    }
                    break;
                case UIGestureRecognizerState.Changed:
                    if (!isInside && isTaped)
                    {
                        isTaped = false;
                        TapAnimation(0.3, _alpha);
                        FinishTapExecute();
                    }
                    else if (isTaped)
                    {
                        var diffX = Math.Abs(coordinate.X - startPoint.X);
                        var diffY = Math.Abs(coordinate.Y - startPoint.Y);
                        var maxDiff = Math.Max(diffX, diffY);
                        if (maxDiff > 4)
                        {
                            isTaped = false;
                            TapAnimation(0.3, _alpha);
                            FinishTapExecute();
                        }
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
                            if (timer.IsEnabled)
                            {
                                TapExecute();
                                timer.Stop();
                            }
                        }
                        TapAnimation(0.3, _alpha);
                        FinishTapExecute();
                    }
                    isTaped = false;
                    break;
                case UIGestureRecognizerState.Cancelled:
                case UIGestureRecognizerState.Failed:
                    if (isTaped)
                    {
                        isTaped = false;
                        TapAnimation(0.3, _alpha);
                        FinishTapExecute();
                    }
                    break;
            }
        }

        private void TimerInit()
        {
            if (timer == null)
                timer = new TimerPlatform(OnTimerElapsed);
        }

        private void TimerDispose()
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
        }

        private void OnTimerElapsed()
        {
            if (isTaped)
            {
                isTaped = false;

                LongTapExecute();
                FinishTapExecute();
                TapAnimation(0.3, _alpha);
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

        private void StartTapExecute()
        {
            if (commandStartTap != null)
            {
                var param = Touch.GetStartTapParameter(Element);
                if (commandStartTap.CanExecute(param))
                    commandStartTap.Execute(param);
            }
        }

        private void FinishTapExecute()
        {
            if (commandFinishTap != null)
            {
                var param = Touch.GetFinishTapParameter(Element);
                if (commandFinishTap.CanExecute(param))
                    commandFinishTap.Execute(param);
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
                await UIView.AnimateAsync(duration, () =>
                {
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

    [Preserve(AllMembers = true)]
    internal class TouchUITapGestureRecognizerDelegate : UIGestureRecognizerDelegate
    {
        public static void Preserve()
        {
        }

        public override bool ShouldRecognizeSimultaneously(UIGestureRecognizer gestureRecognizer,
            UIGestureRecognizer otherGestureRecognizer)
        {
            return true;
        }
    }
}