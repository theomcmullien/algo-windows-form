using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoProject
{
    internal class Coord
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Coord(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Calculates the distance between 2 Coord objects
        /// </summary>
        /// <param name="other"></param>
        /// <returns>A double containing the distance from the current Coord to the passed in Coord</returns>
        public double DistTo(Coord other)
        {
            return (X - other.X) * (X - other.X) + (Y - other.Y) * (Y - other.Y);
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", X, Y);
        }

        /// <summary>
        /// Override the equals method to allow the comparing of 2 Coord objects
        /// </summary>
        /// <param name="obj"></param>
        public override bool Equals(object obj)
        {
            if (!(obj is Coord)) return false;
            Coord other = obj as Coord;
            return (X == other.X && Y == other.Y);
        }

        /// <summary>
        /// Override the hashcode method to allow the comparing of 2 Coord objects
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (X + Y).GetHashCode();
        }
    }
}
