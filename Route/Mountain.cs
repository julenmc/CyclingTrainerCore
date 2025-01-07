using Route.Reader;
using NLog;
using static System.Formats.Asn1.AsnWriter;

namespace Route
{
    public class Mountain
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static List<Mountain> GetMountains(List<IReader.PointInfo> points)
        {
            IReader.PointInfo forcePoint = new IReader.PointInfo(0,0,0);
            List<Mountain> list = new List<Mountain>();
            int id = 1;
            Mountain mount = new Mountain(id);
            Result r = Result.Start;

            foreach (IReader.PointInfo point in points)
            {
                if (point.Len <= forcePoint.Len && point.Len != 0)
                {
                    Log.Debug($"Forcing mount point at {Math.Round(point.Len,3)}km ({point.Alt}m) until {Math.Round(forcePoint.Len,3)}km");
                    mount.ForcePoint(point);
                }
                else
                {
                    r = mount.AddPoint(point);
                    if (r == Result.EndWarning)
                    {
                        Log.Debug($"Mountain could end at km {Math.Round(point.Len, 3)} ({point.Alt}m)");
                        int index = points.FindIndex(i => i.Len == point.Len);
                        r = mount.Check(points[index]);
                        while (index < points.Count)
                        {
                            if (r == Result.EndWarning)
                            {
                                index++;
                                r = mount.Check(points[index]);
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (r == Result.MountainEnd)
                        {
                            Log.Debug($"Mountain ended at km {Math.Round(point.Len, 3)} ({point.Alt}m)");
                            list.Add(mount);
                            id++;
                            mount.EndPort();
                            mount = new Mountain(id);
                        }
                        else if (r == Result.Continue) 
                        {
                            Log.Debug($"Mountain continues at km {Math.Round(points[index].Len,3)}");
                            mount.ForcePoint(point);
                            forcePoint = points[index];
                        }
                        else
                        {
                            Log.Error($"Should not be here at km {Math.Round(point.Len,3)} ({point.Alt}m)");
                        }
                    }
                    else if (r == Result.NoMountain)
                    {
                        Log.Debug($"No mountain at km {Math.Round(point.Len, 3)}");
                        mount.EndPort();
                        mount = new Mountain(id);
                    }
                }
            }

            if (forcePoint == points.Last())
            {
                list.Add(mount);
            }
            else if ((r == Result.EndWarning || r == Result.Continue) && mount.CheckLastPoint(points.Last()))
            {
                list.Add(mount);
            }

            return list;
        }

        private enum Result
        {
            Start,
            Continue,
            EndWarning,
            MountainEnd,
            NoMountain
        }

        public int Id { get; private set; }
        public double Lenght { get; private set; }
        public double InitKm { get; private set; }
        public double Elevation { get; private set; }
        public double InitAltitude { get; private set; }
        public double MaxAltitude { get; private set; }
        public double Slope { get; private set; }
        public double MaxSlope { get; private set; }

        public Mountain(int ID)
        {
            Id = ID;
            Lenght = 0;
            InitKm = 0;
            Elevation = 0;
            InitAltitude = 0;
            MaxAltitude = 0;
            Slope = 0;
            MaxSlope = 0;
            _points = new List<IReader.PointInfo>();
        }

        private static readonly int PassReqRows = 6;
        private static readonly int PassCheckRows = 7;

        private static readonly double[,] MountReq =
        {
            {2.5, 600},
            {4.0, 400},
            {5.0, 250},
            {6.0, 200},
            {7.0, 100},
            {8.0, 60}
        };

        private static readonly double[,,] CheckLim =
        {
            {   // downhill limits
                {0.0, 200},
                {2000, 600},
                {4000, 800},
                {6000, 900},
                {10000, 1200},
                {15000, 2000}
            },
            {   // flat distances
                {0.0, 500},
                {2000, 800},		// Mount distance / Allowed flat distances. Up to 2km 800m are allowed
				{4000, 1000},
                {6000, 1500},
                {10000, 2000},
                {15000, 3000}
            }
        };

        private List<IReader.PointInfo> _points;

        private bool _isFirstPoint = true;
        private double _prevDistance = 0;
        private double _prevAlt = 0;

        private Result AddPoint(IReader.PointInfo p)
        {
            if (_isFirstPoint == false)
            {
                double dist_diff = (p.Len * 1000) - _prevDistance;

                double slope = p.Slope;

                if (slope < 2)
                {       // If downhill port might end
                    return IsPort(slope, p.Alt) ? Result.EndWarning : Result.NoMountain;
                }
                else if (slope > MaxSlope)
                {
                    MaxSlope = slope;
                }

                Lenght += dist_diff;
                if (p.Alt > _prevAlt) Elevation += p.Alt - _prevAlt;
                if (p.Alt > MaxAltitude) MaxAltitude = p.Alt;
                Slope = (Elevation / Lenght) * 100;


                _prevAlt = p.Alt;
                _prevDistance = p.Len * 1000;
                _points.Add(p);
                return Result.Continue;
            }
            else
            {
                _isFirstPoint = false;
                _points.Add(p);
                InitAltitude = p.Alt;
                InitKm = p.Len;
                _prevAlt = p.Alt;
                _prevDistance = p.Len * 1000;
                return Result.Start;
            }
        }

        private double _forcePrevAuxDistance = 0;
        private void ForcePoint(IReader.PointInfo p) 
        {
            if (_forcePrevAuxDistance < _prevDistance) _forcePrevAuxDistance = _prevDistance;

            double _distDiff = (p.Len * 1000) - _forcePrevAuxDistance;
            _forcePrevAuxDistance = p.Len * 1000;

            double _slope = p.Slope;
            Lenght += _distDiff;
            if (p.Alt > _prevAlt) Elevation += p.Alt - _prevAlt;
            if (p.Alt > MaxAltitude) MaxAltitude = p.Alt;

            if (_slope > MaxSlope) MaxSlope = _slope;
            Slope = (Elevation / Lenght) * 100;

            _prevAlt = p.Alt;
            _prevDistance = p.Len * 1000;
            _points.Add(p);
        }

        private bool _hasToCheckSlope = true;
        private bool _hasToCheckAltitude = true;
        private double _checkPrevAuxDistance = 0;
        private double _checkPrevAlt = 0;
        private Result Check(IReader.PointInfo p)
        {
            if (_checkPrevAuxDistance == 0)
            {
                _checkPrevAuxDistance = _prevDistance;
                _checkPrevAlt = _prevAlt;
            }

            double _distDiff = (p.Len * 1000) - _checkPrevAuxDistance;
            _checkPrevAuxDistance = p.Len * 1000;

            double _pointSlope = p.Slope;              // Pendiente en el punto
            double _checkSlope = ((p.Alt - _prevAlt) / (p.Alt * 1000 - _prevDistance)) * 100;          // Pendiente total en el check
            _checkPrevAlt = p.Alt;

            _hasToCheckAltitude = (p.Alt > MaxAltitude) ? false : true;
            _hasToCheckSlope = (_pointSlope > 1) ? false : true;

            if (!_hasToCheckSlope && !_hasToCheckAltitude)
            {
                _hasToCheckAltitude = false;
                _hasToCheckSlope = false;
                _checkPrevAuxDistance = 0;
                _checkPrevAlt = 0;
                return Result.Continue;
            }
            else if (ContinueCheck(p.Len * 1000 - _prevDistance, _checkSlope) == false)
            {
                _hasToCheckAltitude = false;
                _hasToCheckSlope = false;
                _checkPrevAuxDistance = 0;
                _checkPrevAlt = 0;
                return Result.MountainEnd;
            }
            else
            {
                return Result.EndWarning;
            }
        }

        private bool CheckLastPoint(IReader.PointInfo p)
        {
            if (!_isFirstPoint)
            {
                double slope = p.Slope;

                if (slope < 0)
                {       // Si baja cierro puerto
                    return IsPort(slope, p.Alt) ? true : false;
                }
                else if (slope > MaxSlope)
                {
                    MaxSlope = slope;
                }

                Lenght += p.Len * 1000 - _prevDistance;
                if (p.Alt > _prevAlt) Elevation += p.Alt - _prevAlt;
                if (p.Alt > MaxAltitude) MaxAltitude = p.Alt;
                Slope = (Elevation / Lenght) * 100;
                _points.Add(p);
            }
            return IsPort(p.Slope, p.Alt);
        }

        private bool IsPort(double s, double a)
        {
            for (int i = 0; i < MountReq.GetLength(0); i++)    
            {
                if ((Slope >= MountReq[i,0]) && (Lenght >= MountReq[i,1]))
                {
                    if (s > 0) MaxAltitude = a;
                    return true;
                }
                if (i == MountReq.GetLength(0) - 1)
                {
                    return false;
                }
            }
            return true;
        }

        private bool ContinueCheck(double d, double s)
        {
            int checkRow = (s < -3) ? 0 : 1;     // Primera columna para descensos, segunda para llanos

            // Comprobacion de distancias
            for (int i = 0; i < CheckLim.GetLength(1) - 1; i++)
            {
                if ((Lenght < CheckLim[checkRow,i + 1,0]) && (d > CheckLim[checkRow,i,1])) return false;
            }

            // Comprueba la ultima distancia
            if ((Lenght < CheckLim[checkRow, CheckLim.GetLength(0) - 1,0]) && (d > CheckLim[checkRow, CheckLim.GetLength(0) - 1, 1])) return false;

            return true;
        }

        private void EndPort()
        {
            Lenght = Math.Round(Lenght);
            Elevation = Math.Round(Elevation);
            Slope = Math.Round(Slope,1);
            InitKm = Math.Round(InitKm,1);
            InitAltitude = Math.Round(InitAltitude);
            MaxAltitude = Math.Round(MaxAltitude);
            MaxSlope = Math.Round(MaxSlope,1);
        }
    }
}
