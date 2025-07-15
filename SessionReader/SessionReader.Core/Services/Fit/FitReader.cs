using NLog;
using Dynastream.Fit;
using SessionReader.Core.Models;
using static SessionReader.Core.Services.ISessionReader;

namespace SessionReader.Core.Services.Fit
{
    public class FitReader : ISessionReader
    {
        private readonly Logger Log = LogManager.GetCurrentClassLogger();

        private string _path;
        private double _lenght = 0;
        private double _elevation = 0;
        private List<FitnessData> _fitnessPoints = new List<FitnessData>();

        private List<SectorInfo> _sectorsRaw;
        private List<SectorInfo> _sectorsSmoothed;

        public FitReader(string path)
        {
            _path = path;
            _sectorsSmoothed = new List<SectorInfo>();
            _sectorsRaw = new List<SectorInfo>();
        }

        public string GetName()
        {
            return Path.GetFileName(_path);
        }

        public bool Read()
        {
            try
            {
                FileStream fileStream = new FileStream(_path, FileMode.Open);
                // Create FIT Decoder
                FitDecoder fitDecoder = new FitDecoder(fileStream, Dynastream.Fit.File.Activity);

                // Decode the FIT file
                try
                {
                    Log.Debug("Decoding...");
                    fitDecoder.Decode();
                }
                catch (FileTypeException ex)
                {
                    Log.Error("DecodeDemo caught FileTypeException: " + ex.Message);
                    return false;
                }
                catch (FitException ex)
                {
                    Log.Error("DecodeDemo caught FitException: " + ex.Message);
                }
                catch (Exception ex)
                {
                    Log.Error("DecodeDemo caught Exception: " + ex.Message);
                }
                finally
                {
                    fileStream.Close();
                }

                // Check the time zone offset in the Activity message.
                var timezoneOffset = fitDecoder.FitMessages.ActivityMesgs.FirstOrDefault()?.TimezoneOffset();
                Log.Debug($"The timezone offset for this activity file is {timezoneOffset?.TotalHours ?? 0} hours.");

                // Create the Activity Parser and group the messages into individual sessions.
                ActivityParser activityParser = new ActivityParser(fitDecoder.FitMessages);
                var sessions = activityParser.ParseSessions();
                _sectorsRaw = new List<SectorInfo>();
                _fitnessPoints = new List<FitnessData>();

                foreach (var session in sessions)
                {
                    _lenght += (double)session.Records.Last().GetDistance();
                    // First sector
                    double startElevation = (session.Records[0].GetAltitude() != null) ? (double)session.Records[0].GetAltitude() : 0;
                    double endElevation = (session.Records[1].GetAltitude() != null) ? (double)session.Records[1].GetAltitude() : 0;
                    double distDiff = Math.Round((double)session.Records[1].GetDistance() - (double)session.Records[0].GetDistance(), 3);
                    double startPoint = 0;
                    double endPoint = distDiff;
                    double altDiff = endElevation - startElevation;
                    double slope = Math.Round((endElevation - startElevation) / distDiff * 100, 2);
                    SectorInfo info = new SectorInfo
                    {
                        StartPoint = startPoint, 
                        EndPoint = endPoint, 
                        StartAlt = startElevation, 
                        EndAlt = endElevation, 
                        Slope = slope 
                    };
                    if (altDiff > 0) _elevation += altDiff;
                    _sectorsRaw.Add(info);
                    SavePoint(session.Records[0]);
                    SavePoint(session.Records[1]);
                    // Starting from the second point to create the sectors
                    for (int i = 2; i < session.Records.Count; i++)
                    {
                        startElevation = (session.Records[i - 1].GetAltitude() != null) ? (double)session.Records[i - 1].GetAltitude() : 0;
                        endElevation = (session.Records[i].GetAltitude() != null) ? (double)session.Records[i].GetAltitude() : 0;
                        distDiff = Math.Round((double)session.Records[i].GetDistance() - (double)session.Records[i-1].GetDistance(), 3);
                        startPoint = _sectorsRaw.Last().EndPoint;
                        endPoint = distDiff + startPoint;
                        altDiff = endElevation - startElevation;
                        slope = Math.Round((endElevation - startElevation) / distDiff * 100, 2);
                        info = new SectorInfo
                        {
                            StartPoint = startPoint,
                            EndPoint = endPoint,
                            StartAlt = startElevation,
                            EndAlt = endElevation,
                            Slope = slope
                        };
                        if (altDiff > 0) _elevation += altDiff;
                        _sectorsRaw.Add(info);
                        SavePoint(session.Records[i]);
                    }
                }
                _sectorsSmoothed = RouteSmootherService.SmoothAndAddSectors(_sectorsRaw);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void SavePoint(RecordMesg record)
        {
            FitnessData pointInfo = new FitnessData()
            {
                Timestamp = record.GetTimestamp(),
                Temperature = record.GetTemperature(),
                AccCalories = record.GetCalories(),
                Position = new PointPosition()
                {
                    Longitude = record.GetPositionLong(),
                    Latitude = record.GetPositionLat(),
                    Altitude = record.GetAltitude(),
                    Distance = record.GetDistance()
                },
                Stats = new PointStats()
                {
                    Power = record.GetPower(),
                    Speed = record.GetSpeed(),
                    HeartRate = record.GetHeartRate(),
                    Cadence = record.GetCadence(),
                    RespirationRate = record.GetRespirationRate(),
                },
                Advanced = new PointAdvStats()
                {
                    FractionalCadence = record.GetFractionalCadence(),
                    LeftPco = record.GetLeftPco(),
                    RightPco = record.GetRightPco(),
                    //LeftPowerPhase = record.GetLeftPowerPhase(),
                    //LeftPowerPhasePeak = record.GetLeftPowerPhasePeak(),
                }
            };
            _fitnessPoints.Add(pointInfo);
        }

        public double GetLenght() => _lenght;
        public double GetElevation() => _elevation;
        public List<SectorInfo> GetSmoothedSectors() => _sectorsSmoothed;
        public List<FitnessData> GetFitnessData() => _fitnessPoints;
    }
}
