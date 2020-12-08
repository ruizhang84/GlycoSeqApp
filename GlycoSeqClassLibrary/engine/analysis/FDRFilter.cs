using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqClassLibrary.engine.analysis
{
    public class FDRFilter
    {
        double fdr_;
        double cutoff_;
        List<SearchResult> target_ = new List<SearchResult>();
        List<SearchResult> decoy_ = new List<SearchResult>();
        public FDRFilter(double fdr)
        {
            fdr_ = fdr;
            cutoff_ = -1;
        }

        public void Init()
        {
            // init
            cutoff_ = -1;
            if (decoy_.Count == 0 || target_.Count == 0 ||
                (decoy_.Count * 1.0 / (decoy_.Count + target_.Count) < fdr_))   //trivial case
            {
                return;
            }

            List<double> scores = new List<double>();
            scores.AddRange(target_.Select(p => p.Score()).ToList());
            scores.AddRange(decoy_.Select(p => p.Score()).ToList());
            scores.Sort();
            target_ = target_.OrderBy(p => p.Score()).ToList();
            decoy_ = decoy_.OrderBy(p => p.Score()).ToList();

            // compare and compute
            int i = 0, j = 0, k = 0;
            while (k < target_.Count + decoy_.Count)
            {
                double score = scores[k];
                while (i < target_.Count && target_[i].Score() < score)
                {
                    i++;
                }
                // decoy score is no less than targets
                while (j < decoy_.Count && decoy_[j].Score() < score)
                {
                    j++;
                }
                // compute fdr rate
                double rate = (decoy_.Count - j) * 1.0 / (target_.Count + decoy_.Count - i - j + 1);
                rate = rate * (1.0 + target_.Count / decoy_.Count);
                if (rate <= fdr_)
                {
                     cutoff_ = score;
                    return;
                }
                else
                {
                    k++;
                }

            }
            // set max
            cutoff_ = int.MaxValue;
        }

        public List<SearchResult> Filter()
        {
            return target_
                .Where(p => p.Score() >= cutoff_)
                .OrderBy(p => p.Scan()).ToList();
        }

        public void set_data(List<SearchResult> targets,
            List<SearchResult> decoys)
        {
            // acquire the best score of the scan
            Dictionary<int, double> score_map = new Dictionary<int, double>();
            foreach (var it in targets)
            {
                int scan = it.Scan();
                if (!score_map.ContainsKey(scan))
                {
                    score_map[scan] = it.Score();
                }
                else
                {
                    if (score_map[scan] < it.Score())
                    {
                        score_map[scan] = it.Score();
                    }
                }
            }
            // when target and decoy are in the same spectrum,
            // the one with higher score is picked.
            foreach (var it in decoys)
            {
                int scan = it.Scan();
                if (!score_map.ContainsKey(scan))
                {
                    score_map[scan] = it.Score();
                }
                else
                {
                    if (score_map[scan] < it.Score())
                    {
                        score_map[scan] = it.Score();
                    }
                }
            }
           
            foreach (var it in targets)
            {
                int scan = it.Scan();
                if (score_map[scan] > it.Score())
                    continue;
                target_.Add(it);
            }
            foreach (var it in decoys)
            {
                int scan = it.Scan();
                if (score_map[scan] > it.Score())
                    continue;
                decoy_.Add(it);
            }
        }



        //public List<SearchResult> Target() { return target_; }
        //public List<SearchResult> Decoy() { return decoy_; }
        public double Cutoff() { return cutoff_; }

        public void set_cutoff(double cutoff) { cutoff_ = cutoff; }
    

    }
}
