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
    using System.Threading.Tasks;
    using Desalt.Core.Pipeline;

    internal delegate Task<IExtendedResult<TOutput>> ExecuteFunc<in TInput, TOutput>(
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

        public FakePipelineStage(SimpleExecuteFunc<TInput, TOutput> simpleExecuteFunc) : this(
            (input, cancellationToken) =>
                Task.FromResult<IExtendedResult<TOutput>>(new ExtendedResult<TOutput>(simpleExecuteFunc(input))))
        { }

        public override async Task<IExtendedResult<TOutput>> ExecuteAsync(
            TInput input,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return await _executeFunc(input, cancellationToken);
        }
    }

    internal class IntToStringStage : PipelineStage<int, string>
    {
        public override Task<IExtendedResult<string>> ExecuteAsync(
            int input,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult<IExtendedResult<string>>(
                new ExtendedResult<string>(input.ToString(CultureInfo.InvariantCulture)));
        }
    }

    internal class StringToCharArrayStage : PipelineStage<string, char[]>
    {
        public override Task<IExtendedResult<char[]>> ExecuteAsync(
            string input,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult<IExtendedResult<char[]>>(new ExtendedResult<char[]>(input.ToCharArray()));
        }
    }

    internal class CharArrayToStringStage : PipelineStage<char[], string>
    {
        public override Task<IExtendedResult<string>> ExecuteAsync(
            char[] input,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult<IExtendedResult<string>>(new ExtendedResult<string>(new string(input)));
        }
    }

    internal class StringToIntStage : PipelineStage<string, int>
    {
        public override Task<IExtendedResult<int>> ExecuteAsync(
            string input,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult<IExtendedResult<int>>(new ExtendedResult<int>(int.Parse(input)));
        }
    }

    internal class StringToDoubleStage : PipelineStage<string, double>
    {
        public override Task<IExtendedResult<double>> ExecuteAsync(
            string input,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult<IExtendedResult<double>>(new ExtendedResult<double>(double.Parse(input)));
        }
    }
}
