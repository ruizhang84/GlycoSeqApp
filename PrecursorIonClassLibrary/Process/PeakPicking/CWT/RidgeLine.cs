using System;
using System.Collections.Generic;
using System.Text;

namespace PrecursorIonClassLibrary.Process.PeakPicking.CWT
{
    public class RidgeLine : IComparable<RidgeLine>
    {
        // the current position of the line
        public double Pos { get; set; }
        public int Gap { get; set; }
        public int Length { get; set; }
        // the scale index that line extended
        public List<int> Index { get; set; } = new List<int>();
        // the positions that line extend
        public List<double> Trace { get; set; } = new List<double>();

        public RidgeLine(double pos, int indx)
        { 
            Pos = pos;
            Gap = 0;
            Index = new List<int>() { indx };
            Trace = new List<double>() { pos };
        }

        public void Add(double pos, int indx)
        {
            Pos = pos;
            Gap = 0;
            Index.Add(indx);
            Trace.Add(pos);
        }

        public int CompareTo(RidgeLine other)
        {
            if (Pos < other.Pos) return -1;
            else if (Pos == other.Pos) return 0;
            return 1;
        }
    }
}
