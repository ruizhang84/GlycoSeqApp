using GlycoSeqClassLibrary.model.glycan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqClassLibrary.engine.glycan
{
    public class GlycanBuilder
    {
        protected int hexNAc_;
        protected int hex_;
        protected int fuc_;
        protected int neuAc_;
        protected int neuGc_;
        protected Dictionary<double, List<string>> glycans_; // glycan mass, glycan id
        protected Dictionary<string, IGlycan> glycans_map_; // glycan id -> glycan
        protected List<Monosaccharide> candidates_;

        public GlycanBuilder(int hexNAc=12, int hex=12, int fuc=5, int neuAc=4, int neuGc=0)
        {
            hexNAc_ = hexNAc;
            hex_ = hex;
            fuc_ = fuc;
            neuAc_ = neuAc;
            neuGc_ = neuGc;
            glycans_ = new Dictionary<double, List<string>>();
            glycans_map_ = new Dictionary<string, IGlycan>();
            candidates_ = new List<Monosaccharide>()
            {
                Monosaccharide.GlcNAc,
                Monosaccharide.Man,
                Monosaccharide.Gal,
                Monosaccharide.Fuc,
                Monosaccharide.NeuAc,
                Monosaccharide.NeuGc
            };
        }

        public Dictionary<double, List<string>> Glycans()
            { return glycans_; }

        public Dictionary<string, IGlycan> GlycanMaps()
            { return glycans_map_; }

        public List<Monosaccharide> Candidates() { return candidates_; }

        public int HexNAc() { return hexNAc_; }
        public int Hex() { return hex_; }
        public int Fuc() { return fuc_; }
        public int NeuAc() { return neuAc_; }
        public int NeuGc() { return neuGc_; }
        public void set_candidates(List<Monosaccharide> sugars)
            { candidates_ = sugars; }
        public void set_HexNAc(int num) { hexNAc_ = num; }
        public void set_Hex(int num) { hex_ = num; }
        public void set_Fuc(int num) { fuc_ = num; }
        public void set_NeuAc(int num) { neuAc_ = num; }
        public void set_NeuGc(int num) { neuGc_ = num; }

        public void Build()
        {
            NGlycanComplex root = new NGlycanComplex();

            string root_id = root.ID();
            glycans_map_[root_id] = root;

            Queue<IGlycan> queue = new Queue<IGlycan>();
            queue.Enqueue(glycans_map_[root_id]);

            while (queue.Count > 0)
            {
                IGlycan node = queue.Peek();
                queue.Dequeue();

                // update table id
                double mass = util.mass.Glycan.To.Compute(node);
                string table_id = node.ID();
                node.SetMass(mass);
                if (!glycans_.ContainsKey(mass))
                {
                    glycans_[mass] = new List<string>();
                }
                glycans_[mass].Add(table_id);

                // next
                foreach (var it in candidates_)
                {
                    List<IGlycan> res = node.Grow(it);

                    foreach (var g in res)
                    {
                        if (SatisfyCriteria(g))
                        {
                            string id = g.ID();
                            if (!glycans_map_.ContainsKey(id))
                            {
                                node.Add(g);
                                glycans_map_[id] = g;
                                queue.Enqueue(glycans_map_[id]);
                            }
                            else
                            {
                                node.Add(glycans_map_[id]);
                            }
                        }
                    }
                }

            }
        }

        bool SatisfyCriteria(IGlycan glycan)
        {
            int hexNAc = 0, hex = 0, fuc = 0, neuAc = 0, neuGc = 0;
            SortedDictionary<Monosaccharide, int> composite = glycan.Composition();
            foreach(var key in composite.Keys)
            {
                switch (key)
                {
                case Monosaccharide.GlcNAc:
                    hexNAc += composite[key];
                    break;
                case Monosaccharide.Gal:
                    hex += composite[key];
                    break;
                case Monosaccharide.Man:
                    hex += composite[key];
                    break;    
                case Monosaccharide.Fuc:
                    fuc += composite[key];
                    break;   
                case Monosaccharide.NeuAc:
                    neuAc += composite[key];
                    break;   
                case Monosaccharide.NeuGc:
                    neuGc += composite[key];
                    break;           
                default:
                    break;
                }
            }
            return (hexNAc <= hexNAc_ && hex <= hex_ && fuc <= fuc_
                    && neuAc <= neuAc_ && neuGc <= neuGc_);
        }


    }
}
