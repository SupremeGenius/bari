﻿using System;
using System.Collections.Generic;
using System.Linq;
using Bari.Core.Build;
using Bari.Core.Model;
using Ninject;
using Ninject.Syntax;

namespace Bari.Core.Commands
{
    /// <summary>
    /// Implements 'build' command, which runs one or more builder (<see cref="IBuilder"/>) for a <see cref="Project"/>,
    /// <see cref="Module"/> or product.
    /// </summary>
    public class BuildCommand: ICommand
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof (BuildCommand));

        private readonly IResolutionRoot root;
        private readonly IEnumerable<IProjectBuilderFactory> projectBuilders;

        /// <summary>
        /// Gets the name of the command. This is the string which can be used on the command line interface
        /// to access the particular command.
        /// </summary>
        public string Name
        {
            get { return "build"; }
        }

        /// <summary>
        /// Gets a short, one-liner description of the command
        /// </summary>
        public string Description
        {
            get { return "builds a project, module or product"; }
        }

        /// <summary>
        /// Gets a detailed, multiline description of the command and its possible parameters, usage examples
        /// </summary>
        public string Help
        {
            get { return
@"=Build command=

When used without parameter, it builds every module in the suite. 
Example: `bari build`

When used with a module name, it builds the specified module togeher with every required dependency of it.
Example: `bari build HelloWorldModule`

When used with a project name prefixed by its module, it builds the specified project only, together with every required dependency of it.
Example: `bari build HelloWorldModule.HelloWorld`
"; }
        }

        /// <summary>
        /// Constructs the build command
        /// </summary>
        /// <param name="root">Interface for creating new objects</param>
        /// <param name="projectBuilders">The set of registered project builder factories</param>
        public BuildCommand(IResolutionRoot root, IEnumerable<IProjectBuilderFactory> projectBuilders)
        {
            this.root = root;
            this.projectBuilders = projectBuilders;
        }

        /// <summary>
        /// Runs the command
        /// </summary>
        /// <param name="suite">The current suite model the command is applied to</param>
        /// <param name="parameters">Parameters given to the command (in unprocessed form)</param>
        public void Run(Suite suite, string[] parameters)
        {
            if (parameters.Length == 0)
            {
                var projects = (from module in suite.Modules
                                from project in module.Projects
                                select project).ToList();

                var context = root.Get<IBuildContext>();
                foreach (var projectBuilder in projectBuilders)
                {
                    projectBuilder.AddToContext(context, projects);
                }

                var outputs = context.Run();

                foreach (var outputPath in outputs)
                    log.InfoFormat("Generated output for build: {0}", outputPath);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}