namespace CyclingTrainer.SessionAnalyzer.Models
{
    public class Thresholds
    {
        public float CvStart { get; set; }
        public float CvFollow { get; set; }
        public float Range { get; set; }
        public float MaRel { get; set; }

        public Thresholds(float cvStart, float cvFollow, float range, float maRel)
        {
            CvStart = cvStart;
            CvFollow = cvFollow;
            Range = range;
            MaRel = maRel;
        }

        public Thresholds() { }

        public Thresholds Copy()
        {
            return new Thresholds
            {
                CvStart = this.CvStart,
                CvFollow = this.CvFollow,
                Range = this.Range,
                MaRel = this.MaRel,
            };
        }
    }

    public class IntervalThresholdValues
    {
        public Thresholds Max { get; set; } = new();
        public Thresholds Min { get; set; } = new();
        public Thresholds Default { get; set; } = new();
    }

    public class IntervalGroupThresholds
    {
        public Thresholds Short { get; set; } = new();
        public Thresholds Medium { get; set; } = new();
        public Thresholds Long { get; set; } = new();
    }
}