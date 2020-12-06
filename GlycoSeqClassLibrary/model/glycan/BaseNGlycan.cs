using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqClassLibrary.model.glycan
{
    public abstract class BaseNGlycan : IGlycan
    {
        protected List<IGlycan> glycans = new List<IGlycan>();
        protected SortedDictionary<Monosaccharide, int> composite 
            = new SortedDictionary<Monosaccharide, int>();
        protected string id;
        protected double mass;

        public abstract int[] Table();

        public void Add(IGlycan glycan)
        {
            glycans.Add(glycan);
        }

        public List<IGlycan> Children()
        {
            return glycans;
        }

        public SortedDictionary<Monosaccharide, int> Composition()
        {
            return composite;
        }

        public abstract List<IGlycan> Grow(Monosaccharide monosaccharide);

        public string ID()
        {
            return string.Join(" ", Table()); ;
        }

        public double Mass()
        {
            return mass;
        }

        public string Name()
        {
            string name = "";
            foreach(Monosaccharide sugar in composite.Keys)
            {
                switch (sugar)
                {
                    case Monosaccharide.GlcNAc:
                        name += "GlcNAc-" + composite[sugar] + " ";
                        break;
                    case Monosaccharide.Man:
                        name += "Man-" + composite[sugar] + " ";
                        break;
                    case Monosaccharide.Gal:
                        name += "Gal-" + composite[sugar] + " ";
                        break;
                    case Monosaccharide.Fuc:
                        name += "Fuc-" + composite[sugar] + " ";
                        break;
                    case Monosaccharide.NeuAc:
                        name += "NeuAc-" + composite[sugar] + " ";
                        break;
                    case Monosaccharide.NeuGc:
                        name += "NeuGc-" + composite[sugar] + " ";
                        break;
                    default:
                        break;
                }
            }
            return name;
        }

        public void SetComposition(SortedDictionary<Monosaccharide, int> composite)
        {
            this.composite = new SortedDictionary<Monosaccharide, int>();
            foreach(var key in composite.Keys)
            {
                this.composite[key] = composite[key];
            }
        }

        public void SetMass(double mass)
        {
            this.mass = mass;
        }

    }
}
