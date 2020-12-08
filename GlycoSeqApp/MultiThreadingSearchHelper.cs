using GlycoSeqClassLibrary.engine.analysis;
using GlycoSeqClassLibrary.engine.protein;
using GlycoSeqClassLibrary.model.glycan;
using GlycoSeqClassLibrary.model.protein;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqApp
{
    public class ProgressingEventArgs : EventArgs
    {
        public int Start { get; set; }
        public int End { get; set; }
    }

    public class Counter
    {
        public event EventHandler<ProgressingEventArgs> progressChange;

        protected virtual void OnProgressChanged(ProgressingEventArgs e)
        {
            EventHandler<ProgressingEventArgs> handler = progressChange;
            handler?.Invoke(this, e);
        }

        public void Add(int start, int end)
        {
            ProgressingEventArgs e = new ProgressingEventArgs
            {
                Start = start,
                End = end
            };
            OnProgressChanged(e);
        }
    }

    public class MultiThreadingSearchHelper
    {

        public static void ReportResults(
            string path, 
            List<SearchResult> results,
            Dictionary<string, IGlycan> glycans_map)
        {
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

            using (FileStream ostrm = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(ostrm))
                {
                    writer.WriteLine("scan,peptide,glycan,site,score");
                    foreach (SearchResult r in res)
                    {
                        writer.WriteLine(r.Scan().ToString() + ", " + r.Sequence() + ","
                            + r.Glycan() + "," + r.ModifySite().ToString() + "," + r.Score().ToString());
                        writer.Flush();
                    }
                }
            }


        }
        public static string Reverse(string stringToReverse)
        {
            char[] stringArray = stringToReverse.ToCharArray();
            string reverse = string.Empty;
            for (int i = stringArray.Length - 1; i >= 0; i--)
            {
                reverse += stringArray[i];
            }

            return reverse;
        }

        public static List<string> GeneratePeptides(List<IProtein> proteins)
        {
            // peptides
            List<Proteases> proteases = new List<Proteases>();
            foreach (string enzyme in SearchingParameters.Access.DigestionEnzyme)
            {
                switch (enzyme)
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
                    case "Trypsin":
                        proteases.Add(Proteases.Trypsin);
                        break;
                }
            }

            HashSet<string> peptides = new HashSet<string>();
            ProteinDigest proteinDigest = new ProteinDigest(
                SearchingParameters.Access.MissCleavage,
                SearchingParameters.Access.MiniPeptideLength, proteases[0]);
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
            return peptides.ToList();
        }
    }
}
