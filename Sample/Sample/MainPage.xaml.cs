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

            CommandNextPage = new Command(() =>
            {
                Navigation.PushAsync(new Page1());
            });

            CommandChangeEnabled = new Command(() =>
            {
                IsEnabledTouch = !IsEnabledTouch;
            });

            BindingContext = this;
        }

        public int CountTap { get; set; }
        public int CountLongTap { get; set; }
        public bool IsEnabledTouch { get; set; } = true;
        public ICommand CommandChangeEnabled { get; set; }
        public ICommand CommandNextPage { get; set; }
        public ICommand CommandTap { get; set; }
        public ICommand CommandLongTap { get; set; }
    }
}
