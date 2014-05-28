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
        PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time");
        PerformanceCounter ramCounter = new PerformanceCounter();

        public Form1()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 2;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;

            serialPort1.PortName = comboBox1.Text;
            serialPort1.BaudRate = 9600;
            serialPort1.Open();

            if (serialPort1.IsOpen)
            {
                button1.Enabled = false;
            }

            myComputer = new Computer();

            myComputer.GPUEnabled = true;
            myComputer.CPUEnabled = true;
            myComputer.HDDEnabled = true;

            myComputer.Open();

            aTimer = new System.Timers.Timer(10000);    // Create a timer with a ten second interval.
            aTimer.Elapsed += new ElapsedEventHandler(getPCInfo);   // Hook up the Elapsed event for the timer.
            aTimer.Interval = 1000;
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
                    send_str((hardwareItem.Sensors[5].Value).ToString() + "{C:"); //CPU TOTAL TEMP
                    Console.WriteLine("CPU: " + (hardwareItem.Sensors[5].Value).ToString().Substring(0, 2));
                    if ((int)(hardwareItem.Sensors[2].Value) < 10)
                    {
                        send_str("0"+(int)(hardwareItem.Sensors[2].Value) + "%:");
                    }
                    else
                        send_str((int)(hardwareItem.Sensors[2].Value) + "%:");
                }

                if (hardwareItem.HardwareType == HardwareType.GpuNvidia)
                {
                    hardwareItem.Update();
                    send_str((hardwareItem.Sensors[0].Value).ToString() + "{C:"); //GPU TOTAL TEMP
                    Console.WriteLine("GPU: " + (hardwareItem.Sensors[0].Value).ToString().Substring(0, 2));
                }

                if (hardwareItem.HardwareType == HardwareType.GpuAti)
                {
                    hardwareItem.Update();
                    send_str((hardwareItem.Sensors[0].Value).ToString() + "{C:"); //GPU TOTAL TEMP
                    Console.WriteLine("GPU: " + (hardwareItem.Sensors[0].Value).ToString().Substring(0, 2));
                }

                if (hardwareItem.HardwareType == HardwareType.HDD)
                {
                    hardwareItem.Update();
                    send_str((hardwareItem.Sensors[0].Value).ToString() + "{C;"); //HDD TOTAL TEMP
                    Console.WriteLine("HDD: " + (hardwareItem.Sensors[0].Value).ToString());
                }

                if (hardwareItem.HardwareType == HardwareType.RAM)
                {
                    hardwareItem.Update();
                    send_str((hardwareItem.Sensors[0].Value).ToString() + "{C;"); //HDD TOTAL TEMP
                    Console.WriteLine("RAM: " + (hardwareItem.Sensors[0].Value).ToString());
                }

                //cpuCounter.InstanceName = "_Total";
                //send_str((int)cpuCounter.NextValue() + ":");

                //ramCounter.CounterName = "% Committed Bytes In Use";
                //ramCounter.CategoryName = "Memory";
                //send_str((int)ramCounter.NextValue() + ";");
            }
        }
    
        private void send_str(String str)
        {
            int iterator = 0;
            byte[] bTab = new byte[str.Length];
            byte[] bBufor = new byte[str.Length];

            foreach (char c in str)
            {
                bTab[iterator] = (byte)c;
                iterator++;
            }

            iterator = 0;
            if (!serialPort1.IsOpen) return; //jezeli port zamknięty to nie wysyłaj
            foreach (char c in bTab)
            {
                serialPort1.Write(bTab, iterator, 1);
                Console.WriteLine("Wysłam: " + bTab[iterator]);
                if (bBufor[0] == bTab[0])
                    Console.WriteLine("Potwierdzam!");
                else
                    Console.WriteLine("ERROR! " + bBufor);
                iterator++;
            }
        }
    }
}
