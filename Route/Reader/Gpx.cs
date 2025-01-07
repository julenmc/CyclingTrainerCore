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

        private List<PointInfo> _points;
        private int _index = 0;
        private double _prevElevation = 0;

        public Gpx(string p) 
        {
            _path = p;
            _points = new List<PointInfo>();
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
                List<PointInfo> points = new List<PointInfo>();
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
                                _prevElevation = (segment.TrackPoints.StartPoint.Elevation != null) ? (double)segment.TrackPoints.StartPoint.Elevation : 0;
                                double totalDistance = 0;
                                PointInfo info = new PointInfo(totalDistance, _prevElevation, 0);
                                points.Add(info);
                                for (int i = 1; i < segment.TrackPoints.Count; i++)
                                {
                                    double elevation = (segment.TrackPoints[i].Elevation != null) ? (double)segment.TrackPoints[i].Elevation : 0;
                                    double distDiff = segment.TrackPoints[i].GetDistanceFrom(segment.TrackPoints[i - 1]);
                                    totalDistance += Math.Round(distDiff, 3);
                                    double altDiff = elevation - _prevElevation;
                                    double slope = Math.Round(altDiff / (distDiff * 1000) * 100, 2);
                                    info = new PointInfo(totalDistance, elevation, slope);
                                    _prevElevation = elevation;
                                    if (altDiff > 0) Elevation += altDiff;
                                    points.Add(info);
                                }
                            }

                            SmoothRoute(points);
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

        public List<PointInfo> GetAllPoints()
        {
            return _points; 
        }

        private static readonly double SmoothDistance = 0.025; // Kilometers
        private void SmoothRoute(List<PointInfo> points)
        {
            int index = 0;
            _points.Add(points[index]);
            for (int i = 1; i < points.Count; i++)
            {
                if (points[i].Len - _points[index].Len >= SmoothDistance)
                {
                    double slope = (points[i].Alt - _points[index].Alt) / (points[i].Len - _points[index].Len) / 10;
                    PointInfo point = new PointInfo(points[i].Len, points[i].Alt, slope);
                    _points.Add(point);
                    index++;
                }
            }
        }
    }
}
