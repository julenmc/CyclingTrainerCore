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

        public bool Read()
        {
            try
            {
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
                                double distance = 0;
                                PointInfo info = new PointInfo(distance, _prevElevation);
                                _points.Add(info);
                                for (int i = 1; i < segment.TrackPoints.Count; i++)
                                {
                                    double elevation = (segment.TrackPoints[i].Elevation != null) ? (double)segment.TrackPoints[i].Elevation : 0;
                                    distance += Math.Round(segment.TrackPoints[i].GetDistanceFrom(segment.TrackPoints[i - 1]),3);
                                    info = new PointInfo(distance, elevation);
                                    double diff = elevation - _prevElevation;
                                    _prevElevation = elevation;
                                    if (diff > 0) Elevation += diff;
                                    _points.Add(info);
                                }
                            }
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
    }
}
