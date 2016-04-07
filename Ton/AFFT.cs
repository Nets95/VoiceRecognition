using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ton
{
   public  class AFFT
    {
        public struct polar1
        {
            public double real;
            public double img;

        };
        public double[] Result;
       private float Fs;
       private int N;
       private polar1 [] F;
       private int R;

       public void DSPclass(float[] DSP1,int f1)
        {
            N = DSP1.Length;
            R = DSP1.Length;
           F = new polar1[N];
           Fs = (float)f1;
         
        }

       public void FFT1(float[] DSP1)
       {
         polar1[] x = new polar1[DSP1.Length];
            for (int v = 0; v < N; v++)
           {
               x[v].real = DSP1[v];
               x[v].img = 0;
           } 
          
          F = FFT(x);
          int temp;
        }       


       public polar1[] FFT(polar1[] x)
       {
           int N2 = x.Length;
           polar1[] X = new polar1[N2];
           if (N2 == 1)
           {
               return x;
           }
           polar1[] odd = new polar1[N2 / 2];
           polar1[] even = new polar1[N2 / 2];
           polar1[] Y_Odd = new polar1[N2 / 2];
           polar1[] Y_Even = new polar1[N2 / 2];


           for (int t = 0; t < N2 / 2; t++)
           {
               even[t].img = x[t * 2].img;
               even[t].real = x[t * 2].real;
               odd[t].img = x[(t * 2) + 1].img;
               odd[t].real = x[(t * 2) + 1].real;
           }
           Y_Even = FFT(even);
           Y_Odd = FFT(odd);
           polar1 temp4;

           for (int k = 0; k < (N2 / 2); k++)
           {
               temp4 = Complex1(k, N2);
               X[k].real = Y_Even[k].real + (Y_Odd[k].real * temp4.real);
               X[k + (N2 / 2)].real = Y_Even[k].real - (Y_Odd[k].real * temp4.real);  
               X[k].img = Y_Even[k].img + (Y_Odd[k].real * temp4.img);
                X[k + (N2 / 2)].img = Y_Even[k].img - (Y_Odd[k].real * temp4.img);
               }

           return X;
       }


  public polar1 Complex2(int K, int N, int F3)
        {
            polar1 temp;
            double temp1;
            temp1 = (2D * K *F3) / N;
            if (temp1 % 2 == 0 || temp1 == 0)
            {
                temp.real = 1D;
                temp.img = 0D;

            }
            else if ((temp1 - 1) % 2 == 0)
            {
                temp.real = -1D;
                temp.img = 0D;
            }
            else if ((temp1 / .5D) - 1 % 2 == 0)
            {
                if ((temp1 - .5D) % 2 == 0)
                {
                    temp.real = 0D;
                    temp.img = -1D;
                }
                else
                {
                    temp.real = 0D;
                    temp.img = 1D;
                }
            }
            else
            {
                temp.real = Math.Cos(temp1 * Math.PI);
                temp.img = -1D * Math.Sin(temp1 * Math.PI);
            }


            return temp;

        }
        public polar1 Complex1(int K, int N3)
        {
           polar1 temp;
            double temp1;
            temp1 = (2D * Math.PI *K) / N3;
                    temp.real =  Math.Cos(temp1);
                    temp.img =  Math.Sin(temp1);
            return temp;

        }
       
        public int Apm(double[] img, double[] real)
        {
          
            for (int i = 0; i < R; i++)
            {
            img[i] = F[i].img;
            real[i] = F[i].real;
            }

            return R;
        }


        public int frequencies(double[] freq)
        {
            Result = new double[freq.Length];
            bool flag = false;
            bool flagD = false;
            float tempOld = 0;
            float tempNew =0;
            int tempc = 0;
            int counter = 0;
            for (int i = 0; i < R; i++)
                    {
                        if (((i / N) * Fs) >= (Fs / 2))
                        {
                            return counter;
                        }
                if ((int)F[i].img != 0 )
                    {
                    flag = true;
                    tempOld = (float)(Math.Abs(F[i].img));
                    }
                else
                    {
                    if (flagD == true)
                    {
                        freq[counter] = ((float)tempc / (float)N) * Fs;
                        Result[counter] = freq[counter];
                       // Ctemp[counter] = tempNew; //magnitude(F[tempc]);
                        counter++;
                        flagD = false;
                    }
                    flag = false;
                    tempOld = 0;
                    tempNew = 0;
                    }
                if(flag == true)
                    {
                    if (tempOld > tempNew)
                    {
                        tempNew = tempOld;
                        tempc = i;
                        flagD = true;
                    }
                    }
                    }
            return counter;  
        }

    }
    }

