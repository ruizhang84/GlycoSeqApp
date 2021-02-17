using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqClassLibrary.engine.protein
{
    public class ProteinModification
    {

        public static HashSet<string> DynamicModification(HashSet<string> peptides,
            FilterSequence filter, bool oxidation=true, bool deamidation=true)
        {
            HashSet<string> peptideModifieds = new HashSet<string>();
            if (oxidation)
                peptides = Oxidation(peptides);
            if (deamidation)
                peptides = Deamidation(peptides);

            foreach(string peptide in peptides)
            {
                if (filter(peptide))
                    peptideModifieds.Add(peptide);
            }

            return peptideModifieds;
        }

        public static HashSet<string> Oxidation(HashSet<string> peptides)
        {
            HashSet<string> results = new HashSet<string>();
            foreach (string peptide in peptides)
            {
                results.UnionWith(Oxidation(peptide));
            }
            return results;
        }

        public static HashSet<string> Oxidation(string sequence)
        {
            return Modification(sequence, 'M', '$');
        }

        public static HashSet<string> Deamidation(HashSet<string> peptides)
        {
            HashSet<string> results = new HashSet<string>();
            foreach(string peptide in peptides)
            {
                results.UnionWith(Deamidation(peptide));
            }
            return results;
        }

        public static HashSet<string> Deamidation(string sequence)
        {
            HashSet<string> results = new HashSet<string>();
            results.UnionWith(Modification(sequence, 'N', '@'));
            results.UnionWith(Modification(sequence, 'Q', '#'));
            return results;
        }

        public static string Interpret(string sequence)
        {
            return sequence.Replace("$", "M*").
                Replace("@", "N^").Replace("#", "Q^");
        }


        public static HashSet<string> Modification(string sequecne, char origin, char replace)
        {
            HashSet<string> results = new HashSet<string>();
            List<int> index = FindChar(sequecne, origin);
            List<List<int>> permutes = Combination(index);
            foreach(List<int> vec in permutes)
            {
                results.Add(ReplaceString(sequecne, vec, replace));
            }
            return results;
        }
        

        public static List<List<int>> Combination(List<int> sequence)
        {
            List<List<int>> results = new List<List<int>>();
            List<int> temp = new List<int>();
            RecurCombination(sequence, 0, temp, ref results);
            return results;
        }


        protected static string ReplaceString(string sequence, 
            List<int> index, char replace)
        {
            StringBuilder builder = new StringBuilder(sequence);
            foreach(int i in index)
            {
                builder[i] = replace;
            }
            return builder.ToString();
        }

        protected static List<int> FindChar(string sequence, char c)
        {
            List<int> index = new List<int>();
            for(int i = 0; i < sequence.Length; i++)
            {
                if (sequence[i] == c)
                    index.Add(i);
            }
            return index;
        }

        protected static void RecurCombination(List<int> sequence, int index, 
            List<int> temp, ref List<List<int>> results)
        {
            if (index == sequence.Count)
            {
                results.Add(temp.ToList());
                return;
            }

            temp.Add(sequence[index]);
            RecurCombination(sequence, index + 1, temp, ref results);
            temp.RemoveAt(temp.Count-1);
            RecurCombination(sequence, index + 1, temp, ref results);
        }

    }
}
