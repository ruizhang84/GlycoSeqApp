using PrecursorIonClassLibrary.Algorithm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrecursorIonClassLibrary.Process.PeakPicking.CWT
{
    public class RidgeLineFinder
    {
        private double window; // sliding window
        private List<double> t; // time series
        private SortedDictionary<double, List<double>> matrix; // 2-d matix (a, coeff)
        private List<RidgeLine> lines;
        private List<RidgeLine> finalLines;
        private double thresh; // gap number threshold
        private double snr; //signale-to-noise ratio
        private double percent; // the percentile for noise
        private double snrWindow; // the window size for percentile 
        private int minLength; // the min length of the ridge line

        public RidgeLineFinder(
            double window = -1, double thresh = -1, double snr = 1.0, int minLength = 2,
            double percent=0.95,  double snrWindow=1.0)
        {
            lines = new List<RidgeLine>();
            finalLines = new List<RidgeLine>();
            this.window = window;
            this.snrWindow = snrWindow;
            this.snr = snr;
            this.percent = percent;
            this.minLength = minLength;
            this.thresh = thresh;
        }

        public void SetParameter(double window = -1, double thresh = -1, 
            double snr = 1.0, int minLength = 2,
            double percent = 0.95, double snrWindow = 1.0)
        {
            this.window = window;
            this.snrWindow = snrWindow;
            this.snr = snr;
            this.percent = percent;
            this.minLength = minLength;
            this.thresh = thresh;
        }

        public List<double> Coeffcient (double scale)
        {
            if (!matrix.ContainsKey(scale))
                return new List<double>();
            return matrix[scale];
        }       
        
        // find the max value within a window size
        public List<int> WindowMaxamIndexes(double scale)
        {
            HashSet<int> indexes = new HashSet<int>();
            double winSize = window;
            if (winSize <= 0)
                winSize = scale / 2.0;
        
            if (!matrix.ContainsKey(scale) || t.Count == 0)
                return indexes.ToList();

            List<double> coeff = matrix[scale];
            if (coeff.Count != t.Count)
                throw new Exception("the coefficient size is not equal to time size!");

            LinkedList<int> indexer = new LinkedList<int>();

            // init
            int idx = 0, maxIdx = 0;
            for(; idx < t.Count; idx++)
            {
                if (t[idx] - t[0] >= winSize)
                    break;

                while (indexer.Count > 0 && coeff[indexer.Last.Value] < coeff[idx])
                {
                    indexer.RemoveLast();
                }
                indexer.AddLast(idx);
                if (coeff[idx] > coeff[maxIdx])
                    maxIdx = idx;
            }
            indexes.Add(maxIdx);

            //sliding window
            for(; idx < t.Count; idx++)
            {
                while(indexer.Count > 0 && t[idx] - t[indexer.First.Value] > winSize)
                {
                    indexer.RemoveFirst();
                }

                while (indexer.Count > 0 && coeff[indexer.Last.Value] <= coeff[idx])
                {
                    indexer.RemoveLast();
                }
                indexer.AddLast(idx);
                indexes.Add(indexer.First.Value); 
            }

            return indexes.OrderBy(i => i).ToList();
        }
         
        public List<int> LocalMaxamIndexes(double scale)
        {
            List<int> indexes = new List<int>();
            List<int> maxIndexes = WindowMaxamIndexes(scale);
            List<double> coeff = matrix[scale];

            foreach (int index in maxIndexes)
            {
                if (index > 0 && coeff[index - 1] > coeff[index])
                    continue;
                else if (index < coeff.Count - 1 && coeff[index + 1] > coeff[index])
                    continue;
                indexes.Add(index);
            }
            return indexes;
        }

        private void IdentifyRidgeLines(double scale, int indx)
        {
            List<int> local = LocalMaxamIndexes(scale);

            // init ridge line
            if (lines.Count == 0)
            {
                foreach(int pos in local)
                {
                    lines.Add(new RidgeLine(t[pos], indx));
                }
                return;
            }

            // try extend ridge lines
            double winSize = window;
            if (winSize <= 0)
                winSize = scale / 2.0;

            List<RidgeLine> new_lines = new List<RidgeLine>();
            // Add 1 to gap, later will reset to zero if extend
            foreach(RidgeLine l in lines)
            {
                l.Gap += 1;
                l.Length += 1;
            }
            // try to extend the lines
            foreach (int p in local)
            {
                int i = BinarySearch.Search(
                    lines.Select(l => l.Pos).ToList(),
                    t[p], winSize);
                if (i == -1)
                    new_lines.Add(new RidgeLine(t[p], indx));
                else
                {
                    lines[i].Add(t[p], indx);
                }
            }
            // remove lines if they are over gapping
            foreach(RidgeLine l in lines)
            {
                if (l.Gap <= thresh)
                    new_lines.Add(l);
                else
                {
                    finalLines.Add(l);
                }
            }
            lines = new_lines;
        }
        public static double Percentile(IEnumerable<double> seq, double percentile)
        {
            var elements = seq.ToArray();
            Array.Sort(elements);
            double realIndex = percentile * (elements.Length - 1);
            int index = (int)realIndex;
            double frac = realIndex - index;
            if (index + 1 < elements.Length)
                return elements[index] * (1 - frac) + elements[index + 1] * frac;
            else
                return elements[index];
        }

        private double SNR(RidgeLine line)
        {
            double strength = 0;
            int index = t.IndexOf(line.Pos);
            foreach(var item in matrix)
            {
                strength = Math.Max(strength, item.Value[index]); 
            }

            List<double> coeff = matrix[1.0];
            List<double> coeffList = new List<double>();
            for(int i = index; i < coeff.Count; i++)
            {
                if (t[i] - t[index] > snrWindow)
                    break;
                coeffList.Add(Math.Abs(coeff[i]));
            }
            for(int i = index-1; i >= 0; i--)
            {
                if (t[index] - t[i] > snrWindow)
                    break;
                coeffList.Add(Math.Abs(coeff[i]));
            }
            double noise = Math.Max(0.001, Percentile(coeffList, percent));

            return strength / noise;
        }

        public List<RidgeLine> FilterRidgeLine(List<RidgeLine> final)
        {
            List<RidgeLine> filtered = new List<RidgeLine>();
            final.Sort();

            // merge lines at the same position
            foreach(RidgeLine line in final)
            {
                if (line.Length < minLength)
                    continue;

                if (filtered.Count == 0)
                {
                    if (SNR(line) > snr)
                        filtered.Add(line);
                }
                else if (filtered.Last().Pos == line.Pos )
                {
                    if (filtered.Last().Length < line.Length && SNR(line) > snr)
                    {
                        filtered[filtered.Count - 1] = line;
                    }
                }
                else
                {
                    if (SNR(line) > snr)
                        filtered.Add(line);
                }
            }

            return filtered;
        }

        public void Init(List<double> t, SortedDictionary<double, List<double>> matrix)
        {
            lines.Clear();
            finalLines.Clear();
            this.t = t;
            this.matrix = matrix;
            if (thresh < 0)
               thresh = matrix.Keys.Min();
        }

        public List<RidgeLine> Find(List<double> t, SortedDictionary<double, List<double>> matrix)
        {
            // init
            Init(t, matrix);

            int indx = matrix.Count - 1;
            foreach (var item in matrix.Reverse())
            {
                double scale = item.Key;
                IdentifyRidgeLines(scale, indx--);
            }
            finalLines.AddRange(lines);
            return FilterRidgeLine(finalLines);
        }



    }
}
