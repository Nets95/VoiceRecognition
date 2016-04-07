using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ton
{
   public class fft
    {
      public  void fft_make(int p, double[] c)   // показатель двойки (например, для БПФ на 256 точек это 8) // результирующий массив поворотных множителей c[1 << p]
     {
          int n, i;
          double w, f;
          n = 1 << p; // размер массива (число точек БПФ)
          w = (2 * Math.PI) / (float) n;
          f = 0;
          for (i = 0; i < n; i++)
          {
                c[i++] =  Math.Cos(f);
                
                c[i++] = -Math.Sin(f);
                
                f += w;
          }
      }
//----------------------------------------------------------------------------
// функция расчёта поворотных множителей для ОБПФ
public void fft_make_reverse(int p,double[] c)
      // показатель двойки (например, для ОБПФ на 256 точек это 8)
   // результирующий массив поворотных множителей c[1 << p]
{
  int n, i;
  double w, f;
  n = 1 << p; // размер массива (число точек ОБПФ)
  w = (2 * Math.PI) / (float) n;
  f = 0;
  for (i = 0; i < n; i++)
  {
    c[i++] = Math.Cos(f);
      
    c[i++] = Math.Sin(f);
      
    f += w;
  }
}
//----------------------------------------------------------------------------
// функция прямого БПФ
public void fft_calc(int p, double[] c,  double[] In, double[] Out, bool norm)
             // показатель двойки (например, для БПФ на 256 точек это 8)
    // массив поворотных множителей БПФ
   // входной массив
        // выходной массив
          // признак нормировки
{
  int i;
  int n = 1 << p;  // число точек БПФ
  int n2 = n >> 1; // n/2
  double re, im, re1, im1, re2, im2; // c, c1, c2
  double[] p1 = new double[Out.Length];
  double[] p2 = new double[Out.Length];
  double p0;

  // копировать элементы массива `in` в массив `out` с битовой инверсией
  for (i = 0; i < n; i++)
  {
    int j = fft_binary_inversion(p, i) << 1;
    int k = i << 1;

    p0 = In[i]  + j;
    p1[i] = Out[i] + k; 
    p1[i++] = p0++; // out[i] = in[j]
      
    p1[i]   = p0;   //

    p0 = In[i]  + k;
    p1[i] = Out[i] + j; 
    p1[i++]= p0++; // out[j] = in[i]
      
    p1[i]   = p0;   //
  }
  
  // выполнение бабочек ("понеслась душа в рай" (C) Hokum)
  for (i = 0; i < p; i++)
  {
    int m = 1 << (i + 1); // через сколько эл-тов бабочка * 2
    int r = m << 1;       // размер группы * 2
    int nom = 0;          // номер группы * r
    int k = 0;            // номер эл-та в группе * 2
    int y = 0;            // индекс W * 2
    int z = 0;
    int h = 1 << (p - i); // шаг для W * 2
    int j;

    for (j = n2; j > 0; j--)
    {
      if (k >= m)
      {
        k = y = 0;
        nom += r;
	    z = nom;
      }

      // c <= c[y]
      p0 = c[j] + y;
      re = p0++;
      im = p0;

      // c2 <= out[z + m]
      p1[i]  = Out[j] + (z + m);
      re2 = p1[i++];
      im2 = p1[i++];
   
      // c1 <= c2 * c
      re1 = re2 * re - im2 * im;
      im1 = im2 * re + re2 * im;

      // c2 <= out[z]
      p2[i]  = Out[j] + z;
      re2 = p2[i++];
      im2 = p2[i];

      // out[z]     <= c2 + c1
      // out[z + m] <= c2 - c1
      p2[i--] = im2 + im1;
        
      p1[i--] = im2 - im1;
        
      p2[i]   = re2 + re1;
      p1[i]   = re2 - re1;

      k += 2;
      z += 2;
      y += h;
    }
  }

  if (norm)
  { // нормировка
    re = 1 / ((float) n);
    p1 = Out;
    for (i = n; i > 0; i--)
    {
      p1[i] *= re;  
      p1[i] *= re;
        
    }
  }
}
//----------------------------------------------------------------------------
// функция перестановки отсчётов спектра (что бы "0" в центре)
void fft_shift(int p, double[] data)
         // показатель двойки (например, для БПФ на 256 точек это 8)
   // массив после БПФ
{
  int n = 1 << p; // число точек БПФ
  double[] buf = new double[2 * n];
  if (buf == null)
  {
	//FFT_DBG("fft.c: malloc return NULL");
  }
  else
  {
      for (int i = 0; i < buf.Length; i += sizeof(float) * 2 * n)
      {
          buf[i] = data[i];
          data[i + n] = buf[i];
          data[i] = buf[i + n];
      }
    
  }
}
       public  int fft_binary_inversion(
  int p, // показатель двойки (например, для БПФ на 256 точек это 8)
  int i) // исходный индекс [0, (1 << p) - 1]
{
  int j = 0;
  while (p-- > 0)
  {
    j <<= 1;
    j |= i & 1;
    i >>= 1;
  }
  return j;
}

    }
}
