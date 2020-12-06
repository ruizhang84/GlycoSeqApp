using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqClassLibrary.engine.analysis
{
    public class SearchResult
    {
        int scan_;
        string peptide_;
        string glycan_;
        int pos_;
        double score_;

        public int Scan()  { return scan_; }
        public int ModifySite()  { return pos_; }
        public string Sequence()  { return peptide_; }
        public string Glycan()  { return glycan_; }
        public double Score() { return score_; }

        public void set_scan(int scan) { scan_ = scan; }
        public void set_site(int pos) { pos_ = pos; }
        public void set_peptide(string seq) { peptide_ = seq; }
        public void set_glycan(string glycan) { glycan_ = glycan; }
        public void set_score(double score) { score_ = score; }

    }
}
