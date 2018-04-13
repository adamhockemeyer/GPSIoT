using System;
using System.Collections.Generic;
using GPSIoT.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace GPSIoT
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();



            MainPage = new MainPage();
        }
    }
}
