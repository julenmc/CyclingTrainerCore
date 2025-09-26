# Sprint Service Specifications
## Introduction
This service is used to search for sprints in an activity.

The service will be passed the list of all points saved in the activity (class [`FitnessData`](../../SessionReader/SessionReader.Core/Models/FitnessData.cs)), which will include information such as power or heart rate.

# Requirements
It will consist of a single function that will analyze the activity and perform the corresponding tasks (such as saving the sprints in the repository). These are the requirements:
1. Three configurable parameters will be passed as input parameters (in addition to all the activity points):
    1. Minimum sprint time.
    2. Sprint start trigger.
    3. Sprint end trigger.
2. The service itself will be responsible for saving the sprints in the repository ([`IntervalsRepository´](../SessionAnalyzer.Core/Services/Intervals/IntervalRepository.cs)).
3. It is allowed to drop below the lower trigger by up to one second.
4. As soon as a sprint is detected, [`IntervalsRepository´](../SessionAnalyzer.Core/Services/Intervals/IntervalRepository.cs) will be updated to remove this data from the points to be analyzed.

* Note: the start and end triggers of the sprint are there so that the detection of the sprint works as a hysteresis cycle: the start trigger being higher than the end trigger.