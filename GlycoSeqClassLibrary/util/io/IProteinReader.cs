using MultiGlycanClassLibrary.model.protein;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqClassLibrary.util.io
{
    public interface IProteinReader
    {
        List<IProtein> Read(string path);
        string Path();
        void SetPath(string path);
    } 
}
