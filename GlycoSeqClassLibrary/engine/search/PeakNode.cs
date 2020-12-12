using SpectrumData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqClassLibrary.engine.search
{
    using PeakMatch = Dictionary<string, Dictionary<string, HashSet<int>>>;

    delegate bool GlycanFilter(string glycan_id);

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

        PeakMatch MaxBy(List<IPeak> peaks, GlycanFilter glycanFilter)
        {
            PeakMatch best = new PeakMatch();
            foreach (var peptide in matches_.Keys)
            {
                best[peptide] = new Dictionary<string, HashSet<int>>();
                double best_score = 0;
                foreach (var g in matches_[peptide].Keys)
                {
                    if (!glycanFilter(g)) continue;

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
            return best;
        }

        void MergePeakMatch(ref PeakMatch to, PeakMatch from)
        {
            foreach(var peptide in from.Keys)
            {
                if (!to.ContainsKey(peptide))
                {
                    to[peptide] = new Dictionary<string, HashSet<int>>();
                }
                foreach(var glycan_id in from[peptide].Keys)
                {
                    to[peptide][glycan_id] = from[peptide][glycan_id];
                }
            }
        }

        PeakMatch MaxByHybrid(List<IPeak> peaks)
        {
            PeakMatch best = new PeakMatch();
            foreach (var peptide in matches_.Keys)
            {
                best[peptide] = new Dictionary<string, HashSet<int>>();
                Dictionary<string, List<string>> mannosePart = new Dictionary<string, List<string>>();
                foreach(var glycan_id in matches_[peptide].Keys)
                {
                    if (glycan_id.Length != 31) continue;

                    string mannose = glycan_id.Substring(8, 4);
                    if(!mannosePart.ContainsKey(mannose))
                    {
                        mannosePart[mannose] = new List<string>();
                    }
                    mannosePart[mannose].Add(glycan_id);
                }

                foreach(var glycan_ids in mannosePart.Values)
                {
                    double best_score = 0;
                    Dictionary<string, HashSet<int>> sub_best = new Dictionary<string, HashSet<int>>();
                    foreach (string glycan_id in glycan_ids)
                    {
                        double score = SearchHelper.ComputePeakScore(peaks, matches_[peptide][glycan_id]);
                        if (best_score < score)
                        {
                            best_score = score;
                            sub_best = new Dictionary<string, HashSet<int>>();
                            sub_best[glycan_id] = matches_[peptide][glycan_id];
                        }
                        else if (best_score == score)
                        {
                            sub_best[glycan_id] = matches_[peptide][glycan_id];
                        }
                    }

                    foreach(string glycan_id in sub_best.Keys)
                    {
                        best[peptide][glycan_id] = sub_best[glycan_id];
                    }
                }
            }
            return best;
        }

        public void Max(List<IPeak> peaks)
        {
            PeakMatch best = new PeakMatch();

            GlycanFilter complexFilter = new GlycanFilter((s) => s.Length == 47);
            MergePeakMatch(ref best, MaxBy(peaks, complexFilter));
            GlycanFilter highMannoseFilter = new GlycanFilter((s) => s.Length == 11);
            MergePeakMatch(ref best, MaxBy(peaks, highMannoseFilter));
            MergePeakMatch(ref best, MaxByHybrid(peaks));

            matches_ = best;
        }

    }
}
