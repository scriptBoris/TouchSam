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
        private List<CancellationTokenSource> cancels = new List<CancellationTokenSource>();

        public bool IsEnabled { get; private set; }

        public static void Init() { }

        public TimerPlatform(Action callback)
        {
            this.callback = callback;
        }

        public void Start(double time)
        {
            foreach (var cancelItem in cancels)
                cancelItem.Cancel();

            var timeSpan = TimeSpan.FromMilliseconds(time);
            var cancel = new CancellationTokenSource();
            cancels.Add(cancel);
            IsEnabled = true;

            Device.StartTimer(timeSpan,
                () => 
                {
                    IsEnabled = false;

                    if (!cancel.IsCancellationRequested)
                    {
                        callback.Invoke();
                    }
                    cancels.Remove(cancel);
                    return false;
                });
        }

        public void Stop()
        {
            IsEnabled = false;
            foreach (var cancelItem in cancels)
                cancelItem.Cancel();

            cancels.Clear();

            //Interlocked.Exchange(ref cancellation, new CancellationTokenSource()).Cancel();
        }
    }
}