﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace TouchSam.Droid
{
    [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
    public static class Initialize
    {
        [Obsolete("Use preserve method, platform dependent. " +
            "Example: TouchSam.Droid.TouchDroid.Preserve")]
        public static void Init()
        {
            Touch.Init();
            TouchDroid.Init();
        }
    }
}