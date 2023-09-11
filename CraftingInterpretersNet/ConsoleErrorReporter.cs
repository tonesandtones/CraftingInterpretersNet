using System;
using CraftingInterpretersNet.Abstractions;

namespace CraftingInterpretersNet;

public class ConsoleErrorReporter : BaseErrorReporter
{
    protected override void Report(int line, string where, string message)
    {
        {
            var outMessage = $"[line {line}] Error{where}: {message}";
            Console.WriteLine(outMessage);
            HasReceivedError = true;
        }
    }
}