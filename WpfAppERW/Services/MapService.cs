
using Microsoft.Maps.MapControl.WPF;
using System.Windows.Media;


namespace WpfAppERW.Services {
    public class MapService {
        public static void CleanMap(Map myMap) {
            myMap.Children.Clear(); // очистка карты от всех маршрутных точек
        }

        public static void DisplayRoute(Map myMap, List<Location> waypoints) {
            MapPolyline routeLine = new MapPolyline();
            routeLine.Locations = new LocationCollection();
            foreach (Location point in waypoints) {
                Pushpin pin = new Pushpin(); // добавление точек
                pin.Location = point; // маршрута
                myMap.Children.Add(pin); // на карту
                routeLine.Locations.Add(point);
            }
            routeLine.Stroke = new SolidColorBrush(Colors.Blue);
            routeLine.StrokeThickness = 3;
            myMap.Children.Add(routeLine);
        }


    }
}
