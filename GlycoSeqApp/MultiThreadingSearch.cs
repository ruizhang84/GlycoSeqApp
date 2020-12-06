using GlycoSeqClassLibrary.algorithm;
using GlycoSeqClassLibrary.engine.analysis;
using GlycoSeqClassLibrary.engine.glycan;
using GlycoSeqClassLibrary.engine.protein;
using GlycoSeqClassLibrary.engine.search;
using GlycoSeqClassLibrary.model.protein;
using GlycoSeqClassLibrary.util.io;
using PrecursorIonClassLibrary.Averagine;
using PrecursorIonClassLibrary.Brain;
using PrecursorIonClassLibrary.Charges;
using PrecursorIonClassLibrary.Process;
using PrecursorIonClassLibrary.Process.PeakPicking.Neighbor;
using SpectrumData;
using SpectrumData.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqApp
{
    public class SearchTask
    {
        public SearchTask(int scan, List<IPeak> peaks, double mz, int charge)
        {
            Scan = scan;
            Peaks = peaks;
            PrecursorMZ = mz;
            Charge = charge;
        }
        public int Scan { get; set; }
        public List<IPeak> Peaks { get; set; }
        public double PrecursorMZ { get; set; }
        public int Charge { get; set; }
    }

    public class MultiThreadingSearch
    {
        Counter counter;
        Queue<SearchTask> tasks;
        List<SearchResult> targets;
        List<SearchResult> decoys;
        List<string> peptides;
        List<string> decoyPeptides;
        private readonly object queueLock = new object();
        private readonly object resultLock = new object();
        private readonly double searchRange = 2;
        GlycanBuilder glycanBuilder = new GlycanBuilder();


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

        public MultiThreadingSearch(Counter counter)
        {
            this.counter = counter;

            // read spectrum
            tasks = new Queue<SearchTask>();
            GenerateTasks();

            // build pepeptides
            IProteinReader proteinReader = new FastaReader();
            List<IProtein> proteins = proteinReader.Read(SearchingParameters.Access.FastaFile);
            List<IProtein> decoyProteins = new List<IProtein>();
            foreach (IProtein protein in proteins)
            {
                IProtein p = new BaseProtein();
                p.SetSequence(Reverse(protein.Sequence()));
                decoyProteins.Add(p);
            }
            peptides = GeneratePeptides(proteins);
            decoyPeptides = GeneratePeptides(decoyProteins);

            // build glycans
            glycanBuilder.Build();
            
        }

        List<string> GeneratePeptides(List<IProtein> proteins)
        {
            // peptides
            List<Proteases> proteases = new List<Proteases>();
            foreach(string enzyme in SearchingParameters.Access.DigestionEnzyme)
            {
                switch(enzyme)
                {
                    case "Chymotrypsin":
                        proteases.Add(Proteases.Chymotrypsin);
                        break;
                    case "GluC":
                        proteases.Add(Proteases.GluC);
                        break;
                    case "Pepsin":
                        proteases.Add(Proteases.Pepsin);
                        break;
                    case "":
                        proteases.Add(Proteases.Trypsin);
                        break;
                }
            }

            HashSet<string> peptides = new HashSet<string>();
            foreach (Proteases enzyme in proteases)
            {
                ProteinDigest proteinDigest = new ProteinDigest(
                    SearchingParameters.Access.MissCleavage, 
                    SearchingParameters.Access.MiniPeptideLength, enzyme);
                foreach (IProtein protein in proteins)
                {
                    peptides.UnionWith(proteinDigest.Sequences(protein.Sequence(),
                        ProteinPTM.ContainsNGlycanSite));
                }
            }
            return peptides.ToList();
        }

        public List<SearchResult> Target()
            { return targets; }

        public List<SearchResult> Decoy()
            { return decoys; }

        SearchTask TryGetTask()
        {
            lock (queueLock)
            {
                if (tasks.Count > 0)
                    return tasks.Dequeue();
                return null;
            }
        }

        void UpdateTask(List<SearchResult> t, List<SearchResult> d)
        {
            lock (resultLock)
            {
                targets.AddRange(t);
                decoys.AddRange(d);
            }
        }

        public void Run()
        {
            List<Task> searches = new List<Task>();
            for (int i = 0; i < SearchingParameters.Access.ThreadNums; i++)
            {
                searches.Add(Task.Run(() => Search()));
            }

            Task.WaitAll(searches.ToArray());
        }

        void GenerateTasks()
        {
            ISpectrumReader reader = new ThermoRawSpectrumReader();
            LocalMaximaPicking picking = new LocalMaximaPicking();
            IProcess process = new LocalNeighborPicking();
            reader.Init(SearchingParameters.Access.MSMSFile);
            
            ISpectrum ms1 = null;
            List<IPeak> majorPeaks = new List<IPeak>();

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

                    SearchTask searchTask = new SearchTask(i, ms2.GetPeaks(), precursorMZ, charge);
                    tasks.Enqueue(searchTask);

                    
                }
            }
        }


        void Search()
        {
            List<SearchResult> tempResults = new List<SearchResult>();
            List<SearchResult> tempDecoyResults = new List<SearchResult>();

            ISearch<string> oneSearcher = new BucketSearch<string>(
                SearchingParameters.Access.MS1ToleranceBy, SearchingParameters.Access.MS1Tolerance);
            PrecursorMatch precursorMatcher = new PrecursorMatch(oneSearcher);
            precursorMatcher.Init(peptides, glycanBuilder.GlycanMaps());

            ISearch<string> twoSearcher = new BucketSearch<string>(
               SearchingParameters.Access.MS1ToleranceBy, SearchingParameters.Access.MS1Tolerance);
            PrecursorMatch decoyPrecursorMatcher = new PrecursorMatch(twoSearcher);
            decoyPrecursorMatcher.Init(decoyPeptides, glycanBuilder.GlycanMaps());

            ISearch<string> moreSearcher = new BucketSearch<string>(
                SearchingParameters.Access.MS2ToleranceBy, SearchingParameters.Access.MSMSTolerance);
            SequenceSearch sequenceSearcher = new SequenceSearch(moreSearcher);

            ISearch<int> extraSearcher = new BucketSearch<int>(
                SearchingParameters.Access.MS2ToleranceBy, SearchingParameters.Access.MSMSTolerance);
            GlycanSearch glycanSearcher = new GlycanSearch(extraSearcher, glycanBuilder.GlycanMaps());

            SearchAnalyzer searchAnalyzer = new SearchAnalyzer();

            SearchTask task;
            while ((task = TryGetTask()) != null)
            {
                //precursor match
                var pre_results = precursorMatcher.Match(task.PrecursorMZ, task.Charge);
                if (pre_results.Count > 0)
                {
                    // spectrum search
                    var peptide_results = sequenceSearcher.Search(task.Peaks, task.Charge, pre_results);
                    if (peptide_results.Count > 0)
                    {
                        var glycan_results = glycanSearcher.Search(task.Peaks, task.Charge, pre_results);
                        if (glycan_results.Count > 0)
                        {
                            tempResults.AddRange(
                                searchAnalyzer.Analyze(task.Scan, task.Peaks, peptide_results, glycan_results));
                        }
                    }
                }

                var decoy_results = precursorMatcher.Match(task.PrecursorMZ, task.Charge);
                if (decoy_results.Count > 0)
                {
                    // spectrum search
                    var peptide_results = sequenceSearcher.Search(task.Peaks, task.Charge, decoy_results);
                    if (peptide_results.Count > 0)
                    {
                        var glycan_results = glycanSearcher.Search(task.Peaks, task.Charge, decoy_results);
                        if (glycan_results.Count > 0)
                        {
                            tempDecoyResults.AddRange(
                                searchAnalyzer.Analyze(task.Scan, task.Peaks, peptide_results, glycan_results));
                        }
                    }
                }

                counter.Add();
            }

            UpdateTask(tempResults, tempDecoyResults);
        }
    }
}
