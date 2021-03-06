﻿using System;

namespace HomeCenter.Services.MotionService
{
    /// <summary>
    /// Parameters that determinate how motion service is working
    /// </summary>
    public class MotionConfiguration
    {
        /// <summary>
        /// Time in which we analyze move to find potential motion vectors
        /// </summary>
        public TimeSpan MotionTimeWindow { get; set; }

        /// <summary>
        /// Minimal difference in movement that is possible to do physically
        /// </summary>
        public TimeSpan MotionMinDiff { get; set; }

        /// <summary>
        /// Value by which we will increase area turn off timeout when it is turned on too quick
        /// </summary>
        public double TurnOffTimeoutExtenderFactor { get; set; }

        /// <summary>
        /// Factor by which we will increase turn off timeout after each move in room
        /// </summary>
        public double TurnOffTimeoutIncrementFactor { get; set; }

        /// <summary>
        /// Time interval for periodic move calculations
        /// </summary>
        public TimeSpan PeriodicCheckTime { get; set; }

        /// <summary>
        /// Time after which we can remove confused vector
        /// </summary>
        public TimeSpan ConfusionResolutionTimeOut { get; set; }

        /// <summary>
        /// Time after which we can try to unconfuse vector that was uncertain before
        /// </summary>
        public TimeSpan ConfusionResolutionTime { get; set; }
    }
}