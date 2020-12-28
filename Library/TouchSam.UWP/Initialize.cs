using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSam.UWP
{
    [Xamarin.Forms.Internals.Preserve(AllMembers = true)]
    public static class Initialize
    {
        [Obsolete("Use preserve method, platform dependent. " +
            "Example: TouchSam.UWP.TouchUWP.Preserve")]
        public static void Init()
        {
            Touch.Init();
            TouchUWP.Init();
        }
    }
}
