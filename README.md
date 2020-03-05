# TouchSam for Xamarin.Forms

## Nuget Installation
[https://www.nuget.org/packages/TouchSam/](https://www.nuget.org/packages/TouchSam/)


```bash
Install-Package TouchSam
```


## Supported Platforms
 - Android
 - iOS (сurrently not available)
 - UWP
 
 
 ## Install android project
```c#
public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
{
     protected override void OnCreate(Bundle savedInstanceState)
     {
          TabLayoutResource = Resource.Layout.Tabbar;
          ToolbarResource = Resource.Layout.Toolbar;
          base.OnCreate(savedInstanceState);

          Xamarin.Forms.Forms.Init(this, savedInstanceState);
          TouchSam.Droid.Initialize.Init();
          LoadApplication(new App());
     }
}
```


## Install UWP project
In file **App.xaml.cs**, enter `DataGridSam.UWP.Initialize.Init();` as below
```c#
protected override void OnLaunched(LaunchActivatedEventArgs e)
{
     Frame rootFrame = Window.Current.Content as Frame;
     if (rootFrame == null)
     {
          rootFrame = new Frame();
          rootFrame.NavigationFailed += OnNavigationFailed;
          Xamarin.Forms.Forms.Init(e);
          TouchSam.UWP.Initialize.Init();

          if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
          {
          }

          Window.Current.Content = rootFrame;
     }
     ...
}
```


## Example
```xml
<ContentPage xmlns="http://xamarin.com/schemas/2014/forms"
	     xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	     xmlns:touch="clr-namespace:TouchSam;assembly=TouchSam"
	     x:Class="Sample.Views.TestView">
    <StackLayout touch:Touch.Color="Accent"
                 touch:Touch.Tap="{Binding CommandTap}"
                 touch:Touch.LongTap="{Binding CommandLongTap}"
                 touch:Touch.LongTapLatency="200"
                 BackgroundColor="Beige"
                 Padding="15">
        <Label Text="Click me!"/>
    </StackLayout>
</ContentPage>
```
