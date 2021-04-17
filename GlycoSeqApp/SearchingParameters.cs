using GlycoSeqClassLibrary.algorithm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqApp
{
    public class SearchingParameters
    {
        //spectrum
        public double MS1Tolerance { get; set; } = 10;
        public double MSMSTolerance { get; set; } = 0.01;
        public ToleranceBy MS1ToleranceBy { get; set; } = ToleranceBy.PPM;
        public ToleranceBy MS2ToleranceBy { get; set; } = ToleranceBy.Dalton;

        //peptides
        public List<string> DigestionEnzyme { get; set; } = new List<string> { "Trypsin", "GluC" };
        public int MissCleavage { get; set; } = 2;
        public int MiniPeptideLength { get; set; } = 5;
        public bool Oxidiatoin { get; set; } = true;
        public bool Deamidation { get; set; } = true;
        public double Cysteine { get; set;  } = 57.02146; // Carboxyamidomethylcysteine

        //glycan
        public bool ComplexInclude { get; set; } = true;
        public bool HybridInclude { get; set; } = false;
        public bool MannoseInclude { get; set; } = false;
        public int HexNAc { get; set; } = 12;
        public int Hex { get; set; } = 12;
        public int Fuc { get; set; } = 5;
        public int NeuAc { get; set; } = 4;
        public int NeuGc { get; set; } = 0;

        //performance
        public int ThreadNums { get; set; } = 6;

        //result
        public double FDRValue { get; set; } = 0.01;

        //file
        public List<string> MSMSFiles { get; set; } = new List<string>();
        public string FastaFile { get; set; }

        SearchingParameters()
        {
        }

        public void Update()
        {
            MS1Tolerance = ConfigureParameters.Access.MS1Tolerance;
            MSMSTolerance = ConfigureParameters.Access.MSMSTolerance;
            MS1ToleranceBy = ConfigureParameters.Access.MS1ToleranceBy;
            MS2ToleranceBy = ConfigureParameters.Access.MS2ToleranceBy;
            DigestionEnzyme = ConfigureParameters.Access.DigestionEnzyme.ToList();
            MissCleavage = ConfigureParameters.Access.MissCleavage;
            MiniPeptideLength = ConfigureParameters.Access.MiniPeptideLength;
            Oxidiatoin = ConfigureParameters.Access.Oxidiatoin;
            Deamidation = ConfigureParameters.Access.Deamidation;
            Cysteine = ConfigureParameters.Access.Cysteine;
            ComplexInclude = ConfigureParameters.Access.ComplexInclude;
            HybridInclude = ConfigureParameters.Access.HybridInclude;
            MannoseInclude = ConfigureParameters.Access.MannoseInclude;
            HexNAc = ConfigureParameters.Access.HexNAc;
            Hex = ConfigureParameters.Access.Hex;
            Fuc = ConfigureParameters.Access.Fuc;
            NeuAc = ConfigureParameters.Access.NeuAc;
            NeuGc = ConfigureParameters.Access.NeuGc;
            ThreadNums = ConfigureParameters.Access.ThreadNums;
            FDRValue = ConfigureParameters.Access.FDRValue;
        }

        protected static readonly Lazy<SearchingParameters>
            lazy = new Lazy<SearchingParameters>(() => new SearchingParameters());

        public static SearchingParameters Access { get { return lazy.Value; } }

    }

    public class ConfigureParameters
    {
        //spectrum
        public double MS1Tolerance { get; set; } = 10;
        public double MSMSTolerance { get; set; } = 0.01;
        public ToleranceBy MS1ToleranceBy { get; set; } = ToleranceBy.PPM;
        public ToleranceBy MS2ToleranceBy { get; set; } = ToleranceBy.Dalton;
        //peptides
        public List<string> DigestionEnzyme { get; set; } = new List<string> { "Trypsin", "GluC" };
        public int MissCleavage { get; set; } = 2;
        public int MiniPeptideLength { get; set; } = 5;
        public bool Oxidiatoin { get; set; } = true;
        public bool Deamidation { get; set; } = true;
        public double Cysteine { get; set; } = 57.02146;

        //glycan
        public bool ComplexInclude { get; set; } = true;
        public bool HybridInclude { get; set; } = false;
        public bool MannoseInclude { get; set; } = false;
        public int HexNAc { get; set; } = 12;
        public int Hex { get; set; } = 12;
        public int Fuc { get; set; } = 5;
        public int NeuAc { get; set; } = 4;
        public int NeuGc { get; set; } = 0;

        //Performance
        public int ThreadNums { get; set; } = 6;

        //result
        public double FDRValue { get; set; } = 0.01;

        protected static readonly Lazy<ConfigureParameters>
            lazy = new Lazy<ConfigureParameters>(() => new ConfigureParameters());

        public static ConfigureParameters Access { get { return lazy.Value; } }

        private ConfigureParameters() { }

    }
}
