using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

namespace GPSIoT.Helpers
{
    public class DeviceData
    {
        public DeviceData(string myName)
        {
            this.Name = myName;
        }

        public string Name
        {
            get; set;
        }
    }

    public class IoTClient
    {
        // Found in Azure on the IoT Hub --> Settings --> Shared Access policies, Select device, then Copy the connection string.
        private readonly string DeviceConnectionString = "HostName=IoT-ahock.azure-devices.net;SharedAccessKeyName=device;SharedAccessKey=HKjjXQuoDar3pjl1AbhoCdr6Fkg2osdV4eSIfO1j/bU=";

        DeviceClient deviceClient;
        public TransportType Protocol { get; private set; }

        Action<object> callMeLogger;
        Action<object> getDeviceNameLogger;
        Action<object> errorHandler;

        public IoTClient(TransportType protocol, Action<object> callMeLogger, Action<object> getDeviceNameLogger, Action<object> errorHandler)
        {
            this.Protocol = protocol;
            this.callMeLogger = callMeLogger;
            this.getDeviceNameLogger = getDeviceNameLogger;
            this.errorHandler = errorHandler;
        }

        public async Task Start()
        {
            try
            {
                this.deviceClient = DeviceClient.CreateFromConnectionString(DeviceConnectionString, Plugin.DeviceInfo.CrossDeviceInfo.Current.Id, this.Protocol);

                await this.deviceClient.OpenAsync();

                // Set up callbacks:
                if (this.Protocol == TransportType.Mqtt || this.Protocol == TransportType.Mqtt_Tcp_Only || this.Protocol == TransportType.Mqtt_WebSocket_Only)
                {
                    await deviceClient.SetMethodHandlerAsync("microsoft.management.immediateReboot", ImmediateReboot, null);
                    await deviceClient.SetMethodHandlerAsync("GetDeviceName", GetDeviceName, new DeviceData("Some Device"));
                }

                Debug.WriteLine("Exited!\n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in sample: {1} {0}", ex.Message, ex.GetType().ToString());
            }
        }

        public Task CloseAsync()
        {
            return this.deviceClient.CloseAsync();
        }

        Task<MethodResponse> ImmediateReboot(MethodRequest methodRequest, object userContext)
        {
            this.callMeLogger(methodRequest.DataAsJson);

            return Task.FromResult(new MethodResponse(new byte[0], 200));
        }

        Task<MethodResponse> GetDeviceName(MethodRequest methodRequest, object userContext)
        {
            MethodResponse retValue;
            if (userContext == null)
            {
                retValue = new MethodResponse(new byte[0], 500);
            }
            else
            {
                var d = userContext as DeviceData;
                string result = "{\"name\":\"" + d.Name + "\"}";
                retValue = new MethodResponse(Encoding.UTF8.GetBytes(result), 200);
            }

            this.getDeviceNameLogger(methodRequest.DataAsJson);

            return Task.FromResult(retValue);
        }

        //public async Task UploadFile(Windows.Storage.StorageFile file)
        //{
        //    var fileName = file.Name;
        //    using (Windows.Storage.Streams.IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
        //        await deviceClient.UploadToBlobAsync(fileName, stream.AsStream());
        //}

        public Task SendEvent(string message)
        {
            var dataBuffer = string.Format("Msg from App: '{0}'. Sent at: {1}. Protocol used: {2}.", message, DateTime.Now.ToLocalTime(), Protocol);
            Message eventMessage = new Message(Encoding.UTF8.GetBytes(dataBuffer));
            Debug.WriteLine(string.Format("Sending message: '{0}'", dataBuffer));
            return deviceClient.SendEventAsync(eventMessage);
        }

        public async Task<string> ReceiveCommand()
        {
            Debug.WriteLine("\nDevice waiting for commands from IoTHub...\n");
            Message receivedMessage;
            string messageData;

            while (true)
            {
                receivedMessage = await this.deviceClient.ReceiveAsync();

                if (receivedMessage != null)
                {
                    messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                    Debug.WriteLine("\t{0}> Received message: {1}", DateTime.Now.ToLocalTime(), messageData);

                    await deviceClient.CompleteAsync(receivedMessage);

                    return messageData;
                }

                //  Note: In this sample, the polling interval is set to 
                //  10 seconds to enable you to see messages as they are sent.
                //  To enable an IoT solution to scale, you should extend this //  interval. For example, to scale to 1 million devices, set 
                //  the polling interval to 25 minutes.
                //  For further information, see
                //  https://azure.microsoft.com/documentation/articles/iot-hub-devguide/#messaging
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }
    }
}
