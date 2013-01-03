﻿using System.IO;
using System.Xml;
using Bari.Core.Generic;
using Bari.Core.Model;
using Bari.Core.Test.Helper;
using Bari.Plugins.Csharp.VisualStudio;
using Bari.Plugins.Csharp.VisualStudio.CsprojSections;
using Moq;
using NUnit.Framework;

namespace Bari.Plugins.Csharp.Test.VisualStudio
{
    [TestFixture]
    public class CsprojGeneratorTest
    {
        [Test]
        public void SectionsAreCalled()
        {
            var secA = new Mock<ICsprojSection>();
            var secB = new Mock<ICsprojSection>();
            var secC = new Mock<ICsprojSection>();

            var project = new Project("testproject", new Module("testmodule", new Suite(new TestFileSystemDirectory("test"))));
            var refs = new[] {new TargetRelativePath("test")};

            using (var output = new StringWriter())
            using (var versionOutput = new StringWriter())
            {
                var generator = new CsprojGenerator(new[] {secA.Object, secB.Object, secC.Object});
                generator.Generate(project, refs, output, versionOutput, "version.cs");

                secA.Verify(s => s.Write(It.IsAny<XmlWriter>(), project, It.IsAny<ICsprojGeneratorContext>()), Times.Once());
                secB.Verify(s => s.Write(It.IsAny<XmlWriter>(), project, It.IsAny<ICsprojGeneratorContext>()), Times.Once());
                secC.Verify(s => s.Write(It.IsAny<XmlWriter>(), project, It.IsAny<ICsprojGeneratorContext>()), Times.Once());
            }
        }
    }
}