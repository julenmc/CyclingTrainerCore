using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Route.Reader
{
    public interface IReader
    {
        public class PointInfo
        {
            public double Len { get; }
            public double Alt {  get; }

            public PointInfo(double x, double y)
            {
                Len = x;
                Alt = y;
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
        public List<PointInfo> GetAllPoints();
    }
}
