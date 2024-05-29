using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfAppERW.Data;

namespace WpfAppERW.Services
{
    public class RouteCalculationService
    {

        const string BingMapsApiKey = Constants.BingMapsApiKey;
        const string RoutingServiceUrl = Constants.RoutingServiceUrl;

        public static async ValueTask<List<Location>> CalculateRoute(Map myMap, Location startLocation, Location endLocation) {
            string requestUrl = $"{RoutingServiceUrl}?waypoint.1={startLocation.Latitude},{startLocation.Longitude}&waypoint.2={endLocation.Latitude},{endLocation.Longitude}&key={BingMapsApiKey}";
            List<Location> routePoints = new List<Location>();
            using (HttpClient client = new HttpClient()) {
                HttpResponseMessage response = await client.GetAsync(requestUrl);
                if (response.IsSuccessStatusCode) {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    using (JsonDocument jsonDoc = JsonDocument.Parse(jsonString)) {
                        JsonElement root = jsonDoc.RootElement;
                        // получение необходимых элементов маршрута
                        JsonElement itinerary = root.GetProperty("resourceSets")[0].GetProperty("resources")[0].GetProperty("routeLegs")[0].GetProperty("itineraryItems");
                        //myMap.Children.Clear(); // очистка предыдущего маршрута
                        routePoints.Add(startLocation);                     
                        foreach (JsonElement item in itinerary.EnumerateArray()) {
                            double latitude = item.GetProperty("maneuverPoint").GetProperty("coordinates")[0].GetDouble();
                            double longitude = item.GetProperty("maneuverPoint").GetProperty("coordinates")[1].GetDouble();
                            Location location = new Location(latitude, longitude);
                            routePoints.Add(location);
                          
                        }                       
                        routePoints.Add(endLocation);                   
                    }
                }
                return routePoints;
            }
        }
        public static async Task<Location> FindClosestWaypoint(Location startLocation, List<Location> waypoints) {
            double distance;
            double shortestDistance = 13589310;
            Location closestWaypoint = null;
            foreach (Location waypoint in waypoints) {
                distance = await CalculateDistance(startLocation, waypoint);
                if (distance < shortestDistance) {
                    shortestDistance = distance;
                    closestWaypoint = waypoint;
                }
            }
            return closestWaypoint;
        }
        public static async Task<double> CalculateDistance(Location startLocation, Location endLocation) {
            string requestUrl = $"{RoutingServiceUrl}?waypoint.1={startLocation.Latitude},{startLocation.Longitude}&waypoint.2={endLocation.Latitude},{endLocation.Longitude}&key={BingMapsApiKey}";
            using (HttpClient client = new HttpClient()) {
                HttpResponseMessage response = await client.GetAsync(requestUrl);
                if (response.IsSuccessStatusCode) {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    using (JsonDocument jsonDoc = JsonDocument.Parse(jsonString)) {
                        JsonElement root = jsonDoc.RootElement;
                        double routeLength = root.GetProperty("resourceSets")[0].GetProperty("resources")[0].GetProperty("travelDistance").GetDouble();
                        return routeLength;
                    }
                }
                else {
                    return 0.0;
                }
            }
        }       

    }
}
