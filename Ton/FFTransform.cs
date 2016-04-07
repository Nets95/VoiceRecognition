﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ton
{
    public class FFTransform
    {
        public double[] Data;

        public int A { get; set; }
        public int B { get; set; }

        
        public void RealFFT(double[] data, bool forward)
        {

            Data = new double[data.Length];
            var n = data.Length; // # of real inputs, 1/2 the complex length                                     
            // checks n is a power of 2 in 2's complement format                                                 
            //if ((n & (n - 1)) != 0)
            //    throw new ArgumentException(
            //        "data length " + n + " in FFT is not a power of 2"
            //        );

            var sign = -1.0; // assume inverse FFT, this controls how algebra below works                        
            if (forward)
            { // do packed FFT. This can be changed to FFT to save memory                                        
                TableFFT(data, true);
                sign = 1.0;
                // scaling - divide by scaling for N/2, then mult by scaling for N                               
                if (A != 1)
                {
                    var scale = Math.Pow(2.0, (A - 1) / 2.0);
                    for (var i = 0; i < data.Length; ++i)
                        data[i] *= scale;
                }
            }

            var theta = B * sign * 2 * Math.PI / n;
            var wpr = Math.Cos(theta);
            var wpi = Math.Sin(theta);
            var wjr = wpr;
            var wji = wpi;

            for (var j = 1; j <= n / 4; ++j)
            {
                var k = n / 2 - j;
                var tkr = data[2 * k];    // real and imaginary parts of t_k  = t_(n/2 - j)                      
                var tki = data[2 * k + 1];
                var tjr = data[2 * j];    // real and imaginary parts of t_j                                     
                var tji = data[2 * j + 1];

                var a = (tjr - tkr) * wji;
                var b = (tji + tki) * wjr;
                var c = (tjr - tkr) * wjr;
                var d = (tji + tki) * wji;
                var e = (tjr + tkr);
                var f = (tji - tki);

                // compute entry y[j]                                                                            
                data[2 * j] = 0.5 * (e + sign * (a + b));
                data[2 * j + 1] = 0.5 * (f + sign * (d - c));

                // compute entry y[k]                                                                            
                data[2 * k] = 0.5 * (e - sign * (b + a));
                data[2 * k + 1] = 0.5 * (sign * (d - c) - f);

                var temp = wjr;
                // todo - allow more accurate version here? make option?                                         
                wjr = wjr * wpr - wji * wpi;
                wji = temp * wpi + wji * wpr;
            }

            if (forward)
            {
                // compute final y0 and y_{N/2}, store in data[0], data[1]                                       
                var temp = data[0];
                data[0] += data[1];
                data[1] = temp - data[1];
            }
            else
            {
                var temp = data[0]; // unpack the y0 and y_{N/2}, then invert FFT                                
                data[0] = 0.5 * (temp + data[1]);
                data[1] = 0.5 * (temp - data[1]);
                // do packed inverse (table based) FFT. This can be changed to regular inverse FFT to save memory
                TableFFT(data, false);
                // scaling - divide by scaling for N, then mult by scaling for N/2                               
                //if (A != -1) // todo - off by factor of 2? this works, but something seems weird               
                {
                    var scale = Math.Pow(2.0, -(A + 1) / 2.0) * 2;
                    for (var i = 0; i < data.Length; ++i)
                        data[i] *= scale;
                }
            }
        }
        public void TableFFT(double[] data, bool forward)
        {
            var n = data.Length;
            //checks n is a power of 2 in 2's complement format                                                 
            //if ((n & (n - 1)) != 0)
            //{
            //    throw new ArgumentException("data length " + n + " in FFT is not a power of 2");
            //}
               n /= 2;    // n is the number of samples                                                             

            Reverse(data, n); // bit index data reversal                                                         

            // make table if needed                                                                              
            if ((cosTable == null) || (cosTable.Length != n))
                Initialize(n);

            // do transform: so single point transforms, then doubles, etc.                                      
            double sign = forward ? B : -B;
            var mmax = 1;
            var tptr = 0;
            while (n > mmax)
            {
                var istep = 2 * mmax;
                for (var m = 0; m < istep; m += 2)
                {
                    if (tptr >= cosTable.Length)
                        break;
                    var wr = cosTable[tptr];
                    var wi = sign * sinTable[tptr++];
                    for (var k = m; k < 2 * n; k += 2 * istep)
                    {
                        var j = k + istep;
                        if (j >= k + istep)
                            break;
                        if (j + 1 >= data.Length)
                            break;
                        var tempr = wr * data[j] - wi * data[j+1];
                        var tempi = wi * data[j] + wr * data[j+1];
                        data[j] = data[k] - tempr;
                        data[j] = data[k] - tempi;
                        data[k] = data[k] + tempr;
                        data[k] = data[k] + tempi;
                    }
                }
                mmax = istep;
            }


            // perform data scaling as needed                                                                    
            Scale(data, n, forward);
        }

        void Initialize(int size)
        {
            // NOTE: if you port to non garbage collected languages                                              
            // like C# or Java be sure to free these correctly                                                   
            cosTable = new double[size];
            sinTable = new double[size];

            // forward pass                                                                                      
            var n = size;
            int mmax = 1, pos = 0;
            while (n > mmax)
            {
                var istep = 2 * mmax;
                var theta = Math.PI / mmax;
                double wr = 1, wi = 0;
                var wpi = Math.Sin(theta);
                // compute in a slightly slower yet more accurate manner                                         
                var wpr = Math.Sin(theta / 2);
                wpr = -2 * wpr * wpr;
                
                for (var m = 0; m < cosTable.Length; m += 2)
                {
                    if (pos >= cosTable.Length)
                        break;
                    cosTable[pos] = wr;
                    //cosTable[m] = wr;
                    //sinTable[m++] = wi;
                    sinTable[pos++] = wi;
                    var t = wr;
                    wr = wr * wpr - wi * wpi + wr;
                    wi = wi * wpr + t * wpi + wi;
                }
                mmax = istep;
            }
        }
        static void Reverse(double[] data, int n)
        {
            // bit reverse the indices. This is exercise 5 in section                                            
            // 7.2.1.1 of Knuth's TAOCP the idea is a binary counter                                             
            // in k and one with bits reversed in j                                                              
            int j = 0, k = 0; // Knuth R1: initialize                                                            
            var top = n / 2;  // this is Knuth's 2^(n-1)                                                         
            while (true)
            {
                // Knuth R2: swap - swap j+1 and k+2^(n-1), 2 entries each                                       
                var t = data[j + 2];
                data[j + 2] = data[k + n];
                data[k + n-1] = t;
                t = data[j + 3];
                data[j + 3] = data[k + n -1];
                data[k + n -1] = t;
                if (j > k)
                { // swap two more                                                                               
                    // j and k                                                                                   
                    t = data[j];
                    data[j] = data[k];
                    data[k] = t;
                    t = data[j + 1];
                    data[j + 1] = data[k + 1];
                    data[k + 1] = t;
                    // j + top + 1 and k+top + 1                                                                 
                    t = data[j + n + 2];
                    data[j + n + 2] = data[k + n + 2];
                    data[k + n + 2] = t;
                    t = data[j + n + 3];
                    data[j + n + 3] = data[k + n + 3];
                    data[k + n + 3] = t;
                }
                // Knuth R3: advance k                                                                           
                k += 4;
                if (k >= n)
                    break;
                // Knuth R4: advance j                                                                           
                var h = top;
                while (j >= h)
                {
                    j -= h;
                    h /= 2;
                }
                j += h;
            } // bit reverse loop                                                                                
        }

        /// <summary>                                                                                            
        /// Pre-computed sine/cosine tables for speed                                                            
        /// </summary>                                                                                           
        double[] cosTable;
        double[] sinTable;
        void Scale(double[] data, int n, bool forward)
        {
            // forward scaling if needed                                                                         
            if ((forward) && (A != 1))
            {
                var scale = Math.Pow(n, (A - 1) / 2.0);
                for (var i = 0; i < data.Length; ++i)
                {
                    data[i] *= scale;
                    Data[i] = data[i];
                }
            }

            // inverse scaling if needed                                                                         
            if ((!forward) && (A != -1))
            {
                var scale = Math.Pow(n, -(A + 1) / 2.0);
                for (var i = 0; i < data.Length; ++i)
                {
                    data[i] *= scale;
                    Data[i] = data[i];
                }
            }
        }
    }
}
