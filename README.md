# GPSIoT

This is a very basic Xamarin.Forms app that will use your location (or the mock simulator location) and allow you to toggle sending position data to an Azure IoT hub.

This solution utilizes the following technologies:

* Xamarin.Forms (With the following Nuget Packages)
  * Xamarin.Forms.Maps
  * Xam.Plugin.DeviceInfo
  * Xam.Plugin.Geolocator
  * Microsoft.Azure.Devices.Client
* Azure IoT Hub
* Azure Storage - Blob (Optional

*Note: To use this with your own IoT hub, update GPSIoT --> Helpers --> IoTClient.cs --> 'DeviceConnectionString' to your IoT hub device connection string.  To find your IoT hub connection string, go to the IoT Hub that you created, under 'Settings' click 'Shared access polocies', click on the 'device' policy, and then copy the connection string from the panel that appears.*

### Example:

![Example of iOS App](Demo.gif)



Example output from the IoT hub that was then sent to blob storage can be viewed at: [Example output data](example-iot-data.txt)


```json
connectionDeviceGenerationId$636590890761524060
enqueuedTime82018-04-13T16:18:26.3940000ZMsg from App: 
'{
"Timestamp":"2018-04-13T16:18:26.3242496+00:00",
"Latitude":37.33444843,
"Longitude":-122.04226334,
"Altitude":0.0,
"Accuracy":5.0,
"AltitudeAccuracy":0.0,
"Heading":0.0,
"Speed":33.61
}'. 
Sent at: 4/13/2018 9:18:26 AM. Protocol used: Amqp.
```


To get your IoT hub data into an Azure blob storage, Add a custom endpoint in your IoT hub, and connect that to your storage account.  Then add a Route in your IoT hub to use your storage account to write the IoT hub data to.  Alternatively you can just use the IoT hub event-hub compatible endpoint to hook into the the data received by IoT hub.
