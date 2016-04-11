using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ton
{
    public class MFCC
    {
        static int my_rint(double x)
        {
            if (2 * x == (double)Math.Round(2 * x))
                x += 0.0001;

            return ((int)Math.Round(x));
        }

        static public double[] mfcc(double[] signal)
        {
            double lowestFrequency = 133.3333;
            double linearFilters = 13;
            double linearSpacing = 66.66666666;
            double logFilters = 27;
            double logSpacing = 1.0711703;
            double fftSize = 512;
            double cepstralCoefficients = 13;
            double windowSize = 256;
            double samplingRate = 44100;
            double totalFilters;
            double[] mfccFilterWeights;
            double[] freqs;
            double[] triangleHeight;
            double[] lower;
            double[] upper;
            double[] center;
            double[] fftFreqs;
            double[] hamWindow;
            double[] mfccDCTMatrix;
            double[] preEmphasized;
            double first;
            double last;
            double[] fftData;
            double[] empreinte;
            double maxi;
            double[] earMag;
            double[] ceps;
            double loga;
            double windowStep;
            double cols;
            double frameRate = 100;
            double[] result;

            int i, j, k;


            preEmphasized = new double[1024];

            totalFilters = linearFilters + logFilters;
            freqs = new double[(int)totalFilters + 2];
            for (i = 0; i < linearFilters; i++)
                freqs[i] = lowestFrequency + i * linearSpacing;
            for (i = (int)linearFilters; i < (int)totalFilters + 2; i++)
                freqs[i] = freqs[(int)linearFilters - 1] * Math.Pow(logSpacing, i - linearFilters + 1);
            lower = new double[(int)totalFilters];
            upper = new double[(int)totalFilters];
            center = new double[(int)totalFilters];
            triangleHeight = new double[(int)totalFilters];
            fftData = new double[(int)fftSize];

            for (i = 0; i < (int)totalFilters; i++)
                lower[i] = freqs[i];
            for (i = 1; i < (int)totalFilters + 1; i++)
                center[i - 1] = freqs[i];
            for (i = 2; i < (int)totalFilters + 2; i++)
                upper[i - 2] = freqs[i];
            hamWindow = new double[(int)windowSize];
            mfccFilterWeights = new double[(int)(totalFilters * fftSize)];
            for (i = 0; i < totalFilters; i++)
                triangleHeight[i] = 2 / (upper[i] - lower[i]);
            fftFreqs = new double[(int)fftSize];
            for (i = 0; i < fftSize; i++)
                fftFreqs[i] = (i / fftSize) * samplingRate;

            for (i = 0; i < totalFilters; i++)
            {
                for (j = 0; j < (int)fftSize; j++)
                {
                    if ((fftFreqs[j] > lower[i]) && (fftFreqs[j] <= center[i]))
                        mfccFilterWeights[(int)fftSize * i + j] = triangleHeight[i] * (fftFreqs[j] - lower[i]) / (center[i] - lower[i]);
                    if ((fftFreqs[j] > center[i]) && (fftFreqs[j] < upper[i]))
                        mfccFilterWeights[(int)fftSize * i + j] = (triangleHeight[i] * (fftFreqs[j] - lower[i]) / (center[i] - lower[i])) + (triangleHeight[i] * (upper[i] - fftFreqs[j]) / (upper[i] - center[i]));
                }
            }

            for (i = 0; i < windowSize; i++)
                hamWindow[i] = 0.54 - 0.46 * Math.Cos(2 * Math.PI * i / windowSize);
            mfccDCTMatrix = new double[(int)(cepstralCoefficients * totalFilters)];
            for (i = 0; i < cepstralCoefficients; i++)
                for (j = 0; j < totalFilters; j++)
                    mfccDCTMatrix[j * (int)cepstralCoefficients + i] = 1 / Math.Sqrt(totalFilters / 2) * Math.Cos(i * (2 * j + 1) * Math.PI / 2 / totalFilters);
            for (i = 0; i < totalFilters; i++)
                mfccDCTMatrix[i * (int)cepstralCoefficients] *= 1 / Math.Sqrt(2);
            for (i = 1; i < 1024; i++)
                preEmphasized[i] = signal[i] + signal[i - 1] * -0.97;
            preEmphasized[0] = signal[0];


            earMag = new double[(int)totalFilters];
            windowStep = samplingRate / frameRate;
            cols = (int)((1024 - windowSize) / windowStep);
            ceps = new double[(int)(sizeof(double) * cols * linearFilters)];
            for (i = 0; i < cols; i++)
            {
                first = i * windowStep + 1;
                last = first + windowSize - 1;
                for (j = 0; j < fftSize; j++)
                    fftData[j] = 0;
                for (j = 0; j < windowSize; j++)
                    fftData[j] = preEmphasized[(int)my_rint(first + j - 1)] * hamWindow[j];
                Form1 obj = new Form1();
                double[] fftMag = Form1.GetResult(); // FourierTransform.FFT(ref signal);
                for (j = 0; j < totalFilters; j++)
                {
                    loga = 0;
                    for (k = 0; k < fftSize; k++)
                        loga += fftMag[k] * mfccFilterWeights[k + j * (int)fftSize];

                    earMag[j] = Math.Log10(loga);
                }
                for (j = 0; j < linearFilters; j++)
                {
                    loga = 0;
                    for (k = 0; k < totalFilters; k++)
                        loga +=
                          earMag[k] * mfccDCTMatrix[j + k * (int)linearFilters];
                    ceps[j + i * (int)linearFilters] = loga;
                }
            }


            empreinte = new double[12];

            for (i = 1; i < 13; i++)
                empreinte[i - 1] = (ceps[i] + ceps[i + 13] + ceps[i + 26] + ceps[i + 39] + ceps[i + 52] + ceps[i + 65]);

            maxi = 0;
            for (i = 0; i < 12; i++)
            {
                if (maxi * maxi < empreinte[i] * empreinte[i])
                    maxi = empreinte[i];
            }
            for (i = 0; i < 12; i++)
                empreinte[i] = (empreinte[i] / Math.Abs(maxi)) * 2;

            return empreinte;

        }
      
    }
}
