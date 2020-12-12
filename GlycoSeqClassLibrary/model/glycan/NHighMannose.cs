using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqClassLibrary.model.glycan
{
    public class NHighMannose : BaseNGlycan, IGlycan
    {
        private int[] table_ = new int[6];

        public override int[] Table() { return table_; }
        void SetTable(int[] table)
        {
            for (int i = 0; i < table_.Length; i++)
                table_[i] = table[i];
        }

        void AddMonosaccharide(Monosaccharide sugar)
        {
            if (composite.ContainsKey(sugar))
            {
                composite[sugar] += 1;
            }
            else
            {
                composite[sugar] = 1;
            }
        }

        public override List<IGlycan> Grow(Monosaccharide monosaccharide)
        {
            List<IGlycan> glycans = new List<IGlycan>();
            switch (monosaccharide)
            {
                case Monosaccharide.GlcNAc:
                    if (ValidAddGlcNAc())
                    {
                        NHighMannose ptr = CreateByAddGlcNAc();
                        glycans.Add(ptr);
                    }
                    
                    break;
                case Monosaccharide.Man:
                    if (ValidAddManCore())
                    {
                        NHighMannose ptr = CreateByAddManCore();
                        glycans.Add(ptr);
                    }
                    else if (ValidAddManBranch())
                    {
                        List<NHighMannose> gs = CreateByAddManBranch();
                        glycans.AddRange(gs);

                    }
                    break;

                case Monosaccharide.Fuc:
                    if (ValidAddFucCore())
                    {
                        NHighMannose ptr = CreateByAddFucCore();
                        glycans.Add(ptr);
                    }  
                    break;

                default:
                    break;
            }
            return glycans;
        }


        bool ValidAddGlcNAc()
        {
            return table_[0] < 2;
        }

        NHighMannose CreateByAddGlcNAc()
        {
            var g = new NHighMannose();
            g.SetTable(table_);
            g.table_[0] = g.table_[0] + 1;
            g.SetComposition(composite);
            g.AddMonosaccharide(Monosaccharide.GlcNAc);

            return g;
        }


        bool ValidAddManCore()
        {
            return (table_[0] == 2 && table_[1] < 3);
        }

        NHighMannose CreateByAddManCore()
        {
            var g = new NHighMannose();
            g.SetTable(table_);
            g.table_[1] = g.table_[1] + 1;
            g.SetComposition(composite);
            g.AddMonosaccharide(Monosaccharide.Man);
            glycans.Add(g);
            return g;
        }

        bool ValidAddManBranch()
        {
            if (table_[0] == 2 && table_[1] == 3)
            {
                return true;
            }
            return false;
        }

        List<NHighMannose> CreateByAddManBranch()
        {
            List<NHighMannose> glycans = new List<NHighMannose>();
            for (int i = 0; i < 3; i++)
            {
                if (i == 0 || table_[i + 3] < table_[i + 2]) // make it order
                {

                    var g = new NHighMannose();
                    g.SetTable(table_);
                    g.table_[i + 3] = g.table_[i + 3] + 1;
                    g.SetComposition(composite);
                    g.AddMonosaccharide(Monosaccharide.Man);
                    glycans.Add(g);

                }
            }
            return glycans;
        }

        bool ValidAddFucCore()
        {
            return table_[1] == 0 && table_[2] == 0;  //core
        }

        NHighMannose CreateByAddFucCore()
        {
            var g = new NHighMannose();
            g.SetTable(table_);
            g.table_[2] = 1;
            g.SetComposition(composite);
            g.AddMonosaccharide(Monosaccharide.Fuc);
            return g;
        }

    }
}
