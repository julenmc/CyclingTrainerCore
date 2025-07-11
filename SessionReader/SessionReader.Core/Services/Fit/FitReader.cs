using NLog;
using Dynastream.Fit;
using SessionReader.Core.Models;
using static SessionReader.Core.Models.IReader;

namespace SessionReader.Core.Services.Fit
{
    public class FitReader : IReader
    {
        private readonly Logger Log = LogManager.GetCurrentClassLogger();
        public double Lenght { get; private set; }
        public double Elevation { get; private set; }
        public List<PointInfo> Points { get; private set; } = default!;

        private string _path;

        private List<SectorInfo> _sectors;

        public FitReader(string path)
        {
            _path = path;
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
                List<SectorInfo> sectors = new List<SectorInfo>();
                Points = new List<PointInfo>();

                foreach (var session in sessions)
                {
                    Lenght += (double)session.Records.Last().GetDistance()/1000;
                    // First sector
                    double startElevation = (session.Records[0].GetAltitude() != null) ? (double)session.Records[0].GetAltitude() : 0;
                    double endElevation = (session.Records[1].GetAltitude() != null) ? (double)session.Records[1].GetAltitude() : 0;
                    double distDiff = Math.Round((double)session.Records[1].GetDistance()/1000 - (double)session.Records[0].GetDistance()/1000, 3);
                    double startPoint = 0;
                    double endPoint = distDiff;
                    double altDiff = endElevation - startElevation;
                    double slope = Math.Round((endElevation - startElevation) / (distDiff * 1000) * 100, 2);
                    SectorInfo info = new SectorInfo
                    {
                        StartPoint = startPoint, 
                        EndPoint = endPoint, 
                        StartAlt = startElevation, 
                        EndAlt = endElevation, 
                        Slope = slope 
                    };
                    if (altDiff > 0) Elevation += altDiff;
                    sectors.Add(info);
                    SavePoint(session.Records[0]);
                    SavePoint(session.Records[1]);
                    // Starting from the second point to create the sectors
                    for (int i = 2; i < session.Records.Count; i++)
                    {
                        startElevation = (session.Records[i - 1].GetAltitude() != null) ? (double)session.Records[i - 1].GetAltitude() : 0;
                        endElevation = (session.Records[i].GetAltitude() != null) ? (double)session.Records[i].GetAltitude() : 0;
                        distDiff = Math.Round((double)session.Records[i].GetDistance()/1000 - (double)session.Records[i-1].GetDistance()/1000, 3);
                        startPoint = sectors.Last().EndPoint;
                        endPoint = distDiff + startPoint;
                        altDiff = endElevation - startElevation;
                        slope = Math.Round((endElevation - startElevation) / (distDiff * 1000) * 100, 2);
                        info = new SectorInfo
                        {
                            StartPoint = startPoint,
                            EndPoint = endPoint,
                            StartAlt = startElevation,
                            EndAlt = endElevation,
                            Slope = slope
                        };
                        if (altDiff > 0) Elevation += altDiff;
                        sectors.Add(info);
                        SavePoint(session.Records[i]);
                    }
                }
                _sectors = RouteSmootherService.SmoothAndAddSectors(sectors);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void SavePoint(RecordMesg record)
        {
            PointInfo pointInfo = new PointInfo()
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
            Points.Add(pointInfo);
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
