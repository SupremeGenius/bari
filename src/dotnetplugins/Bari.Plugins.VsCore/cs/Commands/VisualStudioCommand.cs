﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Bari.Core.Build;
using Bari.Core.Build.MergingTag;
using Bari.Core.Commands;
using Bari.Core.Commands.Helper;
using Bari.Core.Exceptions;
using Bari.Core.Generic;
using Bari.Core.Model;
using Bari.Plugins.VsCore.Build;

namespace Bari.Plugins.VsCore.Commands
{
    /// <summary>
    /// Implements the 'vs' command, which generates visual studio solution and project
    /// files for a given module or product, and launches Microsoft Visual Studio loading the generated
    /// solution.
    /// </summary>
    public class VisualStudioCommand : ICommand, IHasBuildTarget
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(VisualStudioCommand));

        private readonly IBuildContextFactory buildContextFactory;
        private readonly ICoreBuilderFactory coreBuilderFactory;
        private readonly IFileSystemDirectory targetDir;
        private readonly ICommandTargetParser targetParser;
        private readonly IEnumerable<IProjectBuilderFactory> projectBuilders;
        private string lastTargetStr;

        /// <summary>
        /// Gets the name of the command. This is the string which can be used on the command line interface
        /// to access the particular command.
        /// </summary>
        public string Name
        {
            get { return "vs"; }
        }

        /// <summary>
        /// Gets a short, one-liner description of the command
        /// </summary>
        public string Description
        {
            get { return "generates visual studio solution and projects for a module"; }
        }

        /// <summary>
        /// Gets a detailed, multiline description of the command and its possible parameters, usage examples
        /// </summary>
        public string Help
        {
            get
            {
                return
@"= VisualStudio command=

Generates visual studio solution and project files for the given module or product, 
and launches Visual Studio to open them.

Example: `bari vs HelloWorld`

Optionally bari can start and load the generated solution into Visual Studio immediately:

Example: `bari vs --open HelloWorld`

If called without any module or product name, it adds *every module* to the generated solution.
";
            }
        }

        /// <summary>
        /// If <c>true</c>, the target goal is important for this command and must be explicitly specified by the user 
        /// (if the available goal set is not the default)
        /// </summary>
        public bool NeedsExplicitTargetGoal
        {
            get { return true; }
        }

        /// <summary>
        /// Initializes the command
        /// </summary>
        /// <param name="buildContextFactory">Interface to create new build contexts</param>
        /// <param name="targetDir">Target root directory</param>
        /// <param name="targetParser">Parser used for parsing the target parameter</param>
        public VisualStudioCommand(IBuildContextFactory buildContextFactory, [TargetRoot] IFileSystemDirectory targetDir, ICommandTargetParser targetParser, ICoreBuilderFactory coreBuilderFactory, IEnumerable<IProjectBuilderFactory> projectBuilders)
        {
            this.buildContextFactory = buildContextFactory;
            this.targetDir = targetDir;
            this.targetParser = targetParser;
            this.coreBuilderFactory = coreBuilderFactory;
            this.projectBuilders = projectBuilders;
        }

        /// <summary>
        /// Runs the command
        /// </summary>
        /// <param name="suite">The current suite model the command is applied to</param>
        /// <param name="parameters">Parameters given to the command (in unprocessed form)</param>
        public bool Run(Suite suite, string[] parameters)
        {
            bool openSolution = false;
            string targetStr;

            if (parameters.Length == 0)
                targetStr = String.Empty;
            else if (parameters.Length < 3)
            {
                if (parameters[0] == "--open")
                    openSolution = true;

                targetStr = parameters.Last();
            }
            else
            {
                throw new InvalidCommandParameterException("vs", "Must be called with zero, one or two parameters");
            }

            try
            {
                lastTargetStr = targetStr;
                var target = targetParser.ParseTarget(targetStr);

                Run(suite, target, openSolution);

                return true;
            }
            catch (ArgumentException ex)
            {
                throw new InvalidCommandParameterException("vs", ex.Message);
            }
        }

        private void Run(Suite suite, CommandTarget target, bool openSolution)
        {
            Run(suite, target.Projects.Concat(target.TestProjects), openSolution);
        }

        private void Run(Suite suite, IEnumerable<Project> projects, bool openSolution)
        {
            var prjs = projects.ToArray();
            var context = buildContextFactory.CreateBuildContext(suite);

            // We have to emulate a real build at this point to make sure all the 
            // graph transformations are executed correctly. 
            // Then from the transformed graph we find the first sln builder and 
            // run it.

            IBuilder rootBuilder = coreBuilderFactory.Merge(
                projectBuilders
                    .OfType<VsProjectBuilderFactory>()
                    .Select(pb => pb.Create(prjs))
                    .Where(b => b != null).ToArray(),
                new ProjectBuilderTag("Top level project builders", prjs));

            if (rootBuilder != null)
            {
                context.AddBuilder(rootBuilder);
                var slnBuilder = context.Builders.OfType<SlnBuilder>().FirstOrDefault(
                    builder => new HashSet<Project>(builder.Projects).IsSubsetOf(prjs));
                
                if (slnBuilder == null)
                    throw new InvalidOperationException("Could not find a suitable SLN builder");

                var untilSlnBuilder = new MinimalBuilderFilter(slnBuilder);
                context.Run(rootBuilder, untilSlnBuilder.Filter);

                if (openSolution)
                {
                    var slnRelativePath = context.GetResults(slnBuilder).FirstOrDefault();
                    if (slnRelativePath != null)
                    {
                        log.InfoFormat("Opening {0} with Visual Studio...", slnRelativePath);

                        var localTargetDir = targetDir as LocalFileSystemDirectory;
                        if (localTargetDir != null)
                        {
                            Process.Start(Path.Combine(localTargetDir.AbsolutePath, slnRelativePath));
                        }
                        else
                        {
                            log.Warn("The --open command only works with local target directory!");
                        }
                    }
                }
            }
        }

        private class MinimalBuilderFilter
        {
            private readonly IBuilder target;
            private bool ran;

            public MinimalBuilderFilter(IBuilder target)
            {
                this.target = target;
            }

            public bool Filter(IBuilder builder)
            {
                if (ran)
                {
                    return false;
                }
                else
                {
                    if (builder == target)
                        ran = true;

                    return true;
                }

            }
        }

        public string BuildTarget
        {
            get { return lastTargetStr; }
        }
    }
}