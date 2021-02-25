using System;

namespace Simula.Scripting.Git.Core
{
    [Flags]
    internal enum LookUpOptions
    {
        None = 0,
        ThrowWhenNoGitObjectHasBeenFound = 1,
        DereferenceResultToCommit = 2,
        ThrowWhenCanNotBeDereferencedToACommit = 4,
    }
}
