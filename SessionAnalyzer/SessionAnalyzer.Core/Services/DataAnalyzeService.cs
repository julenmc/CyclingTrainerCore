using CyclingTrainer.Core.Models;
using NLog;
using CyclingTrainer.SessionReader.Core.Models;
using CyclingTrainer.SessionReader.Core.Repository;
using CyclingTrainer.TrainingDatabase.Core.Repository;

using static CyclingTrainer.Core.Constants.PowerCurveConstants;
using Dynastream.Fit;

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

        public static Session AnalyzeData(int cyclistId)
        {
            List<FitnessData> fitnessData = SessionRepository.GetFitnessData();
            SessionRepository.UpdateAnalyzedData(AnalyzeFitnessData(fitnessData, CyclistsRepository.Get(cyclistId)?.FitnessData));
            return SessionRepository.GetSession();
        }

        internal static AnalyzedData AnalyzeFitnessData(List<FitnessData> fitnessData, CyclistFitnessData? cyclistData = null)
        {
            double totalMiliseconds = 0;            // Tiempo total de la actividad
            double currentSectorMiliseconds = 0;    // Tiempo total del sector, se resetea cuando detecta un hueco sin datos
            double totalPower = 0;
            double totalHr = 0;
            double totalCadence = 0;
            double timeDiff = 0;
            int index = 0;
            int currentFirstItemIndex = 0;
            AnalyzedData data = new AnalyzedData();
            Dictionary<int, PowerCurveSectorInfo> currentPowerCurve = new Dictionary<int, PowerCurveSectorInfo>();
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
                UpdateAverages(timeDiff);
                // Handle power curve
                UpdatePowerCurve(timeDiff);
                // Handle zones
                if (cyclistData != null)
                {
                    UpdateHrZone(timeDiff);
                    UpdatePowerZone(timeDiff);
                }
                
            }
            // Add last point info
            totalMiliseconds += 1000;
            currentSectorMiliseconds += 1000;
            UpdateAverages(1000);
            UpdatePowerCurve(1000);

            // Get averages
            data.AveragePower = (int)Math.Round(totalPower / totalMiliseconds);
            data.AverageHr = (int)Math.Round(totalHr / totalMiliseconds);
            data.AverageCadence = (int)Math.Round(totalCadence / totalMiliseconds);
            return data;

            #region Averages
            void UpdateAverages(double time)
            {
                totalPower += (fitnessData[index].Stats.Power ?? 0) * time;
                totalHr += (fitnessData[index].Stats.HeartRate ?? 0) * time;
                totalCadence += (fitnessData[index].Stats.Cadence ?? 0) * time;
            }
            #endregion

            #region PowerCurve
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
                            PowerCurveData powerCurveData = new PowerCurveData
                            {
                                StartDate = fitnessData[currentFirstItemIndex].Timestamp.GetDateTime(),
                                EndDate = fitnessData[index].Timestamp.GetDateTime(),
                                Power = Math.Round(currentPowerCurve[time].PowerSum / (time * 1000), 2),
                            };
                            if (!data.PowerCurve.ContainsKey(time))
                            {
                                data.PowerCurve.Add(time, powerCurveData);
                            }
                            else if (powerCurveData.Power > data.PowerCurve[time].Power)
                            {
                                data.PowerCurve[time] = powerCurveData;
                                Log.Debug($"New max power at {time}s found: {powerCurveData.Power}W");
                            }
                        }
                    }
                    else if (currentSectorMiliseconds / 1000 > time)
                    {
                        // Busca el siguiente punto para que coincida con el tiempo de la curva
                        int newIndex = currentPowerCurve[time].StartIndex + 1;
                        double curvePointTime = fitnessData[index].Timestamp.GetDateTime().Subtract(fitnessData[newIndex].Timestamp.GetDateTime()).TotalSeconds + sectorTime / 1000;

                        while (curvePointTime > time)
                        {
                            newIndex++;
                            curvePointTime = fitnessData[index].Timestamp.GetDateTime().Subtract(fitnessData[newIndex].Timestamp.GetDateTime()).TotalSeconds + sectorTime / 1000;
                        }
                        if (curvePointTime != time)
                        {
                            currentPowerCurve[time].PowerSum += (fitnessData[index].Stats.Power ?? 0) * sectorTime; // Hay que sumar el tiempo porque si no este sector se pierde
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
                        if (powerAverage > data.PowerCurve[time].Power)
                        {
                            data.PowerCurve[time].Power = powerAverage;
                            data.PowerCurve[time].StartDate = fitnessData[currentPowerCurve[time].StartIndex].Timestamp.GetDateTime();
                            data.PowerCurve[time].EndDate = fitnessData[index].Timestamp.GetDateTime();
                            Log.Debug($"New max power at {time}s found: {powerAverage}W ({fitnessData[index].Timestamp.GetDateTime()})");
                        }
                    }
                }
            }
            #endregion

            #region HrZone
            void UpdateHrZone(double time)
            {
                if (cyclistData.HrZones == null) throw new Exception("No HR data found");
                double timeSeconds = time / 1000;
                // foreach (Zone zone in cyclistData.HrZones)
            }
            #endregion

            #region PowerZone
            void UpdatePowerZone(double time)
            {
                double timeSeconds = time / 1000;
            }
            #endregion
        }

        internal class PowerCurveSectorInfo
        {
            internal int StartIndex { get; set; }
            internal double PowerSum { get; set; }
        }
    }
}
