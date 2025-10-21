using System.ComponentModel.DataAnnotations;
using CyclingTrainer.SessionAnalyzer.Models;

namespace CyclingTrainer.SessionAnalyzer.Constants
{
    public static class IntervalSearchValues
    {
        public readonly static IntervalThresholdValues ShortIntervals = new IntervalThresholdValues
        {
            Max     = new Thresholds(cvStart: 0.10f, cvFollow: 0.10f, range: 0.30f, maRel: 0.10f),
            Min     = new Thresholds(cvStart: 0.10f, cvFollow: 0.10f, range: 0.30f, maRel: 0.10f),
            Default = new Thresholds(cvStart: 0.10f, cvFollow: 0.20f, range: 0.20f, maRel: 0.12f)
        };
        public readonly static IntervalThresholdValues MediumIntervals = new IntervalThresholdValues
        {
            Max     = new Thresholds(cvStart: 0.20f, cvFollow: 0.70f, range: 0.40f, maRel: 0.50f),
            Min     = new Thresholds(cvStart: 0.10f, cvFollow: 0.10f, range: 0.30f, maRel: 0.10f),
            Default = new Thresholds(cvStart: 0.30f, cvFollow: 0.30f, range: 0.50f, maRel: 0.17f)
        };

        public readonly static IntervalThresholdValues LongIntervals = new IntervalThresholdValues
        {
            Max     = new Thresholds(cvStart: 0.10f, cvFollow: 0.10f, range: 0.30f, maRel: 0.10f),
            Min     = new Thresholds(cvStart: 0.10f, cvFollow: 0.10f, range: 0.30f, maRel: 0.10f),
            Default = new Thresholds(cvStart: 0.30f, cvFollow: 0.40f, range: 1.00f, maRel: 0.30f)
        };
    }
}