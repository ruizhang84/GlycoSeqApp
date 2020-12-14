using SpectrumData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectrumData.Spectrum
{
    public class MS2Spectrum : ISpectrum
    {
        protected int scanNum;
        protected double retention;
        protected List<IPeak> peaks;
        protected double precursorMZ;
        protected int precursorCharge;

        public MS2Spectrum(int scanNum, double retention, 
            double precursorMZ, int precursorCharge)
        {
            this.scanNum = scanNum;
            this.retention = retention;
            this.precursorMZ = precursorMZ;
            this.precursorCharge = precursorCharge;
            peaks = new List<IPeak>();
        }

        public double PrecursorMZ()
        {
            return precursorMZ;
        }

        public int PrecursorCharge()
        {
            return precursorCharge;
        }

        public void Add(IPeak peak)
        {
            peaks.Add(peak);
        }
        public void Clear()
        {
            peaks.Clear();
        }

        public ISpectrum Clone()
        {
            ISpectrum spec = new MS2Spectrum(scanNum, retention, 
                precursorMZ, precursorCharge);
            spec.SetPeaks(peaks);
            return spec;
        }

        public List<IPeak> GetPeaks()
        {
            return peaks;
        }

        public double GetRetention()
        {
            return retention;
        }

        public int GetScanNum()
        {
            return scanNum;
        }

        public void SetPeaks(List<IPeak> peaks)
        {
            this.peaks = peaks;
        }
    }
}
