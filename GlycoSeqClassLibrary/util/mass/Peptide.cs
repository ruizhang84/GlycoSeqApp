using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqClassLibrary.util.mass
{
    public class Peptide
    {
        protected static readonly Lazy<Peptide>
        lazy = new Lazy<Peptide>(() => new Peptide());

        public static Peptide To { get { return lazy.Value; } }

        protected Peptide()
        {
        }

        public double Compute(string sequence)
        {
            double mass = 18.0105;  //water
            foreach (char s in sequence)
            {
                if (s == 'C')
                {
                    mass += 57.02146;
                }
                mass += GetAminoAcidMW(s);
            }
            return mass;
        }

        public double GetAminoAcidMW(char amino)
        {
            switch (amino)
            {
                case 'A':
                    return 71.0371;
                case 'C':
                    return 103.00919;
                case 'D':
                    return 115.02694;
                case 'E':
                    return 129.04259;
                case 'F':
                    return 147.06841;
                case 'G':
                    return 57.02146;
                case 'H':
                    return 137.05891;
                case 'I':
                    return 113.08406;
                case 'K':
                    return 128.09496; //128.09497
                case 'L':
                    return 113.08406;
                case 'M':
                    return 131.04049;
                case 'N':
                    return 114.04293;
                case 'P':
                    return 97.05276;
                case 'Q':
                    return 128.05858;
                case 'R':
                    return 156.10111; //156.10112
                case 'S':
                    return 87.03203;
                case 'T':
                    return 101.04768;
                case 'V':
                    return 99.06841; //99.06842
                case 'W':
                    return 186.07931; //186.07932
                case 'Y':
                    return 163.06333;
                default:
                    return 118.9;   //Average molecular weight of an amino acid
            }
        }
    }
}
