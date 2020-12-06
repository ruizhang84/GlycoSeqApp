using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqClassLibrary.model.protein
{
    public class BaseProtein : IProtein
    {
        protected string id;
        protected string sequence;

        public string ID()
        {
            return id;
        }

        public string Sequence()
        {
            return sequence;
        }

        public void SetID(string id)
        {
            this.id = id;
        }

        public void SetSequence(string sequence)
        {
            this.sequence = sequence;
        }
    }
}
