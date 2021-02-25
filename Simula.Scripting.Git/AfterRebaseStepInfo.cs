﻿namespace Simula.Scripting.Git
{
    /// <summary>
    /// Information about a rebase step that was just completed.
    /// </summary>
    public class AfterRebaseStepInfo
    {
        /// <summary>
        /// Needed for mocking.
        /// </summary>
        protected AfterRebaseStepInfo()
        { }

        internal AfterRebaseStepInfo(RebaseStepInfo stepInfo, Commit commit, long completedStepIndex, long totalStepCount)
        {
            StepInfo = stepInfo;
            Commit = commit;
            WasPatchAlreadyApplied = false;
            CompletedStepIndex = completedStepIndex;
            TotalStepCount = totalStepCount;
        }

        /// <summary>
        /// Constructor to call when the patch has already been applied for this step.
        /// </summary>
        /// <param name="stepInfo"></param>
        /// <param name="completedStepIndex"/>
        /// <param name="totalStepCount"></param>
        internal AfterRebaseStepInfo(RebaseStepInfo stepInfo, long completedStepIndex, long totalStepCount)
            : this (stepInfo, null, completedStepIndex, totalStepCount)
        {
            WasPatchAlreadyApplied = true;
        }

        /// <summary>
        /// The info on the completed step.
        /// </summary>
        public virtual RebaseStepInfo StepInfo { get; private set; }

        /// <summary>
        /// The commit generated by the step, if any.
        /// </summary>
        public virtual Commit Commit { get; private set; }

        /// <summary>
        /// Was the changes for this step already applied. If so,
        /// <see cref="AfterRebaseStepInfo.Commit"/> will be null.
        /// </summary>
        public virtual bool WasPatchAlreadyApplied { get; private set; }

        /// <summary>
        /// The index of the step that was just completed.
        /// </summary>
        public virtual long CompletedStepIndex { get; private set; }

        /// <summary>
        /// The total number of steps in the rebase operation.
        /// </summary>
        public virtual long TotalStepCount { get; private set; }
    }
}
