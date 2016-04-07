using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge.Math;
namespace Ton
{
   public class LastFFT
    {
       public enum Direction
       {
           /// <summary>
           /// Forward direction of Fourier transformation.
           /// </summary>
           Forward = 1,

           /// <summary>
           /// Backward direction of Fourier transformation.
           /// </summary>
           Backward = -1
       };		
       public static void FFT(Complex[] data, Direction direction)
       {
           int n = data.Length;
           int m = Tools.Log2(n);

           // reorder data first
           ReorderData(data);

           // compute FFT

           int tn = 1, tm;

           for (int k = 1; k <= m; k++)
           {
               Complex[] rotation = LastFFT.GetComplexRotation(k, direction);

               tm = tn;
               tn <<= 1;

               for (int i = 0; i < tm; i++)
               {
                   Complex t = rotation[i];

                   for (int even = i; even < n; even += tn)
                   {
                       int odd = even + tm;
                       Complex ce = data[even];
                       Complex co = data[odd];

                       double tr = co.Re * t.Re - co.Im * t.Im;
                       double ti = co.Re * t.Im + co.Im * t.Re;

                       data[even].Re += tr;
                       data[even].Im += ti;

                       data[odd].Re = ce.Re - tr;
                       data[odd].Im = ce.Im - ti;
                   }
               }
           }

           if (direction == Direction.Forward)
           {
               for (int i = 0; i < n; i++)
               {
                   data[i].Re /= (double)n;
                   data[i].Im /= (double)n;
               }
           }
       }


       private const int minLength = 2;
       private const int maxLength = Int32.MaxValue;
       private const int minBits = 1;
       private const int maxBits = 14;
       private static int[][] reversedBits = new int[maxBits][];
       private static Complex[,][] complexRotation = new Complex[maxBits, 2][];
       
       
       
       private static int[] GetReversedBits(int numberOfBits)
       {
           if ((numberOfBits < minBits) || (numberOfBits > maxBits))
               throw new ArgumentOutOfRangeException();

           // check if the array is already calculated
           if (reversedBits[numberOfBits - 1] == null)
           {
               int n = Tools.Pow2(numberOfBits);
               int[] rBits = new int[n];

               // calculate the array
               for (int i = 0; i < n; i++)
               {
                   int oldBits = i;
                   int newBits = 0;

                   for (int j = 0; j < numberOfBits; j++)
                   {
                       newBits = (newBits << 1) | (oldBits & 1);
                       oldBits = (oldBits >> 1);
                   }
                   rBits[i] = newBits;
               }
               reversedBits[numberOfBits - 1] = rBits;
           }
           return reversedBits[numberOfBits - 1];
       }

       private static Complex[] GetComplexRotation(int numberOfBits, Direction direction)
       {
           int directionIndex = (direction == Direction.Forward) ? 0 : 1;

           // check if the array is already calculated
           if (complexRotation[numberOfBits - 1, directionIndex] == null)
           {
               int n = 1 << (numberOfBits - 1);
               double uR = 1.0;
               double uI = 0.0;
               double angle = System.Math.PI / n * (int)direction;
               double wR = System.Math.Cos(angle);
               double wI = System.Math.Sin(angle);
               double t;
               Complex[] rotation = new Complex[n];

               for (int i = 0; i < n; i++)
               {
                   rotation[i] = new Complex(uR, uI);
                   t = uR * wI + uI * wR;
                   uR = uR * wR - uI * wI;
                   uI = t;
               }

               complexRotation[numberOfBits - 1, directionIndex] = rotation;
           }
           return complexRotation[numberOfBits - 1, directionIndex];
       }

       // Reorder data for FFT using
       private static void ReorderData(Complex[] data)
       {
           int len = data.Length;

           // check data length
           if ((len < minLength) || (len > maxLength) || (!Tools.IsPowerOf2(len)))
               throw new ArgumentException("Incorrect data length.");

           int[] rBits = GetReversedBits(Tools.Log2(len));

           for (int i = 0; i < len; i++)
           {
               int s = rBits[i];

               if (s > i)
               {
                   Complex t = data[i];
                   data[i] = data[s];
                   data[s] = t;
               }
           }
       }
    }
}
