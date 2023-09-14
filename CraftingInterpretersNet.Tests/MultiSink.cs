using CraftingInterpretersNet.Abstractions;

namespace CraftingInterpretersNet.Tests;

public class MultiSink : IOutputSink 
{
    private readonly IOutputSink[] _sinks;

    public MultiSink(params IOutputSink[] sinks)
    {
        _sinks = sinks;
    }

    public void Print(string? value)
    {
        foreach (var sink in _sinks)
        {
            sink.Print(value);
        }
    }
}