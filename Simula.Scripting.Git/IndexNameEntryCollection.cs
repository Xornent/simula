using System;
using System.Collections;
using System.Collections.Generic;
using Simula.Scripting.Git.Core;
using Simula.Scripting.Git.Core.Handles;

namespace Simula.Scripting.Git
{
    /// <summary>
    /// The collection of <see cref="Simula.Scripting.Git.IndexNameEntry"/>s in a
    /// <see cref="Simula.Scripting.Git.Repository"/> index that reflect the
    /// original paths of any rename conflicts that exist in the index.
    /// </summary>
    public class IndexNameEntryCollection : IEnumerable<IndexNameEntry>
    {
        private readonly Index index;

        /// <summary>
        /// Needed for mocking purposes.
        /// </summary>
        protected IndexNameEntryCollection()
        { }

        internal IndexNameEntryCollection(Index index)
        {
            this.index = index;
        }

        private unsafe IndexNameEntry this[int idx]
        {
            get
            {
                git_index_name_entry* entryHandle = Proxy.git_index_name_get_byindex(index.Handle, (UIntPtr)idx);
                return IndexNameEntry.BuildFromPtr(entryHandle);
            }
        }

        #region IEnumerable<IndexNameEntry> Members

        private List<IndexNameEntry> AllIndexNames()
        {
            var list = new List<IndexNameEntry>();

            int count = Proxy.git_index_name_entrycount(index.Handle);

            for (int i = 0; i < count; i++)
            {
                list.Add(this[i]);
            }

            return list;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator{T}"/> object that can be used to iterate through the collection.</returns>
        public virtual IEnumerator<IndexNameEntry> GetEnumerator()
        {
            return AllIndexNames().GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
