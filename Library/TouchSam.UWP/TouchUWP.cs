using System;
using System.ComponentModel;
using System.Threading.Tasks;
using TouchSam;
using TouchSam.UWP;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ResolutionGroupName("TouchSam")]
[assembly: ExportEffect(typeof(TouchUWP), nameof(Touch))]
namespace TouchSam.UWP
{
    [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
    public class TouchUWP : PlatformEffect
    {
        private const uint animationTime = 50;
        private bool isPressed;
        private bool isCanTouch;

        public UIElement View => Control ?? Container;
        public bool IsDisposed => (Container as IVisualElementRenderer)?.Element == null;

        [Obsolete("Use Preserve")]
        public static void Init() { }

        public static void Preserve()
        {
            Touch.Preserve();
        }

        protected override void OnAttached()
        {
            if (View != null)
            {
                View.PointerPressed += OnPointerPressed;
                View.PointerReleased += OnPointerReleased;
                View.PointerExited += OnPointerExited;
                isCanTouch = Touch.GetIsEnabled(Element);
            }
        }

        protected override void OnDetached()
        {
            isCanTouch = false;
            if (IsDisposed)
                return;

            if (View != null)
            {
                View.PointerPressed -= OnPointerPressed;
                View.PointerReleased -= OnPointerReleased;
                View.PointerExited -= OnPointerExited;
            }
        }

        protected override void OnElementPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(e);

            if (e.PropertyName == Touch.IsEnabledProperty.PropertyName)
            {
                isCanTouch = Touch.GetIsEnabled(Element);

                if (!isCanTouch && isPressed)
                    EndAnimationTap();
            }
        }

        private async void StartAnimationTap()
        {
            isPressed = true;
            if (Element is VisualElement cont)
            {
                await cont.ScaleTo(0.95, animationTime, Easing.SinIn);
            }
        }

        private async void EndAnimationTap()
        {
            isPressed = false;
            if (Element is VisualElement cont)
            {
                await cont.ScaleTo(1, animationTime, Easing.SinOut);
            }
        }

        private void OnPointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (!isCanTouch)
                return;

            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                var p = e.GetCurrentPoint((UIElement)sender);
                if ( !(p.Properties.IsLeftButtonPressed || p.Properties.IsRightButtonPressed) )
                {
                    return;
                }
            }

            StartAnimationTap();
            OnStartTapped();
        }

        private void OnPointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (!isCanTouch)
                return;

            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                var p = e.GetCurrentPoint((UIElement)sender);
                if (!(p.Properties.PointerUpdateKind == Windows.UI.Input.PointerUpdateKind.LeftButtonReleased ||
                    p.Properties.PointerUpdateKind == Windows.UI.Input.PointerUpdateKind.RightButtonReleased))
                    return;

                if (p.Properties.PointerUpdateKind == Windows.UI.Input.PointerUpdateKind.LeftButtonReleased)
                {
                    OnTapped();
                }
                else if (p.Properties.PointerUpdateKind == Windows.UI.Input.PointerUpdateKind.RightButtonReleased)
                {
                    OnRightTapped();
                }
            }

            EndAnimationTap();
            OnFinishTapped();
        }

        private void OnPointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (isPressed)
                EndAnimationTap();
        }

        private void OnStartTapped()
        {
            var cmd = Touch.GetStartTap(Element);
            var param = Touch.GetStartTapParameter(Element);
            if (cmd?.CanExecute(param) ?? false)
                cmd.Execute(param);
        }

        private void OnFinishTapped()
        {
            var cmd = Touch.GetFinishTap(Element);
            var param = Touch.GetFinishTapParameter(Element);
            if (cmd?.CanExecute(param) ?? false)
                cmd.Execute(param);
        }

        private void OnTapped()
        {
            var cmd = Touch.GetTap(Element);
            var param = Touch.GetTapParameter(Element);
            if (cmd?.CanExecute(param) ?? false)
                cmd.Execute(param);
        }

        private void OnRightTapped()
        {
            var cmd = Touch.GetLongTap(Element);
            var param = Touch.GetLongTapParameter(Element);
            if (cmd?.CanExecute(param) ?? false)
                cmd.Execute(param);
        }
    }
}
