using SpectrumData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqClassLibrary.engine.search
{
    using PeakMatch = Dictionary<string, Dictionary<string, HashSet<int>>>;

    public class PeakNode
    {
        int miss_ = 1;
        double mass_ = 0;
        PeakMatch matches_ = new PeakMatch();

        public int Missing() { return miss_; }
        public void set_miss(int miss) { miss_ = miss; }
        public double Mass() { return mass_; }
        public void set_mass(double mass) { mass_ = mass; }
        public PeakMatch Matches() { return matches_; }
        public void set_matches(PeakMatch matches)
            { matches_ = matches; }

        public void Add(string peptide, string glycan_id, List<int> peaks)
        {
            if (!matches_.ContainsKey(peptide))
            {
                matches_[peptide] = new Dictionary<string, HashSet<int>>();
            }
            if (!matches_[peptide].ContainsKey(glycan_id))
            {
                matches_[peptide][glycan_id] = new HashSet<int>();
            }

            matches_[peptide][glycan_id].UnionWith(peaks);
        }

        public void Add(List<int> peaks)
        {
            foreach (var key in matches_.Keys)
            {
                foreach (var g in matches_[key].Keys)
                {
                    matches_[key][g].UnionWith(peaks);
                }
            }
        }


        public void Max(List<IPeak> peaks)
        {
            PeakMatch best = new PeakMatch();
            foreach (var peptide in matches_.Keys)
            {
                best[peptide] = new Dictionary<string, HashSet<int>> ();
                double best_score = 0;
                foreach (var g in matches_[peptide].Keys)
                {
                    double score = SearchHelper.ComputePeakScore(peaks, matches_[peptide][g]);
                    if (best_score < score)
                    {
                        best_score = score;
                        best[peptide] = new Dictionary<string, HashSet<int>>();
                        best[peptide][g] = matches_[peptide][g];
                    }
                    else if (best_score == score)
                    {
                        best[peptide][g] = matches_[peptide][g];
                    }
                }
            }
            matches_ = best;
        }

    }
}
