using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace PrecursorIonClassLibrary.Algorithm
{
    public class CWT
    {
        public static double[] Convolve(double[] u, double[] v)
        {
            int m = u.Length;
            int n = v.Length;
            int k = n + m -1;

            double[] w = new double[k];
            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (i-j >= 0 && i-j < n)
                    {
                        w[i] += u[j] * v[i-j];
                    }
                }
            }
            int skip = k - Math.Max(m, n) - (int) Math.Floor(Math.Min(m, n) / 2.0);
            return w.Skip(skip).Take(Math.Max(m, n)).ToArray();
        }

        public static double[] Transform(double[] x, double a)
        {

            int size = (int)Math.Ceiling(Math.Min(10 * a, x.Length));
            double[] points = new double[size];

            for (int i = 0; i < size; i++)
            {
                points[i] = i - (size - 1) / 2.0;
            }

            double[] psi = MaxicanHat.Wavelet(points, a).Reverse().ToArray();
                 
            // compute convolution of signal and wavelet
            return Convolve(x, psi);
        }

    }
}
