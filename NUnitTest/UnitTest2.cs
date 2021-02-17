using GlycoSeqClassLibrary.engine.protein;
using GlycoSeqClassLibrary.model.protein;
using GlycoSeqClassLibrary.util.io;
using NUnit.Framework;
using PrecursorIonClassLibrary.Process;
using PrecursorIonClassLibrary.Process.PeakPicking.CWT;
using PrecursorIonClassLibrary.Process.PeakPicking.Neighbor;
using PrecursorIonClassLibrary.Process.Refinement;
using SpectrumData;
using SpectrumData.Reader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        [Test]
        public void test2()
        {
            string fasta = @"C:\Users\Rui Zhang\Downloads\haptoglobin.fasta";
            // peptides
            IProteinReader proteinReader = new FastaReader();
            List<IProtein> proteins = proteinReader.Read(fasta);

            List<Proteases> proteases = new List<Proteases>()
                { Proteases.Trypsin, Proteases.GluC };

            HashSet<string> peptides = new HashSet<string>();

            ProteinDigest proteinDigest = new ProteinDigest(2, 5, proteases[0]);
            foreach (IProtein protein in proteins)
            {
                peptides.UnionWith(proteinDigest.Sequences(protein.Sequence(),
                    ProteinPTM.ContainsNGlycanSite));
            }

            for (int i = 1; i < proteases.Count; i++)
            {
                proteinDigest.SetProtease(proteases[i]);
                List<string> peptidesList = peptides.ToList();
                foreach (string seq in peptidesList)
                {
                    peptides.UnionWith(proteinDigest.Sequences(seq,
                        ProteinPTM.ContainsNGlycanSite));
                }
            }

            HashSet<string> peptideModifieds = ProteinModification.DynamicModification(
                peptides, ProteinPTM.ContainsNGlycanSite);

            foreach (var s in peptideModifieds)
            {
                Console.WriteLine(s);
            }
            Console.WriteLine(peptides.Count);
            Console.WriteLine(peptideModifieds.Count);
        }
    }
}
