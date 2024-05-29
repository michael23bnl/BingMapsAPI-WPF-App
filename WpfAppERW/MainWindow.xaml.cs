using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using Microsoft.Maps.MapControl.WPF;
using System.Security.Cryptography.X509Certificates;
using System.Data;
using System.Security.Cryptography;
using XPlat.Device.Geolocation;
using XPlat.Services.Maps;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using WpfAppERW.Services;
using System.Windows.Controls.Primitives;
using System.Device.Location;
using WpfAppERW.Data;
using System.Globalization;
namespace WpfAppERW {

    

    public partial class MainWindow : Window {

        public bool listBoxSelectionChangedExecuted = false;

        public Pushpin? existingPin;

        public MainWindow() {
            InitializeComponent();
            PreviewMouseDown += Window_PreviewMouseDown;
            PreviewMouseDown += MapControl_MapTapped;
        }

        private async void MapControl_MapTapped(object sender, MouseButtonEventArgs e) {
            if (e.RightButton == MouseButtonState.Pressed) {
                if (existingPin != null) {
                    myMap.Children.Remove(existingPin);
                    existingPin = null;
                }

                Point mousePosition = e.GetPosition(myMap);
                Location location = myMap.ViewportPointToLocation(mousePosition);

                var pin = new Pushpin();
                pin.Location = location;
                pin.Content = "Моя метка";

                myMap.Children.Add(pin);

                existingPin = pin;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {

        }       

        private async void CalculateRouteButton_Click(object sender, RoutedEventArgs e) {
            string startLocation = startLocationTextBox.Text;
            string endLocation = endLocationTextBox.Text;
            if (string.IsNullOrEmpty(startLocation) || string.IsNullOrEmpty(endLocation)) {
                MessageBox.Show("Не все поля заполнены");
            }
            else {
                var startTask = GeocodingService.GetCoordinatesAsync(startLocation);
                var finishTask = GeocodingService.GetCoordinatesAsync(endLocation);
                var startTaskResult = await startTask;
                var endTaskResult = await finishTask;
                List<Location> routePoints = await RouteCalculationService.CalculateRoute(myMap,
                    new Location(startTaskResult.Value.Item1, startTaskResult.Value.Item2), 
                    new Location(endTaskResult.Value.Item1, endTaskResult.Value.Item2));
                MapService.DisplayRoute(myMap, routePoints);
            }
        }
        private async void LocateCafesButton_Click(object sender, RoutedEventArgs e) {
            Dictionary<string, string> cafes = CafeAdresses.cafes;
            foreach (var cafe in cafes) {
                string coordinates = await CafeService.GeocodeCafeAddress(cafe.Value);
                if (coordinates != null) {
                    CafeService.AddCafePinToMap(myMap, coordinates, cafe.Key);
                }
            }
        }

        private void ClearMapButton_Click(object sender, RoutedEventArgs e) {
            MapService.CleanMap(myMap);
            existingPin = null;
        }

        private async void ClosestWaypointButton_Click(object sender, RoutedEventArgs e) {
            if (existingPin != null) {
                Location currentLocation = existingPin.Location;
                List<Location> locations = await CafeService.CafeLocations(CafeAdresses.cafes);
                Location closestWaypoint = await RouteCalculationService.FindClosestWaypoint(currentLocation, locations);
                List<Location> routePoints = await RouteCalculationService.CalculateRoute(myMap, currentLocation, closestWaypoint);
                MapService.DisplayRoute(myMap, routePoints);
            }
            else {
                MessageBox.Show("Укажите ваше текущее местоположения");
            }
        }    

        private async void MyLocationButton_Click(object sender, RoutedEventArgs e) {
            LocationService.GetLocationAsync(myMap);
        }
       

        

       


        private async void locationAutoCompleteTextBox_TextChanged(object sender, TextChangedEventArgs e) {

            TextBox locationAutoCompleteBox = sender as TextBox;
            string searchText = locationAutoCompleteBox.Text;

            List<string> suggestions = await AutoCompleteService.GetLocationSuggestionsAsync(searchText);

            if (listBoxSelectionChangedExecuted == false) {

                UpdateSuggestionsUI(locationAutoCompleteBox, suggestions);
            }
            else {
                listBoxSelectionChangedExecuted = false;
            }

        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            TextBox locationAutoCompleteBox = e.Source as TextBox;          
            // проверяем, было ли нажатие мыши вне текстового поля
            if (!IsMouseOverTextBox(locationAutoCompleteBox)) {
                // если да, скрываем список подсказок
                HideSuggestionsUI(locationAutoCompleteBox);
            }
            else {
                ShowSuggestionsUI(locationAutoCompleteBox);
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            // получаем ListBox, из которого был выбран элемент
            ListBox listBox = sender as ListBox;

            // проверяем, что был выбран элемент
            if (listBox.SelectedItem != null) {

                TextBox locationAutoCompleteBox = listBox.Tag as TextBox;
                // получаем выбранную подсказку
                string selectedSuggestion = listBox.SelectedItem.ToString();

                // устанавливаем выбранную подсказку в текстовом поле
                locationAutoCompleteBox.Text = selectedSuggestion;

                // скрываем список подсказок
                HideSuggestionsUI(locationAutoCompleteBox);
                listBoxSelectionChangedExecuted = true;
            }
        }

        public bool IsMouseOverTextBox(TextBox textBox) {
            if (textBox == null) {
                return false;
            }
            // проверяем, находится ли указатель мыши над текстовым полем           
            Point mousePos = Mouse.GetPosition(textBox);
            return new Rect(0, 0, textBox.ActualWidth, textBox.ActualHeight).Contains(mousePos);
        }

        public void HideSuggestionsUI(TextBox autoCompleteBox) {

            var mainWindow = Application.Current.MainWindow;

            if (mainWindow != null) {
                // перебираем все дочерние элементы главного окна
                IterateThroughChildren(mainWindow);
            }
        }

        private void IterateThroughChildren(DependencyObject parent) {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is TextBox textBox) {
                    // получаем Popup, связанный с TextBox
                    Popup popup = textBox.Tag as Popup;

                    // если Popup существует, скрываем его
                    if (popup != null) {
                        popup.IsOpen = false;
                    }
                }
                else {
                    // рекурсивно вызываем этот метод для всех дочерних элементов
                    IterateThroughChildren(child);
                }
            }
        }

        public static void ShowSuggestionsUI(TextBox autoCompleteBox) {
            // получаем Popup, связанный с TextBox
            Popup popup = autoCompleteBox.Tag as Popup;

            // если Popup существует, скрываем его
            if (popup != null) {
                popup.IsOpen = true;
            }
        }

        public void UpdateSuggestionsUI(TextBox autoCompleteBox, List<string> suggestions) {
            // получаем Popup, связанный с TextBox
            Popup popup = autoCompleteBox.Tag as Popup;

            // если Popup не существует, создаём новый
            if (popup == null) {
                popup = new Popup();
                autoCompleteBox.Tag = popup;
            }

            // очищаем предыдущие подсказки перед отображением новых
            popup.Child = null;

            // создаём ListBox для отображения подсказок
            ListBox listBox = new ListBox();

            listBox.SelectionChanged += ListBox_SelectionChanged;

            listBox.Tag = autoCompleteBox; // устанавливаем Tag на TextBox

            // добавляем подсказки в ListBox
            foreach (string suggestion in suggestions) {
                listBox.Items.Add(suggestion);
            }

            // устанавливаем ListBox как содержимое Popup
            popup.Child = listBox;

            // показываем Popup под TextBox
            popup.PlacementTarget = autoCompleteBox;
            popup.Placement = PlacementMode.Bottom;
            popup.IsOpen = true;
        }







    }
}