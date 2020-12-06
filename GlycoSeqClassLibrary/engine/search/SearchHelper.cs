using SpectrumData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqClassLibrary.engine.search
{
    public class SearchHelper
    {
        public static string MakeKeySequence(string seq, int pos)
        {
            return seq + "|" + pos.ToString();
        }

        public static string MakeKeyGlycoSequence(string glycan_id, string seq)
        {
            return seq + "|" + glycan_id;
        }

        public static Tuple<string, int> ExtractSequence(string key)
        {
            string[] words = key.Split('|');
            return new Tuple<string, int>(words[0], int.Parse(words[1]));
        }

        public static Tuple<string, string> ExtractGlycoSequence(string key)
        {
            string[] words = key.Split('|');
            return new Tuple<string, string>(words[0], words[1]);
        }

        public static double ComputePeakScore(List<IPeak> peaks, HashSet<int> peak_indexes)
        {
            double score = 0;
            foreach (int index in peak_indexes)
            {
                score += Math.Log(peaks[index].GetIntensity());
            }
            return score;
        }

        // for computing the peptide ions
        public static List<double> ComputePTMPeptideMass(string seq, int pos)
        {
            List<double> mass_list = new List<double>();
            double mass = util.mass.Peptide.To.Compute(seq.Substring(0, pos));
            for (int i = pos; i < seq.Length - 1; i++) // seldom at n
            {
                char amino = seq[i];
                mass += util.mass.Peptide.To.GetAminoAcidMW(amino);
                mass_list.Add(util.mass.Ion.To.Compute(mass, util.mass.IonType.b));
                mass_list.Add(util.mass.Ion.To.Compute(mass, util.mass.IonType.c));
            }
            mass = util.mass.Peptide.To.Compute(seq.Substring(pos + 1, seq.Length - pos));
            for (int i = pos; i >= 1; i--)
            {
                char amino = seq[i];
                mass += util.mass.Peptide.To.GetAminoAcidMW(amino);
                mass_list.Add(util.mass.Ion.To.Compute(mass, util.mass.IonType.y));
                mass_list.Add(util.mass.Ion.To.Compute(mass, util.mass.IonType.z));
            }
            return mass_list;
        }

        public static List<double> ComputeNonePTMPeptideMass(string seq, int pos)
        {
            List<double> mass_list = new List<double>();
            double mass = 18.0105;
            for (int i = 0; i < pos; i++)
            {
                char amino = seq[i];
                mass += util.mass.Peptide.To.GetAminoAcidMW(amino);
                mass_list.Add(util.mass.Ion.To.Compute(mass, util.mass.IonType.b));
                mass_list.Add(util.mass.Ion.To.Compute(mass, util.mass.IonType.c));
            }
            mass = 18.0105;
            for (int i = seq.Length - 1; i > pos; i--)
            {
                char amino = seq[i];
                mass += util.mass.Peptide.To.GetAminoAcidMW(amino);
                mass_list.Add(util.mass.Ion.To.Compute(mass, util.mass.IonType.y));
                mass_list.Add(util.mass.Ion.To.Compute(mass, util.mass.IonType.z));
            }
            return mass_list;
        }

        public static double ComputeGlycanMass(List<model.glycan.IGlycan> glycans)
        {
            double sums = 0.0;
            foreach (var glycan in glycans)
            {
                sums += glycan.Mass();
            }
            return sums * 1.0 / glycans.Count;
        }
    }
}
