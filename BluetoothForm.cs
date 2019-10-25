using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework.Forms;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SerialPortBluetooth
{
    
    public partial class Form1 : MetroForm
    {
        //Thread Scanner ;
        List<Temperature> tempList;
        SerialPort sp;

        public Form1()
        {
            InitializeComponent();
            sp = new System.IO.Ports.SerialPort("COM1", 9600, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
            tempList = new List<Temperature>();
            chart1.ChartAreas[0].AxisX.LabelStyle.Enabled = false;
        }
        //---------------------------------------------------------------------
        private void Text1Append(string msg)
        {
            Text1.AppendText(Environment.NewLine + msg + Environment.NewLine);
            //Text1.ScrollToCaret();  
        }
        private void writetoserialport(string msg)
        {
            if (sp.IsOpen)
            {
                if (Properties.Settings.Default.SendEndOfLineChar)
                     sp.WriteLine(msg);
                else sp.Write(msg);
                Text1Append("###Sending data:" + msg);
            }
            else Text1Append("###Error sending data:" + msg);                

        }
        

        //---------------------------------------------------------------------
 
        private void serialportdatareceived(object sender,SerialDataReceivedEventArgs e)
        {
            MethodInvoker mi;
            mi = delegate()
            {
                string s = sp.ReadExisting();

                foreach (char c in s.ToArray())
                {
                    tempList.Add(new Temperature(c, DateTime.Now));
                    listBox1.Items.Add(tempList[tempList.Count - 1]);
                    currtemp.Text = "Temperature courante: " + tempList[tempList.Count - 1].temp + "°";
                    chart1.Series[0].Points.AddY(tempList[tempList.Count - 1].temp);
                }
                
            };

            this.Invoke(mi);
        }

        private void Start_Click(object sender, EventArgs e)
        {

            if (Ports.SelectedIndex == -1)
            {

                MetroFramework.MetroMessageBox.Show(this, "Error","Select port.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (Speeds.SelectedIndex == -1)
            {
                MetroFramework.MetroMessageBox.Show(this, "Error", "Select baud rate.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            sp.PortName = Ports.Text;
            sp.DataReceived += serialportdatareceived;
            sp.BaudRate = Convert.ToInt32(Speeds.Text.Split(' ')[0]);
            Text1Append("###Starting " + sp.PortName + " at speed " + sp.BaudRate + " baud.");
                
            try{
                sp.Open();
            }catch{
                Text1Append("###Error opening port.");
                return;
            }
            Text1Append("###Port opened.");

            #region Enable Stop state
            Start.Enabled = false;
            Ports.Enabled = false;
            Speeds.Enabled = false;
            Stop.Enabled = true;
            #endregion
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            if (sp.IsOpen == false)
                return;
                
            sp.Close();
            Text1Append("###Port Closed.");
            
            #region Enable Start State
                Ports.Enabled = true;
                Speeds.Enabled = true;
                Start.Enabled = true;
                Stop.Enabled = false;
            #endregion
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Stop.PerformClick();
        }


        private void clear_Click(object sender, EventArgs e)
        {
            Text1.Clear();
            listBox1.Items.Clear();
        }

        private void aboutbtn_Click(object sender, EventArgs e)
        {
            MetroFramework.MetroMessageBox.Show(this, "   --- SerialPort-Bluetooth communication V0.1 ---\n\n                        Developed by Abdalmoez\n\n  Feel free to use this project.", "SerialPort-Bluetooth communication V0.1",MessageBoxButtons.OK,MessageBoxIcon.Information);
        }

        private void Ports_DropDown_1(object sender, EventArgs e)
        {
            Ports.Items.Clear();
            foreach (string s in SerialPort.GetPortNames())
                Ports.Items.Add(s);
        }

        private void textBox11_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                writetoserialport(textBox1.Text);
                textBox1.Text = "";
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count == 0)
                return;
            SaveFileDialog s=new SaveFileDialog();
            s.Filter = "Text files(*.txt)|All files(*.*)";
            if (s.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            string path = s.FileName;
            if (path.ToLower().LastIndexOf(".txt") != path.Length - 4)
                path += ".txt";

            using (StreamWriter outputFile = new StreamWriter(path))
            {
                foreach (Temperature t in tempList)
                    outputFile.WriteLine(t.ToString());
            }   
        }

    }
    class Temperature
    {
        public int temp;
        public DateTime datetime;
        public Temperature(int Temp,DateTime Now)
        {
            datetime = Now;
            temp = Temp;
        }
        public override string ToString()
        {
            return datetime + " : " + temp + "°";
        }
    }
}
