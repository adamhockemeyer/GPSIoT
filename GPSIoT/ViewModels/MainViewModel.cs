using System;
using Xamarin.Forms;

using Plugin.Geolocator.Abstractions;

using Microsoft.Azure.Devices.Client;
using GPSIoT.Helpers;
using System.Threading.Tasks;

namespace GPSIoT.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        bool _isLocationSupported;
        public bool IsLocationSupported
        {
            get => _isLocationSupported;
            set => SetProperty(ref _isLocationSupported, value);
        }

        bool _isListening;
        public bool IsListening
        {
            get => _isListening;
            set => SetProperty(ref _isListening, value);
        }

        bool _isSendingToIoTHub;
        public bool IsSendingToIoTHub
        {
            get => _isSendingToIoTHub;
            set => SetProperty(ref _isSendingToIoTHub, value);
        }

        double _currentLat;
        public double CurrentLat
        {
            get => _currentLat;
            set => SetProperty(ref _currentLat, value);
        }

        double _currentLong;
        public double CurrentLong
        {
            get => _currentLong;
            set => SetProperty(ref _currentLong, value);
        }

        double _currentHeading;
        public double CurrentHeading
        {
            get => _currentHeading;
            set => SetProperty(ref _currentHeading, value);
        }

        double _currentSpeed;
        public double CurrentSpeed
        {
            get => _currentSpeed;
            set => SetProperty(ref _currentSpeed, value);
        }

        double _currentAccuracy;
        public double CurrentAccuracy
        {
            get => _currentAccuracy;
            set => SetProperty(ref _currentAccuracy, value);
        }

        Command _toggleListeningCommand;
        public Command ToggleListeningCommand
        {
            get => _toggleListeningCommand;
            set => SetProperty(ref _toggleListeningCommand, value);
        }

        Command _toggleSendToIoTHubCommand;
        public Command ToggleSendToIoTHubCommand
        {
            get => _toggleSendToIoTHubCommand;
            set => SetProperty(ref _toggleSendToIoTHubCommand, value);
        }

        Command _getPositionCommand;
        public Command GetPositionCommand
        {
            get => _getPositionCommand;
            set => SetProperty(ref _getPositionCommand, value);
        }

        public event Action<Position> PositionChanged;

        IoTClient client;

        public MainViewModel()
        {
            client = new IoTClient(TransportType.Amqp,
                                   (call) => { System.Diagnostics.Debug.WriteLine($"Call: {call}"); }, 
                                   (deviceName) => { System.Diagnostics.Debug.WriteLine($"Device Name: {deviceName}"); }, 
                                   (data) => { System.Diagnostics.Debug.WriteLine($"Data: {data}"); });

            UpdateStatus();

            InitCommands();

            GetPositionCommand?.Execute(null);
        }

		public override void SubscribeEvents()
		{
            base.SubscribeEvents();

            Plugin.Geolocator.CrossGeolocator.Current.PositionChanged += Current_PositionChanged;

            Task.Run(async () =>
            {
                await client.Start();
            });
		}

		public override void UnsubscribeEvents()
		{
            base.UnsubscribeEvents();

            Plugin.Geolocator.CrossGeolocator.Current.PositionChanged -= Current_PositionChanged;

            Task.Run(async () =>
            {
                await client.CloseAsync();
            });
		}

		void InitCommands()
        {
            ToggleListeningCommand = new Command(async () =>
            {
                if(!Plugin.Geolocator.CrossGeolocator.IsSupported)
                {
                    Acr.UserDialogs.UserDialogs.Instance.Toast("Geolocation is not supported.", TimeSpan.FromSeconds(3));
                    return;
                }

                if(Plugin.Geolocator.CrossGeolocator.Current.IsListening)
                {
                    await Plugin.Geolocator.CrossGeolocator.Current.StopListeningAsync();

                    UpdateStatus();
                }
                else
                {
                    double minutes = 5;
                    double minDistance = 0.1;

                    await Plugin.Geolocator.CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromMinutes(minutes), minDistance, true);

                    UpdateStatus();

                    Acr.UserDialogs.UserDialogs.Instance.Toast($"Listening to location changes for {minutes} minutes.", TimeSpan.FromSeconds(3));
                }
            });

            ToggleSendToIoTHubCommand = new Command(async () =>
            {
                IsSendingToIoTHub = !IsSendingToIoTHub;
            });


            GetPositionCommand = new Command(async () =>
            {
                Position position = await Plugin.Geolocator.CrossGeolocator.Current.GetPositionAsync(TimeSpan.FromSeconds(5), includeHeading: true);

                SetLocationInfo(position);
            });
        }

        void UpdateStatus()
        {
            IsLocationSupported = Plugin.Geolocator.CrossGeolocator.IsSupported;

            IsListening = Plugin.Geolocator.CrossGeolocator.Current.IsListening;

        }

        void SetLocationInfo(Position position)
        {
            CurrentLat = position.Latitude;
            CurrentLong = position.Longitude;
            CurrentHeading = position.Heading;
            CurrentSpeed = position.Speed;
            CurrentAccuracy = position.Accuracy;

            PositionChanged?.Invoke(position);

            if(IsSendingToIoTHub)
            {
                SendToIoTHub(position);
            }

        }

        void SendToIoTHub(Position position)
        {
            try
            {
                Task.Run(async () =>
                {
                   await client.SendEvent(Newtonsoft.Json.JsonConvert.SerializeObject(position));
                });

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to send position to IoT Hub. {ex.Message}");
            }
        }

        void Current_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Updating position info. {e.Position.Timestamp}");

            SetLocationInfo(e.Position);
        }

    }
}
