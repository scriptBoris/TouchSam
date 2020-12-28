using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TouchSam.iOS
{
    [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
    public static class Initialize
    {
        [Obsolete("Use preserve method, platform dependent. " +
            "Example: TouchSam.iOS.TouchIOS.Preserve")]
        public static void Init()
        {
            Touch.Init();
            TouchIOS.Init();
            TimerPlatform.Init();
            TouchUITapGestureRecognizerDelegate.Preserve();
        }
    }
}
