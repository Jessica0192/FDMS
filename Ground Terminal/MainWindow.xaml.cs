using System;
using System.Collections.Generic;
using System.Windows;

namespace Ground_Terminal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadDataToGrid()
        {
            dataGrid.ItemsSource = LoadCollectionData();
        }

        private void SearchTextBox_Search(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("On every key press");
        }

        //private void searchBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        //{
        //    //var filtered = data.Where(x => x.Type.StartsWith(searchBox.Text));
        //    //dataGrid.ItemsSource = filtered;
        //}

        private List<AircraftTelemetryData> LoadCollectionData()
        {
            List<AircraftTelemetryData> data = new List<AircraftTelemetryData>();

            //use loop to generate list of AircraftTelemetryData object with data from DB
            data.Add(new AircraftTelemetryData()
            {
                Timestamp = new DateTime(),
                AccelX = 0.00,
                AccelY = 0.00,
                AccelZ = 0.00,
                Weight = 0.00,
                Altitude = 0.00,
                Pitch = 0.00,
                Bank = 0.00
            });

            return data;
        }

        private void toggleRealTimeMode_Checked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Button toggled on!");
        }

        private void toggleRealTimeMode_Unchecked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Button toggled off!");
        }
    }

    public class AircraftTelemetryData
    {
        public DateTime Timestamp { get; set; }
        public double AccelX { get; set; }
        public double AccelY { get; set; }
        public double AccelZ { get; set; }
        public double Weight { get; set; }
        public double Altitude { get; set; }
        public double Pitch { get; set; }
        public double Bank { get; set; }
    }
}
