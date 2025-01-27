using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Route.Reader.IReader;

using GpxTools;
using GpxTools.Gpx;
using System.Drawing;

namespace Route.Reader
{
    public class Gpx : IReader
    {
        public double Lenght { get; private set; }
        public double Elevation { get; private set; }

        private string _path;

        private List<SectorInfo> _sectors;

        public Gpx(string p) 
        {
            _path = p;
            _sectors = new List<SectorInfo>();
            Lenght = 0;
            Elevation = 0;
        }

        public string GetName()
        {
            return Path.GetFileName(_path);
        }

        public bool Read()
        {
            try
            {
                List<SectorInfo> sectors = new List<SectorInfo>();
                FileStream fRead = new FileStream(_path, FileMode.Open, FileAccess.Read);
                using (GpxReader reader = new GpxReader(fRead))
                {
                    while (reader.Read())
                    {
                        if (reader.ObjectType == GpxObjectType.Track)
                        {
                            IList<GpxTrackSegment> segments = reader.Track.Segments;

                            foreach (GpxTrackSegment segment in segments)
                            {
                                Lenght += segment.TrackPoints.GetLength();
                                // Starting from the second point to create the sectors
                                for (int i = 1; i < segment.TrackPoints.Count; i++)
                                {
                                    double startElevation = (segment.TrackPoints[i-1].Elevation != null) ? (double)segment.TrackPoints[i-1].Elevation : 0;
                                    double endElevation = (segment.TrackPoints[i].Elevation != null) ? (double)segment.TrackPoints[i].Elevation : 0;
                                    double startPoint = Math.Round(segment.TrackPoints[i-1].GetDistanceFrom(segment.TrackPoints[0]), 3);
                                    double endPoint = Math.Round(segment.TrackPoints[i].GetDistanceFrom(segment.TrackPoints[0]), 3);
                                    double distDiff = endPoint - startPoint;
                                    double altDiff = endElevation - startElevation;
                                    double slope = Math.Round((endElevation - startElevation) / (distDiff * 1000) * 100, 2);
                                    SectorInfo info = new SectorInfo(startPoint, endPoint, startElevation, endElevation, slope);
                                    if (altDiff > 0) Elevation += altDiff;
                                    sectors.Add(info);
                                }
                            }
                            _sectors = Smoother.SmoothAndAddSectors(sectors);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                //throw new Exception($"Route reading error: {ex.Message}");
                return false;
            }
        }

        public double GetLenght()
        {
            return Lenght; 
        }

        public double GetElevation()
        {
            return Elevation;
        }

        public List<SectorInfo> GetAllSectors()
        {
            return _sectors; 
        }
    }
}
