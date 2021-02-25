﻿using System;
using System.Collections;
using System.Collections.Generic;
using Simula.Scripting.Git.Core;
using Simula.Scripting.Git.Core.Handles;

namespace Simula.Scripting.Git
{
    /// <summary>
    /// Exposes properties of a remote that can be updated.
    /// </summary>
    public class RemoteUpdater
    {
        private readonly UpdatingCollection<string> fetchRefSpecs;
        private readonly UpdatingCollection<string> pushRefSpecs;
        private readonly Repository repo;
        private readonly string remoteName;

        /// <summary>
        /// Needed for mocking purposes.
        /// </summary>
        protected RemoteUpdater()
        { }

        internal RemoteUpdater(Repository repo, Remote remote)
        {
            Ensure.ArgumentNotNull(repo, "repo");
            Ensure.ArgumentNotNull(remote, "remote");

            this.repo = repo;
            this.remoteName = remote.Name;

            fetchRefSpecs = new UpdatingCollection<string>(GetFetchRefSpecs, SetFetchRefSpecs);
            pushRefSpecs = new UpdatingCollection<string>(GetPushRefSpecs, SetPushRefSpecs);
        }

        internal RemoteUpdater(Repository repo, string remote)
        {
            Ensure.ArgumentNotNull(repo, "repo");
            Ensure.ArgumentNotNull(remote, "remote");

            this.repo = repo;
            this.remoteName = remote;

            fetchRefSpecs = new UpdatingCollection<string>(GetFetchRefSpecs, SetFetchRefSpecs);
            pushRefSpecs = new UpdatingCollection<string>(GetPushRefSpecs, SetPushRefSpecs);
        }

        private IEnumerable<string> GetFetchRefSpecs()
        {
            using (RemoteHandle remoteHandle = Proxy.git_remote_lookup(repo.Handle, remoteName, true))
            {
                return Proxy.git_remote_get_fetch_refspecs(remoteHandle);
            }
        }

        private void SetFetchRefSpecs(IEnumerable<string> value)
        {
            repo.Config.UnsetAll(string.Format("remote.{0}.fetch", remoteName), ConfigurationLevel.Local);

            foreach (var url in value)
            {
                Proxy.git_remote_add_fetch(repo.Handle, remoteName, url);
            }
        }

        private IEnumerable<string> GetPushRefSpecs()
        {
            using (RemoteHandle remoteHandle = Proxy.git_remote_lookup(repo.Handle, remoteName, true))
            {
                return Proxy.git_remote_get_push_refspecs(remoteHandle);
            }
        }

        private void SetPushRefSpecs(IEnumerable<string> value)
        {
            repo.Config.UnsetAll(string.Format("remote.{0}.push", remoteName), ConfigurationLevel.Local);

            foreach (var url in value)
            {
                Proxy.git_remote_add_push(repo.Handle, remoteName, url);
            }
        }

        /// <summary>
        /// Set the default TagFetchMode value for the remote.
        /// </summary>
        public virtual TagFetchMode TagFetchMode
        {
            set { Proxy.git_remote_set_autotag(repo.Handle, remoteName, value); }
        }

        /// <summary>
        /// Sets the url defined for this <see cref="Remote"/>
        /// </summary>
        public virtual string Url
        {
            set { Proxy.git_remote_set_url(repo.Handle, remoteName, value); }
        }

        /// <summary>
        /// Sets the push url defined for this <see cref="Remote"/>
        /// </summary>
        public virtual string PushUrl
        {
            set { Proxy.git_remote_set_pushurl(repo.Handle, remoteName, value); }
        }

        /// <summary>
        /// Sets the list of <see cref="RefSpec"/>s defined for this <see cref="Remote"/> that are intended to
        /// be used during a Fetch operation
        /// </summary>
        public virtual ICollection<string> FetchRefSpecs
        {
            get { return fetchRefSpecs; }
            set { fetchRefSpecs.ReplaceAll(value); }
        }

        /// <summary>
        /// Sets or gets the list of <see cref="RefSpec"/>s defined for this <see cref="Remote"/> that are intended to
        /// be used during a Push operation
        /// </summary>
        public virtual ICollection<string> PushRefSpecs
        {
            get { return pushRefSpecs; }
            set { pushRefSpecs.ReplaceAll(value); }
        }

        private class UpdatingCollection<T> : ICollection<T>
        {
            private readonly Lazy<List<T>> list;
            private readonly Action<IEnumerable<T>> setter;

            public UpdatingCollection(Func<IEnumerable<T>> getter,
                Action<IEnumerable<T>> setter)
            {
                list = new Lazy<List<T>>(() => new List<T>(getter()));
                this.setter = setter;
            }

            public void Add(T item)
            {
                list.Value.Add(item);
                Save();
            }

            public void Clear()
            {
                list.Value.Clear();
                Save();
            }

            public bool Contains(T item)
            {
                return list.Value.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                list.Value.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return list.Value.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool Remove(T item)
            {
                if (!list.Value.Remove(item))
                {
                    return false;
                }

                Save();
                return true;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return list.Value.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return list.Value.GetEnumerator();
            }

            public void ReplaceAll(IEnumerable<T> newValues)
            {
                Ensure.ArgumentNotNull(newValues, "newValues");
                list.Value.Clear();
                list.Value.AddRange(newValues);
                Save();
            }

            private void Save()
            {
                setter(list.Value);
            }
        }
    }
}
