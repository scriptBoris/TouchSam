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
        public UIElement View => Control ?? Container;
        public bool IsDisposed => (Container as IVisualElementRenderer)?.Element == null;

        public static void Init() { }

        protected override void OnAttached()
        {
            if (View != null)
            {
                View.Tapped += OnTapped;
                View.RightTapped += OnRightTapped;
            }
        }

        protected override void OnDetached()
        {
            if (IsDisposed)
                return;

            if (View != null)
            {
                View.Tapped -= OnTapped;
                View.RightTapped -= OnRightTapped;
            }
        }

        //private Windows.UI.Color color;
        private async void Tap()
        {
            if (Element is ContentView cont)
            {
                //var c = Touch.GetColor(Element);
                //if (c == Color.Default)
                //    return;

                //color = c.ToWindowsColor();

                await cont.RelScaleTo(0.2, 40, Easing.SinIn);
                await cont.RelScaleTo(-0.2, 40, Easing.SinOut);
            }
        }

        private void OnTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Tap();

            var cmd = Touch.GetTap(Element);
            var param = Touch.GetTapParameter(Element);
            if (cmd?.CanExecute(param) ?? false)
                cmd.Execute(param);
        }

        private void OnRightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            Tap();

            var cmd = Touch.GetLongTap(Element);
            var param = Touch.GetLongTapParameter(Element);
            if (cmd?.CanExecute(param) ?? false)
                cmd.Execute(param);
        }
    }
}
