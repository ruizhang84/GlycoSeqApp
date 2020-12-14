using NUnit.Framework;
using PrecursorIonClassLibrary.Process;
using PrecursorIonClassLibrary.Process.PeakPicking.CWT;
using PrecursorIonClassLibrary.Process.PeakPicking.Neighbor;
using PrecursorIonClassLibrary.Process.Refinement;
using SpectrumData;
using SpectrumData.Reader;
using System.IO;

namespace NUnitTest
{
    public class UnitTest2
    {
        [Test]
        public void test1()
        {
            string path = @"C:\Users\Rui Zhang\Downloads\ZC_20171218_C16_R1.raw";
            ThermoRawSpectrumReader reader = new ThermoRawSpectrumReader();
            reader.Init(path);

            ISpectrum ms2 = reader.GetSpectrum(7638);
            //IProcess processer = new LocalNeighborPicking();
            IProcess processer = new WeightedAveraging(new LocalNeighborPicking());
            //RidgeLineFinder coeffMatrix = new RidgeLineFinder(1.0, 2, 1, 2);
            //IProcess processer = new PeakPickingCWT(coeffMatrix);
            ms2 = processer.Process(ms2);
            using (StreamWriter outputFile = new StreamWriter(@"C:\Users\Rui Zhang\Downloads\peak.csv"))
            {
                outputFile.WriteLine("mz,intensity");
                foreach(IPeak pk in ms2.GetPeaks())
                {
                    outputFile.WriteLine(pk.GetMZ().ToString() + "," + pk.GetIntensity().ToString());
                }
            }

        }
    }
}
