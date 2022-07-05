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

        /// <summary>
        /// Calculates the signed area value of 3 Coord objects
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns>Returns an int containing the signed area value of 3 Coord objects</returns>
        public static int SignedArea(Coord a, Coord b, Coord c)
        {
            double cmp = a.X * b.Y - b.X * a.Y + b.X * c.Y - c.X * b.Y + c.X * a.Y - a.X * c.Y;
            if (cmp > 0) return 1;
            else if (cmp < 0) return -1;
            else return 0;
        }

        /// <summary>
        /// Compares 2 points in relation of the pivot, used for comparing the most anti-clockwise point
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>
        /// Returns a positive or negative value representing the coord being either anticlockwise
        /// or clockwise in relation to the pivot and the comparing coord.
        /// </returns>
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
