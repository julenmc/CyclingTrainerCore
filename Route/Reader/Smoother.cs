using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Route.Reader.IReader;

namespace Route.Reader
{
    internal static class Smoother
    {
        private static readonly double FirstSmoothDistance = 0.025; // Kilometers
        internal static List<SectorInfo> FirstSmooth(List<SectorInfo> input)
        {
            List<SectorInfo> output = new List<SectorInfo>();
            int arrayIndex = 0;
            for (int i = 1; i < input.Count - 1; i++)
            {
                if (input[i].EndPoint - input[arrayIndex].EndPoint >= FirstSmoothDistance)
                {
                    double slope = (input[i].EndAlt - input[arrayIndex].EndAlt) / (input[i].EndPoint - input[arrayIndex].EndPoint) / 10;
                    SectorInfo point = new SectorInfo(input[arrayIndex].StartPoint, input[i].EndPoint, input[arrayIndex].StartAlt, input[i].EndAlt, slope);
                    output.Add(point);
                    arrayIndex = i;
                }
            }
            double s = (input.Last().EndAlt - output.Last().EndAlt) / (input.Last().EndPoint - output.Last().EndPoint) / 10;
            SectorInfo p = new SectorInfo(input[arrayIndex].StartPoint, input.Last().EndPoint, input[arrayIndex].StartAlt, input.Last().EndAlt, s);
            output.Add(p);

            return output;
        }


        private static readonly double SecondSmoothDistance = 0.01; // Kilometers
        internal static List<SectorInfo> SecondSmooth(List<SectorInfo> input)
        {
            List<SectorInfo> output = new List<SectorInfo>();
            output.AddRange(SmoothFirstPoint(input));
            output.AddRange(SmoothMiddlePoints(input));
            output.AddRange(SmoothLastPoint(input));

            return output;
        }

        private static List<SectorInfo> SmoothFirstPoint(List<SectorInfo> input)
        {
            List<SectorInfo> output = new List<SectorInfo>();
            double newPoints = Math.Ceiling(input[1].EndPoint / SecondSmoothDistance);   // Points to add in sector
            if (newPoints % 2 != 0) newPoints++;
            double pointDistance = input[1].EndPoint / newPoints;
            double change = 0.5 / (newPoints / 2);
            double currentSlope = input.First().Slope;
            double nextSlope = input[1].Slope;
            double currentLen = 0;
            double currentAlt = input.First().EndAlt;
            // First half mantains
            for (int j = 0; j < newPoints / 2; j++)
            {
                double prevLen = currentLen;
                currentLen += pointDistance;
                double prevAlt = currentAlt;
                currentAlt = GetAltWithSlope(pointDistance, currentSlope, currentAlt);
                output.Add(new SectorInfo(prevLen, currentLen, prevAlt, currentAlt, currentSlope));
            }
            // Second half depends on next sector
            for (int j = 1; j < newPoints / 2 + 1; j++)
            {
                double auxNextValue = change * j;
                double newSlope = auxNextValue * nextSlope + (1 - auxNextValue) * currentSlope;
                double prevLen = currentLen;
                currentLen += pointDistance;
                double prevAlt = currentAlt;
                currentAlt = GetAltWithSlope(pointDistance, newSlope, currentAlt);
                output.Add(new SectorInfo(prevLen, currentLen, prevAlt, currentAlt, newSlope));
            }
            return output;
        }

        private static List<SectorInfo> SmoothMiddlePoints(List<SectorInfo> input)
        {
            List<SectorInfo> output = new List<SectorInfo>();
            double currentLen = input[1].EndPoint;
            double currentAlt = input[1].EndAlt;
            for (int i = 1; i < input.Count - 2; i++)
            {
                double newPoints = Math.Ceiling((input[i].EndPoint - input[i - 1].EndPoint) / SecondSmoothDistance);   // Points to add in sector
                if (newPoints % 2 != 0) newPoints++;
                double pointDistance = input[i].EndPoint / newPoints;
                double change = 0.5 / (newPoints / 2);
                double currentSlope = input[i].Slope;
                double prevSectorSlope = input[i - 1].Slope;
                for (int j = 0; j < newPoints / 2; j++)
                {
                    double auxPrevValue = 0.5 - change * j;
                    double newSlope = auxPrevValue * prevSectorSlope + (1 - auxPrevValue) * currentSlope;
                    double prevLen = currentLen;
                    currentLen += pointDistance;
                    double prevAlt = currentAlt;
                    currentAlt = GetAltWithSlope(pointDistance, newSlope, currentAlt);
                    output.Add(new SectorInfo(prevLen, currentLen, prevAlt, currentAlt, newSlope));
                }
                double nextSectorSlope = input[i + 1].Slope;   
                for (int j = 1; j < newPoints / 2 + 1; j++)
                {
                    double auxNextValue = change * j;
                    double newSlope = auxNextValue * nextSectorSlope + (1 - auxNextValue) * currentSlope;
                    double prevLen = currentLen;
                    currentLen += pointDistance;
                    double prevAlt = currentAlt;
                    currentAlt = GetAltWithSlope(pointDistance, newSlope, currentAlt);
                    output.Add(new SectorInfo(prevLen, currentLen, prevAlt, currentAlt, newSlope));
                }
            }
            return output;
        }

        private static List<SectorInfo> SmoothLastPoint(List<SectorInfo> input)
        {
            List<SectorInfo> output = new List<SectorInfo>();
            double newPoints = Math.Ceiling((input.Last().EndPoint - input[input.Count - 2].EndPoint) / SecondSmoothDistance);   // Points to add in sector
            if (newPoints % 2 != 0) newPoints++;
            double pointDistance = (input.Last().EndPoint - input[input.Count - 2].EndPoint) / newPoints;
            double change = 0.5 / (newPoints / 2);
            double currentSlope = input.Last().Slope;
            double prevSlope = input[input.Count - 2].Slope;
            double currentLen = input[input.Count - 2].EndPoint;
            double currentAlt = input[input.Count - 2].EndAlt;
            // First half depends on previous sector
            for (int j = 0; j < newPoints; j++)
            {
                double auxNextValue = 0.5 - change * j;
                double newSlope = auxNextValue * prevSlope + (1 - auxNextValue) * currentSlope;
                double prevLen = currentLen;
                currentLen += pointDistance;
                double prevAlt = currentAlt;
                currentAlt = GetAltWithSlope(pointDistance, prevSlope, currentAlt);
                output.Add(new SectorInfo(prevLen, currentLen, prevAlt, currentAlt, prevSlope));
            }
            //// Second half depends on next sector
            //for (int j = 0; j < newPoints / 2; j++)
            //{
            //    currentLen += pointDistance;
            //    currentAlt = GetAltWithSlope(pointDistance, currentSlope, currentAlt);
            //    output.Add(new PointInfo(currentLen, currentAlt, currentSlope));
            //}
            return output;
        }

        private static double GetAltWithSlope(double len, double slope, double prevAlt)
        {
            return prevAlt + (len * 1000) * slope / 100;
        }
    }
}
