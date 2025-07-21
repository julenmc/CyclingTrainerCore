using CyclingTrainer.Core.Models;
using NLog;
using CyclingTrainer.SessionReader.Core.Models;
using CyclingTrainer.SessionReader.Core.Repository;

using static CyclingTrainer.Core.Constants.PowerCurveConstants;

namespace CyclingTrainer.SessionAnalyzer.Core.Services
{
    public static class DataAnalyzeService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const int MaxTimeDiff = 3000;

        public static Session AnalyzeData()
        {
            List<FitnessData> fitnessData = SessionRepository.GetFitnessData();
            SessionRepository.UpdateAnalyzedData(AnalyzeFitnessData(fitnessData));
            return SessionRepository.GetSession();
        }

        internal static AnalyzedData AnalyzeFitnessData(List<FitnessData> fitnessData)
        {
            double totalMiliseconds = 0;            // Tiempo total de la actividad
            double currentSectorMiliseconds = 0;    // Tiempo total del sector, se resetea cuando detecta un hueco sin datos
            double totalPower = 0;
            double totalHr = 0;
            double totalCadence = 0;
            double timeDiff = 0;
            int index = 0;
            int currentFirstItemIndex = 0;
            Dictionary<int, PowerCurveSectorInfo> currentPowerCurve = new Dictionary<int, PowerCurveSectorInfo>();
            Dictionary<int, PowerCurveData> maxPowerCurve = new Dictionary<int, PowerCurveData>();
            CreateInitialPowerCurve();

            for (index = 0; index < fitnessData.Count - 1; index++)
            {
                timeDiff = fitnessData[index + 1].Timestamp.GetDateTime().Subtract(fitnessData[index].Timestamp.GetDateTime()).TotalMilliseconds;
                if (timeDiff > MaxTimeDiff)
                {
                    Log.Warn($"Time difference between records {index} and {index + 1} is too high: {timeDiff} ms. This may cause incorrect average values, record is ignored");
                    // Reset currents
                    currentSectorMiliseconds = 0;
                    currentFirstItemIndex = index + 1;
                    CreateInitialPowerCurve();
                    continue;
                }
                totalMiliseconds += timeDiff;
                currentSectorMiliseconds += timeDiff;

                // Handle averages
                totalPower += (fitnessData[index].Stats.Power ?? 0) * timeDiff;
                totalHr += (fitnessData[index].Stats.HeartRate ?? 0) * timeDiff;
                totalCadence += (fitnessData[index].Stats.Cadence ?? 0) * timeDiff;

                // Handle power curve
                UpdatePowerCurve(timeDiff);
            }
            // Add last point info
            totalMiliseconds += 1000;
            currentSectorMiliseconds += 1000;
            totalPower += (double)(fitnessData.Last().Stats.Power ?? 0) * 1000;
            totalHr += (double)(fitnessData.Last().Stats.HeartRate ?? 0) * 1000;
            totalCadence += (double)(fitnessData.Last().Stats.Cadence ?? 0) * 1000;
            UpdatePowerCurve(1000);

            // Get averages
            AnalyzedData data = new AnalyzedData();
            data.AveragePower = (int)Math.Round(totalPower / totalMiliseconds);
            data.AverageHr = (int)Math.Round(totalHr / totalMiliseconds);
            data.AverageCadence = (int)Math.Round(totalCadence / totalMiliseconds);
            data.PowerCurve = maxPowerCurve;
            return data;

            void CreateInitialPowerCurve()
            {
                currentPowerCurve = new Dictionary<int, PowerCurveSectorInfo>();
                foreach (int time in StandardTimePoints)
                {
                    currentPowerCurve.Add(time, new PowerCurveSectorInfo { StartIndex = index, PowerSum = 0 });
                }
            }

            void UpdatePowerCurve(double sectorTime)
            {
                foreach (int time in StandardTimePoints)
                {
                    if (currentSectorMiliseconds / 1000 <= time)
                    {
                        currentPowerCurve[time].PowerSum += (fitnessData[index].Stats.Power ?? 0) * sectorTime;
                        if (currentSectorMiliseconds / 1000 == time)
                        {
                            PowerCurveData data = new PowerCurveData
                            {
                                StartDate = fitnessData[currentFirstItemIndex].Timestamp.GetDateTime(),
                                EndDate = fitnessData[index].Timestamp.GetDateTime(),
                                Power = Math.Round(currentPowerCurve[time].PowerSum / (time * 1000), 2),
                            };
                            if (!maxPowerCurve.ContainsKey(time))
                            {
                                maxPowerCurve.Add(time, data);
                            }
                            else if (data.Power > maxPowerCurve[time].Power)
                            {
                                maxPowerCurve[time] = data;
                                Log.Debug($"New max power at {time}s found: {data.Power}W");
                            }
                        }
                    }
                    else if (currentSectorMiliseconds / 1000 > time)
                    {
                        // Busca el siguiente punto para que coincida con el tiempo de la curva
                        int newIndex = currentPowerCurve[time].StartIndex + 1;
                        double curvePointTime = fitnessData[index].Timestamp.GetDateTime().Subtract(fitnessData[newIndex].Timestamp.GetDateTime()).TotalSeconds + sectorTime / 1000;
                        // if (index > 158 && time == 60)
                        // {
                        //     newIndex = newIndex;
                        // }
                        while (curvePointTime > time)
                        {
                            newIndex++;
                            curvePointTime = fitnessData[index].Timestamp.GetDateTime().Subtract(fitnessData[newIndex].Timestamp.GetDateTime()).TotalSeconds + sectorTime / 1000;
                        }
                        if (curvePointTime != time)
                        {
                            currentPowerCurve[time].PowerSum += (fitnessData[index].Stats.Power ?? 0) * sectorTime;
                            continue;   // Se ha pasado, este tiempo no vale
                        }

                        // Quitar del total los puntos que ya no cuentan
                        for (int i = currentPowerCurve[time].StartIndex; i < newIndex; i++)
                        {
                            double auxTime = fitnessData[i + 1].Timestamp.GetDateTime().Subtract(fitnessData[i].Timestamp.GetDateTime()).TotalMilliseconds;
                            currentPowerCurve[time].PowerSum -= (fitnessData[i].Stats.Power ?? 0) * auxTime;
                        }
                        // Sumar al total el nuevo punto
                        currentPowerCurve[time].PowerSum += (fitnessData[index].Stats.Power ?? 0) * sectorTime;
                        currentPowerCurve[time].StartIndex = newIndex;

                        // Actualizar la potencia maxima en caso de que sea necesario
                        double powerAverage = Math.Round(currentPowerCurve[time].PowerSum / (time * 1000), 2);
                        if (powerAverage > maxPowerCurve[time].Power)
                        {
                            maxPowerCurve[time].Power = powerAverage;
                            maxPowerCurve[time].StartDate = fitnessData[currentPowerCurve[time].StartIndex].Timestamp.GetDateTime();
                            maxPowerCurve[time].EndDate = fitnessData[index].Timestamp.GetDateTime();
                            Log.Debug($"New max power at {time}s found: {powerAverage}W ({fitnessData[index].Timestamp.GetDateTime()})");
                        }
                    }
                }
            }
        }

        internal class PowerCurveSectorInfo
        {
            internal int StartIndex { get; set; }
            internal double PowerSum { get; set; }
        }
    }
}
