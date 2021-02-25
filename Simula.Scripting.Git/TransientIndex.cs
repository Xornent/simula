using System;
using Simula.Scripting.Git.Core.Handles;

namespace Simula.Scripting.Git
{
    /// <summary>
    /// An implementation of <see cref="Index"/> with disposal managed by the caller
    /// (instead of automatically disposing when the repository is disposed)
    /// </summary>
    public class TransientIndex: Index, IDisposable
    {
        /// <summary>
        /// Needed for mocking purposes.
        /// </summary>
        protected TransientIndex()
        { }

        internal TransientIndex(IndexHandle handle, Repository repo)
            : base(handle, repo)
        {
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Handle.SafeDispose();
        }
    }
}
