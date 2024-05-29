using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using WpfAppERW.Data;

namespace WpfAppERW.Services
{
    public class AutoCompleteService
    {
        const string BingMapsApiKey = Constants.BingMapsApiKey;
        public const string AutoCompleteServiceUrl = Constants.AutoCompleteServiceUrl;
        public static async Task<List<string>> GetLocationSuggestionsAsync(string searchText) {

            if (string.IsNullOrWhiteSpace(searchText)) {
                // если строка поиска пустая, возвращаем пустой список подсказок
                return new List<string>();
            }

            string requestUrl = $"{AutoCompleteServiceUrl}?query={searchText}&key={BingMapsApiKey}";

            using (HttpClient client = new HttpClient()) {
                HttpResponseMessage response = await client.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode) {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    List<string> suggestions = ParseSuggestions(jsonString);
                    return suggestions;
                }
                else {
                    // обработка ошибки, если запрос не удался
                    throw new Exception($"Failed to get location suggestions for {searchText}. Error: {response.ReasonPhrase}");
                }
            }
        }

        private static List<string> ParseSuggestions(string jsonString) {
            List<string> suggestions = new List<string>();

            using (JsonDocument jsonDoc = JsonDocument.Parse(jsonString)) {
                JsonElement root = jsonDoc.RootElement;

                // получаем массив подсказок
                JsonElement resourceSets = root.GetProperty("resourceSets")[0];
                JsonElement resources = resourceSets.GetProperty("resources")[0];
                JsonElement valueArray = resources.GetProperty("value");

                // итерируем по массиву подсказок и извлекаем текстовое описание места
                foreach (JsonElement suggestionElement in valueArray.EnumerateArray()) {
                    JsonElement addressElement = suggestionElement.GetProperty("address");
                    string? formattedAddress = addressElement.GetProperty("formattedAddress").GetString();
                    suggestions.Add(formattedAddress);
                }
            }

            return suggestions;
        }       

    }
}
