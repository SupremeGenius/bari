﻿using Bari.Core.Generic;

namespace Bari.Core.Model
{
    /// <summary>
    /// TestProject is a special <see cref="Project"/>, containing unit tests and not used in regular builds
    /// </summary>
    public class TestProject: Project
    {
        public override IFileSystemDirectory RootDirectory
        {
            get { return Module.RootDirectory.GetChildDirectory("tests").GetChildDirectory(Name); }
        }

        /// <summary>
        /// Constructs the test project
        /// </summary>
        /// <param name="name">Name of the project</param>
        /// <param name="module">Module where he project belongs to</param>
        public TestProject(string name, Module module) : base(name, module)
        {
        }
    }
}