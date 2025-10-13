namespace CyclingTrainer.SessionAnalyzer.Core.Constants
{
    public class Thresholds
    {
        public float Max { get; set; }
        public float Min { get; set; }
        public float Default { get; set; }
    }

    public class Parameters
    {
        public Thresholds CvStart { get; set; }
        public Thresholds CvFollow { get; set; }
        public Thresholds Range { get; set; }
        public Thresholds MaRel { get; set; }

        public Parameters(Thresholds cvStart, Thresholds cvFollow, Thresholds range, Thresholds maRel)
        {
            CvStart = cvStart;
            CvFollow = cvFollow;
            Range = range;
            MaRel = maRel;
        }
    }

    public static class IntervalSearchValues
    {
        public readonly static Parameters ShortIntervals = new Parameters(
            new Thresholds { Max = 0.1f, Min = 0.01f, Default = 0.1f },    // cvStart
            new Thresholds { Max = 0.1f, Min = 0.01f, Default = 0.2f },    // cvFollow
            new Thresholds { Max = 0.3f, Min = 0.01f, Default = 0.2f },    // range
            new Thresholds { Max = 0.1f, Min = 0.01f, Default = 0.1f }     // maRel
        );
        public readonly static Parameters MediumIntervals = new Parameters(
            new Thresholds { Max = 0.2f, Min = 0.01f, Default = 0.3f },
            new Thresholds { Max = 0.7f, Min = 0.03f, Default = 0.25f },
            new Thresholds { Max = 0.4f, Min = 0.1f, Default = 0.5f },
            new Thresholds { Max = 0.5f, Min = 0.05f, Default = 0.15f }
        ); 
        public readonly static Parameters LongIntervals = new Parameters(
            new Thresholds { Max = 0.2f, Min = 0.1f, Default = 0.3f },
            new Thresholds { Max = 1f, Min = 0.05f, Default = 0.4f},
            new Thresholds { Max = 0.5f, Min = 0.1f, Default = 0.6f },
            new Thresholds { Max = 1f, Min = 0.1f, Default = 0.25f}
        ); 
    }
}