using Android.Animation;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms.Platform.Android;

[assembly: Xamarin.Forms.ResolutionGroupName("TouchSam")]
[assembly: Xamarin.Forms.ExportEffect(typeof(TouchSam.Droid.TouchDroid), nameof(TouchSam.Touch))]
namespace TouchSam.Droid
{
    [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
    public class TouchDroid : PlatformEffect
    {
        private readonly Rect _rect = new Rect();
        private readonly int[] _location = new int[2];
        private float startX;
        private float startY;
        private float currentX;
        private float currentY;

        private System.Timers.Timer timer;
        private Color tapColor;
        private byte tapColorAlpha;
        private ObjectAnimator animator;
        private FrameLayout overlayAnimation;
        private RippleDrawable ripple;
        private GestureTouchSam gesture;

        public bool IsEnabled => Touch.GetIsEnabled(Element);
        public bool IsSdk21 => Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop;
        public View View => Control ?? Container;
        public bool IsDisposed => (Container as IVisualElementRenderer)?.Element == null;

        [Obsolete("Use Preserve")]
        public static void Init()
        {
            GestureTouchSam.Preserve();
            GView.Preserve();
        }

        public static void Preserve()
        {
            Touch.Preserve();
            GestureTouchSam.Preserve();
            GView.Preserve();
        }

        protected override void OnAttached()
        {
            // Gesture touches
            gesture = new GestureTouchSam(this, null, View);
            View.TouchDelegate = gesture;
            View.Clickable = true;
            View.LongClickable = true;
            View.SoundEffectsEnabled = true;

            // Animation tap
            overlayAnimation = new FrameLayout(Container.Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(-1, -1),
                Clickable = false,
                Focusable = false,
            };

            Container.AddView(overlayAnimation);
            Container.LayoutChange += OnViewLayoutChanged;
            overlayAnimation.BringToFront();

            if (IsSdk21)
            {
                tapColor = Touch.GetColor(Element).ToAndroid();
                overlayAnimation.Background = CreateRipple(tapColor);
            }

            // Updates
            UpdateAnimationColor();
            UpdateLongTapCommand();
            UpdateLongTapLatency();
        }

        protected override void OnDetached()
        {
            if (IsDisposed)
                return;

            Container.RemoveView(overlayAnimation);
            Container.LayoutChange -= OnViewLayoutChanged;

            overlayAnimation.Pressed = false;
            overlayAnimation.Foreground = null;
            overlayAnimation.Dispose();

            TimerDispose();

            if (IsSdk21)
                ripple?.Dispose();

            gesture.Dispose();
            gesture = null;
        }

        protected override void OnElementPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(e);

            if (e.PropertyName == Touch.ColorProperty.PropertyName)
            {
                UpdateAnimationColor();
            }
            else if (e.PropertyName == Touch.LongTapProperty.PropertyName)
            {
                UpdateLongTapCommand();
            }
            else if (e.PropertyName == Touch.LongTapLatencyProperty.PropertyName)
            {
                UpdateLongTapLatency();
            }
        }

        private void UpdateAnimationColor()
        {
            var color = Touch.GetColor(Element);
            if (color == Xamarin.Forms.Color.Default)
                return;

            tapColor = color.ToAndroid();
            tapColorAlpha = (tapColor.A == 255) ? (byte)80 : tapColor.A;

            if (IsSdk21)
                ripple.SetColor(GetRippleColorSelector(tapColor));
        }

        private void UpdateLongTapCommand()
        {
            var command = Touch.GetLongTap(Element);
            TimerDispose();

            if (command != null)
            {
                timer = new System.Timers.Timer();
                timer.Elapsed += OnTimerEvent;
                timer.Interval = Touch.GetLongTapLatency(Element);
                timer.AutoReset = false;
            }
        }

        private void UpdateLongTapLatency()
        {
            if (timer == null)
                return;

            timer.Interval = Touch.GetLongTapLatency(Element);
        }

        private void TimerDispose()
        {
            if (timer == null)
                return;

            timer.Elapsed -= OnTimerEvent;
            timer.Stop();
            timer.Close();
            timer.Dispose();
            timer = null;
        }

        private void OnTimerEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            int x = (int)currentX;
            int y = (int)currentY;
            if (IsViewInBounds(View, x, y))
            {
                timer.Stop();
                Xamarin.Forms.Device.BeginInvokeOnMainThread(LongTap);
            }
        }

        private void AnimationStart(float x, float y)
        {
            if (IsSdk21)
                ForceStartRipple(x, y);
            else
                AnimationStartOverlay();
        }

        private void AnimationEnd()
        {
            if (IsSdk21)
                ForceEndRipple();
            else
                TapAnimation(250, tapColorAlpha, 0);
        }

        #region command actions
        private void StartTap()
        {
            var cmd = Touch.GetStartTap(Element);
            var param = Touch.GetStartTapParameter(Element);
            if (cmd == null)
                return;

            if (cmd.CanExecute(param))
                cmd.Execute(param);
        }

        private void FinishTap()
        {
            var cmd = Touch.GetFinishTap(Element);
            var param = Touch.GetFinishTapParameter(Element);
            if (cmd == null)
                return;

            if (cmd.CanExecute(param))
                cmd.Execute(param);
        }

        private void Tap()
        {
            var cmd = Touch.GetTap(Element);
            var param = Touch.GetTapParameter(Element);
            if (cmd == null)
                return;

            if (cmd.CanExecute(param))
                cmd.Execute(param);
        }

        private void LongTap()
        {
            var cmdLong = Touch.GetLongTap(Element);
            var paramLong = Touch.GetLongTapParameter(Element);

            if (cmdLong == null)
            {
                Tap();
                return;
            }

            if (cmdLong.CanExecute(paramLong))
                cmdLong.Execute(paramLong);
        }
        #endregion command actions

        #region Animation ripple
        private RippleDrawable CreateRipple(Color color)
        {
            if (Element is Xamarin.Forms.Layout)
            {
                var mask = new ColorDrawable(Color.White);
                ripple = new RippleDrawable(GetRippleColorSelector(color), null, mask);
                return ripple;
            }

            var back = View.Background;
            if (back == null)
            {
                var mask = new ColorDrawable(Color.White);
                return ripple = new RippleDrawable(GetRippleColorSelector(color), null, mask);
            }

            if (back is RippleDrawable)
            {
                ripple = (RippleDrawable)back.GetConstantState().NewDrawable();
                ripple.SetColor(GetRippleColorSelector(color));

                return ripple;
            }

            return ripple = new RippleDrawable(GetRippleColorSelector(color), back, null);
        }

        private static ColorStateList GetRippleColorSelector(int pressedColor)
        {
            return new ColorStateList
            (
                new int[][] { new int[] { } },
                new int[] { pressedColor, }
            );
        }

        private void ForceStartRipple(float x, float y)
        {
            if (IsDisposed || !(overlayAnimation.Background is RippleDrawable ripple))
                return;

            overlayAnimation.BringToFront();

            if (overlayAnimation.Width > 200 || overlayAnimation.Height > 200)
                ripple.SetHotspot(x, y);

            overlayAnimation.Pressed = true;
        }

        private void ForceEndRipple()
        {
            if (IsDisposed) 
                return;

            overlayAnimation.Pressed = false;
        }

        #endregion Animation ripple

        #region Animation fade
        private void AnimationStartOverlay()
        {
            if (IsDisposed)
                return;

            ClearAnimation();

            overlayAnimation.BringToFront();
            var color = this.tapColor;
            color.A = tapColorAlpha;
            overlayAnimation.SetBackgroundColor(color);
        }

        private void TapAnimation(long duration, byte startAlpha, byte endAlpha)
        {
            if (IsDisposed)
                return;

            overlayAnimation.BringToFront();

            var start = tapColor;
            var end = tapColor;
            start.A = startAlpha;
            end.A = endAlpha;

            ClearAnimation();
            animator = ObjectAnimator.OfObject(overlayAnimation,
                "BackgroundColor",
                new ArgbEvaluator(),
                start.ToArgb(),
                end.ToArgb());
            animator.SetDuration(duration);
            animator.RepeatCount = 0;
            animator.RepeatMode = ValueAnimatorRepeatMode.Restart;
            animator.Start();
            animator.AnimationEnd += OnAnimationEnd;
        }

        private void OnAnimationEnd(object sender, EventArgs eventArgs)
        {
            if (IsDisposed) return;

            ClearAnimation();
        }

        private void ClearAnimation()
        {
            if (animator == null)
                return;
            animator.AnimationEnd -= OnAnimationEnd;
            animator.Cancel();
            animator.Dispose();
            animator = null;
        }
        #endregion Animation fade

        private List<GView> GetNestedTouchs(View view)
        {
            var res = new List<GView>();

            if (view.TouchDelegate is GestureTouchSam g && g != gesture)
                res.Add(new GView(view, g));

            if (view is ViewGroup gview)
            {
                for (int i = 0; i < gview.ChildCount; i++)
                {
                    var child = gview.GetChildAt(i);
                    res.AddRange(GetNestedTouchs(child));
                }
            }

            return res;
        }

        private void OnViewLayoutChanged(object sender, View.LayoutChangeEventArgs layoutChangeEventArgs)
        {
            var group = (ViewGroup)sender;
            if (group == null || IsDisposed)
                return;

            overlayAnimation.Right = group.Width;
            overlayAnimation.Bottom = group.Height;
        }

        private bool IsViewInBounds(View view, int x, int y)
        {
            view.GetDrawingRect(_rect);
            view.GetLocationOnScreen(_location);

            var rect = new Xamarin.Forms.Rectangle(_location[0], _location[1], _rect.Width(), _rect.Height());
            bool res = rect.Contains(x, y);
            return res;
        }

        [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
        internal class GestureTouchSam : TouchDelegate
        {
            private readonly View view;
            private readonly TouchDroid host;
            private readonly float touchSlop;
            private bool isGestureProcessed;
            private GestureTouchSam nestedGesture;

            internal static void Preserve() { }
            internal GestureTouchSam(TouchDroid host, Rect bounds, View delegateView) : base(bounds, delegateView)
            {
                this.view = delegateView;
                this.host = host;
                this.touchSlop = ViewConfiguration.Get(view.Context).ScaledTouchSlop;
            }

            public override bool OnTouchEvent(MotionEvent e)
            {
                float x = e.RawX;
                float y = e.RawY;
                float internalX = e.GetX();
                float internalY = e.GetY();
                host.currentX = x;
                host.currentY = y;

                // Detect nestered gesture
                if (nestedGesture != null)
                {
                    nestedGesture.OnTouchEvent(e);

                    // Detect END for nestered gesture
                    if (e.Action == MotionEventActions.Cancel || e.Action == MotionEventActions.Up)
                        nestedGesture = null;

                    return true;
                }

                // START
                if (e.Action == MotionEventActions.Down)
                {
                    if (!host.IsEnabled || isGestureProcessed)
                        return true;

                    // Detect nested TouchSam gestures
                    var childs = host.GetNestedTouchs(view);
                    foreach (var item in childs)
                    {
                        if (host.IsViewInBounds(item.View, (int)x, (int)y))
                        {
                            nestedGesture = item.Gesture;
                            nestedGesture.OnTouchEvent(e);
                            return true;
                        }
                    }

                    isGestureProcessed = true;
                    host.startX = x;
                    host.startY = y;
                    host.StartTap();
                    host.AnimationStart(internalX, internalY);

                    // Try start long tap timer
                    if (Touch.GetLongTap(host.Element) != null)
                    {
                        host.timer?.Stop();
                        host.timer?.Start();
                    }
                }
                // MOVE
                else if (e.Action == MotionEventActions.Move || e.Action == MotionEventActions.Scroll)
                {
                    if (!isGestureProcessed)
                        return true;

                    float deltaX = Math.Abs(host.startX - x);
                    float deltaY = Math.Abs(host.startY - y);

                    if (deltaX > touchSlop || deltaY > touchSlop || !host.IsEnabled)
                    {
                        host.FinishTap();
                        host.AnimationEnd();
                        host.timer?.Stop();
                        isGestureProcessed = false;
                    }
                }
                // UP
                else if (e.Action == MotionEventActions.Up || e.Action == MotionEventActions.Cancel)
                {
                    if (host.IsDisposed || !isGestureProcessed)
                        return true;

                    isGestureProcessed = false;
                    host.AnimationEnd();
                    host.FinishTap();

                    if (e.Action == MotionEventActions.Up && host.IsEnabled)
                    {
                        // Tap sound :)
                        view.PlaySoundEffect(SoundEffects.Click);

                        if (host.IsViewInBounds(view, (int)e.RawX, (int)e.RawY))
                        {
                            if (Touch.GetLongTap(host.Element) == null)
                                host.Tap();
                            else if (host.timer == null || host.timer.Enabled)
                                host.Tap();
                        }
                    }

                    host.timer?.Stop();
                }

                return true;
            }
        }

        [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
        internal class GView
        {
            internal static void Preserve() { }

            internal View View;
            internal GestureTouchSam Gesture;

            internal GView(View view, GestureTouchSam gesture)
            {
                View = view;
                Gesture = gesture;
            }
        }
    }
}