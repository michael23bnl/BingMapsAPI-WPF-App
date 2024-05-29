using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WpfAppERW.Services
{
    public class LocationService {
        public static async Task GetLocationAsync(Map myMap) {
            using (var httpClient = new HttpClient()) {
                var response = await httpClient.GetAsync("https://ipinfo.io/json");
                var content = await response.Content.ReadAsStringAsync();

                // обработка JSON ответа и извлечение координат местоположения
                var jsonDoc = JsonDocument.Parse(content);
                var root = jsonDoc.RootElement;

                string latitude = root.GetProperty("loc").GetString()?.Split(',')[0];
                string longitude = root.GetProperty("loc").GetString()?.Split(',')[1];

                Console.WriteLine($"Latitude: {latitude}, Longitude: {longitude}");
                Location point = new Location(Convert.ToDouble(latitude, CultureInfo.InvariantCulture), Convert.ToDouble(longitude, CultureInfo.InvariantCulture));

                Pushpin pin = new Pushpin(); // добавление 
                pin.Location = point; // точки
                myMap.Children.Add(pin); // на карту                    

            }
        }
    }
}
