using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqClassLibrary.engine.protein
{
    public class ProteinPTM
    {
        public static bool ContainsNGlycanSite(string sequence)
        {
            for (int i = 0; i < sequence.Length - 2; i++)
            {
                char s = sequence[i];
                char nxs = sequence[i + 2];
                if (s == 'N' && (nxs == 'S' || nxs == 'T')) return true;
            }

            return false;
        }

        public static List<int> FindNGlycanSite(string sequence)
        {
            List<int> pos = new List<int>();
            for (int i = 0; i < sequence.Length - 2; i++)
            {
                char s = sequence[i];
                char nxs = sequence[i + 2];
                if (s == 'N' && (nxs == 'S' || nxs == 'T')) pos.Add(i);
            }

            return pos;
        }

        public static List<int> FindOGlycanSite(string sequence)
        {
            List<int> pos = new List<int>();
            for (int i = 0; i < sequence.Length; i++)
            {
                char s = sequence[i];
                if (s == 'S' || s == 'T') pos.Add(i);
            }

            return pos;
        }

        public static bool ContainsOGlycanSite(string sequence)
        {
            for (int i = 0; i < sequence.Length; i++)
            {
                char s = sequence[i];
                if (s == 'S' || s == 'T') return true;
            }

            return false;
        }
    }
}
