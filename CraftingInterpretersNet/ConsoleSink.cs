using System;
using CraftingInterpretersNet.Abstractions;

namespace CraftingInterpretersNet;

public class ConsoleSink : IOutputSink
{
    public void Print(string? value)
    {
        Console.WriteLine(value);
    }
}