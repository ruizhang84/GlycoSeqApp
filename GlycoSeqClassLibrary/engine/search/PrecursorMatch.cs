using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqClassLibrary.engine.search
{
    public class PrecursorMatch
    {
        algorithm.ISearch<string> searcher_;
        List<algorithm.Point<string>> peptides_;
        List<model.glycan.IGlycan> glycans_;

        public PrecursorMatch(algorithm.ISearch<string> searcher)
        {
            searcher_ = searcher;
        }

        void Init(List<string> peptides, Dictionary<string, model.glycan.IGlycan> glycans)
        {
            foreach(var it in peptides)
            {
                algorithm.Point<string> seq = new algorithm.Point<string>(util.mass.Peptide.To.Compute(it), it);
                peptides_.Add(seq);
            }
        
            foreach(var it in glycans.Values)
            {
                glycans_.Add(it);
            }
            searcher_.Init(peptides_);
        }

        Dictionary<string, List<model.glycan.IGlycan>> Match(double precursor, int charge)
        {
            Dictionary<string, List<model.glycan.IGlycan>> results = 
                new Dictionary<string, List<model.glycan.IGlycan>>();
            double mass = util.mass.Spectrum.To.Compute(precursor, charge);


            foreach (var glycan in glycans_)
            {
                double target = mass - glycan.Mass();
                if (target <= 0)
                    continue;

                List<string> peptides = searcher_.Search(target, mass);
                if (peptides_.Count == 0)
                    continue;

                // check pentacore
                 var composition = glycan.Composition();
                if (!composition.ContainsKey(model.glycan.Monosaccharide.GlcNAc) 
                    || composition[model.glycan.Monosaccharide.GlcNAc] < 3)
                    continue;

                foreach (var seq in peptides)
                {
                    if (!results.ContainsKey(seq))
                    {
                        results[seq] = new List<model.glycan.IGlycan>();
                    }
                    results[seq].Add(glycan);
                }
            }
            return results;
        }
    }
}
