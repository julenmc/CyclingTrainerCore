using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Route.Reader
{
    public interface IReader
    {
        public class SectorInfo
        {
            public double StartPoint { get; }
            public double EndPoint { get; }
            public double StartAlt {  get; }
            public double EndAlt { get; }
            public double Slope { private set;  get; }
            
            public SectorInfo(double sp, double ep, double sa, double ea, double slope)
            {
                StartPoint = sp;
                EndPoint = ep;
                StartAlt = sa;
                EndAlt = ea;
                Slope = slope;
            }
        }

        public enum ReaderResult
        {
            Value,
            End,
            Error
        }

        public bool Read();
        public double GetLenght();
        public double GetElevation();
        public string GetName();
        public List<SectorInfo> GetAllSectors();
    }
}
