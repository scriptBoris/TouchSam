using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TouchSam.iOS
{
    [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
    public static class Initialize
    {
        public static void Init()
        {
            Touch.Init();
            TouchIOS.Init();
            TimerPlatform.Init();
        }
    }
}
