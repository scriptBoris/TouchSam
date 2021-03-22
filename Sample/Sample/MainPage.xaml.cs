using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Sample
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            CommandStartTap = new Command(() =>
            {
                CountStartTap++;
            });

            CommandFinishTap = new Command(() =>
            {
                CountFinishTap++;
            });

            CommandTap = new Command(() =>
            {
                CountTap++;
                //DisplayAlert("Success", "Tap", "OK");
            });

            CommandLongTap = new Command(() =>
            {
                CountLongTap++;
                //DisplayAlert("Success", "Long tap", "OK");
            });

            CommandWithScroll = new Command(() =>
            {
                Navigation.PushAsync(new PageWithScroll());
            });

            CommandNestedTaps = new Command(() =>
            {
                Navigation.PushAsync(new PageNestedTap());
            });

            CommandChangeEnabled = new Command(() =>
            {
                IsEnabledTouch = !IsEnabledTouch;
            });

            BindingContext = this;
        }

        public int CountStartTap { get; set; }
        public int CountFinishTap { get; set; }
        public int CountTap { get; set; }
        public int CountLongTap { get; set; }
        public bool IsEnabledTouch { get; set; } = true;
        public ICommand CommandChangeEnabled { get; set; }
        public ICommand CommandWithScroll { get; set; }
        public ICommand CommandNestedTaps { get; set; }
        public ICommand CommandStartTap { get; set; }
        public ICommand CommandFinishTap { get; set; }
        public ICommand CommandTap { get; set; }
        public ICommand CommandLongTap { get; set; }
    }
}
