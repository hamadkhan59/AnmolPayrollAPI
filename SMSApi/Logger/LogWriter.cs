using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    public static class LogWriter
    {
        static private Object thisLock = new Object();
        static string logfileLocation = ConfigurationManager.AppSettings["logfilelocation"];
        static string logFileName = ConfigurationManager.AppSettings["fileName"];
        static int filesize = int.Parse(ConfigurationManager.AppSettings["fileSize"]);
        static long byteCount = 1000000;
        //static long byteCount = 10;
        public static void WriteLog(string logMessage)
        {
            CheckFileSize();
            LogWrite(logMessage);
        }

        public static void WriteLogStart(string logMessage)
        {
            LogWrite("================================================================================");
            LogWrite(logMessage);
        }

        public static void WriteLogEnd(string logMessage)
        {
            LogWrite(logMessage);
            //LogWrite("================================Action Finished=================================");
        }

        static void LogWrite(string logMessage)
        {
            try
            {
                int fileCount = int.Parse(ConfigurationManager.AppSettings["fileCount"]);
                using (StreamWriter w = File.AppendText(logfileLocation + logFileName + fileCount.ToString() + ".txt"))
                {

                    Log(logMessage, w);
                }
            }
            catch (Exception ex)
            {
            }
        }

        static void CheckFileSize()
        {
            int fileCount = int.Parse(ConfigurationManager.AppSettings["fileCount"]);
            string path = logfileLocation + logFileName + fileCount.ToString() + ".txt";
            using (var file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
            {
                long size = file.Length;
                if (size >= byteCount * filesize)
                {
                    fileCount++;
                    ConfigurationManager.AppSettings.Set("fileCount", fileCount.ToString());
                    CheckFileSize();
                }
            }

            //FileInfo fi = new FileInfo(logfileLocation + fileCount.ToString() + ".txt");
            //if (fi.Exists == false)
            //{
            //    fi.Create();
            //    fi = new FileInfo(logfileLocation + fileCount.ToString() + ".txt");
            //}

            //long size = fi.Length;
            //if (size >= byteCount * filesize)
            //{
            //    fileCount++;
            //    ConfigurationManager.AppSettings.Remove("fileCount");
            //    ConfigurationManager.AppSettings.Add("fileCount", fileCount.ToString());
            //}

        }
        static void Log(string logMessage, TextWriter txtWriter)
        {
            try
            {
                lock (thisLock)
                {
                    txtWriter.Write("\r\nOnical Logs | " + DateTime.Now.ToString() + " | Bio Metrice service | info | " + logMessage);
                    txtWriter.Close();
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
