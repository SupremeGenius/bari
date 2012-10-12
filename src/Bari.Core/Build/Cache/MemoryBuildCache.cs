﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Bari.Core.Generic;

namespace Bari.Core.Build.Cache
{
    /// <summary>
    /// Simple build cache which only stores build outputs until the process is running
    /// </summary>
    public class MemoryBuildCache : IBuildCache
    {
        private readonly IDictionary<Type, MemoryCacheItem> cache = new Dictionary<Type, MemoryCacheItem>();

        /// <summary>
        /// Locks the cache for a given builder. 
        /// 
        /// <para>Until calling <see cref="IBuildCache.UnlockForBuilder"/>, it is guaranteed that no
        /// <see cref="IBuildCache.Store"/> operation will be ran for the given builder from other
        /// threads.</para>
        /// </summary>
        /// <param name="builder">Builder type</param>
        public void LockForBuilder(Type builder)
        {
            GetOrCreate(builder).EnterUpgradeableLock();
        }

        /// <summary>
        /// Removes the lock put by the <see cref="IBuildCache.LockForBuilder"/> method.
        /// </summary>
        /// <param name="builder">Builder type</param>
        public void UnlockForBuilder(Type builder)
        {
            GetOrCreate(builder).ExitUpgradeableLock();
        }

        /// <summary>
        /// Store build outputs in the cache by reading them from the file system
        /// </summary>
        /// <param name="builder">Builder type (first part of the key)</param>
        /// <param name="fingerprint">Dependency fingerprint created when the builder was executed (second part of the key)</param>
        /// <param name="outputs">Target-relative path of the build outputs to be cached</param>
        /// <param name="targetRoot">File system abstraction of the root target directory</param>
        public void Store(Type builder, IDependencyFingerprint fingerprint, IEnumerable<TargetRelativePath> outputs, IFileSystemDirectory targetRoot)
        {
            MemoryCacheItem item = GetOrCreate(builder);            

            var map = new Dictionary<TargetRelativePath, byte[]>();
            // TODO: load files
            item.Update(fingerprint, map);
        }

        /// <summary>
        /// Checks if the cache contains stored outputs for a given builder with a given dependency fingerprint
        /// 
        /// <para>If <see cref="IBuildCache.Restore"/> will be also called, the cache must be locked first using
        /// the <see cref="IBuildCache.LockForBuilder"/> method.</para>
        /// </summary>
        /// <param name="builder">Builder type</param>
        /// <param name="fingerprint">Current dependency fingerprint</param>
        /// <returns>Returns <c>true</c> if there are stored outputs for the given builder and fingerprint combination.</returns>
        public bool Contains(Type builder, IDependencyFingerprint fingerprint)
        {
            lock (cache)
            {
                MemoryCacheItem item;
                if (cache.TryGetValue(builder, out item))
                {
                    return item.MatchesFingerprint(fingerprint);
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Restores the stored files for a given builder to a file system directory
        /// 
        /// <para>The cache only stores the latest stored results and this is what will be restored
        /// to the target directory. To verify if it was generated with the correct dependency fingerprint,
        /// use <see cref="IBuildCache.Contains"/>.</para>
        /// <para>To ensure thread safety, use <see cref="IBuildCache.LockForBuilder"/>.</para>
        /// </summary>
        /// <param name="builder">Builder type</param>
        /// <param name="targetRoot">Target file system directory</param>
        /// <returns>Returns the target root relative paths of all the restored files</returns>
        public ISet<TargetRelativePath> Restore(Type builder, IFileSystemDirectory targetRoot)
        {
            MemoryCacheItem item;
            lock (cache)
                cache.TryGetValue(builder, out item);

            if (item != null)
            {                
                var outputs = item.Outputs;
                var paths = new HashSet<TargetRelativePath>();
                foreach (var pair in outputs)
                {                    
                    // TODO: save data

                    paths.Add(pair.Key);
                }

                return paths;
            }
            else
            {
                return new HashSet<TargetRelativePath>();
            }
        }

        private MemoryCacheItem GetOrCreate(Type builder)
        {
            Contract.Requires(builder != null);
            Contract.Ensures(Contract.Result<MemoryCacheItem>() != null);

            lock (cache)
            {
                MemoryCacheItem item;
                if (!cache.TryGetValue(builder, out item))
                {
                    item = new MemoryCacheItem();
                    cache.Add(builder, item);
                }
                return item;
            }
        }
    }
}