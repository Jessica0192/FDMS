using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
            LoadDataToGrid();
        }

        private void LoadDataToGrid()
        {
            dataGrid.ItemsSource = LoadCollectionData();
        }

        private void SearchTextBox_Search(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("On every key press");
        }

        private List<AircraftTelemetryData> LoadCollectionData()
        {
            String connectionString = "";
            SqlConnection connection = new SqlConnection(connectionString);
            SqlDataReader reader = null;
            List<AircraftTelemetryData> data = new List<AircraftTelemetryData>();

            //connection.Open();
            //while (reader.Read())
            //{
            //    data.Add(new AircraftTelemetryData()
            //    {
            //        Timestamp = reader.GetDateTime(0),
            //        AccelX = reader.GetDouble(1),
            //        AccelY = reader.GetDouble(2),
            //        AccelZ = reader.GetDouble(3),
            //        Weight = reader.GetDouble(4),
            //        Altitude = reader.GetDouble(5),
            //        Pitch = reader.GetDouble(6),
            //        Bank = reader.GetDouble(7)
            //    });
            //}
            //use loop to generate list of AircraftTelemetryData object with data from DB
            for(int i=0; i<15; i++)
            {
                data.Add(new AircraftTelemetryData()
                {
                    Timestamp = new DateTime(),
                    AccelX = Convert.ToDouble(i),
                    AccelY = Convert.ToDouble(i),
                    AccelZ = Convert.ToDouble(i),
                    Weight = Convert.ToDouble(i),
                    Altitude = Convert.ToDouble(i),
                    Pitch = Convert.ToDouble(i),
                    Bank = Convert.ToDouble(i)
                });
            }
            

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
