using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqClassLibrary.engine.protein
{
    public enum Proteases { Trypsin, Pepsin, Chymotrypsin, GluC };

    public delegate bool FilterSequence(string sequence);

    public class ProteinDigest
    {
        int miss_cleavage_;
        int min_length_;
        Proteases enzyme_;

        public ProteinDigest(int missCleavage, int minLength, Proteases enzyme)
        {
            miss_cleavage_ = missCleavage;
            min_length_ = minLength;
            enzyme_ = enzyme;
        }

        public int MissCleavage() { return miss_cleavage_; }
        public int MinLength() { return min_length_; }
        public Proteases Enzyme() { return enzyme_; }
        public void set_min_length(int length) { min_length_ = length; }
        public void set_miss_cleavage(int num) { miss_cleavage_ = num; }
        public void SetProtease(Proteases enzyme) { enzyme_ = enzyme; }

        public List<string> Sequences(string sequence, FilterSequence filter)
        {
            List<string> pepList = new List<string>();
            List<int> cutOffPosition = FindCutOffPosition(sequence);
            //generate substring from sequences
            int start, end;
            for (int i = 0; i <= miss_cleavage_; i++)
            {
                for (int j = 0; j < cutOffPosition.Count - i - 1; j++)
                {
                    start = cutOffPosition[j] + 1;
                    end = cutOffPosition[j + 1 + i];
                    if (end - start + 1 >= min_length_)  // put minimum length in place
                    {
                        string sub = sequence.Substring(start, end - start + 1);
                        if (filter(sub))
                            pepList.Add(sub);
                    }
                }
            }
            return pepList;
        }

        List<int> FindCutOffPosition(string sequence)
        {
            //get cleavable position, make all possible peptide cutoff  positoins
            List<int> cutOffPosition = new List<int>();

            cutOffPosition.Add(-1); //trivial to include starting place

            for (int i = 0; i < sequence.Length; i++)
            {
                if (IsCleavablePosition(sequence, i))    //enzyme
                {
                    cutOffPosition.Add(i);
                }
            }
            if (!IsCleavablePosition(sequence, sequence.Length - 1))
            {
                cutOffPosition.Add(sequence.Length - 1); //trivial to include ending place
            }

            return cutOffPosition;
        }

        bool IsCleavablePosition(string sequence, int index)
        {
            char s = char.ToUpper(sequence[index]);
            switch (enzyme_)
            {
                //cleaves peptides on the C-terminal side of lysine and arginine
                case Proteases.Trypsin:
                    //proline residue is on the carboxyl side of the cleavage site
                    if (index < sequence.Length - 1 && char.ToUpper(sequence[index + 1]) == 'P')
                    {
                        return false;
                    }
                    else if (s == 'K' || s == 'R')
                    {
                        return true;
                    }
                    break;
                //cuts after aromatic amino acids such as phenylalanine, tryptophan, and tyrosine.
                case Proteases.Pepsin:
                    if (s == 'W' || s == 'F' || s == 'Y')
                    {
                        return true;
                    }
                    break;
                case Proteases.Chymotrypsin:
                    if (index < sequence.Length - 1 && char.ToUpper(sequence[index + 1]) == 'P')
                    {
                        return false;
                    }
                    else if (s == 'W' || s == 'F' || s == 'Y')
                    {
                        return true;
                    }
                    break;
                case Proteases.GluC:
                    if (index < sequence.Length - 1 && char.ToUpper(sequence[index + 1]) == 'P')
                    {
                        return false;
                    }
                    else if (s == 'E' || s == 'D')
                    {
                        return true;
                    }
                    break;
                default:
                    break;
            }

            return false;
        }

    }
}
