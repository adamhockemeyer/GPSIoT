using System;
using System.Collections.Generic;
using GPSIoT.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GPSIoT.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainViewModel ViewModel { get; private set; }

        public MainPage()
        {
            InitializeComponent();

            BindingContext = ViewModel = ViewModel ?? new MainViewModel();


            // Helps dealing with the 'notch' on iPhone X
            On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);

        }

		protected override void OnAppearing()
		{
            base.OnAppearing();

            ViewModel.SubscribeEvents();

            ViewModel.PositionChanged += ViewModel_PositionChanged;
		}

		protected override void OnDisappearing()
		{
            base.OnDisappearing();

            ViewModel.UnsubscribeEvents();

            ViewModel.PositionChanged -= ViewModel_PositionChanged;
		}

        void ViewModel_PositionChanged(Plugin.Geolocator.Abstractions.Position pos)
        {

            MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(pos.Latitude, pos.Longitude), Distance.FromMiles(0.25)));

        }

	}
}
