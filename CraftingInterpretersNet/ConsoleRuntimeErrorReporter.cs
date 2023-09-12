using System;
using CraftingInterpretersNet.Abstractions;

namespace CraftingInterpretersNet;

public class ConsoleRuntimeErrorReporter: BaseRuntimeErrorReporter
{
    protected override void Report(string message, int line)
    {
        Console.Error.WriteLine($"{message}\n[line {line}]");
    }
}