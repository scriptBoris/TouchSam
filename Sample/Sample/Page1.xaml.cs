using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sample
{
    public partial class Page1 : ContentPage
    {
        public Page1()
        {
            InitializeComponent();
            CommandTap = new Command(() =>
            {
                DisplayAlert("Success", "Tap", "OK");
            });

            CommandLongTap = new Command(() =>
            {
                DisplayAlert("Success", "Long tap", "OK");
            });
            BindingContext = this;
        }

        public ICommand CommandTap { get; set; }
        public ICommand CommandLongTap { get; set; }
    }
}