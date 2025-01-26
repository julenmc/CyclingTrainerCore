using Route.Reader;
using NLog;
using static System.Formats.Asn1.AsnWriter;

namespace Route
{
    public class Mountain
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static List<Mountain> GetMountains(List<IReader.SectorInfo> sectors)
        {
            IReader.SectorInfo forceSector = new IReader.SectorInfo(0,0,0,0,0);
            List<Mountain> list = new List<Mountain>();
            int id = 1;
            Mountain mount = new Mountain(id);
            Result r = Result.NoMountain;

            foreach (IReader.SectorInfo sector in sectors)
            {
                if (sector.EndPoint <= forceSector.EndPoint && sector.EndPoint != 0)
                {
                    Log.Debug($"Forcing mount point at {Math.Round(sector.EndPoint,3)}km ({sector.EndAlt}m) until {Math.Round(forceSector.EndPoint,3)}km");
                    mount.ForcePoint(sector);
                }
                else
                {
                    r = mount.AddSector(sector);
                    if (r == Result.EndWarning)
                    {
                        Log.Debug($"Mountain could end at km {Math.Round(sector.StartPoint, 3)} ({sector.StartAlt}m)");
                        int index = sectors.FindIndex(i => i.EndPoint == sector.EndPoint);
                        r = mount.Check(sectors[index]);
                        while (index < sectors.Count)
                        {
                            if (r == Result.EndWarning)
                            {
                                index++;
                                r = mount.Check(sectors[index]);
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (r == Result.MountainEnd)
                        {
                            Log.Debug($"Mountain ended at km {Math.Round(sector.EndPoint, 3)} ({sector.EndAlt}m)");
                            list.Add(mount);
                            id++;
                            mount.EndPort();
                            mount = new Mountain(id);
                        }
                        else if (r == Result.Continue) 
                        {
                            Log.Debug($"Mountain continues at km {Math.Round(sectors[index].EndPoint,3)}");
                            mount.ForcePoint(sector);
                            forceSector = sectors[index];
                        }
                        else
                        {
                            Log.Error($"Should not be here at km {Math.Round(sector.EndPoint,3)} ({sector.EndAlt}m)");
                        }
                    }
                    else if (r == Result.NoMountain)
                    {
                        Log.Debug($"No mountain at km {Math.Round(sector.EndPoint, 3)}");
                        mount.EndPort();
                        mount = new Mountain(id);
                    }
                }
            }

            if (forceSector == sectors.Last())
            {
                list.Add(mount);
            }
            //else if ((r == Result.EndWarning || r == Result.Continue) && mount.CheckLastPoint(sectors.Last()))
            //{
            //    list.Add(mount);
            //}
            else if (r == Result.Continue)
            {
                mount.EndPort();
                list.Add(mount);
            }

            return list;
        }

        private enum Result
        {
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
            _sectors = new List<IReader.SectorInfo>();
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

        private List<IReader.SectorInfo> _sectors;

        private bool _isFirstPoint = true;

        private Result AddSector(IReader.SectorInfo sector)
        {
            if (!_isFirstPoint)
            {
                double distDiff = (sector.EndPoint - sector.StartPoint) * 1000;
                double slope = sector.Slope;

                if (slope < 2)
                {       // If downhill port might end
                    return IsPort(slope, sector.EndAlt) ? Result.EndWarning : Result.NoMountain;
                }
                else if (slope > MaxSlope)
                {
                    MaxSlope = slope;
                }

                Lenght += distDiff;
                if (sector.EndAlt > _sectors.Last().EndAlt) Elevation += sector.EndAlt - sector.StartAlt;
                if (sector.EndAlt > MaxAltitude) MaxAltitude = sector.EndAlt;
                Slope = (Elevation / Lenght) * 100;


                _sectors.Add(sector);
                return Result.Continue;
            }
            else
            {
                if (sector.Slope > 2)
                {
                    _isFirstPoint = false;
                    _sectors.Add(sector);
                    Elevation = sector.EndAlt - sector.StartAlt;
                    Lenght = (sector.EndPoint - sector.StartPoint) * 1000;
                    Slope = sector.Slope;
                    MaxAltitude = sector.EndAlt;
                    MaxSlope = sector.Slope;
                    InitAltitude = sector.StartAlt;
                    InitKm = sector.StartPoint;
                    return Result.Continue;
                }
                else return Result.NoMountain;
            }
        }

        private double _forcePrevAuxDistance = 0;
        private void ForcePoint(IReader.SectorInfo sector) 
        {
            if (_forcePrevAuxDistance < _sectors.Last().EndPoint * 1000) _forcePrevAuxDistance = _sectors.Last().EndPoint * 1000;

            double _distDiff = (sector.EndPoint * 1000) - _forcePrevAuxDistance;
            _forcePrevAuxDistance = sector.EndPoint * 1000;

            double _slope = sector.Slope;
            Lenght += _distDiff;
            if (sector.EndAlt > _sectors.Last().EndAlt) Elevation += sector.EndAlt - _sectors.Last().EndAlt;
            if (sector.EndAlt > MaxAltitude) MaxAltitude = sector.EndAlt;

            if (_slope > MaxSlope) MaxSlope = _slope;
            Slope = (Elevation / Lenght) * 100;

            _sectors.Add(sector);
        }

        private bool _hasToCheckSlope = true;
        private bool _hasToCheckAltitude = true;
        private double _checkPrevAuxDistance = 0;
        private double _checkPrevAlt = 0;
        private Result Check(IReader.SectorInfo sector)
        {
            if (_checkPrevAuxDistance == 0)
            {
                _checkPrevAuxDistance = _sectors.Last().EndPoint * 1000;
                _checkPrevAlt = _sectors.Last().EndAlt;
            }

            double _distDiff = (sector.EndPoint * 1000) - _checkPrevAuxDistance;
            _checkPrevAuxDistance = sector.EndPoint * 1000;

            double _sectorSlope = sector.Slope;              // Pendiente en el punto
            double _checkSlope = ((sector.EndAlt - _sectors.First().StartAlt) / (sector.EndPoint * 1000 - _sectors.First().StartPoint)) * 100;          // Pendiente total en el check
            _checkPrevAlt = sector.EndAlt;

            _hasToCheckAltitude = (sector.EndAlt > MaxAltitude) ? false : true;
            _hasToCheckSlope = (_sectorSlope > 1) ? false : true;

            if (!_hasToCheckSlope && !_hasToCheckAltitude)
            {
                _hasToCheckAltitude = false;
                _hasToCheckSlope = false;
                _checkPrevAuxDistance = 0;
                _checkPrevAlt = 0;
                return Result.Continue;
            }
            else if (ContinueCheck((sector.EndPoint - _sectors.Last().EndPoint) * 1000, _checkSlope) == false)
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

        private bool CheckLastPoint(IReader.SectorInfo p)
        {
            if (!_isFirstPoint)
            {
                double slope = p.Slope;

                if (slope < 0)
                {       // Si baja cierro puerto
                    return IsPort(slope, p.EndAlt) ? true : false;
                }
                else if (slope > MaxSlope)
                {
                    MaxSlope = slope;
                }

                Lenght += p.EndPoint * 1000 - _sectors.Last().EndPoint;
                if (p.EndAlt > _sectors.Last().EndAlt) Elevation += p.EndAlt - _sectors.Last().EndAlt;
                if (p.EndAlt > MaxAltitude) MaxAltitude = p.EndAlt;
                Slope = (Elevation / Lenght) * 100;
                _sectors.Add(p);
            }
            return IsPort(p.Slope, p.EndAlt);
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
