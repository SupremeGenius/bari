﻿using System;
using System.Collections.Generic;
using Bari.Core.Build.Cache;
using Bari.Core.Build.Dependencies;
using Bari.Core.Generic;
using Bari.Core.Model;

namespace Bari.Core.Build
{
    /// <summary>
    /// Copies the contents of a given project's <c>content</c> source set to the target directory
    /// </summary>
    [ShouldNotCache]
    [AggressiveCacheRestore]
    public class ContentBuilder: BuilderBase<ContentBuilder>, IEquatable<ContentBuilder>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof (ContentBuilder));

        private readonly Project project;
        private readonly IFileSystemDirectory suiteRoot;
        private readonly IFileSystemDirectory targetRoot;
        private readonly SourceSetDependencies dependencies;

        public ContentBuilder(Project project, ISourceSetFingerprintFactory fingerprintFactory, [SuiteRoot] IFileSystemDirectory suiteRoot, [TargetRoot] IFileSystemDirectory targetRoot)
        {
            this.project = project;
            this.suiteRoot = suiteRoot;
            this.targetRoot = targetRoot;
            dependencies = new SourceSetDependencies(fingerprintFactory, project.GetSourceSet("content"));
        }

        /// <summary>
        /// Dependencies required for running this builder
        /// </summary>
        public override IDependencies Dependencies
        {
            get
            {
                return dependencies;
            }
        }

        /// <summary>
        /// Gets an unique identifier which can be used to identify cached results
        /// </summary>
        public override string Uid
        {
            get { return project.Module + "." + project.Name; }
        }

        /// <summary>
        /// Runs this builder
        /// </summary>
        /// <param name="context">Current build context</param>
        /// <returns>Returns a set of generated files, in target relative paths</returns>
        public override ISet<TargetRelativePath> Run(IBuildContext context)
        {
            var contents = project.GetSourceSet("content");
            var contentsDir = project.RootDirectory.GetChildDirectory("content");

            var targetDir = targetRoot.GetChildDirectory(project.RelativeTargetPath, createIfMissing: true);
            var result = new HashSet<TargetRelativePath>();
            foreach (var sourcePath in contents.Files)
            {
                log.DebugFormat("Copying content {0}...", sourcePath);

                var relativePath = suiteRoot.GetRelativePathFrom(contentsDir, sourcePath);

                suiteRoot.CopyFile(sourcePath, targetDir, relativePath);

                result.Add(new TargetRelativePath(project.RelativeTargetPath, relativePath));
            }

            return result;
        }
        
        public override BuilderName Name
        {
            get
            {
                return new BuilderName(project, "content"); 
            }
        }

        public override string ToString()
        {
            return String.Format("[{0}.{1}/content]", project.Module.Name, project.Name);
        }

        public bool Equals(ContentBuilder other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(project, other.project);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ContentBuilder) obj);
        }

        public override int GetHashCode()
        {
            return (project != null ? project.GetHashCode() : 0);
        }

        public static bool operator ==(ContentBuilder left, ContentBuilder right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ContentBuilder left, ContentBuilder right)
        {
            return !Equals(left, right);
        }
    }
}