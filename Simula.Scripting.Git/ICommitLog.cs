using System.Collections.Generic;

namespace Simula.Scripting.Git
{
    /// <summary>
    /// A log of commits in a <see cref="Repository"/>.
    /// </summary>
    public interface ICommitLog : IEnumerable<Commit>
    {
        /// <summary>
        /// Gets the current sorting strategy applied when enumerating the log.
        /// </summary>
        CommitSortStrategies SortedBy { get; }
    }
}
