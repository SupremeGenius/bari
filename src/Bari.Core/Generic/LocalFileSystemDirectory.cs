﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bari.Core.Generic
{
    /// <summary>
    /// Default implementation of <see cref="IFileSystemDirectory"/>, using the .NET 
    /// file operations.
    /// </summary>
    public class LocalFileSystemDirectory: IFileSystemDirectory
    {
        private readonly string path;

        /// <summary>
        /// Constructes the directory abstraction
        /// </summary>
        /// <param name="path">Absolute path to the directory</param>
        public LocalFileSystemDirectory(string path)
        {
            this.path = path;
        }

        /// <summary>
        /// Enumerates all the files within the directory by their names
        /// </summary>
        public IEnumerable<string> Files
        {
            get { return Directory.EnumerateFiles(path).Select(Path.GetFileName); }
        }

        /// <summary>
        /// Enumerates all the child directories of the directory by their names
        /// 
        /// <para>Use <see cref="IFileSystemDirectory.GetChildDirectory"/> to get any of these children.</para>
        /// </summary>
        public IEnumerable<string> ChildDirectories
        {
            get { return Directory.EnumerateDirectories(path).Select(Path.GetFileName); }
        }

        /// <summary>
        /// Gets interface for a given child directory
        /// </summary>
        /// <param name="name">Name of the child directory</param>
        /// <returns>Returns either a directory abstraction or <c>null</c> if it does not exists.</returns>
        public IFileSystemDirectory GetChildDirectory(string name)
        {
            string childPath = Path.Combine(path, name);
            if (Directory.Exists(childPath))
                return new LocalFileSystemDirectory(childPath);
            else
                return null;
        }

        /// <summary>
        /// Gets the relative path from this directory to another directory (in any depth)
        ///         /// <para>If the given argument is not a child of this directory, an <see cref="ArgumentException"/>will
        /// be thrown.</para>
        /// </summary>
        /// <param name="childDirectory">The child directory to get path to</param>
        /// <returns>Returns the path</returns>
        public string GetRelativePath(IFileSystemDirectory childDirectory)
        {
            var localChildDirectory = childDirectory as LocalFileSystemDirectory;
            if (localChildDirectory == null)            
                throw new ArgumentException("Only LocalFileSystemDirectory objects are supported", "childDirectory");

            var childPath = localChildDirectory.path;
            if (!childPath.StartsWith(path))
                throw new ArgumentException("The argument is not a child directory of this directory", "childDirectory");

            return childPath.Substring(path.Length).TrimStart('\\');
        }

        /// <summary>
        /// Creates a child directory if it does not exist yet
        /// </summary>
        /// <param name="name">Name of the child directory</param>
        /// <returns>Returns the directory abstraction of the new (or already existing) directory</returns>
        public IFileSystemDirectory CreateDirectory(string name)
        {
            string childPath = Path.Combine(path, name);
            Directory.CreateDirectory(childPath);

            return new LocalFileSystemDirectory(childPath);
        }

        /// <summary>
        /// Creates a new text file with a text writer in this directory
        /// </summary>
        /// <param name="name">Name of the new file</param>
        /// <returns>Returns the text writer to be used to write the contents of the file.</returns>
        public TextWriter CreateTextFile(string name)
        {
            string childPath = Path.Combine(path, name);

            return File.CreateText(childPath);
        }

        /// <summary>
        /// Gets the last modification's date for a given file which lies in this directory subtree
        /// </summary>
        /// <param name="relativePath">The relative path to the file from this directory</param>
        /// <returns>Returns the last modified date.</returns>
        /// <exception cref="ArgumentException">If the file does not exist.</exception>
        public DateTime GetLastModifiedDate(string relativePath)
        {
            string absolutePath = Path.Combine(path, relativePath);
            if (!File.Exists(absolutePath))
                throw new ArgumentException("File does not exists", "relativePath");

            return File.GetLastWriteTimeUtc(absolutePath);
        }
    }
}