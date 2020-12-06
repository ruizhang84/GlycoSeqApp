using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqClassLibrary.util.mass
{
    public enum IonType { a, b, c, x, y, z };

    public class Ion
    {
        protected static readonly Lazy<Ion>
            lazy = new Lazy<Ion>(() => new Ion());

        public static Ion To { get { return lazy.Value; } }

        protected Ion()
        {
        }

        public const double kCarbon = 12.0;
        public const double kNitrogen = 14.003074;
        public const double kOxygen = 15.99491463;
        public const double kHydrogen = 1.007825;

        public double Compute(double mass, IonType type)
        {
            switch (type)
            {
                case IonType.a:
                    mass = mass - kOxygen * 2 - kHydrogen * 2 - kCarbon;
                    break;
                case IonType.b:
                    mass = mass - kOxygen - kHydrogen * 2;
                    break;
                case IonType.c:
                    mass = mass - kOxygen + kHydrogen + kNitrogen;
                    break;
                case IonType.x:
                    mass += kCarbon + kOxygen - kHydrogen * 2;
                    break;
                case IonType.y:
                    break;
                case IonType.z:
                    mass = mass - kNitrogen - kHydrogen * 3;
                    break;
            }
            return mass;
        }
    }
}
