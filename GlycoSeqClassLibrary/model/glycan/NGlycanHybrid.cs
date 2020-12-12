using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqClassLibrary.model.glycan
{
    public class NGlycanHybrid : BaseNGlycan, IGlycan
    {
        private int[] table_ = new int[16];

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
                    if (ValidAddGlcNAcCore())
                    {
                        NGlycanHybrid ptr = CreateByAddGlcNAcCore();
                        glycans.Add(ptr);
                    }
                    else if (ValidAddGlcNAc())
                    {
                        if (ValidAddGlcNAcBisect())
                        {
                            NGlycanHybrid ptr = CreateByAddGlcNAcBisect();
                            glycans.Add(ptr);
                        }
                        if (ValidAddGlcNAcBranch())
                        {
                            List<NGlycanHybrid> gs = CreateByAddGlcNAcBranch();
                            glycans.AddRange(gs);
                        }
                    }
                    break;

                case Monosaccharide.Man:
                    if (ValidAddManCore())
                    {
                        NGlycanHybrid ptr = CreateByAddManCore();
                        glycans.Add(ptr);
                    }
                    else if (ValidAddManBranch())
                    {
                        List<NGlycanHybrid> gs = CreateByAddManBranch();
                        glycans.AddRange(gs);

                    }
                    break;

                case Monosaccharide.Gal:
                    if (ValidAddGal())
                    {
                        List<NGlycanHybrid> gs = CreateByAddGal();
                        glycans.AddRange(gs);
                    }
                    break;

                case Monosaccharide.Fuc:
                    if (ValidAddFucCore())
                    {
                        NGlycanHybrid ptr = CreateByAddFucCore();
                        glycans.Add(ptr);
                    }
                    else if (ValidAddFucTerminal())
                    {
                        List<NGlycanHybrid> gs = CreateByAddFucTerminal();
                        glycans.AddRange(gs);
                    }
                    break;

                case Monosaccharide.NeuAc:
                    if (ValidAddNeuAc())
                    {
                        List<NGlycanHybrid> gs = CreateByAddNeuAc();
                        glycans.AddRange(gs);
                    }
                    break;

                case Monosaccharide.NeuGc:
                    if (ValidAddNeuGc())
                    {
                        List<NGlycanHybrid> gs = CreateByAddNeuGc();
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

        NGlycanHybrid CreateByAddGlcNAcCore()
        {
            var g = new NGlycanHybrid();
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

        NGlycanHybrid CreateByAddGlcNAcBisect()
        {
            var g = new NGlycanHybrid();
            g.SetTable(table_);
            g.table_[3] = 1;
            g.SetComposition(composite);
            g.AddMonosaccharide(Monosaccharide.GlcNAc);
            return g;
        }

        bool ValidAddGlcNAcBranch()
        {
            for (int i = 0; i < 2; i++)
            {
                if (i == 0 || table_[i + 6] < table_[i + 5]) // make it order
                {
                    if (table_[i + 6] == table_[i + 8] && table_[i + 10] == 0 && table_[i + 12] == 0 && table_[i + 14] == 0)
                    //equal GlcNAc Gal, no Fucose attached at terminal, no terminal NeuAc, NeuGc
                    {
                        return true;
                    }
                }
            }
            return false;

        }

        List<NGlycanHybrid> CreateByAddGlcNAcBranch()
        {
            List<NGlycanHybrid> glycans = new List<NGlycanHybrid>();
            for (int i = 0; i < 2; i++)
            {
                if (i == 0 || table_[i + 6] < table_[i + 5]) // make it order
                {
                    if (table_[i + 6] == table_[i + 8] && table_[i + 10] == 0 && table_[i + 12] == 0 && table_[i + 14] == 0)
                    {
                        var g = new NGlycanHybrid();
                        g.SetTable(table_);
                        g.table_[i + 6] = g.table_[i + 6] + 1;
                        g.SetComposition(composite);
                        g.AddMonosaccharide(Monosaccharide.GlcNAc);
                        glycans.Add(g);
                    }
                }
            }
            return glycans;
        }

        bool ValidAddManCore()
        {
            return (table_[0] == 2 && table_[1] < 3);
        }

        NGlycanHybrid CreateByAddManCore()
        {
            var g = new NGlycanHybrid();
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

        List<NGlycanHybrid> CreateByAddManBranch()
        {
            List<NGlycanHybrid> glycans = new List<NGlycanHybrid>();
            for (int i = 0; i < 2; i++)
            {
                if (i == 0 || table_[i + 4] < table_[i + 3]) // make it order
                {

                    var g = new NGlycanHybrid();
                    g.SetTable(table_);
                    g.table_[i + 4] = g.table_[i + 4] + 1;
                    g.SetComposition(composite);
                    g.AddMonosaccharide(Monosaccharide.Man);
                    glycans.Add(g);

                }
            }
            return glycans;
        }

        bool ValidAddGal()
        {
            for (int i = 0; i < 2; i++)
            {
                if (i == 0 || table_[i + 8] < table_[i + 7]) // make it order
                {
                    if (table_[i + 6] == table_[i + 8] + 1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        List<NGlycanHybrid> CreateByAddGal()
        {
            List<NGlycanHybrid> glycans = new List<NGlycanHybrid>();
            for (int i = 0; i < 2; i++)
            {
                if (i == 0 || table_[i + 8] < table_[i + 7]) // make it order
                {
                    if (table_[i + 6] == table_[i + 8] + 1)
                    {
                        var g = new NGlycanHybrid();
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
            return (table_[0] == 1 && table_[1] == 0 && table_[2] == 0);  //core
        }

        NGlycanHybrid CreateByAddFucCore()
        {
            var g = new NGlycanHybrid();
            g.SetTable(table_);
            g.table_[2] = 1;
            g.SetComposition(composite);
            g.AddMonosaccharide(Monosaccharide.Fuc);
            return g;
        }

        bool ValidAddFucTerminal()
        {
            for (int i = 0; i < 2; i++)
            {
                if (i == 0 || table_[i + 10] < table_[i + 9]) // make it order
                {
                    if (table_[i + 10] == 0 && table_[i + 6] > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        List<NGlycanHybrid> CreateByAddFucTerminal()
        {
            List<NGlycanHybrid> glycans = new List<NGlycanHybrid>();
            for (int i = 0; i < 2; i++)
            {
                if (i == 0 || table_[i + 10] < table_[i + 9]) // make it order
                {
                    if (table_[i + 10] == 0 && table_[i + 6] > 0)
                    {
                        var g = new NGlycanHybrid();
                        g.SetTable(table_);
                        g.table_[i + 10] = 1;
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
            for (int i = 0; i < 2; i++)
            {
                if (i == 0 || table_[i + 12] < table_[i + 11]) // make it order
                {
                    if (table_[i + 6] > 0 && table_[i + 6] == table_[i + 8] && table_[i + 12] == 0 && table_[i + 14] == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        List<NGlycanHybrid> CreateByAddNeuAc()
        {
            List<NGlycanHybrid> glycans = new List<NGlycanHybrid>();
            for (int i = 0; i < 2; i++)
            {
                if (i == 0 || table_[i + 12] < table_[i + 11]) // make it order
                {
                    if (table_[i + 6] > 0 && table_[i + 6] == table_[i + 8] && table_[i + 12] == 0 && table_[i + 14] == 0)
                    {
                        var g = new NGlycanHybrid();
                        g.SetTable(table_);
                        g.table_[i + 12] = 1;
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
            for (int i = 0; i < 2; i++)
            {
                if (i == 0 || table_[i + 14] < table_[i + 13]) // make it order
                {
                    if (table_[i + 6] > 0 && table_[i + 6] == table_[i + 8] && table_[i + 12] == 0 && table_[i + 14] == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        List<NGlycanHybrid> CreateByAddNeuGc()
        {
            List<NGlycanHybrid> glycans = new List<NGlycanHybrid>();
            for (int i = 0; i < 2; i++)
            {
                if (i == 0 || table_[i + 14] < table_[i + 13]) // make it order
                {
                    if (table_[i + 6] > 0 && table_[i + 6] == table_[i + 8] && table_[i + 12] == 0 && table_[i + 14] == 0)
                    {
                        var g = new NGlycanHybrid();
                        g.SetTable(table_);
                        g.table_[i + 14] = 1;
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
