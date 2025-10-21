using CyclingTrainer.Core.Models;
using NLog;
using CyclingTrainer.SessionReader.Models;

namespace CyclingTrainer.SessionReader.Services
{
    internal static class ClimbFinderService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private enum Result
        {
            Continue,
            EndWarning,
            ClimbEnd,
            NoClimb
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
            Result r = Result.NoClimb;

            foreach (SectorInfo sector in sectors)
            {
                if (sector.EndPoint <= forceSector.EndPoint && sector.EndPoint != 0)
                {
                    Log.Trace($"Forcing mount point at {Math.Round(sector.EndPoint)}m ({sector.EndAlt}m) until {Math.Round(forceSector.EndPoint)}m");
                    ForcePoint(climb, sector);
                }
                else
                {
                    r = AddSector(climb, sector);
                    if (r == Result.EndWarning)
                    {
                        Log.Debug($"Climb could end at {Math.Round(sector.StartPoint)}m ({sector.StartAlt}m)");
                        int index = sectors.FindIndex(i => i.EndPoint == sector.EndPoint);
                        r = Check(climb, sectors[index]);
                        while (index < sectors.Count)
                        {
                            if (r == Result.EndWarning)
                            {
                                index++;
                                if (!(index < sectors.Count))
                                {
                                    r = IsPort(climb, 0, 0) ? Result.ClimbEnd : Result.NoClimb; // Checks in last point if it was a port
                                    break;
                                }
                                r = Check(climb, sectors[index]);
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (r == Result.ClimbEnd)
                        {
                            Log.Debug($"Climb ended at {Math.Round(sector.StartPoint)}m ({sector.StartAlt}m)");
                            EndClimb(climb);
                            list.Add(climb);
                            id++;
                            climb = new Climb();
                            climb.Id = id;
                        }
                        else if (r == Result.Continue)
                        {
                            Log.Debug($"Climb continues at {Math.Round(sectors[index].EndPoint)}m");
                            ForcePoint(climb, sector);
                            forceSector = sectors[index];
                        }
                        else
                        {
                            Log.Error($"Should not be here at {Math.Round(sector.EndPoint)}m ({sector.EndAlt}m). r = {r}");
                        }
                    }
                    else if (r == Result.NoClimb)
                    {
                        Log.Trace($"No Climb at {Math.Round(sector.EndPoint)}m");
                        _isFirstPoint = true;
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
                double distDiff = (sector.EndPoint - sector.StartPoint);
                double slope = sector.Slope;

                if (slope < 2)
                {       // If downhill port might end
                    return IsPort(climb, slope, sector.EndAlt) ? Result.EndWarning : Result.NoClimb;
                }
                else if (slope > climb.MaxSlope)
                {
                    climb.MaxSlope = slope;
                }

                climb.Distance += distDiff;
                if (sector.EndAlt > _sectors.Last().EndAlt) climb.HeightDiff += sector.EndAlt - sector.StartAlt;
                if (sector.EndAlt > climb.AltitudeEnd) climb.AltitudeEnd = sector.EndAlt;
                climb.AverageSlope = (climb.HeightDiff / climb.Distance) * 100;

                _sectors.Add(sector);
                Log.Trace($"Climb info. Length={climb.Distance}, Elevation={climb.HeightDiff}");
                return Result.Continue;
            }
            else
            {
                if (sector.Slope > 2)
                {
                    _isFirstPoint = false;
                    _sectors.Add(sector);
                    climb.HeightDiff = sector.EndAlt - sector.StartAlt;
                    climb.Distance = (sector.EndPoint - sector.StartPoint);
                    climb.AverageSlope = sector.Slope;
                    climb.AltitudeEnd = sector.EndAlt;
                    climb.MaxSlope = sector.Slope;
                    climb.AltitudeInit = sector.StartAlt;
                    climb.InitRouteDistance = sector.StartPoint;
                    Log.Debug($"New climb could start at {Math.Round(climb.InitRouteDistance)}");
                    return Result.Continue;
                }
                else return Result.NoClimb;
            }
        }

        private static void ForcePoint(Climb climb, SectorInfo sector)
        {
            if (_forcePrevAuxDistance < _sectors.Last().EndPoint) _forcePrevAuxDistance = _sectors.Last().EndPoint;

            double _distDiff = sector.EndPoint - _forcePrevAuxDistance;
            _forcePrevAuxDistance = sector.EndPoint;

            double _slope = sector.Slope;
            climb.Distance += _distDiff;
            if (sector.EndAlt > _sectors.Last().EndAlt) climb.HeightDiff += sector.EndAlt - _sectors.Last().EndAlt;
            if (sector.EndAlt > climb.AltitudeEnd) climb.AltitudeEnd = sector.EndAlt;

            if (_slope > climb.MaxSlope) climb.MaxSlope = _slope;
            climb.AverageSlope = (climb.HeightDiff / climb.Distance) * 100;

            _sectors.Add(sector);
        }

        private static Result Check(Climb climb, SectorInfo sector)
        {
            if (_checkPrevAuxDistance == 0)
            {
                _checkPrevAuxDistance = _sectors.Last().EndPoint;
                _checkPrevAlt = _sectors.Last().EndAlt;
            }

            double _distDiff = sector.EndPoint - _checkPrevAuxDistance;
            _checkPrevAuxDistance = sector.EndPoint;

            double _sectorSlope = sector.Slope;              // Pendiente en el punto
            double _checkSlope = ((sector.EndAlt - _sectors.First().StartAlt) / (sector.EndPoint - _sectors.First().StartPoint)) * 100;          // Pendiente total en el check
            _checkPrevAlt = sector.EndAlt;

            _hasToCheckAltitude = (sector.EndAlt > climb.AltitudeEnd) ? false : true;
            _hasToCheckSlope = (_sectorSlope > 1) ? false : true;

            if (!_hasToCheckSlope && !_hasToCheckAltitude)
            {
                _hasToCheckAltitude = false;
                _hasToCheckSlope = false;
                _checkPrevAuxDistance = 0;
                _checkPrevAlt = 0;
                return Result.Continue;
            }
            else if (ContinueCheck(climb, sector.EndPoint - _sectors.Last().EndPoint, _checkSlope) == false)
            {
                _hasToCheckAltitude = false;
                _hasToCheckSlope = false;
                _checkPrevAuxDistance = 0;
                _checkPrevAlt = 0;
                return Result.ClimbEnd;
            }
            else
            {
                return Result.EndWarning;
            }
        }

        private static bool IsPort(Climb climb, double s, double a)
        {
            for (int i = 0; i < MountReq.GetLength(0); i++)
            {
                if ((climb.AverageSlope >= MountReq[i, 0]) && (climb.Distance >= MountReq[i, 1]))
                {
                    if (s > 0) climb.AltitudeEnd = a;
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
                if ((climb.Distance < CheckLim[checkRow, i + 1, 0]) && (d > CheckLim[checkRow, i, 1])) return false;
            }

            // Comprueba la ultima distancia
            if ((climb.Distance < CheckLim[checkRow, CheckLim.GetLength(0) - 1, 0]) && (d > CheckLim[checkRow, CheckLim.GetLength(0) - 1, 1])) return false;

            return true;
        }

        private static void EndClimb(Climb climb)
        {
            climb.Distance = Math.Round(climb.Distance);
            climb.HeightDiff = Math.Round(climb.HeightDiff);
            climb.AverageSlope = Math.Round(climb.AverageSlope, 1);
            climb.InitRouteDistance = Math.Round(climb.InitRouteDistance);
            climb.EndRouteDistance = Math.Round(climb.InitRouteDistance + climb.Distance);
            climb.AltitudeInit = Math.Round(climb.AltitudeInit);
            climb.AltitudeEnd = Math.Round(climb.AltitudeEnd);
            climb.MaxSlope = Math.Round(climb.MaxSlope, 1);
            _isFirstPoint = true;

            Log.Info($"New climb found: Id = {climb.Id}, Lenght = {climb.Distance} m, Elevation = {climb.HeightDiff} m, InitPoint = {climb.InitRouteDistance}m, EndPoint = {climb.EndRouteDistance}m");
        }
    }
}
