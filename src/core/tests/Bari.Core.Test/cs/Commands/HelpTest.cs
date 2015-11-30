﻿using Bari.Core.Commands;
using Bari.Core.Exceptions;
using Bari.Core.Generic;
using Bari.Core.Model;
using Bari.Core.Test.Helper;
using Bari.Core.UI;
using FluentAssertions;
using NUnit.Framework;
using Ninject;
using System;

namespace Bari.Core.Test.Commands
{
    [TestFixture]
    public class HelpTest
    {
        private IKernel kernel;
        private TestUserOutput output;

        [SetUp]
        public void SetUp()
        {
            kernel = new StandardKernel();            
            output = new TestUserOutput();
            kernel.Bind<IUserOutput>().ToConstant(output).InSingletonScope();
            kernel.Bind<IFileSystemDirectory>().ToConstant(new TestFileSystemDirectory("root")).WhenTargetHas
                <SuiteRootAttribute>();
            kernel.Bind<IFileSystemDirectory>().ToConstant(new TestFileSystemDirectory("target")).WhenTargetHas
                <TargetRootAttribute>();
            kernel.Bind<Lazy<IFileSystemDirectory>>().ToConstant(
                new Lazy<IFileSystemDirectory>(() => new TestFileSystemDirectory("cache"))).WhenTargetHas
                <CacheRootAttribute>();

            Kernel.RegisterCoreBindings(kernel);
        }

        [TearDown]
        public void TearDown()
        {
            kernel.Dispose();
        }
    
        [Test]
        public void Exists()
        {
            var cmd = kernel.Get<ICommand>("help");
            cmd.Should().NotBeNull();
            cmd.Name.Should().Be("help");
        }

        [Test]
        public void HasHelpAndDescription()
        {
            var cmd = kernel.Get<ICommand>("help");
            cmd.Description.Should().NotBeNullOrWhiteSpace();
            cmd.Help.Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        public void HelpCalledWithMoreThanOneParametersThrowException()
        {
            var cmd = kernel.Get<ICommand>("help");
            Assert.That(cmd.Run(kernel.Get<Suite>(), new[] {"test1", "test2"}), Throws.TypeOf<InvalidCommandParameterException>());
        }

        [Test]
        public void UnknownCommandNameInParameterThrowsException()
        {
            var cmd = kernel.Get<ICommand>("help");
            Assert.That(cmd.Run(kernel.Get<Suite>(), new[] { "non-existing-command" }), Throws.TypeOf<InvalidCommandParameterException>());
        }

        [Test]
        public void CalledWithoutArgumentPrintsDescriptions()
        {
            var cmd = kernel.Get<ICommand>("help");
            cmd.Run(kernel.Get<Suite>(), new string[0]);

            output.Messages.Should().NotBeEmpty();
            output.Descriptions.Should().NotBeEmpty();
            output.Descriptions.Should().Contain(t => t.Item1 == "help");
        }

        [Test]
        public void CalledWithOneArgumentPrintsHelpString()
        {
            var cmd = kernel.Get<ICommand>("help");
            cmd.Run(kernel.Get<Suite>(), new[] {"help"});

            string.Join("\n", output.Messages).Should().StartWith(cmd.Help);
        }

        [Test]
        public void SupportsAnyRegisteredCommand()
        {
            var cmd = kernel.Get<ICommand>("help");

            var dummy = new DummyCommand
                            {
                                Name = "dummy",
                                Description = "dummy description",
                                Help = "dummy help"
                            };
            kernel.Bind<ICommand>().ToConstant(dummy).Named("dummy");

            cmd.Run(kernel.Get<Suite>(), new string[0]);
            output.Messages.Should().NotBeEmpty();
            output.Descriptions.Should().NotBeEmpty();
            output.Descriptions.Should().Contain(t => t.Item1 == "dummy" && t.Item2 == "dummy description");

            output.Reset();
            cmd.Run(kernel.Get<Suite>(), new[] { "dummy" });

            output.Messages.Should().HaveCount(2);
            output.Messages.Should().HaveElementAt(0, "dummy help");
            output.Descriptions.Should().BeEmpty();
        }
    }
}