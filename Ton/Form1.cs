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
        static double[] data;
        static double[] Result;
        public static double[] GetResult()
        {
            return Result;
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
        FFTransform ffTransforme = new FFTransform();
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






            // Object of FFTransform class;
           ///
          ///  ffTransforme.RealFFT(data, true);
            var window = 0.0;
            double krokForDouble = 0.0;

            //Object of AFFT classs;
           // AFFT secondOne = new AFFT();
            //secondOne.frequencies(data);

            double fn = 8000;
            int ifn = Convert.ToInt32(fn);
            int p2 = (int)wave.SampleCount;
            ////////////////////////////////////////////////////////////////////////////////////////////////////
            //FFT MMA = new FFT();
            
            //double[] c = new double[data.Length];
            //double[] Out = new double[data.Length];
            
            
            //fft End = new fft();
            //End.fft_make(p2, c);
            //End.fft_calc(p2, c, data, Out, true);
            //for (int d = 0; d < data.Length; d++)
            //{
            //   // window = 0.54 - 0.46 * Math.Cos((2 * Math.PI * d) / data.Length - 1);
            //    chart2.Series[0].Points.AddXY(d, Math.Abs(data[d]));
            //}
            ///////////////////////////////////////////////////////////////////////////////////////////////////
            double[] x = new double[data.Length];
            double[] y = new double[data.Length];
            int siz = 2 * data.Length;
            Result = new double[2*data.Length];

            int Krok = 0;
            
            for (int j = 0; j < data.Length; j++)
            {
                x[j] = data[j];
               
            }
            //for (int j = 0; j < data.Length; j+=2)
            //{
            //    x[Krok] = data[j];
            //    Krok++;
            //}
            //Krok = 0;
            //for (int j = 1; j <= data.Length-1; j+=2)
            //{
            //    y[Krok] = 0;
            //    //y[Krok] = data[j];
            //    Krok++;
            //}
            double m = 15;
            // m = Math.Round(Math.Log(data.Length/2,2));

            NeedForSpeed ob = new NeedForSpeed();

            NeedForSpeed.init((uint)m);
            NeedForSpeed.run( ref x,  ref y, false);

        //fastFourierTransform ob = new fastFourierTransform();
        //ob.FFT(1, (long)m, x, y);
        //Krok = 0;
        // for (int j = data.Length-1; j > 0 ; j-=2)
        //{
        //    Result[j] = x[Krok];
        //    Krok++;
        //}
        // Krok = 0;
        // for (int j = data.Length-2; j >= 1 ; j-=2)
        //{
        //    Result[j] = y[Krok];
        //    Krok++;
        //}
        //Krok = 0;

        //for (int j = 0; j < data.Length - 2; j += 2)
        //{
        //    Result[j] = x[Krok];
        //    Krok++;
        //}
        //for (int j = 0; j < data.Length - 2; j += 2)
        //{
        //    Result[j] = x[Krok];
        //    Krok++;
        //}
        //Krok = 0;
        //for (int j = 1; j < data.Length - 2; j += 2)
        //{
        //    Result[j] = y[Krok];
        //    Krok++;
        //}
        int k = 1;
            int p = 0;
           // double max = Result[0];
            //for (int d = 0; d < Result.Length; d++)
            //{
            //    if (max < Result[d]) { max = Result[d]; }
            //}
            for (int j = 0; j < (data.Length/2)-2; j++)
             {
                 if (k > (data.Length / 2) - 1)
                     break;
                window = 0.54 - 0.46 * Math.Cos((2 * Math.PI * j) / Result.Length - 1);
                Result[j] = window * Math.Sqrt(x[p] * x[p] + y[p] * y[p]);
                 //k += 1;
                 p ++; 
             }

            int iter = 0;


           

            for (int d = 0; d <22050; d++)
            {
                chart2.Series[0].Points.AddXY(iter, Result[d]);
                iter++;
            }

            double[] mc = MFCC.mfcc(data);
            double suma = 0;
            int count = 0;
            for (int d = 0; d < 12; d++)
            {
                    suma += Math.Abs(mc[d]);
                    count++;
                
                chart3.Series[0].Points.AddXY(d, mc[d]);
                iter++;
            }
            //if (suma < )
            //    MessageBox.Show("Men");
            //else
            //    MessageBox.Show("Woomen");
            MessageBox.Show((suma/count).ToString());
            // 23:26:41
            //оце кусок коду,який відповідає просто за спектрограмму
            //Вова набирає повідомлення..


            //for (int j = 0; j < ffTransforme.Data.Length; j+=2)
            //{
            //    if (ffTransforme.Data[j] > 0)
            //        chart2.Series[0].Points.AddXY(j, ffTransforme.Data[j]*window);
            //        //chart2.Series[0].Points.AddXY(j, secondOne.Result[j]);

            //}

            btnStart.Enabled = true;
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }
    }

}
