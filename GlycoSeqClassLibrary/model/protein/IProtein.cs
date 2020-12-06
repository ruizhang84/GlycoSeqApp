using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqClassLibrary.model.protein
{
    public interface IProtein
    {
        string Sequence();
        string ID();
        void SetSequence(string sequence);
        void SetID(string id);
    }
}
