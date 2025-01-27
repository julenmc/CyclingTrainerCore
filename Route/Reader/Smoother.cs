using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Route.Reader.IReader;
using static System.Formats.Asn1.AsnWriter;

namespace Route.Reader
{
    internal static class Smoother
    {
        private static readonly double FirstSmoothDistance = 0.025; // Kilometers

        internal static List<SectorInfo> SmoothAndAddSectors(List<SectorInfo> input)
        {
            List<SectorInfo> output = FirstSmooth(input);
            output = AddSectors(output);
            return output;
        }

        private static List<SectorInfo> FirstSmooth(List<SectorInfo> input)
        {
            List<SectorInfo> output = new List<SectorInfo>();
            int arrayIndex = 0;
            for (int i = 0; i < input.Count - 1; i++)
            {
                if (input[i].EndPoint >= FirstSmoothDistance)
                {
                    double slope = (input[i].EndAlt - input[0].StartAlt) / (input[i].EndPoint - input[0].StartPoint) / 10;
                    SectorInfo point = new SectorInfo(input[0].StartPoint, input[i].EndPoint, input[0].StartAlt, input[i].EndAlt, slope);
                    output.Add(point);
                    arrayIndex = i + 1;
                    break;
                }
            }
            
            for (int i = arrayIndex; i < input.Count - 1; i++)
            {
                if (input[i].EndPoint - output.Last().EndPoint >= FirstSmoothDistance)
                {
                    double slope = (input[i].EndAlt - output.Last().EndAlt) / (input[i].EndPoint - output.Last().EndPoint) / 10;
                    SectorInfo point = new SectorInfo(output.Last().EndPoint, input[i].EndPoint, output.Last().EndAlt, input[i].EndAlt, slope);
                    output.Add(point);
                }
            }
            double s = (input.Last().EndAlt - output.Last().EndAlt) / (input.Last().EndPoint - output.Last().EndPoint) / 10;
            SectorInfo p = new SectorInfo(input.Last().EndPoint, input.Last().EndPoint, input.Last().EndAlt, input.Last().EndAlt, s);
            output.Add(p);

            return output;
        }


        private static readonly double SecondSmoothDistance = 0.01; // Kilometers
        private static List<SectorInfo> AddSectors(List<SectorInfo> input)
        {
            List<SectorInfo> output = new List<SectorInfo>();
            output.AddRange(AddFirstSectors(input));
            output.AddRange(AddMiddleSectors(input));
            output.AddRange(AddLastSectors(input));

            return output;
        }

        private static List<SectorInfo> AddFirstSectors(List<SectorInfo> input)
        {
            List<SectorInfo> output = new List<SectorInfo>();
            double newPoints = Math.Ceiling(input.First().EndPoint / SecondSmoothDistance);   // Points to add in sector
            if (newPoints % 2 != 0) newPoints++;
            double pointDistance = input.First().EndPoint / newPoints;
            double change = 0.5 / (newPoints / 2);
            double currentSlope = input.First().Slope;
            double nextSlope = input[1].Slope;
            double currentLen = input.First().StartPoint;
            double currentAlt = input.First().StartAlt;
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

        private static List<SectorInfo> AddMiddleSectors(List<SectorInfo> input)
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

        private static List<SectorInfo> AddLastSectors(List<SectorInfo> input)
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
