using GlycoSeqClassLibrary.algorithm;
using GlycoSeqClassLibrary.engine.analysis;
using GlycoSeqClassLibrary.engine.glycan;
using GlycoSeqClassLibrary.engine.protein;
using GlycoSeqClassLibrary.engine.search;
using GlycoSeqClassLibrary.model.protein;
using GlycoSeqClassLibrary.util.io;
using NUnit.Framework;
using PrecursorIonClassLibrary.Averagine;
using PrecursorIonClassLibrary.Brain;
using PrecursorIonClassLibrary.Charges;
using PrecursorIonClassLibrary.Process;
using PrecursorIonClassLibrary.Process.PeakPicking.Neighbor;
using SpectrumData;
using SpectrumData.Reader;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public class Tests
    {
        static string Reverse(string stringToReverse)
        {
            char[] stringArray = stringToReverse.ToCharArray();
            string reverse = string.Empty;
            for (int i = stringArray.Length - 1; i >= 0; i--)
            {
                reverse += stringArray[i];
            }

            return reverse;
        }

        [Test]
        public void Test1()
        {
            string path = @"C:\Users\Rui Zhang\Downloads\ZC_20171218_C16_R1.raw";
            string fasta = @"C:\Users\Rui Zhang\Downloads\haptoglobin.fasta";
            // peptides
            IProteinReader proteinReader = new FastaReader();
            List<IProtein> proteins = proteinReader.Read(fasta);
            List<IProtein> decoyProteins = new List<IProtein>();
            foreach (IProtein protein in proteins)
            {
                IProtein p = new BaseProtein();
                p.SetSequence(Reverse(protein.Sequence()));
                decoyProteins.Add(p);
            }

            List<Proteases> proteases = new List<Proteases>()
                { Proteases.Trypsin, Proteases.GluC };

            HashSet<string> peptides = new HashSet<string>();
            
            ProteinDigest proteinDigest = new ProteinDigest(2, 5, proteases[0]);
            foreach (IProtein protein in decoyProteins)
            {
                peptides.UnionWith(proteinDigest.Sequences(protein.Sequence(),
                    ProteinPTM.ContainsNGlycanSite));
            }

            for (int i = 1; i < proteases.Count; i++ )
            {
                proteinDigest.SetProtease(proteases[i]);
                List<string> peptidesList = peptides.ToList();
                foreach (string seq in peptidesList)
                {
                    peptides.UnionWith(proteinDigest.Sequences(seq,
                        ProteinPTM.ContainsNGlycanSite));
                }
            }

            Assert.True(peptides.Contains("KDNLTYVGDGETR"));

            // build glycan
            GlycanBuilder glycanBuilder = new GlycanBuilder();
            glycanBuilder.Build();


            // search
            List<SearchResult> searchResults = new List<SearchResult>();

            ThermoRawSpectrumReader reader = new ThermoRawSpectrumReader();
            LocalMaximaPicking picking = new LocalMaximaPicking();
            IProcess process = new LocalNeighborPicking();
            reader.Init(path);
            double searchRange = 2;
            ISpectrum ms1 = null;
            List<IPeak> majorPeaks = new List<IPeak>();

            ISearch<string> oneSearcher = new BucketSearch<string>(ToleranceBy.PPM, 10);
            PrecursorMatch precursorMatcher = new PrecursorMatch(oneSearcher);
            precursorMatcher.Init(peptides.ToList(), glycanBuilder.GlycanMaps());

            ISearch<string> moreSearcher = new BucketSearch<string>(ToleranceBy.Dalton, 0.01);
            SequenceSearch sequenceSearcher = new SequenceSearch(moreSearcher);

            ISearch<int> extraSearcher = new BucketSearch<int>(ToleranceBy.Dalton, 0.01);
            GlycanSearch glycanSearcher = new GlycanSearch(extraSearcher, glycanBuilder.GlycanMaps());

            SearchAnalyzer searchAnalyzer = new SearchAnalyzer();

            for (int i = reader.GetFirstScan(); i < reader.GetLastScan(); i++)
            {
                if (reader.GetMSnOrder(i) < 2)
                {
                    ms1 = reader.GetSpectrum(i);
                    majorPeaks = picking.Process(ms1.GetPeaks());
                }
                else
                {
                    double mz = reader.GetPrecursorMass(i, reader.GetMSnOrder(i));
                    if (ms1.GetPeaks()
                        .Where(p => p.GetMZ() > mz - searchRange && p.GetMZ() < mz + searchRange)
                        .Count() == 0)
                        continue;

                    Patterson charger = new Patterson();
                    int charge = charger.Charge(ms1.GetPeaks(), mz - searchRange, mz + searchRange);

                    // find evelope cluster
                    EnvelopeProcess envelope = new EnvelopeProcess();
                    var cluster = envelope.Cluster(majorPeaks, mz, charge);
                    if (cluster.Count == 0)
                        continue;

                    // find monopeak
                    Averagine averagine = new Averagine(AveragineType.GlycoPeptide);
                    BrainCSharp braincs = new BrainCSharp();
                    MonoisotopicSearcher searcher = new MonoisotopicSearcher(averagine, braincs);
                    MonoisotopicScore result = searcher.Search(mz, charge, cluster);
                    double precursorMZ = result.GetMZ();

                    // search
                    ISpectrum ms2 = reader.GetSpectrum(i);
                    ms2 = process.Process(ms2);

                    //precursor match
                    var pre_results = precursorMatcher.Match(precursorMZ, charge);
                    if (pre_results.Count == 0)
                        continue;

                    // spectrum search
                    var peptide_results = sequenceSearcher.Search(ms2.GetPeaks(), charge, pre_results);
                    if (peptide_results.Count == 0)
                        continue;

                    var glycan_results = glycanSearcher.Search(ms2.GetPeaks(), charge, pre_results);
                    if (glycan_results.Count == 0)
                        continue;

                    var temp_results = searchAnalyzer.Analyze(i, ms2.GetPeaks(), peptide_results, glycan_results);
                    break;
                }
            }
               
        }
    }
}