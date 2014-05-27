using System;
using OpenHardwareMonitor.Hardware;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Timers;

namespace COMParam
{
    public partial class Form1 : Form
    {
        private static System.Timers.Timer aTimer;
        private static Computer myComputer;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button2.Enabled = true;
            button1.Enabled = false;

            serialPort1.PortName = comboBox1.Text;
            serialPort1.BaudRate = 9600;
            serialPort1.Open();

            if (serialPort1.IsOpen)
            {
                button2.Enabled = true;
                button1.Enabled = false;
                textBox1.ReadOnly = false;
            }

            myComputer = new Computer();

            myComputer.GPUEnabled = true;
            myComputer.CPUEnabled = true;
            myComputer.HDDEnabled = true;

            myComputer.Open();

            aTimer = new System.Timers.Timer(10000);    // Create a timer with a ten second interval.

            aTimer.Elapsed += new ElapsedEventHandler(getPCInfo);   // Hook up the Elapsed event for the timer.

            aTimer.Interval = 1200; //czestotliwosc odswiezania
            aTimer.Enabled = true;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.Enabled = true;
            comboBox1.Enabled = false;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
                button1.Enabled = true;
                button2.Enabled = false;
                textBox1.ReadOnly = true;
            }

            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialPort1.IsOpen) serialPort1.Close();
            myComputer.Close();
        }

        private void getPCInfo(object source, ElapsedEventArgs e)
        {
            foreach (var hardwareItem in myComputer.Hardware)
            {
                if (hardwareItem.HardwareType == HardwareType.CPU)
                {
                    hardwareItem.Update();
                    send_str((hardwareItem.Sensors[5].Value).ToString()); //CPU TOTAL TEMP
                    Console.WriteLine("CPU: "+(hardwareItem.Sensors[5].Value).ToString());
                }

                if (hardwareItem.HardwareType == HardwareType.GpuNvidia)
                {
                    hardwareItem.Update();
                    send_str((hardwareItem.Sensors[0].Value).ToString()); //GPU TOTAL TEMP
                    Console.WriteLine("GPU: "+(hardwareItem.Sensors[0].Value).ToString());
                }

                //if (hardwareItem.HardwareType == HardwareType.HDD)
                //{
                //    send_str((hardwareItem.Sensors[0].Value).ToString()); //HDD TOTAL TEMP
                //    Console.WriteLine("HDD: "+(hardwareItem.Sensors[0].Value).ToString());
                //}
            }
        }

        private void send_str(String str)
        {
            if (!serialPort1.IsOpen) return; //jezeli port zamknięty to nie wysyłaj

            //char[] buff = new char[1]; //deklaracja bufora

            //buff[0] = ch;

            serialPort1.Write(str);
        }

        private void send_ch(char ch)
        {
            if (!serialPort1.IsOpen) return; //jezeli port zamknięty to nie wysyłaj

            char[] buff = new char[1]; //deklaracja bufora

            buff[0] = ch;

            serialPort1.Write(buff, 0, buff.Length);

            //textBox1.Text = "Wysłano!";
        }
    }
}
