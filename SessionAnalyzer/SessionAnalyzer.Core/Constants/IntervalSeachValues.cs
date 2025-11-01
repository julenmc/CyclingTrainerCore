using System.ComponentModel.DataAnnotations;
using CyclingTrainer.SessionAnalyzer.Models;

namespace CyclingTrainer.SessionAnalyzer.Constants
{
    public static class IntervalSearchValues
    {
        public readonly static IntervalThresholdValues ShortIntervals = new IntervalThresholdValues
        {
            Max     = new Thresholds(cvStart: 0.20f, cvFollow: 0.30f, range: 0.30f, maRel: 0.20f),
            Min     = new Thresholds(cvStart: 0.10f, cvFollow: 0.10f, range: 0.10f, maRel: 0.10f),
            Default = new Thresholds(cvStart: 0.15f, cvFollow: 0.20f, range: 0.20f, maRel: 0.15f)
        };
        public readonly static IntervalThresholdValues MediumIntervals = new IntervalThresholdValues
        {
            Max     = new Thresholds(cvStart: 0.40f, cvFollow: 0.50f, range: 0.70f, maRel: 0.30f),
            Min     = new Thresholds(cvStart: 0.20f, cvFollow: 0.10f, range: 0.30f, maRel: 0.10f),
            Default = new Thresholds(cvStart: 0.30f, cvFollow: 0.30f, range: 0.50f, maRel: 0.20f)
        };

        public readonly static IntervalThresholdValues LongIntervals = new IntervalThresholdValues
        {
            Max     = new Thresholds(cvStart: 0.60f, cvFollow: 0.60f, range: 2.00f, maRel: 0.50f),
            Min     = new Thresholds(cvStart: 0.20f, cvFollow: 0.20f, range: 0.50f, maRel: 0.10f),
            Default = new Thresholds(cvStart: 0.40f, cvFollow: 0.40f, range: 1.00f, maRel: 0.30f)
        };
    }
}