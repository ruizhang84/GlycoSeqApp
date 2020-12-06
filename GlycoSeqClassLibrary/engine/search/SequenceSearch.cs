using SpectrumData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqClassLibrary.engine.search
{
    public class SequenceSearch
    {
        algorithm.ISearch<string> searcher_;
        // mass table to store the computed ptm / non-ptm results
        Dictionary<string, List<double>> ptm_mass_table_
            = new Dictionary<string, List<double>>();
        Dictionary<string, List<double>> mass_table_ 
            = new Dictionary<string, List<double>>();

        public SequenceSearch(algorithm.ISearch<string> searcher)
        {
            searcher_ = searcher;
        }

        // peptide seq, glycan*
        public Dictionary<string, HashSet<int>> Search(List<IPeak> peaks, int max_charge,
            Dictionary<string, List<model.glycan.IGlycan>> candidate)
        {
            InitSearch(candidate);
            Dictionary<string, HashSet<int>> results = new Dictionary<string, HashSet<int>>();

            // search peaks
            for(int i = 0; i< peaks.Count; i++)
            {
                IPeak peak = peaks[i];
                for(int charge = 1; charge < max_charge; charge++)
                {
                    double target = util.mass.Spectrum.To.Compute(peak.GetMZ(), charge);
                    List<string> glyco_sequences = searcher_.Search(target);
                    foreach(string seq in glyco_sequences)
                    {
                        if(!results.ContainsKey(seq))
                        {
                            results[seq] = new HashSet<int>();
                        }
                        results[seq].Add(i);
                    }
                }
            }
            return results;
        }

        protected void InitSearch(Dictionary<string, List<model.glycan.IGlycan>> candidate)
        {
            List<algorithm.Point<string>> peptides_points = new List<algorithm.Point<string>>();
            foreach (var peptide in candidate.Keys)
            {
                // get glycan mass
                double glycan_mean_mass = SearchHelper.ComputeGlycanMass(candidate[peptide]);

                // create points
                foreach (int pos in protein.ProteinPTM.FindNGlycanSite(peptide))
                {
                    string table_key = SearchHelper.MakeKeySequence(peptide, pos);
                    //init table
                    if (!mass_table_.ContainsKey(table_key))
                    {

                        List<double> mass_list = SearchHelper.ComputeNonePTMPeptideMass(peptide, pos);
                        mass_table_[table_key] = new List<double>(mass_list);
                        mass_list = SearchHelper.ComputePTMPeptideMass(peptide, pos);
                        ptm_mass_table_[table_key] = new List<double>(mass_list);
                    }
                    // retreive table
                    foreach (double mass in mass_table_[table_key])
                    {
                        algorithm.Point<string> point = new algorithm.Point<string>(mass, table_key);
                        peptides_points.Add(point);
                    }
                    foreach (double mass in ptm_mass_table_[table_key])
                    {
                        algorithm.Point<string> point = new algorithm.Point<string>(mass + glycan_mean_mass, table_key);
                        peptides_points.Add(point);
                    }
                }
            }

            searcher_.Init(peptides_points);
        }



    }
}
