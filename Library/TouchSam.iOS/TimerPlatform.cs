using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UIKit;
using Xamarin.Forms;

namespace TouchSam.iOS
{
    public class TimerPlatform
    {
        private readonly Action callback;
        private CancellationTokenSource cancellation;

        public bool IsEnabled { get; private set; }

        public static void Init() { }

        public TimerPlatform(Action callback)
        {
            this.callback = callback;
        }

        public void Start(double time)
        {
            var timeSpan = TimeSpan.FromMilliseconds(time);
            cancellation = new CancellationTokenSource();
            IsEnabled = true;

            Device.StartTimer(timeSpan,
                () => 
                {
                    IsEnabled = false;

                    if (cancellation.IsCancellationRequested)
                        return false;

                    callback.Invoke();
                    return false;
                });
        }

        public void Stop()
        {
            IsEnabled = false;
            Interlocked.Exchange(ref cancellation, new CancellationTokenSource()).Cancel();
        }
    }
}