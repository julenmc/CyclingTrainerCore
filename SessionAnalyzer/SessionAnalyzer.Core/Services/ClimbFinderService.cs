using NLog;
using SessionAnalyzer.Core.Models;

namespace SessionAnalyzer.Core.Services
{
    internal static class ClimbFinderService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private enum Result
        {
            Continue,
            EndWarning,
            MountainEnd,
            NoMountain
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

        private static bool _isFirstPoint = true;
        private static List<SectorInfo> _sectors = new List<SectorInfo>();
        private static double _forcePrevAuxDistance = 0;
        private static bool _hasToCheckSlope = true;
        private static bool _hasToCheckAltitude = true;
        private static double _checkPrevAuxDistance = 0;
        private static double _checkPrevAlt = 0;

        internal static List<Climb> GetClimbs(List<SectorInfo> sectors)
        {
            _isFirstPoint = true;
            _sectors = new List<SectorInfo>();
            _forcePrevAuxDistance = 0;
            _hasToCheckSlope = true;
            _hasToCheckAltitude = true;
            _checkPrevAuxDistance = 0;
            _checkPrevAlt = 0;

            SectorInfo forceSector = new SectorInfo();
            List<Climb> list = new List<Climb>();
            int id = 1;
            Climb climb = new Climb();
            climb.Id = id;
            Result r = Result.NoMountain;

            foreach (SectorInfo sector in sectors)
            {
                if (sector.EndPoint <= forceSector.EndPoint && sector.EndPoint != 0)
                {
                    Log.Debug($"Forcing mount point at {Math.Round(sector.EndPoint, 3)}km ({sector.EndAlt}m) until {Math.Round(forceSector.EndPoint, 3)}km");
                    ForcePoint(climb, sector);
                }
                else
                {
                    r = AddSector(climb, sector);
                    if (r == Result.EndWarning)
                    {
                        Log.Debug($"Mountain could end at km {Math.Round(sector.StartPoint, 3)} ({sector.StartAlt}m)");
                        int index = sectors.FindIndex(i => i.EndPoint == sector.EndPoint);
                        r = Check(climb, sectors[index]);
                        while (index < sectors.Count)
                        {
                            if (r == Result.EndWarning)
                            {
                                index++;
                                r = Check(climb, sectors[index]);
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (r == Result.MountainEnd)
                        {
                            Log.Debug($"Mountain ended at km {Math.Round(sector.StartPoint, 3)} ({sector.StartAlt}m)");
                            EndClimb(climb);
                            list.Add(climb);
                            id++;
                            climb = new Climb();
                            climb.Id = id;
                        }
                        else if (r == Result.Continue)
                        {
                            Log.Debug($"Mountain continues at km {Math.Round(sectors[index].EndPoint, 3)}");
                            ForcePoint(climb, sector);
                            forceSector = sectors[index];
                        }
                        else
                        {
                            Log.Error($"Should not be here at km {Math.Round(sector.EndPoint, 3)} ({sector.EndAlt}m)");
                        }
                    }
                    else if (r == Result.NoMountain)
                    {
                        Log.Debug($"No mountain at km {Math.Round(sector.EndPoint, 3)}");
                        climb = new Climb();
                        climb.Id = id;
                    }
                }
            }

            if (forceSector == sectors.Last() || r == Result.Continue)
            {
                EndClimb(climb);
                list.Add(climb);
            }

            return list;
        }

        private static Result AddSector(Climb climb, SectorInfo sector)
        {
            if (!_isFirstPoint)
            {
                double distDiff = (sector.EndPoint - sector.StartPoint) * 1000;
                double slope = sector.Slope;

                if (slope < 2)
                {       // If downhill port might end
                    return IsPort(climb, slope, sector.EndAlt) ? Result.EndWarning : Result.NoMountain;
                }
                else if (slope > climb.MaxSlope)
                {
                    climb.MaxSlope = slope;
                }

                climb.Lenght += distDiff;
                if (sector.EndAlt > _sectors.Last().EndAlt) climb.Elevation += sector.EndAlt - sector.StartAlt;
                if (sector.EndAlt > climb.MaxAltitude) climb.MaxAltitude = sector.EndAlt;
                climb.Slope = (climb.Elevation / climb.Lenght) * 100;

                _sectors.Add(sector);
                Log.Debug($"Climb info. Length={climb.Lenght}, Elevation={climb.Elevation}");
                return Result.Continue;
            }
            else
            {
                if (sector.Slope > 2)
                {
                    _isFirstPoint = false;
                    _sectors.Add(sector);
                    climb.Elevation = sector.EndAlt - sector.StartAlt;
                    climb.Lenght = (sector.EndPoint - sector.StartPoint) * 1000;
                    climb.Slope = sector.Slope;
                    climb.MaxAltitude = sector.EndAlt;
                    climb.MaxSlope = sector.Slope;
                    climb.InitAltitude = sector.StartAlt;
                    climb.InitKm = sector.StartPoint;
                    Log.Debug($"Climb start info. Length={climb.Lenght}, Elevation={climb.Elevation}");
                    return Result.Continue;
                }
                else return Result.NoMountain;
            }
        }

        private static void ForcePoint(Climb climb, SectorInfo sector)
        {
            if (_forcePrevAuxDistance < _sectors.Last().EndPoint * 1000) _forcePrevAuxDistance = _sectors.Last().EndPoint * 1000;

            double _distDiff = (sector.EndPoint * 1000) - _forcePrevAuxDistance;
            _forcePrevAuxDistance = sector.EndPoint * 1000;

            double _slope = sector.Slope;
            climb.Lenght += _distDiff;
            if (sector.EndAlt > _sectors.Last().EndAlt) climb.Elevation += sector.EndAlt - _sectors.Last().EndAlt;
            if (sector.EndAlt > climb.MaxAltitude) climb.MaxAltitude = sector.EndAlt;

            if (_slope > climb.MaxSlope) climb.MaxSlope = _slope;
            climb.Slope = (climb.Elevation / climb.Lenght) * 100;

            _sectors.Add(sector);
            Log.Debug($"Climb info. Length={climb.Lenght}, Elevation={climb.Elevation}");
        }

        private static Result Check(Climb climb, SectorInfo sector)
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

            _hasToCheckAltitude = (sector.EndAlt > climb.MaxAltitude) ? false : true;
            _hasToCheckSlope = (_sectorSlope > 1) ? false : true;

            if (!_hasToCheckSlope && !_hasToCheckAltitude)
            {
                _hasToCheckAltitude = false;
                _hasToCheckSlope = false;
                _checkPrevAuxDistance = 0;
                _checkPrevAlt = 0;
                return Result.Continue;
            }
            else if (ContinueCheck(climb, (sector.EndPoint - _sectors.Last().EndPoint) * 1000, _checkSlope) == false)
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

        private static bool CheckLastPoint(Climb climb, SectorInfo p)
        {
            if (!_isFirstPoint)
            {
                double slope = p.Slope;

                if (slope < 0)
                {       // Si baja cierro puerto
                    return IsPort(climb, slope, p.EndAlt) ? true : false;
                }
                else if (slope > climb.MaxSlope)
                {
                    climb.MaxSlope = slope;
                }

                climb.Lenght += p.EndPoint * 1000 - _sectors.Last().EndPoint;
                if (p.EndAlt > _sectors.Last().EndAlt) climb.Elevation += p.EndAlt - _sectors.Last().EndAlt;
                if (p.EndAlt > climb.MaxAltitude) climb.MaxAltitude = p.EndAlt;
                climb.Slope = (climb.Elevation / climb.Lenght) * 100;
                _sectors.Add(p);
            }
            return IsPort(climb, p.Slope, p.EndAlt);
        }

        private static bool IsPort(Climb climb, double s, double a)
        {
            for (int i = 0; i < MountReq.GetLength(0); i++)
            {
                if ((climb.Slope >= MountReq[i, 0]) && (climb.Lenght >= MountReq[i, 1]))
                {
                    if (s > 0) climb.MaxAltitude = a;
                    return true;
                }
                if (i == MountReq.GetLength(0) - 1)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool ContinueCheck(Climb climb, double d, double s)
        {
            int checkRow = (s < -3) ? 0 : 1;     // Primera columna para descensos, segunda para llanos

            // Comprobacion de distancias
            for (int i = 0; i < CheckLim.GetLength(1) - 1; i++)
            {
                if ((climb.Lenght < CheckLim[checkRow, i + 1, 0]) && (d > CheckLim[checkRow, i, 1])) return false;
            }

            // Comprueba la ultima distancia
            if ((climb.Lenght < CheckLim[checkRow, CheckLim.GetLength(0) - 1, 0]) && (d > CheckLim[checkRow, CheckLim.GetLength(0) - 1, 1])) return false;

            return true;
        }

        private static void EndClimb(Climb climb)
        {
            climb.Lenght = Math.Round(climb.Lenght);
            climb.Elevation = Math.Round(climb.Elevation);
            climb.Slope = Math.Round(climb.Slope, 1);
            climb.InitKm = Math.Round(climb.InitKm, 1);
            climb.InitAltitude = Math.Round(climb.InitAltitude);
            climb.MaxAltitude = Math.Round(climb.MaxAltitude);
            climb.MaxSlope = Math.Round(climb.MaxSlope, 1);

            Log.Info($"New climb found: Id = {climb.Id}, Lenght = {climb.Lenght} m, Elevation = {climb.Elevation} m, InitKm = {climb.InitKm} km");
        }
    }
}
