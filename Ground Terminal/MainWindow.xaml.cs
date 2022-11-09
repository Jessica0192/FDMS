using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Windows;
using System.Windows.Media.Media3D;

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
            List<AircraftTelemetryData> data = new List<AircraftTelemetryData>();
           
            data.Add(new AircraftTelemetryData()
            {
                Timestamp = DateTime.Now,
                AccelX = 2.23,
                AccelY = 4.45,
                AccelZ = 3.14,
                Weight = 10.23,
                Altitude = 573.85,
                Pitch = 34.4,
                Bank = 23.3
        });
            WriteCollectionData(data);
            LoadDataToGrid();
        }

        public bool isRealTime = false;

        private void LoadDataToGrid()
        {
            dataGrid.ItemsSource = LoadCollectionData();
        }

        private void SearchTextBox_Search(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("On every key press");
        }

        private List<AircraftTelemetryData> recieveTelData()
        {
            try
            {
                List<AircraftTelemetryData> data = new List<AircraftTelemetryData>();
                Int32 port = 13000;
                TcpClient tcpClient = new TcpClient("127.0.0.1", port);
                NetworkStream stream = tcpClient.GetStream();
                Byte[] dataStream = new Byte[256];
                String responseString = String.Empty;

                Int32 bytes = stream.Read(dataStream, 0, dataStream.Length);
                responseString = System.Text.Encoding.ASCII.GetString(dataStream, 0, bytes);

                if(isRealTime ==  true)
                {
                    WriteCollectionData(data);
                }

                return data;
            }
            catch(SocketException e)
            {
                throw e;
            }

        }

        private void WriteCollectionData(List<AircraftTelemetryData> telData)
        {
            String connectionString = @"server=localhost;database=FDMS_Test;trusted_connection=true";
            SqlConnection connection = new SqlConnection(connectionString);

            try
            {
                connection.Open();

                string dataWrite = "INSERT INTO Telemetry_Test (Tail_Number, Date_Time_Stamp, AccelX, AccelY, AccelZ, telWeight, Altitude, Pitch, Bank)";
                dataWrite += " VALUES (@telTailNum, @telDate , @telAccelX, @telAccelY, @telAccelZ, @telWeightPar, @telAlt, @telPitch, @telBank)";

                SqlCommand command = new SqlCommand(dataWrite, connection);
                //command.Parameters.AddWithValue("@telNum", 1);
                command.Parameters.AddWithValue("@telTailNum", 3456);
                command.Parameters.AddWithValue("@telDate", telData[0].Timestamp);
                command.Parameters.AddWithValue("@telAccelX", telData[0].AccelX);
                command.Parameters.AddWithValue("@telAccelY", telData[0].AccelY);
                command.Parameters.AddWithValue("@telAccelZ", telData[0].AccelZ);
                command.Parameters.AddWithValue("@telWeightPar", telData[0].Weight);
                command.Parameters.AddWithValue("@telAlt", telData[0].Altitude);
                command.Parameters.AddWithValue("@telPitch", telData[0].Pitch);
                command.Parameters.AddWithValue("@telBank", telData[0].Bank);

               // command.Prepare();
               int result = command.ExecuteNonQuery();
                if(result == 1)
                {
                    Console.WriteLine("Success");
                }
                connection.Close();
            }
            catch(SqlException e)
            {
                throw e;
            }
        }

        private List<AircraftTelemetryData> LoadCollectionData()
        {
            String connectionString = @"server=localhost; database=FDMS_Test;trusted_connection=true";
            SqlConnection connection = new SqlConnection(connectionString);
            List<AircraftTelemetryData> data = new List<AircraftTelemetryData>();
            SqlCommand readTelcmd = new SqlCommand("SELECT TOP 15 Tail_Number, Date_Time_Stamp, AccelX, AccelY, AccelZ, telWeight, Altitude, Pitch, Bank FROM Telemetry_Test", connection);
            //SqlCommand readGForcecmd = new SqlCommand("SELECT TOP 15 FROM GForce", connection);
            //SqlCommand readAltcmd = new SqlCommand("SELECT TOP 15 FROM Altitude", connection);

            connection.Open();

            SqlDataReader reader = readTelcmd.ExecuteReader();

            while (reader.Read())
            {
                data.Add(new AircraftTelemetryData()
                {
                    Timestamp = reader.GetDateTime(1),
                    AccelX = reader.GetDouble(2),
                    AccelY = reader.GetDouble(3),
                    AccelZ = reader.GetDouble(4),
                    Weight = reader.GetDouble(5),
                    Altitude = reader.GetDouble(6),
                    Pitch = reader.GetDouble(7),
                    Bank = reader.GetDouble(8)
                });
            }
            reader.Close();
            connection.Close();

           // connection.Open();

            //reader = readGForcecmd.ExecuteReader();
            //while(reader.Read())
            //{
            //    data.Add(new AircraftTelemetryData()
            //    {
            //        AccelX = reader.GetDouble(1),
            //        AccelY = reader.GetDouble(2),
            //        AccelZ = reader.GetDouble(3),
            //        Weight = reader.GetDouble(4)
            //    });
            //}
            //reader.Close();
            //connection.Close();

            
            //use loop to generate list of AircraftTelemetryData object with data from DB
            //for (int i=0; i<15; i++)
            //{
            //    data.Add(new AircraftTelemetryData()
            //    {
            //        Timestamp = new DateTime(),
            //        AccelX = Convert.ToDouble(i),
            //        AccelY = Convert.ToDouble(i),
            //        AccelZ = Convert.ToDouble(i),
            //        Weight = Convert.ToDouble(i),
            //        Altitude = Convert.ToDouble(i),
            //        Pitch = Convert.ToDouble(i),
            //        Bank = Convert.ToDouble(i)
            //    });
            //}
            

            return data;
        }

        //private string CalculateCheckSum(int checksum)
        //{
        //    string result = checksum.ToString();
        //    return result;
        //}

        private void toggleRealTimeMode_Checked(object sender, RoutedEventArgs e)
        {
            isRealTime = true;
            MessageBox.Show("Button toggled on!");
        }

        private void toggleRealTimeMode_Unchecked(object sender, RoutedEventArgs e)
        {
            isRealTime = false;
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
