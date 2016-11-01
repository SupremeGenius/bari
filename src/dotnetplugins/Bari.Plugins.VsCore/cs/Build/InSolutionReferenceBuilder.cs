﻿using System;
using System.Collections.Generic;
using Bari.Core.Build;
using Bari.Core.Build.Cache;
using Bari.Core.Generic;
using Bari.Core.Model;

namespace Bari.Plugins.VsCore.Build
{
    [ShouldNotCache]
    public class InSolutionReferenceBuilder: ReferenceBuilderBase<InSolutionReferenceBuilder>, IReferenceBuilder, IEquatable<InSolutionReferenceBuilder>
    {
        private readonly Project project;
        private Reference reference;

        public InSolutionReferenceBuilder(Project project)
        {
            this.project = project;
        }

        /// <summary>
        /// Gets an unique identifier which can be used to identify cached results
        /// </summary>
        public override string Uid
        {
            get { return project.Module.Name + "#" + project.Name; }
        }
        
        /// <summary>
        /// Runs this builder
        /// </summary>
        /// <param name="context">Current build context</param>
        /// <returns>Returns a set of generated files, in target relative paths</returns>
        public override ISet<TargetRelativePath> Run(IBuildContext context)
        {
            return new HashSet<TargetRelativePath>(
                new[]
                    {
                        new TargetRelativePath(String.Empty, "SLN!"+Uid), 
                    });
        }
        
        /// <summary>
        /// Gets or sets the reference to be resolved
        /// </summary>
        public override Reference Reference
        {
            get { return reference; }
            set { reference = value; }
        }

        public override BuilderName Name
        {
            get
            {
                return new BuilderName(project, "ref:" + reference.Uri); 
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return string.Format("[sln-ref:{0}]", Uid);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(InSolutionReferenceBuilder other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(reference, other.reference);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((InSolutionReferenceBuilder) obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return (reference != null ? reference.GetHashCode() : 0);
        }

        public static bool operator ==(InSolutionReferenceBuilder left, InSolutionReferenceBuilder right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(InSolutionReferenceBuilder left, InSolutionReferenceBuilder right)
        {
            return !Equals(left, right);
        }
    }
}