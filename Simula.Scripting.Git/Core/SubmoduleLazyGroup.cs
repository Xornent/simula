using System;
using Simula.Scripting.Git.Core.Handles;

namespace Simula.Scripting.Git.Core
{
    internal class SubmoduleLazyGroup : LazyGroup<SubmoduleHandle>
    {
        private readonly string name;

        public SubmoduleLazyGroup(Repository repo, string name)
            : base(repo)
        {
            this.name = name;
        }

        protected override void EvaluateInternal(Action<SubmoduleHandle> evaluator)
        {
            repo.Submodules.Lookup(name,
                                   handle =>
                                   {
                                       evaluator(handle);
                                       return default(object);
                                   },
                                   true);
        }
    }
}
