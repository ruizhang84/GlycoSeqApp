using GlycoSeqClassLibrary.engine.search;
using SpectrumData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqClassLibrary.engine.analysis
{
    public class SearchAnalyzer
    {
        public List<SearchResult> Analyze(int scan, List<IPeak> peaks,
            Dictionary<string, HashSet<int>> peptide_results,
            Dictionary<string, HashSet<int>> glycan_results)
        {
            List<SearchResult> results = new List<SearchResult>();
            // preprocess to extract peptide
            Dictionary<string, List<string>> peptides_map = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> glycans_map = new Dictionary<string, List<string>>();
            foreach (var key in peptide_results.Keys)
            {
                string peptide = key.Split('|')[0];
            
                if (!peptides_map.ContainsKey(peptide))
                {
                    peptides_map[peptide] = new List<string>();
                }
                peptides_map[peptide].Add(key);
            }
            foreach (var key in glycan_results.Keys)
            {
                string peptide = key.Split('|')[0];
                if (!glycans_map.ContainsKey(peptide))
                {
                    glycans_map[peptide] = new List<string>();
                }
                glycans_map[peptide].Add(key);
            }
        
            // analyze the best matches
            double best_score = 0;
            foreach(string peptide in peptides_map.Keys)
            {
                foreach(string p in peptides_map[peptide])
                {
                
                    foreach(string g in glycans_map[peptide])
                    {
                        // get index
                        HashSet<int> peptides_index = peptide_results[p];
                        HashSet<int> glycans_index = glycan_results[g];
                        // compute score
                        double score = ComputePeakScore(peaks, peptides_index, glycans_index);
                        // create results if higher score
                        if (score > best_score)
                        {
                            best_score = score;
                            results.Clear();
                        }
                        if (score == best_score)
                        {
                            int pos = SearchHelper.ExtractSequence(p).Item2;
                            string glycan = SearchHelper.ExtractGlycoSequence(g).Item2;
                            SearchResult r = new SearchResult();
                            r.set_glycan(glycan);
                            r.set_peptide(peptide);
                            r.set_scan(scan);
                            r.set_site(pos);
                            r.set_score(score);
                            results.Add(r);
                        }
                    }
                }
            }
            return results;
        }

        public double ComputePeakScore(List<IPeak> peaks, 
            HashSet<int> peptides_index, HashSet<int> glycans_index)
        {
            double score = 0;
            foreach(int index in peptides_index)
            {
                score += Math.Log(peaks[index].GetIntensity());
            }
            foreach(int index in glycans_index)
            {
                score += Math.Log(peaks[index].GetIntensity());
            }
            return score;
        }
    }
}
