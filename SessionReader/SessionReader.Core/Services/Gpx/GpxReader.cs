using GpxTools;
using GpxTools.Gpx;
using CyclingTrainer.SessionReader.Core.Models;
using static CyclingTrainer.SessionReader.Core.Services.ISessionReader;

namespace CyclingTrainer.SessionReader.Core.Services.Gpx
{
    public class GpxReader : ISessionReader
    {
        private double _lenght;
        private double _elevation;

        private string _path;

        private List<SectorInfo> _sectorsRaw;
        private List<SectorInfo> _sectorsSmoothed;

        public GpxReader(string p) 
        {
            _path = p;
            _sectorsSmoothed = new List<SectorInfo>();
            _sectorsRaw = new List<SectorInfo>();
            _lenght = 0;
            _elevation = 0;
        }

        public string GetName()
        {
            return Path.GetFileName(_path);
        }

        public bool Read()
        {
            try
            {
                _sectorsRaw = new List<SectorInfo>();
                FileStream fRead = new FileStream(_path, FileMode.Open, FileAccess.Read);
                using (GpxTools.GpxReader reader = new GpxTools.GpxReader(fRead))
                {
                    while (reader.Read())
                    {
                        if (reader.ObjectType == GpxObjectType.Track)
                        {
                            IList<GpxTrackSegment> segments = reader.Track.Segments;

                            foreach (GpxTrackSegment segment in segments)
                            {
                                _lenght += segment.TrackPoints.GetLength() * 1000;
                                // First sector
                                double startElevation = segment.TrackPoints.First().Elevation != null ? (double)segment.TrackPoints.First().Elevation : 0;
                                double endElevation = segment.TrackPoints[1].Elevation != null ? (double)segment.TrackPoints[1].Elevation : 0;
                                double distDiff = Math.Round(segment.TrackPoints[1].GetDistanceFrom(segment.TrackPoints.First()) * 1000, 2);
                                double startPoint = 0;
                                double endPoint = distDiff;
                                double altDiff = endElevation - startElevation;
                                double slope = Math.Round((endElevation - startElevation) / distDiff * 100, 2);
                                SectorInfo info = new SectorInfo(startPoint, endPoint, startElevation, endElevation, slope);
                                if (altDiff > 0) _elevation += altDiff;
                                _sectorsRaw.Add(info);
                                // Starting from the second point to create the sectors
                                for (int i = 2; i < segment.TrackPoints.Count; i++)
                                {
                                    startElevation = segment.TrackPoints[i - 1].Elevation != null ? (double)segment.TrackPoints[i - 1].Elevation : 0;
                                    endElevation = segment.TrackPoints[i].Elevation != null ? (double)segment.TrackPoints[i].Elevation : 0;
                                    distDiff = Math.Round(segment.TrackPoints[i].GetDistanceFrom(segment.TrackPoints[i - 1]) * 1000, 2);
                                    startPoint = _sectorsRaw.Last().EndPoint;
                                    endPoint = distDiff + startPoint;
                                    altDiff = endElevation - startElevation;
                                    slope = Math.Round((endElevation - startElevation) / distDiff * 100, 2);
                                    info = new SectorInfo(startPoint, endPoint, startElevation, endElevation, slope);
                                    if (altDiff > 0) _elevation += altDiff;
                                    _sectorsRaw.Add(info);
                                }
                            }
                            _sectorsSmoothed = RouteSmootherService.SmoothAndAddSectors(_sectorsRaw); 
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

        public double GetLenght() => _lenght;
        public double GetElevation() => _elevation;
        public List<SectorInfo> GetSmoothedSectors() => _sectorsSmoothed;
        public List<FitnessData> GetFitnessData()
        {
            return new List<FitnessData>();
        }
    }
}
