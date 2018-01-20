// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="SimplePipelineTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Pipeline
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Desalt.Core.Pipeline;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SimplePipelineTests
    {
        [TestMethod]
        public void AddStage_should_throw_on_null_arguments()
        {
            var pipeline = new SimplePipeline<object, object>();
            Action action = () => pipeline.AddStage(null);
            action.ShouldThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("stage");
        }

        [TestMethod]
        public void AddStage_should_throw_if_the_first_stage_does_not_accept_the_right_type()
        {
            var pipeline = new SimplePipeline<string, string>();
            Action action = () => pipeline.AddStage(new IntToStringStage());
            action.ShouldThrowExactly<ArgumentException>().And.Message.Should().Contain(typeof(string).Name);
        }

        [TestMethod]
        public void AddStage_should_throw_if_a_stage_is_added_where_no_previous_stages_output_a_compatible_value()
        {
            var pipeline = new SimplePipeline<int, string>();
            pipeline.AddStage(new IntToStringStage());
            Action action = () => pipeline.AddStage(new CharArrayToStringStage());
            action.ShouldThrowExactly<ArgumentException>().And.ParamName.Should().Be("stage");
        }

        [TestMethod]
        public void AddStage_should_allow_multiple_stages_accepting_the_same_input_type()
        {
            var pipeline = new SimplePipeline<int, char[]>();
            pipeline.AddStage(new IntToStringStage());
            pipeline.AddStage(new StringToDoubleStage());
            pipeline.AddStage(new StringToCharArrayStage());
            pipeline.Stages.Count().Should().Be(3);
        }

        [TestMethod]
        public void AddStage_should_allow_a_stage_to_accept_an_input_that_is_a_superclass_of_a_previous_output()
        {
            var pipeline = new SimplePipeline<int, bool>();
            pipeline.AddStage(new FakePipelineStage<int, StringWriter>(input => new StringWriter()));
            pipeline.AddStage(new FakePipelineStage<TextWriter, bool>(input => true));
            pipeline.Stages.Count().Should().Be(2);
        }

        [TestMethod]
        public void AddStage_should_allow_a_stage_to_accept_an_input_that_is_an_interface_of_a_previous_output()
        {
            var pipeline = new SimplePipeline<int, bool>();
            pipeline.AddStage(
                new FakePipelineStage<int, CaseInsensitiveComparer>(input => new CaseInsensitiveComparer()));
            pipeline.AddStage(new FakePipelineStage<IComparer, bool>(input => true));
            pipeline.Stages.Count().Should().Be(2);
        }

        [TestMethod]
        public async Task Execute_should_throw_if_the_last_stage_does_not_output_a_compatible_type()
        {
            var pipeline = new SimplePipeline<int, DateTime>();
            pipeline.AddStage(new IntToStringStage());
            try { await pipeline.ExecuteAsync(123); }
            catch (InvalidOperationException e)
            {
                e.Message.Should().Be("The last stage outputs type 'String' but it should output type 'DateTime'.");
            }
            catch (Exception e) { Assert.Fail($"Expecting InvalidOperationException but got {e.GetType()}"); }
        }

        [TestMethod]
        public async Task Execute_should_run_the_stages_in_order()
        {
            var pipeline = new SimplePipeline<int, char[]>();
            pipeline.AddStage(new IntToStringStage());
            pipeline.AddStage(new StringToCharArrayStage());

            IExtendedResult<char[]> result = await pipeline.ExecuteAsync(123);
            result.Result.Should().Equal('1', '2', '3');
        }

        [TestMethod]
        public async Task Execute_should_preserve_the_outputs_between_stages()
        {
            var pipeline = new SimplePipeline<string, string>();
            pipeline.AddStage(new FakePipelineStage<string, string>(input => input));
            pipeline.AddStage(new StringToCharArrayStage());
            pipeline.AddStage(new FakePipelineStage<string, string>(input => input));

            const string inputString = "Input";
            IExtendedResult<string> result = await pipeline.ExecuteAsync(inputString);
            result.Result.Should().BeSameAs(inputString);
        }

        [TestMethod]
        public async Task Execute_should_stop_on_the_first_stage_that_has_a_failure()
        {
            var pipeline = new SimplePipeline<int, string>();
            pipeline.AddStage(
                new FakePipelineStage<int, string>(
                    (input, token) => Task.FromResult<IExtendedResult<string>>(
                        new ExtendedResult<string>("output", new[] { DiagnosticMessage.Info("First") }))));
            pipeline.AddStage(
                new FakePipelineStage<string, string>(
                    (input, token) => Task.FromResult<IExtendedResult<string>>(
                        new ExtendedResult<string>(
                            "Failed",
                            new[] { DiagnosticMessage.FromException(new Exception("Failed Message")) }))));
            pipeline.AddStage(
                new FakePipelineStage<string, string>(
                    (input, token) => Task.FromResult<IExtendedResult<string>>(
                        new ExtendedResult<string>("Succeeded", new[] { DiagnosticMessage.Info("Third") }))));

            IExtendedResult<string> result = await pipeline.ExecuteAsync(123);
            result.Success.Should().BeFalse();
            result.Messages.Select(m => m.Message).Should().Equal("First", "Failed Message");
        }
    }
}
