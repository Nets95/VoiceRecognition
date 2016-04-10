using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using NAudio.Wave;
using NAudio.FileFormats;
using MathNet.Numerics.IntegralTransforms;
using AForge.Math;

namespace Ton
{
    public partial class Form1 : Form
    {
        public WaveIn waveSource = null;
        public WaveFileWriter waveFile = null;

        public Form1()
        {
            InitializeComponent();
        }

        

        private void button1_Click(object sender, EventArgs e)
        {

            
            btnStart.Enabled = false;
            btnStop.Enabled = true;

            waveSource = new WaveIn();
            waveSource.WaveFormat = new WaveFormat(44100, 1);

            waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
            waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_RecordingStopped);

            waveFile = new WaveFileWriter(@"D:\Test0001.wav", waveSource.WaveFormat);
            BufferedWaveProvider buf = new BufferedWaveProvider(waveSource.WaveFormat);


            waveSource.StartRecording();



        }
        public static double[] data;
        public static double[] Result;


        public double[] FuncSin()
        {
            int sampleRate = 8000;
            double[] buffer = new double[8000];
            double amplitude = 0.25 * short.MaxValue;
            double frequency = 1000;
            for (int n = 0; n < buffer.Length; n++)
            {
                buffer[n] = (short)(amplitude * Math.Cos ((2 * Math.PI * n * frequency) / sampleRate));
            }
            return buffer;
        }
        static double[] Sin;
        public static double[] GetResult()
        {
            return Sin;
        }
        public static double[] GetData()
        {
            return data;
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            btnStop.Enabled = false;

            waveSource.StopRecording();
        }
        void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {

            if (waveFile != null)
            {
                waveFile.Write(e.Buffer, 0, e.BytesRecorded);
                waveFile.Flush();

            }

        }
       
        void waveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (waveSource != null)
            {
                waveSource.Dispose();
                waveSource = null;
            }

            if (waveFile != null)
            {
                waveFile.Dispose();
                waveFile = null;
            }


            var strem = new MemoryStream(File.ReadAllBytes(@"D:\Test0001.wav"));
            WaveFileReader wave = new WaveFileReader(strem);

             data = new double[wave.SampleCount];
            int i = 0;
            for (var block = wave.ReadNextSampleFrame(); block != null; block = wave.ReadNextSampleFrame())
            {
                data[i] = block[0];
                i++;
            }
           // double k = 0.0;// 1.0 / 44100.0;
            double krok = 0.0;

            for (int io = 0; io < data.Length; io++)
            {
                //k = 1/44100;
                chart1.Series[0].Points.AddXY(krok, data[io]);
                krok += 1.0/44100.0;
            }






            
            double krokForDouble = 0.0;

            
            double fn = 8000;
            int ifn = Convert.ToInt32(fn);
            int p2 = (int)wave.SampleCount;
           
            
             double[] x = new double[data.Length];
             double[] y = new double[data.Length];
             int siz = 2 * data.Length;
            

            Result = new double[2*data.Length];



            //for (int j = 0; j < data.Length; j++)
            //{
            //    x[j] = data[j];


            //}
            //int Krok = 0;
            
            //for (int j = 0; j < Sin.Length; j += 2)
            //{
            //    x[Krok] = Sin[j];
            //    Krok++;
            //}
            //Krok = 0;
            //for (int j = 1; j <= Sin.Length - 1; j += 2)
            //{
            //    //y[Krok] = 0;
            //    y[Krok] = Sin[j];
            //    Krok++;
            //}
            //double m = 15;
            // m = Math.Round(Math.Log(data.Length/2,2));

            // NeedForSpeed ob = new NeedForSpeed();

            // NeedForSpeed.init((uint)m);
            //Result =  NeedForSpeed.run( ref x,  ref y, false);

            
            int iter = 0;
            Sin = data;
            System.Numerics.Complex[] com = new System.Numerics.Complex[Sin.Length];
            
            for (int d = 0; d < Sin.Length; d++)
            {
                double doubleValue = Sin[d];
                com[d] = doubleValue;   
            }
            FFTAforge.FFT(ref com);
            //MessageBox.Show(com[0].Real.ToString());

            double compVale = 0.0;
            for (int d = 0; d < Sin.Length; d++)
            {
                compVale = com[d].Magnitude;
                Sin[d] = compVale;
            }
            double window = 0;
            for (int d = 0; d < Sin.Length/2; d++)
            {
                window = 0.54 - 0.46 * Math.Cos((2 * Math.PI * d) / (12 - 1));
                chart2.Series[0].Points.AddXY(iter, window*Sin[d]);
                iter++;
            }

            double[] mfc;
           
            mfc = MFCC.mfcc(Sin);
            double suma = 0;
            int count = 0;
            
            chart3.Series[0].Points.Clear();
            for (int d = 0; d < 12; d++)
            {
                    suma += Math.Abs(mfc[d]);
                    count++;
                 
                 chart3.Series[0].Points.AddXY(d, Math.Abs(mfc[d]));
                iter++;
            }
            double a, b = 0.0;
            a = suma / count;
            if (a < 0.5+0.25)
                MessageBox.Show("Men");
            else
                if ((a > 0.5+0.25)&&(a<1+0.25))
                MessageBox.Show("Woomen");
          //  MessageBox.Show((suma / count).ToString());

            btnStart.Enabled = true;
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }
    }

}
