using NLog;
using CyclingTrainer.Core.Models;
using CyclingTrainer.SessionReader.Models;
using CyclingTrainer.SessionReader.Services.Fit;
using CyclingTrainer.SessionReader.Services.Gpx;

namespace CyclingTrainer.SessionReader.Services
{
    public static class SessionReaderService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        internal static SessionContainer AnalyzeRoute(ISessionReader reader)
        {
            return Read(reader);
        }

        public static SessionContainer ReadRoute(string path)
        {
            switch (Path.GetExtension(path))
            {
                case ".fit":
                    FitReader fitReader = new FitReader(path);
                    return Read(fitReader);
                case ".gpx":
                    GpxReader gpxReader = new GpxReader(path);
                    return Read(gpxReader);
                default:
                    throw new ArgumentException("File not valid");
            }
        }

        private static SessionContainer Read(ISessionReader reader)
        {
            reader.Read();
            Session session = new Session();
            RouteSections routeData = new RouteSections();
            List<FitnessData> fitnessData = new List<FitnessData>();
            session.Name = reader.GetName();
            session.Distance = Math.Round(reader.GetLenght(), 2);
            session.HeightDiff = Math.Round(reader.GetElevation(), 0);
            routeData.Sectors = reader.GetSmoothedSectors();
            routeData.Climbs = ClimbFinderService.GetClimbs(routeData.Sectors);
            Log.Info($"New route analyzed: {session.Name}. Length = {Math.Round(session.Distance / 1000, 2)} km, Elevation = {session.HeightDiff} m");

            fitnessData = reader.GetFitnessData();
            if (fitnessData.Count == 0)
            {
                Log.Warn("No fitness data found in the session. Please check the file.");
                return new SessionContainer(session, routeData, fitnessData);
            }

            Log.Info($"Fitness data found: {fitnessData.Count} records");
            SetClimbCoords(routeData, fitnessData);

            return new SessionContainer(session, routeData, fitnessData);
        }

        private static void SetClimbCoords(RouteSections routeData, List<FitnessData> fitnessData)
        {
            int index = 0;
            foreach (Climb climb in routeData.Climbs)
            {
                while (index < fitnessData.Count && climb.InitRouteDistance < fitnessData[index].Position.Distance)
                {
                    index++;
                }
                if (index >= fitnessData.Count) continue;

                climb.LongitudeInit = fitnessData[index].Position.Longitude ?? 0;
                climb.LongitudeEnd = fitnessData[index+1].Position.Longitude ?? 0;
                climb.LatitudeInit = fitnessData[index].Position.Latitude ?? 0;
                climb.LatitudeEnd = fitnessData[index + 1].Position.Latitude ?? 0;
            }
        }
    }
}
