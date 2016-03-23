using Microsoft.Band;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BandSensorDemo
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            textBlock.Text = string.Empty;

            // Get a list of all available bands
            IBandInfo[] bands = await BandClientManager.Instance.GetBandsAsync();

            if (bands.Length < 1)
            {
                textBlock.Text = "Band was not found";
                return;
            }

            // Connect to first band
            using (IBandClient bandClient = await BandClientManager.Instance.ConnectAsync(bands[0]))
            {
                // Send vibration
                await bandClient.NotificationManager.VibrateAsync(Microsoft.Band.Notifications.VibrationType.RampUp);

                // Check for sensor support
                if (!bandClient.SensorManager.SkinTemperature.IsSupported)
                {
                    textBlock.Text = "Skin temperature sensor is not supported";
                    return;
                }

                // Subscribe to sensor
                bandClient.SensorManager.SkinTemperature.ReadingChanged += async (s, args) =>
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                        CoreDispatcherPriority.Normal, () =>
                        {
                            textBlock.Text = string.Format("Temperature: {0} °C", args.SensorReading.Temperature);
                        });
                };

                // Starts the sensor reading
                await bandClient.SensorManager.SkinTemperature.StartReadingsAsync();

                // Receive data for a while, then stop the subscription
                await Task.Delay(TimeSpan.FromSeconds(5));
                await bandClient.SensorManager.AmbientLight.StopReadingsAsync();
            }
        }
    }
}