using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoProject
{
    internal class RadialSort : IComparer<Coord>
    {
        public Coord Pivot { get; set; }

        public RadialSort(Coord pivot)
        {
            Pivot = pivot;
        }

        //returns the signed area value of 3 points/coords
        public static int SignedArea(Coord a, Coord b, Coord c)
        {
            double cmp = a.X * b.Y - b.X * a.Y + b.X * c.Y - c.X * b.Y + c.X * a.Y - a.X * c.Y;
            if (cmp > 0) return 1;
            else if (cmp < 0) return -1;
            else return 0;
        }

        //compares 2 points in relation of the pivot, used for comparing the most anti-clockwise point
        public int Compare(Coord x, Coord y)
        {
            int cmp = -SignedArea(Pivot, x, y);
            if (cmp == 0)
            {
                if (Pivot.DistTo(x) < Pivot.DistTo(y)) return -1;
                else return 1;
            }
            return cmp;
        }
    }
}
