using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ground_Terminal
{
    internal class Logger
    {
        /// <summary>
        /// Log to eventlog and textfile
        /// </summary>
        /// <param name="message"></param>
        public static void Log(string message)
        {
            //declare date variable
            DateTime now = DateTime.Now;
            //set file path
            string filePath = AppDomain.CurrentDomain.BaseDirectory + @".\GroundTerminalLogger.txt";
            string fullmessage = "";
            //create message to log
            fullmessage = message + "-->" + now.ToString("F") + "\n";
            //check if file exists
            if (!File.Exists(filePath))
            {
                StreamWriter createFile = File.CreateText(filePath);
                createFile.Close();
            }

            //instantiate event log
            EventLog serviceEventLog = new EventLog();
            //check if the event log already exists
            if (!EventLog.SourceExists("MyEventSource"))
            {
                EventLog.CreateEventSource("MyEventSource", "MyEventLog");
            }
            //set source for log
            serviceEventLog.Source = "MyEventSource";
            //set the name of the event log
            serviceEventLog.Log = "MyEventLog";
            //write message to log event
            serviceEventLog.WriteEntry(fullmessage);
            //write message to text file for logs
            StreamWriter sw = File.AppendText(filePath);
            sw.Write(fullmessage);
            //close file stream
            sw.Close();
        }
    }
}
