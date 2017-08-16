// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="FakePipelineStages.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Pipeline
{
    using System.Globalization;
    using System.Threading;
    using Desalt.Core.Pipeline;

    internal delegate IExtendedResult<TOutput> ExecuteFunc<in TInput, out TOutput>(
        TInput input,
        CancellationToken cancellationToken = default(CancellationToken));

    internal delegate TOutput SimpleExecuteFunc<in TInput, out TOutput>(TInput input);

    internal class FakePipelineStage<TInput, TOutput> : PipelineStage<TInput, TOutput>
    {
        private readonly ExecuteFunc<TInput, TOutput> _executeFunc;

        public FakePipelineStage(ExecuteFunc<TInput, TOutput> executeFunc)
        {
            _executeFunc = executeFunc;
        }

        public FakePipelineStage(SimpleExecuteFunc<TInput, TOutput> simpleExecuteFunc)
            : this((input, cancellationToken) => new ExtendedResult<TOutput>(simpleExecuteFunc(input)))
        {
        }

        public override IExtendedResult<TOutput> Execute(
            TInput input,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _executeFunc(input, cancellationToken);
        }
    }

    internal class IntToStringStage : PipelineStage<int, string>
    {
        public override IExtendedResult<string> Execute(
            int input,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return new ExtendedResult<string>(input.ToString(CultureInfo.InvariantCulture));
        }
    }

    internal class StringToCharArrayStage : PipelineStage<string, char[]>
    {
        public override IExtendedResult<char[]> Execute(
            string input,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return new ExtendedResult<char[]>(input.ToCharArray());
        }
    }

    internal class CharArrayToStringStage : PipelineStage<char[], string>
    {
        public override IExtendedResult<string> Execute(
            char[] input,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return new ExtendedResult<string>(new string(input));
        }
    }

    internal class StringToIntStage : PipelineStage<string, int>
    {
        public override IExtendedResult<int> Execute(
            string input,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return new ExtendedResult<int>(int.Parse(input));
        }
    }

    internal class StringToDoubleStage : PipelineStage<string, double>
    {
        public override IExtendedResult<double> Execute(
            string input,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return new ExtendedResult<double>(double.Parse(input));
        }
    }
}
