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
            recieveTelData();
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
            TcpClient tcpClient = new TcpClient();
            Byte[] dataStream = new Byte[512];
            String responseString = String.Empty;

            try
            {
                tcpClient.Connect("127.0.0.1", 13000);
                while (true)
                {
                    //string senderMessage = "Connecting";
                    //tcpServer.AcceptTcpClient();
                    NetworkStream stream = tcpClient.GetStream();
                    //Byte[] sendBytes = Encoding.ASCII.GetBytes(senderMessage);
                    //stream.Write(sendBytes, 0, sendBytes.Length);
                    //stream.Flush();

                    Int32 bytes = stream.Read(dataStream, 0, dataStream.Length);
                    responseString = System.Text.Encoding.ASCII.GetString(dataStream, 0, bytes);
                    string[] dataPackage = responseString.Split('#');
                    string[] telData = dataPackage[3].Split(',');
                    int checksum = CalculateCheckSum(telData[5], telData[6], telData[7]);
                    if (checksum == int.Parse(dataPackage[4]))
                    {
                        string[] dateArrange = telData[0].Split('_');
                        string[] year = dateArrange[2].Split(' ');
                        string dateString = year[0] + "-" + dateArrange[1] + "-" + dateArrange[0] + " " + year[1];
                        DateTime telDate = DateTime.Parse(dateString);

                        data.Add(new AircraftTelemetryData
                        {
                            TailNumber = dataPackage[1],
                            TelDate = telDate,
                            Timestamp = DateTime.Now,
                            AccelX = double.Parse(telData[1]),
                            AccelY = double.Parse(telData[2]),
                            AccelZ = double.Parse(telData[3]),
                            Weight = double.Parse(telData[4]),
                            Altitude = double.Parse(telData[5]),
                            Pitch = double.Parse(telData[6]),
                            Bank = double.Parse(telData[7])
                        });
                        WriteCollectionData(data);
                        if (isRealTime == true)
                        {
                            LoadDataToGrid();
                        }
                        Logger.Log(dataPackage[3]);
                        return data;
                    }
                    else
                    {
                        data.Clear();
                        return data;
                    }
                }
            }
            catch(SocketException e)
            {
                throw e;
            }
            finally
            {
                tcpClient.Close();
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

                string telDataWrite = "INSERT INTO Telemetry (Tail_Number, Tel_Date, Date_Time_Stamp)";
                telDataWrite += " VALUES (@telTailNum, @telDate, @dateStamp)";
                string gDataWrite = "INSERT INTO GForce (Tail_Number, AccelX, AccelY, AccelZ, telWeight)";
                gDataWrite += " VALUES (@telTailNum, @telAccelX, @telAccelY, @telAccelZ, @telWeightPar)";
                string altDataWrite = "INSERT INTO Altitude (Tail_Number, Altitude, Pitch, Bank)";
                altDataWrite += " VALUES (@telTailNum, @telAlt, @telPitch, @telBank)";

                SqlCommand command = new SqlCommand(telDataWrite, connection);
                //command.Parameters.AddWithValue("@telNum", 1);
                command.Parameters.AddWithValue("@telTailNum", telData[0].TailNumber);
                command.Parameters.AddWithValue("@telDate", telData[0].TelDate);
                command.Parameters.AddWithValue("@dateStamp", telData[0].Timestamp);

               // command.Prepare();
               int result = command.ExecuteNonQuery();
                if(result == 1)
                {
                    Console.WriteLine("Success");
                }
                connection.Close();

                connection.Open();
                command = new SqlCommand(gDataWrite, connection);
                command.Parameters.AddWithValue("@telTailNum", telData[0].TailNumber);
                command.Parameters.AddWithValue("@telAccelX", telData[0].AccelX);
                command.Parameters.AddWithValue("@telAccelY", telData[0].AccelY);
                command.Parameters.AddWithValue("@telAccelZ", telData[0].AccelZ);
                command.Parameters.AddWithValue("@telWeightPar", telData[0].Weight);
                result = command.ExecuteNonQuery();
                connection.Close();

                connection.Open();
                command = new SqlCommand(altDataWrite, connection);
                command.Parameters.AddWithValue("@telTailNum", telData[0].TailNumber);
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
