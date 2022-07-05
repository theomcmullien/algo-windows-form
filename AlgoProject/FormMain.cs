using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Web.Script.Serialization;

namespace AlgoProject
{
    public partial class FormMain : Form
    {
        //initialise global variables
        GMapOverlay polygons = new GMapOverlay("polygons");
        GMapOverlay markers = new GMapOverlay("markers");
        bool showingHull = true;

        public FormMain()
        {
            InitializeComponent();
        }

        /*
         * Initialises values of a GMapControl
         * 
         * Iterates through each list of POI's sorted by UserID.
         * For each iteration a marker is added per POI to the GMap
         * and a polygon is created from the convex hull of these points (if there are 3 or more points/markers)
         * 
        */
        private void FormMain_Load(object sender, EventArgs e)
        {
            gmap.MapProvider = BingMapProvider.Instance;
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            //gmap.Position = new PointLatLng(-46.4179, 168.3615); //invercargill center
            gmap.Position = new PointLatLng(-46.407, 168.365); //points center
            gmap.DragButton = MouseButtons.Left;
            gmap.ShowCenter = false;
            gmap.MinZoom = 2;
            gmap.MaxZoom = 18;
            gmap.Zoom = 12;

            List<List<PlaceOfInterest>> ListL = SortIntoLists();

            List<int> colours = new List<int>();

            foreach (List<PlaceOfInterest> listPOI in ListL)
            {
                // 0red 1green 2purple 3orange 4pink 5lightblue 6blue
                if (colours.Count == 0) colours = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };
                int colourPOI = colours[0];
                colours.RemoveAt(0);

                string desc = "";
                
                //adds each POI in the form of a Coord object to the hashset to remove duplicates.
                HashSet<Coord> pointsHash = new HashSet<Coord>();
                foreach (PlaceOfInterest poi in listPOI) pointsHash.Add(new Coord(poi.Latitude, poi.Longitude));

                if (pointsHash.Count >= 3)
                {
                    //convert the hashmap to a list
                    List<Coord> points = new List<Coord>();
                    foreach (Coord c in pointsHash) points.Add(c);

                    Coord pivot = points[0];

                    //find the pivot furthest bottom-left (prioritising left over bottom)
                    for (int i = 1; i < points.Count; i++)
                    {
                        if (points[i].X < pivot.X || (points[i].X == pivot.X && points[i].Y < pivot.Y))
                        {
                            pivot = points[i];
                        }
                    }

                    points.Remove(pivot);
                    points.Sort(new RadialSort(pivot));

                    //create a new list for storing the points in the convex hull
                    List<Coord> hull = new List<Coord>();

                    hull.Add(pivot);
                    hull.Add(points[0]); points.RemoveAt(0);

                    //cycle through the points list, adding and removing points until the hull is created
                    while (points.Count > 0)
                    {
                        hull.Add(points[0]); points.RemoveAt(0);
                        //cycles through hull, removing points creating a concave effect, until the hull is valid.
                        while (!IsValid(hull)) hull.RemoveAt(hull.Count - 2);
                    }

                    //convert list of Point objects to a list of PointLatLng objects for creating the polygon
                    List<PointLatLng> hullPoints = new List<PointLatLng>();
                    foreach (Coord c in hull) hullPoints.Add(new PointLatLng(c.X, c.Y));

                    GMapPolygon polygon = new GMapPolygon(hullPoints, "polygon");
                    polygon.Fill = new SolidBrush(Color.FromArgb(50, Color.Red));
                    polygon.Stroke = new Pen(GetStrokeColour(colourPOI), 1);
                    polygons.Polygons.Add(polygon);
                }
                
                //iterates through each POI, marking it onto the GMap
                foreach (PlaceOfInterest poi in listPOI)
                {
                    PointLatLng point = new PointLatLng(poi.Latitude, poi.Longitude);

                    GMapMarker marker = new GMarkerGoogle(point, GetMarkerColour(colourPOI));
                    desc = string.Format("{0}: {1}\nLatitude: {2}\nLongitude: {3}", poi.UserID, poi.Description, poi.Latitude, poi.Longitude);
                    marker.Tag = desc;
                    marker.ToolTipText = desc;
                    marker.ToolTip.Fill = GetBrushColour(colourPOI);
                    marker.ToolTip.Foreground = Brushes.White;
                    marker.ToolTip.Stroke = Pens.White;
                    marker.ToolTip.TextPadding = new Size(20, 20);

                    markers.Markers.Add(marker);
                }

            }

            //adds the polygons and markers to the GMap overlays
            gmap.Overlays.Add(polygons);
            gmap.Overlays.Add(markers);
            
            //adds GMap to the controls
            Controls.Add(gmap);

            gmap.Zoom = 13;
            Refresh();
        }

        //generates a list of lists for storing all POI's sorted into lists by user ID
        private List<List<PlaceOfInterest>> SortIntoLists()
        {
            List<PlaceOfInterest> listAllPOI = GetLocationData();

            List<List<PlaceOfInterest>> ListL = new List<List<PlaceOfInterest>>();
            foreach (PlaceOfInterest poi in listAllPOI)
            {
                bool inList = false;

                foreach (List<PlaceOfInterest> l in ListL)
                {
                    if (l[0].UserID == poi.UserID)
                    {
                        l.Add(poi);
                        inList = true;
                        break;
                    }
                }

                if (!inList)
                {
                    List<PlaceOfInterest> newList = new List<PlaceOfInterest>();
                    newList.Add(poi);
                    ListL.Add(newList);
                }
            }
            return ListL;
        }

        //gets a json from a given ip, json is deserialised and created into a list of POI objects
        private List<PlaceOfInterest> GetLocationData()
        {
            List<PlaceOfInterest> listPOI = new List<PlaceOfInterest>();
            using (WebClient client = new WebClient())
            {
                var json = client.DownloadString(@"http://developer.kensnz.com/getlocdata");
                JavaScriptSerializer ser = new JavaScriptSerializer();
                var JSONArray = ser.Deserialize<Dictionary<string, string>[]>(json);
                foreach (Dictionary<string, string> map in JSONArray)
                {
                    int id = int.Parse(map["userid"]);
                    if (id != 202251 && id != 202252 && id != 202253) continue; //filters data to only get specific userid's
                    double lat = double.Parse(map["latitude"]);
                    double lng = double.Parse(map["longitude"]);
                    string desc = map["description"];
                    PlaceOfInterest poi = new PlaceOfInterest(id, lat, lng, desc);
                    listPOI.Add(poi);
                }
            }
            return listPOI;
        }

        /*
         * The following method recieves an int parameter and uses a switch statement
         * for returning a specific GMarkerGoogleType.
         * It is used to have a different colour selected per data set.
        */
        private GMarkerGoogleType GetMarkerColour(int colourPOI)
        {
            switch (colourPOI)
            {
                case 0:
                    return GMarkerGoogleType.red;
                case 1:
                    return GMarkerGoogleType.green;
                case 2:
                    return GMarkerGoogleType.purple;
                case 3:
                    return GMarkerGoogleType.orange;
                case 4:
                    return GMarkerGoogleType.pink;
                case 5:
                    return GMarkerGoogleType.lightblue;
                case 6:
                    return GMarkerGoogleType.red;
                default:
                    return GMarkerGoogleType.blue;
            }
        }

        /*
         * The following method recieves an int parameter and uses a switch statement
         * for returning a specific Brush colour.
         * It is used to have a different colour selected per data set.
        */
        private Brush GetBrushColour(int colourPOI)
        {
            switch (colourPOI)
            {
                case 0:
                    return Brushes.Red;
                case 1:
                    return Brushes.Green;
                case 2:
                    return Brushes.Purple;
                case 3:
                    return Brushes.Orange;
                case 4:
                    return Brushes.Pink;
                case 5:
                    return Brushes.LightBlue;
                case 6:
                    return Brushes.Blue;
                default:
                    return Brushes.Blue;
            }
        }

        /*
         * The following method recieves an int parameter and uses a switch statement
         * for returning a specific Color.
         * It is used to have a different colour selected per data set.
        */
        private Color GetStrokeColour(int colourPOI)
        {
            switch (colourPOI)
            {
                case 0:
                    return Color.Red;
                case 1:
                    return Color.Green;
                case 2:
                    return Color.Purple;
                case 3:
                    return Color.Orange;
                case 4:
                    return Color.Pink;
                case 5:
                    return Color.LightBlue;
                case 6:
                    return Color.Blue;
                default:
                    return Color.Blue;
            }
        }

        //Recieves the hull as a parameter and checks the validity. If valid the method will return false, if not, true.
        private static bool IsValid(List<Coord> hull)
        {
            if (hull.Count < 3) return true;
            return RadialSort.SignedArea(hull[hull.Count - 3], hull[hull.Count - 2], hull[hull.Count - 1]) > 0;
        }

        /* 
         * The following method toggles the displaying of convex hulls onto the GMap.
         * If the showingHull boolean is set to true the convex hulls will be removed from the
         * gmap overlays and showingHull is set to false.
         * If the showingHull boolean is set to false the convex hull are added to the gmap
         * overlays and the showingHull is set to true.
         */
        private void buttonToggleHull_Click(object sender, EventArgs e)
        {
            if (showingHull)
            {
                gmap.Overlays.Remove(polygons);
                showingHull = false;
            }
            else
            {
                gmap.Overlays.Add(polygons);
                showingHull = true;
            }
            Refresh();
        }
    }
}
