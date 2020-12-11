using System;
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
    public class TouchUWP : PlatformEffect
    {
        private bool isPressed;

        public UIElement View => Control ?? Container;
        public bool IsDisposed => (Container as IVisualElementRenderer)?.Element == null;
        private const uint animationTime = 50;

        public static void Init() { }

        protected override void OnAttached()
        {
            if (View != null)
            {
                View.PointerPressed += OnPointerPressed;
                View.PointerReleased += OnPointerReleased;
                View.PointerExited += OnPointerExited;
            }
        }

        protected override void OnDetached()
        {
            if (IsDisposed)
                return;

            if (View != null)
            {
                View.PointerPressed -= OnPointerPressed;
                View.PointerReleased -= OnPointerReleased;
                View.PointerExited -= OnPointerExited;
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
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                var p = e.GetCurrentPoint((UIElement)sender);
                if (!p.Properties.IsLeftButtonPressed)
                    return;
            }

            StartAnimationTap();

            var cmd = Touch.GetStartTap(Element);
            var param = Touch.GetStartTapParameter(Element);
            if (cmd?.CanExecute(param) ?? false)
                cmd.Execute(param);
        }

        private void OnPointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            EndAnimationTap();
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                var p = e.GetCurrentPoint((UIElement)sender);
                if (p.Properties.PointerUpdateKind == Windows.UI.Input.PointerUpdateKind.LeftButtonReleased)
                {
                    OnTapped();
                }
                else if (p.Properties.PointerUpdateKind == Windows.UI.Input.PointerUpdateKind.RightButtonReleased)
                {
                    OnRightTapped();
                }
            }
        }

        private void OnPointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (isPressed)
                EndAnimationTap();
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
