/*
* FILE          : MainWindow.xaml.cs
* PROJECT       : SENG3020 - Term Project
* PROGRAMMER    : Troy Hill, Jessica Sim
* FIRST VERSION : 2022-10-30
* DESCRIPTION:
*    This program collects remote telemetry from aircraft for display to the user, storage in a database
*    and provides the ability for the user to request stored data for post anlysis
*/
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

        /*
        * FUNCTION : LoadDataToGrid
        * DESCRIPTION : This function loads the data from database to datagrid in UI
        * PARAMETERS : no parameters
        * RETURNS : void
        */
        private void LoadDataToGrid()
        {
            dataGrid.ItemsSource = LoadCollectionData();
        }

        /*
        * FUNCTION : SearchTextBox_Search
        * DESCRIPTION : This function is being called when user put some text in search box and press enter or press the 'search' icon
        * PARAMETERS : object sender: button control
        *              RoutedEventArgs e: it contains state information and event data associated with routed event
        * RETURNS : void
        */
        private void SearchTextBox_Search(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("On every key press");
        }

        //private List<AircraftTelemetryData> recieveTelData()
        //{
        //    try
        //    {
        //        List<AircraftTelemetryData> data = new List<AircraftTelemetryData>();
        //        Int32 port = 13000;
        //        TcpClient tcpClient = new TcpClient("127.0.0.1", port);
        //        string[] dataPacket = System.Text.Encoding.ASCII.GetString();

        //        if(isRealTime ==  false)
        //        {

        //        }
        //        else if (isRealTime ==  true)
        //        {

        //        }
        //    }
        //    catch(SocketException e)
        //    {

        //    }
        //}

        /*
        * FUNCTION : WriteCollectionData
        * DESCRIPTION : This function makes the connection to FDMS database and insert new data 
        * PARAMETERS : List<AircraftTelemetryData> telData: list of telemetry data
        * RETURNS : void
        */
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

        /*
        * FUNCTION : LoadCollectionData
        * DESCRIPTION : This function makes the connection to FDMS database and gets the collection of telemetry data
        * PARAMETERS : no parameters
        * RETURNS : List<AircraftTelemetryData>: returns list of telemetry data from database
        */
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

        /*
        * FUNCTION : toggleRealTimeMode_Checked
        * DESCRIPTION : This function is being called when Real-Time toggle button is toggled on
        * PARAMETERS : object sender: button control
        *              RoutedEventArgs e: it contains state information and event data associated with routed event
        * RETURNS : void
        */
        private void toggleRealTimeMode_Checked(object sender, RoutedEventArgs e)
        {
            isRealTime = true;
        }

        /*
        * FUNCTION : toggleRealTimeMode_Unchecked
        * DESCRIPTION : This function is being called when Real-Time toggle button is toggled off
        * PARAMETERS : object sender: button control
        *              RoutedEventArgs e: it contains state information and event data associated with routed event
        * RETURNS : void
        */
        private void toggleRealTimeMode_Unchecked(object sender, RoutedEventArgs e)
        {
            isRealTime = false;
        }
    }
}
