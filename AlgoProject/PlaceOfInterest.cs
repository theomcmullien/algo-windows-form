using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoProject
{
    internal class PlaceOfInterest
    {
        public int UserID { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Description { get; set; }

        public PlaceOfInterest(int userID, double latitude, double longitude, string description)
        {
            UserID = userID;
            Latitude = latitude;
            Longitude = longitude;
            Description = description;
        }
    }
}
