using System.Globalization;
using Simula.Scripting.Git.Core;

namespace Simula.Scripting.Git
{
    /// <summary>
    /// Gets the current Simula.Scripting.Git version.
    /// </summary>
    public class Version
    {
        /// <summary>
        /// Needed for mocking purposes.
        /// </summary>
        protected Version()
        { }

        internal static Version Build()
        {
            return new Version();
        }

        /// <summary>
        /// Returns version of the Simula.Scripting.Git library.
        /// </summary>
        public virtual string InformationalVersion => typeof(Version).Assembly.GetName().Version.ToString();

        /// <summary>
        /// Returns all the optional features that were compiled into
        /// libgit2.
        /// </summary>
        /// <returns>A <see cref="BuiltInFeatures"/> enumeration.</returns>
        public virtual BuiltInFeatures Features
        {
            get { return Proxy.git_libgit2_features(); }
        }

        /// <summary>
        /// Returns the SHA hash for the libgit2 library.
        /// </summary>
        public virtual string LibGit2CommitSha => RetrieveAbbrevShaFrom(AssemblyCommitIds.LibGit2CommitSha);

        /// <summary>
        /// Returns the SHA hash for the Simula.Scripting.Git library.
        /// </summary>
        public virtual string LibGit2SCommitSha => RetrieveAbbrevShaFrom(AssemblyCommitIds.LibGit2SCommitSha);

        private string RetrieveAbbrevShaFrom(string sha)
        {
            var index = sha.Length > 7 ? 7 : sha.Length;
            return sha.Substring(0, index);
        }

        /// <summary>
        /// Returns a string representing the Simula.Scripting.Git version.
        /// </summary>
        /// <para>
        ///   The format of the version number is as follows:
        ///   <para>Major.Minor.Patch[-previewTag]+{Simula.Scripting.Git_abbrev_hash}.libgit2-{libgit2_abbrev_hash} (x86|x64 - features)</para>
        /// </para>
        /// <returns></returns>
        public override string ToString()
        {
            return RetrieveVersion();
        }

        private string RetrieveVersion()
        {
            string features = Features.ToString();

            return string.Format(CultureInfo.InvariantCulture,
                                 "{0} ({1} - {2})",
                                 InformationalVersion,
                                 Platform.ProcessorArchitecture,
                                 features);
        }
    }
}
