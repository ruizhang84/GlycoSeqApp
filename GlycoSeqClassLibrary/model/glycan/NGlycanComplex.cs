using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqClassLibrary.model.glycan
{
    public class NGlycanComplex : BaseNGlycan, IGlycan
    {
        private int[] table_ = new int[24];

        public override int[] Table() { return table_;  }
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
                    if (ValidAddGlcNAcCore())
                    {
                        NGlycanComplex ptr = CreateByAddGlcNAcCore();
                        glycans.Add(ptr);
                    }
                    else if (ValidAddGlcNAc())
                    {
                        if (ValidAddGlcNAcBisect())
                        {
                            NGlycanComplex ptr = CreateByAddGlcNAcBisect();
                            glycans.Add(ptr);
                        }
                        if (ValidAddGlcNAcBranch())
                        {
                            List<NGlycanComplex> gs = CreateByAddGlcNAcBranch();
                            glycans.AddRange(gs);
                        }
                    }
                    break;

                case Monosaccharide.Man:
                    if (ValidAddMan())
                    {
                        NGlycanComplex ptr = CreateByAddMan();
                        glycans.Add(ptr);
                    }
                    break;

                case Monosaccharide.Gal:
                    if (ValidAddGal())
                    {
                       List<NGlycanComplex> gs = CreateByAddGal();
                        glycans.AddRange(gs);
                    }
                    break;

                case Monosaccharide.Fuc:
                    if (ValidAddFucCore())
                    {
                        NGlycanComplex ptr = CreateByAddFucCore();
                        glycans.Add(ptr);
                    }
                    if (ValidAddFucTerminal())
                    {
                        List<NGlycanComplex> gs = CreateByAddFucTerminal();
                        glycans.AddRange(gs);
                    }
                    break;

                case Monosaccharide.NeuAc:
                    if (ValidAddNeuAc())
                    {
                        List<NGlycanComplex> gs = CreateByAddNeuAc();
                        glycans.AddRange(gs);
                    }
                    break;

                case Monosaccharide.NeuGc:
                    if (ValidAddNeuGc())
                    {
                        List<NGlycanComplex> gs = CreateByAddNeuGc();
                        glycans.AddRange(gs);
                    }
                    break;

                default:
                    break;
            }
            return glycans;
        }


        bool ValidAddGlcNAcCore()
        {
            return table_[0] < 2;
        }

        bool ValidAddGlcNAc()
        {
            return (table_[0] == 2 && table_[1] == 3);
        }

        NGlycanComplex CreateByAddGlcNAcCore()
        {
            var g = new NGlycanComplex();
            g.SetTable(table_);
            g.table_[0] = g.table_[0] + 1;
            g.SetComposition(composite);
            g.AddMonosaccharide(Monosaccharide.GlcNAc);
       
            return g;
        }

        bool ValidAddGlcNAcBisect()
        {
            //bisect 0, not extanding on GlcNAc
            return (table_[1] == 3 && table_[3] == 0 && table_[4] == 0);
        }

        NGlycanComplex CreateByAddGlcNAcBisect()
        {
            var g = new NGlycanComplex();
            g.SetTable(table_);
            g.table_[3] = 1;
            g.SetComposition(composite);
            g.AddMonosaccharide(Monosaccharide.GlcNAc);
            return g;
        }

        bool ValidAddGlcNAcBranch()
        {
            for (int i = 0; i < 4; i++)
            {
                if (i == 0 || table_[i + 4] < table_[i + 3]) // make it order
                {
                    if (table_[i + 4] == table_[i + 8] && table_[i + 12] == 0 && table_[i + 16] == 0 && table_[i + 20] == 0)
                    //equal GlcNAc Gal, no Fucose attached at terminal, no terminal NeuAc, NeuGc
                    {
                        return true;
                    }
                }
            }
            return false;

        }

        List<NGlycanComplex> CreateByAddGlcNAcBranch()
        {
            List<NGlycanComplex> glycans = new List<NGlycanComplex>();
            for (int i = 0; i < 4; i++)
            {
                if (i == 0 || table_[i + 4] < table_[i + 3]) // make it order
                {
                    if (table_[i + 4] == table_[i + 8] && table_[i + 12] == 0 && table_[i + 16] == 0 && table_[i + 20] == 0)
                    {
                        var g = new NGlycanComplex();
                        g.SetTable(table_);
                        g.table_[i + 4] = g.table_[i + 4] + 1;
                        g.SetComposition(composite);
                        g.AddMonosaccharide(Monosaccharide.GlcNAc);
                        glycans.Add(g);
                    }
                }
            }
            return glycans;
        }

        bool ValidAddMan()
        {
            return (table_[0] == 2 && table_[1] < 3);
        }

        NGlycanComplex CreateByAddMan()
        {
            var g = new NGlycanComplex();
            g.SetTable(table_);
            g.table_[1] = g.table_[1] + 1;
            g.SetComposition(composite);
            g.AddMonosaccharide(Monosaccharide.Man);
            glycans.Add(g);
            return g;
        }

        bool ValidAddGal()
        {
            for (int i = 0; i < 4; i++)
            {
                if (i == 0 || table_[i + 8] < table_[i + 7]) // make it order
                {
                    if (table_[i + 4] == table_[i + 8] + 1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        List<NGlycanComplex> CreateByAddGal()
        {
            List<NGlycanComplex> glycans = new List<NGlycanComplex>();
            for (int i = 0; i < 4; i++)
            {
                if (i == 0 || table_[i + 8] < table_[i + 7]) // make it order
                {
                    if (table_[i + 4] == table_[i + 8] + 1)
                    {
                        var g = new NGlycanComplex();
                        g.SetTable(table_);
                        g.table_[i + 8] = g.table_[i + 8] + 1;
                        g.SetComposition(composite);
                        g.AddMonosaccharide(Monosaccharide.Gal);
                        glycans.Add(g);
                    }
                }
            }
            return glycans;
        }

        bool ValidAddFucCore()
        {
            return (table_[2] == 0);  //core
            //return (table_[0] == 1 && table_[1] == 0 && table_[2] == 0);  //core
        }

        NGlycanComplex CreateByAddFucCore()
        {
            var g = new NGlycanComplex();
            g.SetTable(table_);
            g.table_[2] = 1;
            g.SetComposition(composite);
            g.AddMonosaccharide(Monosaccharide.Fuc);
            return g;
        }

        bool ValidAddFucTerminal()
        {
            for (int i = 0; i < 4; i++)
            {
                if (i == 0 || table_[i + 12] < table_[i + 11]) // make it order
                {
                    if (table_[i + 12] == 0 && table_[i + 4] > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        List<NGlycanComplex> CreateByAddFucTerminal()
        {
            List<NGlycanComplex> glycans = new List<NGlycanComplex>();
            for (int i = 0; i < 4; i++)
            {
                if (i == 0 || table_[i + 12] < table_[i + 11]) // make it order
                {
                    if (table_[i + 12] == 0 && table_[i + 4] > 0)
                    {
                        var g = new NGlycanComplex();
                        g.SetTable(table_);
                        g.table_[i + 12] = 1;
                        g.SetComposition(composite);
                        g.AddMonosaccharide(Monosaccharide.Fuc);
                        glycans.Add(g);
                    }
                }
            }
            return glycans;
        }

        bool ValidAddNeuAc()
        {
            for (int i = 0; i < 4; i++)
            {
                if (i == 0 || table_[i + 16] < table_[i + 15]) // make it order
                {
                    if (table_[i + 4] > 0 && table_[i + 4] == table_[i + 8] && table_[i + 16] == 0 && table_[i + 20] == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        List<NGlycanComplex> CreateByAddNeuAc()
        {
            List<NGlycanComplex> glycans = new List<NGlycanComplex>();
            for (int i = 0; i < 4; i++)
            {
                if (i == 0 || table_[i + 16] < table_[i + 15]) // make it order
                {
                    if (table_[i + 4] > 0 && table_[i + 4] == table_[i + 8] && table_[i + 16] == 0 && table_[i + 20] == 0)
                    {
                        var g = new NGlycanComplex();
                        g.SetTable(table_);
                        g.table_[i + 16] = 1;
                        g.SetComposition(composite);
                        g.AddMonosaccharide(Monosaccharide.NeuAc);
                        glycans.Add(g);
                    }
                }
            }
            return glycans;
        }

        bool ValidAddNeuGc()
        {
            for (int i = 0; i < 4; i++)
            {
                if (i == 0 || table_[i + 20] < table_[i + 19]) // make it order
                {
                    if (table_[i + 4] > 0 && table_[i + 4] == table_[i + 8] && table_[i + 16] == 0 && table_[i + 20] == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        List<NGlycanComplex> CreateByAddNeuGc()
        {
            List<NGlycanComplex> glycans = new List<NGlycanComplex>();
            for (int i = 0; i < 4; i++)
            {
                if (i == 0 || table_[i + 20] < table_[i + 19]) // make it order
                {
                    if (table_[i + 4] > 0 && table_[i + 4] == table_[i + 8] && table_[i + 16] == 0 && table_[i + 20] == 0)
                    {
                        var g = new NGlycanComplex();
                        g.SetTable(table_);
                        g.table_[i + 20] = 1;
                        g.SetComposition(composite);
                        g.AddMonosaccharide(Monosaccharide.NeuGc);
                        glycans.Add(g);
                    }
                }
            }
            return glycans;
        }
    }
}
