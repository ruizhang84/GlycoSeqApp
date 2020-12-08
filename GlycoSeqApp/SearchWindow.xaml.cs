﻿using GlycoSeqClassLibrary.engine.analysis;
using GlycoSeqClassLibrary.engine.glycan;
using GlycoSeqClassLibrary.engine.protein;
using GlycoSeqClassLibrary.model.glycan;
using GlycoSeqClassLibrary.model.protein;
using GlycoSeqClassLibrary.util.io;
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
        int ReadingCounter;
        int progressCounter;
        public SearchWindow()
        {
            InitializeComponent();
            InitProcess();
        }

        private async void InitProcess()
        {
            await Task.Run(Process);
        }

        private Task Process()
        {
           
            Counter readerCounter = new Counter();
            Counter searchCounter = new Counter();
            readerCounter.progressChange += ReadProgressChanged;
            searchCounter.progressChange += SearchProgressChanged;           

            // build pepeptides
            IProteinReader proteinReader = new FastaReader();
            List<IProtein> proteins = proteinReader.Read(SearchingParameters.Access.FastaFile);
            List<IProtein> decoyProteins = new List<IProtein>();
            foreach (IProtein protein in proteins)
            {
                IProtein p = new BaseProtein();
                p.SetSequence(MultiThreadingSearchHelper.Reverse(protein.Sequence()));
                decoyProteins.Add(p);
            }
            List<string> peptides = 
                MultiThreadingSearchHelper.GeneratePeptides(proteins);
            List<string> decoyPeptides = 
                MultiThreadingSearchHelper.GeneratePeptides(decoyProteins);

            // build glycans
            GlycanBuilder glycanBuilder = new GlycanBuilder();
            glycanBuilder.Build();

            foreach(string file in SearchingParameters.Access.MSMSFiles)
            {
                progressCounter = 0;
                ReadingCounter = 0;
                UpdateSignal("Searching...");
                MultiThreadingSearch search =
                    new MultiThreadingSearch(file, readerCounter, searchCounter,
                        peptides, decoyPeptides, glycanBuilder);
                search.Run();
                UpdateSignal("Analyzing...");
                Analyze(file, search.Target(), search.Decoy(), glycanBuilder);
            }

            UpdateSignal("Done");
            return Task.CompletedTask;
        }

        private void Analyze(string msPath, List<SearchResult> targets, List<SearchResult> decoys, 
            GlycanBuilder glycanBuilder)
        {
            FDRFilter filter = new FDRFilter(SearchingParameters.Access.FDRValue);
            filter.set_data(targets, decoys);
            filter.Init();
            List<SearchResult> results = filter.Filter();
            string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(msPath),
                System.IO.Path.GetFileNameWithoutExtension(msPath) + ".csv");
            MultiThreadingSearchHelper.ReportResults(path, results, glycanBuilder.GlycanMaps());
        }

        private void UpdateSignal(string signal)
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new ThreadStart(() => Signal.Text = signal));
        }

        private void UpdateProgress(int start, int end)
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new ThreadStart(() =>
                {
                    SearchingStatus.Value = progressCounter * 1.0 / (end - start) * 1000.0;
                }));
        }

        private void Readingprogress(int start, int end)
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new ThreadStart(() =>
                {
                    ReadingStatus.Value = ReadingCounter * 1.0 / (end - start) * 1000.0;
                }));
        }

        private void SearchProgressChanged(object sender, ProgressingEventArgs e)
        {
            Interlocked.Increment(ref progressCounter);
            UpdateProgress(e.Start, e.End);
        }

        private void ReadProgressChanged(object sender, ProgressingEventArgs e)
        {
            Interlocked.Increment(ref ReadingCounter);
            Readingprogress(e.Start, e.End);
        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    buttonRun.IsEnabled = false;
        //    Task.Run(Process);
        //}
    }
}