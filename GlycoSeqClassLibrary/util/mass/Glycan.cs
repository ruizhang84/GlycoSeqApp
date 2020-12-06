using GlycoSeqClassLibrary.model.glycan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqClassLibrary.util.mass
{
    public class Glycan
    {
        protected static readonly Lazy<Glycan>
            lazy = new Lazy<Glycan>(() => new Glycan());

        public static Glycan To { get { return lazy.Value; } }

        protected Glycan()
        {
            permethylation = false;
        }

        public const double kHexNAc = 203.0794;
        public const double kHex = 162.0528;
        public const double kFuc = 146.0579;
        public const double kNeuAc = 291.0954;
        public const double kNeuGc = 307.0903;

        public const double kPermHexNAc = 245.1263;
        public const double kPermHex = 204.0998;
        public const double kPermFuc = 174.0892;
        public const double kPermNeuAc = 361.1737;  //N-acetyl-neuraminic acid
        public const double kPermNeuGc = 391.1842;  //N-glycolyl-neuraminic acid

        public bool permethylation;
        public void SetPermethylation(bool set)
        {
            permethylation = set;
        }

        private double NativeGlycanMass(IGlycan glycan)
        {
            double mass = 0;
            SortedDictionary<Monosaccharide, int> composite = glycan.Composition();
            foreach (var monosaccharide in composite.Keys)
            {
                switch (monosaccharide)
                {
                    case Monosaccharide.GlcNAc:
                        mass += kHexNAc * composite[monosaccharide];
                        break;
                    case Monosaccharide.Gal:
                        mass += kHex * composite[monosaccharide];
                        break;
                    case Monosaccharide.Man:
                        mass += kHex * composite[monosaccharide];
                        break;
                    case Monosaccharide.Fuc:
                        mass += kFuc * composite[monosaccharide];
                        break;
                    case Monosaccharide.NeuAc:
                        mass += kNeuAc * composite[monosaccharide];
                        break;
                    case Monosaccharide.NeuGc:
                        mass += kNeuGc * composite[monosaccharide];
                        break;
                    default:
                        break;
                }
            }
            return mass;
        }

        private double PermethylatedGlycanMass(IGlycan glycan)
        {
            double mass = 0;
            SortedDictionary<Monosaccharide, int> composite = glycan.Composition();
            foreach (var monosaccharide in composite.Keys)
            {
                switch (monosaccharide)
                {
                    case Monosaccharide.GlcNAc:
                        mass += kPermHexNAc * composite[monosaccharide];
                        break;
                    case Monosaccharide.Gal:
                        mass += kPermHex * composite[monosaccharide];
                        break;
                    case Monosaccharide.Man:
                        mass += kPermHex * composite[monosaccharide];
                        break;
                    case Monosaccharide.Fuc:
                        mass += kPermFuc * composite[monosaccharide];
                        break;
                    case Monosaccharide.NeuAc:
                        mass += kPermNeuAc * composite[monosaccharide];
                        break;
                    case Monosaccharide.NeuGc:
                        mass += kPermNeuGc * composite[monosaccharide];
                        break;
                    default:
                        break;
                }
            }
            return mass;
        }

        public double Compute(IGlycan glycan)
        {
            if (permethylation)
                return PermethylatedGlycanMass(glycan);
            return NativeGlycanMass(glycan);
        }
    }
}
