﻿using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Bari.Core.Generic;
using System;

namespace Bari.Core.Build
{
    /// <summary>
    /// Builder represents a set of the build process where a given set of dependencies
    /// is used to create a set of outputs.
    /// </summary>
    [ContractClass(typeof(IBuilderContracts))]
    public interface IBuilder
    {
        /// <summary>
        /// Dependencies required for running this builder
        /// </summary>
        IDependencies Dependencies { get; }

        /// <summary>
        /// Get the builders to be executed before this builder
        /// </summary>
        IEnumerable<IBuilder> Prerequisites { get; }

        /// <summary>
        /// Gets an unique identifier which can be used to identify cached results
        /// </summary>
        string Uid { get; }
        
        /// <summary>
        /// Runs this builder
        /// </summary>
        /// <param name="context">Current build context</param>
        /// <returns>Returns a set of generated files, in target relative paths</returns>
        ISet<TargetRelativePath> Run(IBuildContext context);

        /// <summary>
        /// Verifies if the builder is able to run. Can be used to fallback to cached results without getting en error.
        /// </summary>
        /// <returns>If <c>true</c>, the builder thinks it can run.</returns>
        bool CanRun();

        /// <summary>
        /// Gets the type of the builder, without any decorators.
        /// </summary>
        /// <value>The type of the builder.</value>
        Type BuilderType { get; }

        void AddPrerequisite(IBuilder target);
        void RemovePrerequisite(IBuilder target);
        
        /// <summary>
        /// Gets the builder's name which can be used to reference the builder
        /// in human-written configuration.
        /// </summary>
        BuilderName Name { get; }
    }

    /// <summary>
    /// Contracts for <see cref="IBuilder"/> interface
    /// </summary>
    [ContractClassFor(typeof(IBuilder))]
    abstract class IBuilderContracts : IBuilder
    {
        /// <summary>
        /// Dependencies required for running this builder
        /// </summary>
        public IDependencies Dependencies
        {
            get
            {
                Contract.Ensures(Contract.Result<IDependencies>() != null);
                return null; // dummy value
            }
        }

        public IEnumerable<IBuilder> Prerequisites
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<IBuilder>>() != null);
                return null; // dummy value
            }
        }

        public string Uid
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return null; // dummy value 
            }
        }

        /// <summary>
        /// Runs this builder
        /// </summary>
        /// <param name="context"> </param>
        /// <returns>Returns a set of generated files, in target relative paths</returns>
        public ISet<TargetRelativePath> Run(IBuildContext context)
        {
            Contract.Requires(context != null);
            Contract.Ensures(Contract.Result<ISet<TargetRelativePath>>() != null);
            return null; // dummy value
        }

        /// <summary>
        /// Verifies if the builder is able to run. Can be used to fallback to cached results without getting en error.
        /// </summary>
        /// <returns>If <c>true</c>, the builder thinks it can run.</returns>
        public abstract bool CanRun();

        public Type BuilderType
        {
            get
            {
                Contract.Ensures(Contract.Result<Type>() != null);
                return null; // dummy
            }
        }

        public void AddPrerequisite(IBuilder target)
        {
            Contract.Requires(target != null);
        }

        public void RemovePrerequisite(IBuilder target)
        {
            Contract.Requires(target != null);
        }
        
        public BuilderName Name 
        { 
            get
            {
                Contract.Ensures(Contract.Result<BuilderName>() != null);
                return null; // dummy
            }
        }
    }
}