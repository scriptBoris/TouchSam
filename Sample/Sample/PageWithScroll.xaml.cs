using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sample
{
    public partial class PageWithScroll : ContentPage
    {
        public PageWithScroll()
        {
            InitializeComponent();
            CommandTap = new Command(Tap);
            CommandLongTap = new Command(LongTap);
            Items = new ObservableCollection<User>
            {
                new User
                {
                    BirthDate = new DateTime(1992, 7, 28),
                    FirstName = "Diana",
                    LastName = "Roseborough",
                    PhotoUrl = "https://randomuser.me/api/portraits/women/19.jpg",
                    Rank = Ranks.OfficePlankton,
                },
                new User
                {
                    BirthDate = new DateTime(1989, 9, 29),
                    FirstName = "Carmen",
                    LastName = "Speights",
                    PhotoUrl = "https://randomuser.me/api/portraits/men/85.jpg",
                    Rank = Ranks.OfficePlankton,
                },
                new User
                {
                    BirthDate = new DateTime(1991, 2, 20),
                    FirstName = "Boris",
                    LastName = "Krit",
                    PhotoUrl = "https://randomuser.me/api/portraits/men/72.jpg",
                    Rank = Ranks.OfficePlankton,
                },
                new User
                {
                    BirthDate = new DateTime(1979, 1, 12),
                    FirstName = "Anna",
                    LastName = "Abraham",
                    PhotoUrl = "https://randomuser.me/api/portraits/women/85.jpg",
                    Rank = Ranks.Manager,
                },
                new User
                {
                    BirthDate = new DateTime(1996, 12, 13),
                    FirstName = "Sam",
                    LastName = "Super",
                    PhotoUrl = "https://randomuser.me/api/portraits/men/55.jpg",
                    Rank = Ranks.Admin,
                },
                new User
                {
                    BirthDate = new DateTime(1996, 8, 8),
                    FirstName = "Tommy",
                    LastName = "Mcsherry",
                    PhotoUrl = "https://randomuser.me/api/portraits/men/46.jpg",
                    Rank = Ranks.Manager,
                },
                new User
                {
                    BirthDate = new DateTime(2001, 1, 27),
                    FirstName = "Candie",
                    LastName = "Hopping",
                    PhotoUrl = "https://randomuser.me/api/portraits/women/26.jpg",
                    Rank = Ranks.OfficePlankton,
                },
                new User
                {
                    BirthDate = new DateTime(1985, 10, 3),
                    FirstName = "Vincent",
                    LastName = "Ruvalcaba",
                    PhotoUrl = "https://randomuser.me/api/portraits/men/15.jpg",
                    Rank = Ranks.OfficePlankton,
                },
                new User
                {
                    BirthDate = new DateTime(1988, 1, 13),
                    FirstName = "Jeffry",
                    LastName = "Wehner",
                    PhotoUrl = "https://randomuser.me/api/portraits/men/64.jpg",
                    Rank = Ranks.OfficePlankton,
                },
            };
            BindingContext = this;
        }

        public ICommand CommandTap { get; set; }
        public ICommand CommandLongTap { get; set; }
        public ObservableCollection<User> Items { get; set; } = new ObservableCollection<User>();

        private void Tap(object param)
        {
            if (param is User user)
            {
                DisplayAlert("User info",

                    $"Name: {user.FirstName} {user.LastName}\n" +
                    $"DateBirth: {user.BirthDate}\n" + 
                    $"Rank: {user.Rank}",

                    "OK");
            }
        }

        private async void LongTap(object param)
        {
            if (param is User user)
            {
                string res = await DisplayActionSheet("Actions", null, null, new string[]
                {
                    "Open profile",
                    "Delete",
                    "Change rank",
                    "Cancel",
                });

                if (res == "Delete")
                {
                    Items.Remove(user);
                }
            }
        }
    }

    public class User : INotifyPropertyChanged
    {
        public DateTime BirthDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Ranks Rank { get; set; }


        // Stupid Xamarin forms
        // UI element "Image" no load image from string (http path)
        private string _photoUrl;
        public string PhotoUrl {
            get => _photoUrl;
            set {
                _photoUrl = value;
                DownloadImage(value);
            }
        }

        private async void DownloadImage(string url)
        {
            var client = new WebClient();
            var byteArray = await client.DownloadDataTaskAsync(url);
            var res = ImageSource.FromStream(() => new MemoryStream(byteArray));
            client.Dispose();
            Image = res;
        }

        public ImageSource Image { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

    }

    public enum Ranks
    {
        OfficePlankton,
        Manager,
        Admin,
    }
}