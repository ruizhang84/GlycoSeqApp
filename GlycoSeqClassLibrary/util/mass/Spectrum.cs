using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqClassLibrary.util.mass
{
    public class Spectrum
    {
        protected static readonly Lazy<Spectrum>
            lazy = new Lazy<Spectrum>(() => new Spectrum());

        public static Spectrum To { get { return lazy.Value; } }

        protected Spectrum()
        {
            ion = 1.0078;
        }

        protected double ion;

        public void SetChargeIon(double ionMass)
        {
            ion = ionMass;
        }

        public double Compute(double mz, int charge)
        {
            return (mz - ion) * charge;
        }

        public double ComputeMZ(double mass, int charge)
        {
            return (mass + ion * charge) / charge;
        }

        public double ComputePPM(double expected, double observed)
        {
            return Math.Abs(expected - observed) / expected * 1000000.0;
        }
    }
}
