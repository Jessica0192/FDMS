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
using System.Text;
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
            //List<AircraftTelemetryData> data = new List<AircraftTelemetryData>();

            //    data.Add(new AircraftTelemetryData()
            //    {
            //        Timestamp = DateTime.Now,
            //        AccelX = 2.23,
            //        AccelY = 4.45,
            //        AccelZ = 3.14,
            //        Weight = 10.23,
            //        Altitude = 573.85,
            //        Pitch = 34.4,
            //        Bank = 23.3
            //});

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

        private List<AircraftTelemetryData> recieveTelData()
        {
            List<AircraftTelemetryData> data = new List<AircraftTelemetryData>();
            Int32 port = 13001;
            TcpListener tcpServer;
            TcpClient tcpClient;
            Byte[] dataStream = new Byte[256];
            String responseString = String.Empty;

            tcpServer = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            tcpServer.Start();
            try
            {
                string senderMessage = "Connecting";
                tcpClient = new TcpClient("127.0.0.1", 13000);
                NetworkStream stream = tcpClient.GetStream();
                Byte[] sendBytes = Encoding.ASCII.GetBytes(senderMessage);
                stream.Write(sendBytes, 0, sendBytes.Length);
                stream.Flush();

                tcpServer.AcceptTcpClient();
                Int32 bytes = stream.Read(dataStream, 0, dataStream.Length);
                responseString = System.Text.Encoding.ASCII.GetString(dataStream, 0, bytes);
                string[] dataPackage = responseString.Split('#');
                string[] telData = dataPackage[2].Split(',');
                int checksum = CalculateCheckSum(telData[5], telData[6], telData[7]);
                if (checksum == int.Parse(dataPackage[3]))
                {

                    WriteCollectionData(data);
                    if (isRealTime == true)
                    {
                        LoadDataToGrid();
                    }
                    Logger.Log(dataPackage[2]);
                    return data;
                }
                else
                {
                    data.Clear();
                    return data;
                }
                
            }
            catch(SocketException e)
            {
                throw e;
            }

        }

        /*
        * FUNCTION : WriteCollectionData
        * DESCRIPTION : This function makes the connection to FDMS database and insert new data 
        * PARAMETERS : List<AircraftTelemetryData> telData: list of telemetry data
        * RETURNS : void
        */
        private void WriteCollectionData(List<AircraftTelemetryData> telData)
        {
            String connectionString = @"server=localhost;database=FDMS;trusted_connection=true";
            SqlConnection connection = new SqlConnection(connectionString);

            try
            {
                connection.Open();

                string telDataWrite = "INSERT INTO Telemetry (Tail_Number, Date_Time_Stamp)";
                telDataWrite += " VALUES (@telTailNum, @telDate)";
                string gDataWrite = "INSERT INTO GForce (AccelX, AccelY, AccelZ, telWeight)";
                gDataWrite += " VALUES (@telAccelX, @telAccelY, @telAccelZ, @telWeightPar)";
                string altDataWrite = "INSERT INTO Altitude (Altitude, Pitch, Bank)";
                altDataWrite += " VALUES (@telAlt, @telPitch, @telBank)";

                SqlCommand command = new SqlCommand(telDataWrite, connection);
                //command.Parameters.AddWithValue("@telNum", 1);
                command.Parameters.AddWithValue("@telTailNum", 3456);
                command.Parameters.AddWithValue("@telDate", telData[0].Timestamp);

               // command.Prepare();
               int result = command.ExecuteNonQuery();
                if(result == 1)
                {
                    Console.WriteLine("Success");
                }
                connection.Close();

                connection.Open();
                command = new SqlCommand(gDataWrite, connection);
                command.Parameters.AddWithValue("@telAccelX", telData[0].AccelX);
                command.Parameters.AddWithValue("@telAccelY", telData[0].AccelY);
                command.Parameters.AddWithValue("@telAccelZ", telData[0].AccelZ);
                command.Parameters.AddWithValue("@telWeightPar", telData[0].Weight);
                result = command.ExecuteNonQuery();
                connection.Close();

                connection.Open();
                command = new SqlCommand(altDataWrite, connection);
                command.Parameters.AddWithValue("@telAlt", telData[0].Altitude);
                command.Parameters.AddWithValue("@telPitch", telData[0].Pitch);
                command.Parameters.AddWithValue("@telBank", telData[0].Bank);
                result = command.ExecuteNonQuery();
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
            String connectionString = @"server=localhost; database=FDMS;trusted_connection=true";
            SqlConnection connection = new SqlConnection(connectionString);
            List<AircraftTelemetryData> data = new List<AircraftTelemetryData>();
            SqlCommand readTelcmd = new SqlCommand("SELECT TOP 15 Tail_Number, Date_Time_Stamp FROM Telemetry", connection);
            SqlCommand readGForcecmd = new SqlCommand("SELECT TOP 15 AccelX, AccelY, AccelZ, telWeight FROM GForce", connection);
            SqlCommand readAltcmd = new SqlCommand("SELECT TOP 15 Altitude, Pitch, Bank FROM Altitude", connection);

            connection.Open();

            SqlDataReader reader = readTelcmd.ExecuteReader();

            while (reader.Read())
            {
                data.Add(new AircraftTelemetryData()
                {
                    Timestamp = reader.GetDateTime(1),
                });
            }
            reader.Close();
            connection.Close();

            int dataIndex = 0;
            connection.Open();

            reader = readGForcecmd.ExecuteReader();
            while (reader.Read())
            {
                data[dataIndex].AccelX = reader.GetDouble(0);
                data[dataIndex].AccelY = reader.GetDouble(1);
                data[dataIndex].AccelZ = reader.GetDouble(2);
                data[dataIndex].Weight = reader.GetDouble(3);
                dataIndex++;
            }
            reader.Close();
            connection.Close();

            connection.Open();

            dataIndex = 0;
            reader = readAltcmd.ExecuteReader();
            while (reader.Read())
            {
                data[dataIndex].Altitude = reader.GetDouble(0);
                data[dataIndex].Pitch = reader.GetDouble(1);
                data[dataIndex].Bank = reader.GetDouble(2);
                dataIndex++;
            }
            reader.Close();
            connection.Close();

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

        private int CalculateCheckSum(string altitude, string pitch, string bank)
        {
            float result = (float.Parse(altitude) + float.Parse(pitch) + float.Parse(bank) / 3);
            return (int) Math.Round(result, 0);
        }

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
