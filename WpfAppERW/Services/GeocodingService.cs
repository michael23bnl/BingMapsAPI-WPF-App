using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WpfAppERW.Data;

namespace WpfAppERW.Services
{
    public class GeocodingService {

        const string BingMapsApiKey = Constants.BingMapsApiKey;
        const string GeocodingServiceUrl = Constants.GeocodingServiceUrl;

        public static async Task<(double, double)?> GetCoordinatesAsync(string location) {
            string requestUrl = $"{GeocodingServiceUrl}?q={location}&key={BingMapsApiKey}";
            using (HttpClient client = new HttpClient()) {
                HttpResponseMessage response = await client.GetAsync(requestUrl);
                if (response.IsSuccessStatusCode) {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    using (JsonDocument jsonDoc = JsonDocument.Parse(jsonString)) {
                        JsonElement root = jsonDoc.RootElement;
                        JsonElement resources = root.GetProperty("resourceSets")[0].GetProperty("resources");

                        if (resources.GetArrayLength() > 0) {
                            // координаты первого результата
                            double latitude = resources[0].GetProperty("point").GetProperty("coordinates")[0].GetDouble();
                            double longitude = resources[0].GetProperty("point").GetProperty("coordinates")[1].GetDouble();
                            return (latitude, longitude);
                        }
                        else {
                            // если результаты не найдены, возвращаем null
                            return null;
                        }
                    }
                }
                else {
                    // обработка ошибки, если запрос не удался
                    throw new Exception($"Failed to get coordinates for {location}. Error: {response.ReasonPhrase}");
                }
            }
        }
    }
}
