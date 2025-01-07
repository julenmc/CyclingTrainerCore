using NLog;

namespace Cyclist
{
    public static class SpeedCalculator
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static readonly double DeltaTime = 0.1; // Seconds

        private static readonly double rho = 1.225;     // Air density
        private static readonly double Cd = 0.9;        // Coeficiente de arrastre
        private static readonly double frontArea = 0.5; // m^2
        private static readonly double gravity = 9.81;
        private static readonly double muR = 0.005;     // Coeficiente rodadura
        private static readonly double BikeWeight = 7;

        public static (double, double) CalculateDistance(CyclistRepository cyclist, double slope, double wind, double power)
        {
            double s = cyclist.Speed; 
            double d = 0; 
            double w = cyclist.Weight + BikeWeight; 

            double vRel = s + wind;

            // Resistive forces
            double forceAir = 0.5 * rho * Cd * frontArea * Math.Pow(vRel, 2); // Air
            double forceRoll = w * gravity * muR; // Roll
            double forceSlope = w * gravity * Math.Sin(slope / 100.0); // Slope

            double forceApplied = s != 0 ? power / s : 200;

            double forceNet = forceApplied - (forceAir + forceRoll + forceSlope);
            double acc = forceNet / w;

            s += acc * DeltaTime;
            if (s < 0) s = 0; 
            d += s * DeltaTime;
            return (d,s);
        }
    }
}
