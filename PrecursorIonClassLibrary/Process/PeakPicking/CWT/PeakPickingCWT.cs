using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PrecursorIonClassLibrary.Charges;
using SpectrumData;

namespace PrecursorIonClassLibrary.Process.PeakPicking.CWT
{
    public class PeakPickingCWT : IProcess
    {
        private double maxScale;
        private double steps;
        private RidgeLineFinder finder;
        private double lowerBound = 200;
        private double upperBound = 2000;
        private double precison = 1.0;

        public PeakPickingCWT(RidgeLineFinder finder, double maxScale=120, double steps=6, 
            double lowerBound=200, double upperBound=2000, double precison=0.01)
        {
            this.finder = finder;
            this.maxScale = maxScale;
            this.steps = steps;
            this.lowerBound = lowerBound;
            this.upperBound = upperBound;
            this.precison = precison;
        }

        public ISpectrum Process(ISpectrum spectrum)
        {
            List<IPeak> peaks = spectrum.GetPeaks();

            double[] signal = peaks.Select(p => p.GetIntensity()).ToArray();
            SortedDictionary<double, List<double>> matrix =
                new SortedDictionary<double, List<double>>();
            for (double a = 1.0; a <= maxScale; a += steps)
            {
                double[] processed = Algorithm.CWT.Transform(signal, a);
                matrix[a] = processed.ToList();
            }

            List<RidgeLine> lines = finder.Find(
                spectrum.GetPeaks().Select(p => p.GetMZ()).ToList(),
                matrix);
            HashSet<double> mz = new HashSet<double>(lines.Select(p => p.Pos).ToList());

            List<IPeak> processedPeaks = peaks.Where(p => mz.Contains(p.GetMZ())).ToList();
            ISpectrum newSpectrum = spectrum.Clone();
            newSpectrum.SetPeaks(processedPeaks);
            return newSpectrum;
        }
    }
}
