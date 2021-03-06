﻿using Simula.Scripting.Git.Core;

namespace Simula.Scripting.Git
{
    /// <summary>
    /// Exposes low level Git object metadata
    /// </summary>
    public sealed class GitObjectMetadata
    {
        private readonly GitObjectType type;

        /// <summary>
        /// Size of the Object
        /// </summary>
        public long Size { get; private set; }

        /// <summary>
        /// Object Type
        /// </summary>
        public ObjectType Type
        {
            get
            {
                return type.ToObjectType();
            }
        }

        internal GitObjectMetadata(long size, GitObjectType type)
        {
            this.Size = size;
            this.type = type;
        }
    }
}
