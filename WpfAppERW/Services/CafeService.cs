using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using WpfAppERW.Data;

namespace WpfAppERW.Services
{
    public class CafeService {


        const string BingMapsApiKey = Constants.BingMapsApiKey;


        public static async Task<string> GeocodeCafeAddress(string address) {
            HttpClient httpClient = new HttpClient();
            string url = $"https://dev.virtualearth.net/REST/v1/Locations?query={address}&key={BingMapsApiKey}";
            HttpResponseMessage response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode) {
                string responseBody = await response.Content.ReadAsStringAsync();
                JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody);
                if (data.GetProperty("resourceSets").GetArrayLength() > 0 && data.GetProperty("resourceSets")[0].GetProperty("resources").GetArrayLength() > 0) {
                    var point = data.GetProperty("resourceSets")[0].GetProperty("resources")[0].GetProperty("point");
                    return $"{point.GetProperty("coordinates")[0]}, {point.GetProperty("coordinates")[1]}";
                }
            }
            return null;
        }
        public static void AddCafePinToMap(Map myMap, string coordinates, string cafeName) {
            var splitCoordinates = coordinates.Split(',');
            if (splitCoordinates.Length == 2
                && double.TryParse(splitCoordinates[0], CultureInfo.InvariantCulture, out double latitude) 
                && double.TryParse(splitCoordinates[1], CultureInfo.InvariantCulture, out double longitude)) {
                var pin = new Pushpin {
                    Location = new Location(latitude, longitude),
                    Content = cafeName
                };
                myMap.Children.Add(pin);
            }          
        }

        public static async Task<List<Location>> CafeLocations(Dictionary<string, string> cafes) {
            List<Location> locations = new List<Location>();    
            foreach (KeyValuePair<string, string> cafe in cafes) {
                string coordinates = await GeocodeCafeAddress(cafe.Value);
                var splitCoordinates = coordinates.Split(',');
                if (splitCoordinates.Length == 2
                    && double.TryParse(splitCoordinates[0], CultureInfo.InvariantCulture, out double latitude)
                    && double.TryParse(splitCoordinates[1], CultureInfo.InvariantCulture, out double longitude)) {
                    locations.Add(new Location(latitude, longitude));           
                }
            }
            return locations;
        }
    }
}
