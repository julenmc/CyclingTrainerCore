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
                                double prevDistance = 0;
                                double prevElevation = (segment.TrackPoints.StartPoint.Elevation != null) ? (double)segment.TrackPoints.StartPoint.Elevation : 0;
                                double nextElevation = (segment.TrackPoints[1].Elevation != null) ? (double)segment.TrackPoints[1].Elevation : 0;
                                double distance = Math.Round(segment.TrackPoints[1].GetDistanceFrom(segment.TrackPoints[0]), 3);
                                double slope = Math.Round((nextElevation - prevElevation) / (distance * 1000) * 100, 2);
                                SectorInfo info = new SectorInfo(0, distance, prevElevation, nextElevation, slope);
                                sectors.Add(info);
                                for (int i = 1; i < segment.TrackPoints.Count - 1; i++)
                                {
                                    double elevation = (segment.TrackPoints[i].Elevation != null) ? (double)segment.TrackPoints[i].Elevation : 0;
                                    nextElevation = (segment.TrackPoints[i+1].Elevation != null) ? (double)segment.TrackPoints[i+1].Elevation : 0;
                                    prevDistance = sectors.Last().EndPoint;
                                    distance = Math.Round(segment.TrackPoints[i + 1].GetDistanceFrom(segment.TrackPoints[i]), 3) + prevDistance;
                                    double distDiff = distance - prevDistance;
                                    double altDiff = elevation - prevElevation;
                                    slope = Math.Round((nextElevation - elevation) / (distDiff * 1000) * 100, 2);
                                    info = new SectorInfo(prevDistance, distance, prevElevation, elevation, slope);
                                    prevElevation = elevation;
                                    if (altDiff > 0) Elevation += altDiff;
                                    sectors.Add(info);
                                }
                                prevDistance = sectors.Last().EndPoint;
                                distance = Math.Round(segment.TrackPoints[segment.TrackPoints.Count-1].GetDistanceFrom(segment.TrackPoints[segment.TrackPoints.Count-2]), 3) + prevDistance;
                                double el = (segment.TrackPoints.EndPoint.Elevation != null) ? (double)segment.TrackPoints.EndPoint.Elevation : 0;
                                if (el > prevElevation) Elevation += el - prevElevation;
                                slope = Math.Round((el - prevElevation) / ((distance - prevDistance) * 1000) * 100, 2);
                                info = new SectorInfo(prevDistance, distance, prevElevation, el, slope);
                                sectors.Add(info);
                            }
                            _sectors = Smoother.FirstSmooth(sectors);
                            _sectors = Smoother.SecondSmooth(_sectors);
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
