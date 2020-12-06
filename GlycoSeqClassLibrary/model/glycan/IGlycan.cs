using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqClassLibrary.model.glycan
{
    public enum Monosaccharide
    { GlcNAc, Man, Gal, Fuc, NeuAc, NeuGc }

    public interface IGlycan
    {
        double Mass();
        void SetMass(double mass);
        List<IGlycan> Children();
        void Add(IGlycan glycan);
        string Name();
        string ID();
        int[] Table();
        SortedDictionary<Monosaccharide, int> Composition();
        void SetComposition(SortedDictionary<Monosaccharide, int> composition);
        List<IGlycan> Grow(Monosaccharide monosaccharide);
    }
}
