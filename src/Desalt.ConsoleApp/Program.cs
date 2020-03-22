// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.ConsoleApp
{
    using System;
    using System.Text;
    using System.Threading.Tasks;

    internal class Program
    {
        private static async Task<int> Main(string[] args)
        {
            // by default, .NET Core doesn't have all code pages needed for Console apps.
            // see the .NET Core Notes in https://msdn.microsoft.com/en-us/library/system.diagnostics.process(v=vs.110).aspx
            // https://github.com/dotnet/roslyn/issues/10785
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Console.OutputEncoding = Encoding.Unicode;

            return await CliApp.RunAsync(args);
        }
    }
}
