using CyclingTrainer.SessionReader.Core.Models;

namespace CyclingTrainer.SessionReader.Core.Services
{
    internal static class RouteSmootherService
    {
        private const double SmoothDistance = 25;      
        private const double SectorMinDistance = 5;   
        private static readonly double[] SectorPercentage = { 0.5, 0.7, 0.85, 1};

        internal static List<SectorInfo> SmoothAndAddSectors(List<SectorInfo> input)
        {
            List<SectorInfo> output = FirstSmooth(input);
            output = DivideSectors(output);
            return output;
        }

        private static List<SectorInfo> FirstSmooth(List<SectorInfo> input)
        {
            List<SectorInfo> output = new List<SectorInfo>();
            int arrayIndex = 0;
            for (int i = 0; i < input.Count - 1; i++)
            {
                if (input[i].EndPoint >= SmoothDistance)
                {
                    double slope = (input[i].EndAlt - input[0].StartAlt) / (input[i].EndPoint - input[0].StartPoint) * 100;
                    SectorInfo point = new SectorInfo
                    {
                        StartPoint = input[0].StartPoint,
                        EndPoint = input[i].EndPoint,
                        StartAlt = input[0].StartAlt,
                        EndAlt = input[i].EndAlt,
                        Slope = slope
                    };
                    output.Add(point);
                    arrayIndex = i + 1;
                    break;
                }
            }
            
            for (int i = arrayIndex; i < input.Count - 1; i++)
            {
                if (input[i].EndPoint - output.Last().EndPoint >= SmoothDistance)
                {
                    double slope = (input[i].EndAlt - output.Last().EndAlt) / (input[i].EndPoint - output.Last().EndPoint) * 100;
                    SectorInfo point = new SectorInfo
                    {
                        StartPoint = output.Last().EndPoint,
                        EndPoint = input[i].EndPoint,
                        StartAlt = output.Last().EndAlt,
                        EndAlt = input[i].EndAlt,
                        Slope = slope
                    };
                    output.Add(point);
                }
            }
            double s = (input.Last().EndAlt - output.Last().EndAlt) / (input.Last().EndPoint - output.Last().EndPoint) * 100;
            SectorInfo p = new SectorInfo
            {
                StartPoint = input.Last().EndPoint,
                EndPoint = input.Last().EndPoint,
                StartAlt = input.Last().EndAlt,
                EndAlt = input.Last().EndAlt,
                Slope = s
            };
            output.Add(p);

            return output;
        }

        private static List<SectorInfo> DivideSectors(List<SectorInfo> sectors)
        {
            List<SectorInfo> output = new List<SectorInfo>();
            output.AddRange(DivideSector(sectors[0], sectors[0], sectors[1]));
            for (int i = 1; i < sectors.Count - 1; i++)
            {
                output.AddRange(DivideSector(sectors[i], sectors[i-1], sectors[i+1]));
            }
            output.AddRange(DivideSector(sectors[sectors.Count - 1], sectors[sectors.Count - 2], sectors[sectors.Count - 1]));
            return output;
        }

        private static SectorInfo[] DivideSector(SectorInfo sector, SectorInfo previous, SectorInfo next)
        {
            double sectorCurrentDist = sector.EndPoint - sector.StartPoint;
            int numSubsectors = 0;
            if ((sectorCurrentDist / SectorPercentage.Length * 2) >= SectorMinDistance)
            {
                numSubsectors = (int)Math.Ceiling(sectorCurrentDist / SectorMinDistance);
                numSubsectors = (numSubsectors % 2 == 0) ? numSubsectors : numSubsectors + 1;   // Always even
            }
            else
            {
                numSubsectors = SectorPercentage.Length;
            }
            double newSectorDist = sectorCurrentDist / numSubsectors;
            SectorInfo[] output = new SectorInfo[numSubsectors];
            double fixedSlope = sector.Slope;
            double startPointPrev = sector.StartPoint;
            double currentAltPrev = sector.StartAlt;
            double currentAltNext = sector.EndAlt;
            for (int i = 0; i < numSubsectors / 2; i++)
            {
                // Smoothing with previous sector
                double startPoint = startPointPrev + newSectorDist * i;
                double endPoint = startPoint + newSectorDist;
                double slopePercentage = (i < SectorPercentage.Length) ? SectorPercentage[i] : 1;
                double slope = sector.Slope * slopePercentage + previous.Slope * (1 - slopePercentage);
                double startAlt = currentAltPrev;
                currentAltPrev = GetAltWithSlope(newSectorDist, slope, currentAltPrev);
                output[i] = new SectorInfo
                {
                    StartPoint = startPoint,
                    EndPoint = endPoint,
                    StartAlt = startAlt,
                    EndAlt = currentAltPrev,
                    Slope = slope
                };
                // Smoothing with next sector
                endPoint = sector.EndPoint - newSectorDist * i;
                startPoint = endPoint - newSectorDist;
                slope = sector.Slope * slopePercentage + next.Slope * (1 - slopePercentage);
                double endAlt = currentAltNext;
                currentAltNext = GetAltWithSlope(newSectorDist, -slope, currentAltNext);
                output[(numSubsectors - 1) - i] = new SectorInfo
                { 
                    StartPoint = startPoint, 
                    EndPoint = endPoint, 
                    StartAlt = currentAltNext, 
                    EndAlt = endAlt, 
                    Slope = slope 
                };
                double fixedAlt = output[(numSubsectors - 1) - i].StartAlt - output[i].EndAlt;
                double fixedDist = output[(numSubsectors - 1) - i].StartPoint - output[i].EndPoint;
                fixedSlope = fixedAlt / fixedDist * 100;
            }
            return output;
        }


        private static double GetAltWithSlope(double len, double slope, double prevAlt)
        {
            return prevAlt + len * slope / 100;
        }
    }
}
