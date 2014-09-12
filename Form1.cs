using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using System.Data.SQLite;

namespace GraphViewer
{
    public partial class Form1 : Form
    {
        SQLiteConnection m_dbConnection;
        public Form1()
        {
            InitializeComponent();
             InitializeOpenFileDialog();
        }

        private void InitializeOpenFileDialog()
        {
            // Set the file dialog to filter for graphics files. 
            openFileDialog1.Filter =
                "Images (*.csv)|*.csv";

            // Allow the user to select files. 
            openFileDialog1.Multiselect = true;
            openFileDialog1.Title = "Select files to plot";
            openFileDialog1.FileName = "";
        }

        private void generateSeriesFromFile(String filename)
        {
            //var reader = new StreamReader(File.OpenRead(@"C:\Users\Utsav\Desktop\200 Hours Test Data\200 Hours Test Data\scope_1.csv"));
            var reader = new StreamReader(File.OpenRead(filename));
            string seriesName = filename.Substring(filename.LastIndexOf('\\'));
            
            chart1.Series.Add(seriesName);
            chart1.Series[seriesName].ChartType = SeriesChartType.FastLine;

            Random randomGen = new Random();
            KnownColor[] names = (KnownColor[])Enum.GetValues(typeof(KnownColor));
            KnownColor randomColorName = names[randomGen.Next(names.Length)];
            Color randomColor = Color.FromKnownColor(randomColorName);

            chart1.Series[seriesName].Color = randomColor;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                try
                {
                    double time = Convert.ToDouble(values[0]);
                    double signal = Convert.ToDouble(values[1]);
                    chart1.Series[seriesName].Points.AddXY(time, signal);
                }
                catch (FormatException)
                {
                    continue;
                }
            }
        }

        private void closeApp_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void generateChart_Click(object sender, EventArgs e)
        {
            chart1.Series.Clear();

            DialogResult dr = openFileDialog1.ShowDialog();

            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                foreach (String file in openFileDialog1.FileNames)
                {
                    generateSeriesFromFile(file);
                }
            }
        }

        private void closeApp_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void createDatabase_Click(object sender, EventArgs e)
        {
            if (File.Exists("GraphViewer.sqlite") == false)
            {
                Console.WriteLine("Trying to create database....\n");
                SQLiteConnection.CreateFile("GraphViewer.sqlite");
                Console.WriteLine("created database.\n");
            }
            else
            {
                Console.WriteLine("database already exists\n");
            }
            connectToDatabase();
            createTable();
            CloseConnection();
        }

        private void connectToDatabase()
        {
            Console.WriteLine("Trying to connect to database....\n");
            m_dbConnection = new SQLiteConnection("Data Source=GraphViewer.sqlite;Version=3;");
            m_dbConnection.Open();
            Console.WriteLine("Connected to database....\n");
        }

        private void createTable()
        {
            Console.WriteLine("Trying to create table....\n");
            string sql = "create table IF NOT EXISTS pipeline_data (ID INTEGER PRIMARY KEY AUTOINCREMENT, survey_date DATETIIME not null," +  
                                                                    "pipeline_id int not null, time BLOB not null, signal BLOB not null)";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            Console.WriteLine("Table created.\n");
            Console.WriteLine("Trying to close connection....\n");
            CloseConnection();
            Console.WriteLine("Connection closed.\n");
        }

        private void CloseConnection()
        {
            if (m_dbConnection != null)
            {
                m_dbConnection.Close();
                m_dbConnection = null;
            }
        }

        public byte[] getByteArray(Double[] data)
        {
            object blob = data;
            if (blob == null) return null;
            byte[] arData = (byte[])blob;
            return arData;
        }
    }
}
