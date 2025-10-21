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

        public Thresholds() {}
    }

    public class IntervalThresholdValues
    {
        public Thresholds Max { get; set; } = new();
        public Thresholds Min { get; set; } = new();
        public Thresholds Default { get; set; } = new();
    }
}