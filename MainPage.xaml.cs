using Microsoft.Maui.Controls;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AverOBus
{
    public partial class MainPage : ContentPage
    {
        private string busNumber;
        private string busTime;
        private string phpApiUrl = "https://yourdomain.com/api/bus_status.php"; // Replace with your PHP API URL

        public MainPage()
        {
            InitializeComponent();
            FetchNextBusAsync();
        }

      

        private async void FetchNextBusAsync()
        {
            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();
                if (location == null)
                {
                    location = await Geolocation.GetLocationAsync(new GeolocationRequest
                    {
                        DesiredAccuracy = GeolocationAccuracy.Medium,
                        Timeout = TimeSpan.FromSeconds(30)
                    });
                }

                if (location != null)
                {
                    // Call your web service to get the next bus number and time
                    using var httpClient = new HttpClient();
                    var response = await httpClient.GetStringAsync($"https://yourdomain.com/api/nextbus.php?lat={location.Latitude}&lng={location.Longitude}");
                    // For demonstration purposes, let's assume the response is in "BusNumber:Time" format.
                    var busInfo = response.Split(':');
                    busNumber = busInfo[0];
                    busTime = busInfo[1];

                    BusInfoLabel.Text = $"Bus {busNumber} at {busTime}";
                }
            }
            catch (Exception ex)
            {
                BusInfoLabel.Text = $"Error fetching bus info: {ex.Message}";
            }
        }

        private async void OnButtonClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var status = button.Text; // Will be "On Time", "Delayed", "Full", or "OK"
            var location = await Geolocation.GetLastKnownLocationAsync();

            if (location != null && !string.IsNullOrEmpty(busNumber) && !string.IsNullOrEmpty(busTime))
            {
                using var httpClient = new HttpClient();
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("busNumber", busNumber),
                    new KeyValuePair<string, string>("status", status),
                    new KeyValuePair<string, string>("lat", location.Latitude.ToString()),
                    new KeyValuePair<string, string>("lng", location.Longitude.ToString()),
                    new KeyValuePair<string, string>("time", DateTime.UtcNow.ToString("o")),
                });

                await httpClient.PostAsync(phpApiUrl, content);
            }
        }
    }
}
