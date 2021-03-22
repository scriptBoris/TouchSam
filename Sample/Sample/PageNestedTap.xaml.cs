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
    public partial class PageNestedTap : ContentPage
    {
        public PageNestedTap()
        {
            CommandActions = new Command(Actions);
            CommandTap = new Command(Tap);
            InitializeComponent();
            BindingContext = this;
        }

        public ICommand CommandTap { get; set; }
        public ICommand CommandActions { get; set; }

        private async void Actions()
        {
            var display = await DisplayActionSheet("Actions", null, null, new string[]
            {
                "Open from ...",
                "Share",
                "Edit",
                "Delete",
                "Cancel",
            });
        }

        private async void Tap()
        {
            await DisplayAlert("Success", "You taped!", "OK");
        }
    }
}