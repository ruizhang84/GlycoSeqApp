using GlycoSeqClassLibrary.engine.analysis;
using GlycoSeqClassLibrary.engine.glycan;
using GlycoSeqClassLibrary.model.glycan;
using SpectrumData;
using SpectrumData.Reader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GlycoSeqApp
{
    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {

        int progressCounter;
        int start;
        int end;
        public SearchWindow()
        {
            InitializeComponent();

            ISpectrumReader reader = new ThermoRawSpectrumReader();
            reader.Init(SearchingParameters.Access.MSMSFile);
            start = reader.GetFirstScan();
            end = reader.GetLastScan();
        }

        private Task Process()
        {
            progressCounter = 0;
            Counter counter = new Counter();
            counter.progressChange += SearchProgressChanged;

            UpdateSignal("Searching...");
            MultiThreadingSearch search = new MultiThreadingSearch(counter);
            search.Run();

            UpdateSignal("Analyzing...");
            List<SearchResult> targets = search.Target();
            List<SearchResult> decoys = search.Decoy();
            Analyze(targets, decoys);

            UpdateSignal("Done");
            return Task.CompletedTask;
        }

        private void ReportResults(List<SearchResult> results)
        {
            GlycanBuilder glycanBuilder = new GlycanBuilder();
            glycanBuilder.Build();
            Dictionary<string, IGlycan> glycans_map = glycanBuilder.GlycanMaps();

            List<SearchResult> res = new List<SearchResult>();
            HashSet<string> seen = new HashSet<string>();
            foreach (var it in results)
            {
                string glycan = glycans_map[it.Glycan()].Name();
                string key = it.Scan().ToString() + "|" + it.ModifySite().ToString() + "|" +
                    it.Sequence() + "|" + glycan;
                if (!seen.Contains(key))
                {
                    seen.Add(key);
                    SearchResult r = it;
                    r.set_glycan(glycan);
                    res.Add(r);
                }
            }

            using (FileStream ostrm = new FileStream(SearchingParameters.Access.OutputFile,
                FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(ostrm))
                {
                    writer.WriteLine("scan,peptide,glycan,site,score");
                    foreach(SearchResult r in res)
                    {
                        writer.WriteLine(r.Scan().ToString() + ", " + r.Sequence() + ","
                            + r.Glycan() + "," + r.ModifySite().ToString() + "," + r.Score().ToString());
                        writer.Flush();
                    }
                }
            }
            

        }

        private void Analyze(List<SearchResult> targets, List<SearchResult> decoys)
        {
            FDRFilter filter = new FDRFilter(SearchingParameters.Access.FDRValue);
            filter.set_data(targets, decoys);
            filter.Init();
            List<SearchResult> results = filter.Filter();
            ReportResults(results);
        }

        private void UpdateSignal(string signal)
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new ThreadStart(() => Signal.Text = signal));
        }

        private void UpdateProgress()
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new ThreadStart(() =>
                {
                    SearchingStatus.Value = progressCounter * 1.0 / (end - start) * 1000.0;
                    ProgessStatus.Text = progressCounter.ToString();
                }));
        }

        private void SearchProgressChanged(object sender, EventArgs e)
        {
            Interlocked.Increment(ref progressCounter);
            UpdateProgress();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            buttonRun.IsEnabled = false;
            Task.Run(Process);
        }
    }

    public class Counter
    {
        public event EventHandler progressChange;

        protected virtual void OnProgressChanged(EventArgs e)
        {
            EventHandler handler = progressChange;
            handler?.Invoke(this, e);
        }

        public void Add()
        {
            EventArgs e = new EventArgs();
            OnProgressChanged(e);
        }
    }
}
