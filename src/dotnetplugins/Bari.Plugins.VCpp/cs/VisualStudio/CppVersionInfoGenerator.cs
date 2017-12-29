﻿using System.IO;
using Bari.Core.Model;

namespace Bari.Plugins.VCpp.VisualStudio
{
    /// <summary>
    /// Generates a Cpp file containing <c>AssemblyVersion</c> and <c>AssemblyFileVersion</c> attributes,
    /// coming from the <see cref="Project"/>.
    /// </summary>
    internal class CppVersionInfoGenerator
    {
        private readonly Project project;

        /// <summary>
        /// Initializes the version info generator
        /// </summary>
        /// <param name="project">Project to generate version info for</param>
        internal CppVersionInfoGenerator(Project project)
        {
            this.project = project;
        }

        /// <summary>
        /// Generates the Cpp code to the given output
        /// </summary>
        /// <param name="output">Output text writer to be used</param>
        public void Generate(TextWriter output)
        {
            output.WriteLine("using namespace System::Reflection;");
            output.WriteLine();
            output.WriteLine("// Version info file generated by bari for project {0}", project.Name);
            if (!string.IsNullOrWhiteSpace(project.EffectiveVersion))
            {
                output.WriteLine("[assembly: AssemblyVersionAttribute(\"{0}\")]", project.EffectiveVersion);
                output.WriteLine("[assembly: AssemblyFileVersion(\"{0}\")]", project.EffectiveVersion);
            }

            if (!string.IsNullOrWhiteSpace(project.EffectiveCopyright))
            {
                output.WriteLine("[assembly: AssemblyCopyright(\"{0}\")];", project.EffectiveCopyright);
            }
        }
    }
}
