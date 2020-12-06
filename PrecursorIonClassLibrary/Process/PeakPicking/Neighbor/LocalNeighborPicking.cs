using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpectrumData;

namespace PrecursorIonClassLibrary.Process.PeakPicking.Neighbor
{
    public class LocalNeighborPicking : IProcess
    {
        public ISpectrum Process(ISpectrum spectrum)
        {
            List<IPeak> peaks = spectrum.GetPeaks();
            List<IPeak> processedPeaks = new List<IPeak>();
           
            int index = 1;
            int end = peaks.Count - 1;
            int head = index + 1;
            while (index < end)
            {
                if (peaks[index - 1].GetIntensity() < peaks[index].GetIntensity())
                {
                    head = index + 1;
                }

                while (head < end
                    && peaks[head].GetIntensity() == peaks[index].GetIntensity())
                {
                    head++;
                }

                if (peaks[head].GetIntensity() < peaks[index].GetIntensity())
                {
                    processedPeaks.Add(peaks[index]);
                    index = head;
                }
                index++;

            }

            ISpectrum newSpectrum = spectrum.Clone();
            newSpectrum.SetPeaks(processedPeaks);
            return newSpectrum;
        }
    }
}
