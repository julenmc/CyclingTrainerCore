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
                                double nextElevation = (segment.TrackPoints[1].Elevation != null) ? (double)segment.TrackPoints[1].Elevation : 0;
                                double distDiff = segment.TrackPoints[1].GetDistanceFrom(segment.TrackPoints[0]);
                                double slope = Math.Round((nextElevation - _prevElevation) / (distDiff * 1000) * 100, 2);
                                double totalDistance = 0;
                                PointInfo info = new PointInfo(totalDistance, _prevElevation, slope);
                                points.Add(info);
                                for (int i = 1; i < segment.TrackPoints.Count - 1; i++)
                                {
                                    double elevation = (segment.TrackPoints[i].Elevation != null) ? (double)segment.TrackPoints[i].Elevation : 0;
                                    nextElevation = (segment.TrackPoints[i+1].Elevation != null) ? (double)segment.TrackPoints[i+1].Elevation : 0;
                                    totalDistance += Math.Round(distDiff, 3);
                                    distDiff = segment.TrackPoints[i + 1].GetDistanceFrom(segment.TrackPoints[i]);
                                    double altDiff = elevation - _prevElevation;
                                    slope = Math.Round((nextElevation - elevation) / (distDiff * 1000) * 100, 2);
                                    info = new PointInfo(totalDistance, elevation, slope);
                                    _prevElevation = elevation;
                                    if (altDiff > 0) Elevation += altDiff;
                                    points.Add(info);
                                }
                                totalDistance += Math.Round(segment.TrackPoints[segment.TrackPoints.Count-1].GetDistanceFrom(segment.TrackPoints[segment.TrackPoints.Count-2]), 3);
                                double el = (segment.TrackPoints.EndPoint.Elevation != null) ? (double)segment.TrackPoints.EndPoint.Elevation : 0;
                                if (el > _prevElevation) Elevation += el - _prevElevation;
                                info = new PointInfo(totalDistance, el, 0);
                                points.Add(info);
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
            int arrayIndex = 0;
            for (int i = 1; i < points.Count - 1; i++)
            {
                if (points[i].Len - points[arrayIndex].Len >= SmoothDistance)
                {
                    double slope = (points[i].Alt - points[arrayIndex].Alt) / (points[i].Len - points[arrayIndex].Len) / 10;
                    PointInfo point = new PointInfo(points[arrayIndex].Len, points[arrayIndex].Alt, slope);
                    _points.Add(point);
                    arrayIndex = i;
                }
            }
            PointInfo p = new PointInfo(points.Last().Len, points.Last().Alt, 0);
            _points.Add(p);
        }
    }
}
