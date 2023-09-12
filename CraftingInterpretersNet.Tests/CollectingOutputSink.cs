using CraftingInterpretersNet.Abstractions;

namespace CraftingInterpretersNet.Tests;

public class CollectingOutputSink: IOutputSink
{
    private readonly List<string?> _messages = new();
    public IEnumerable<string?> Messages => _messages;

    public void Print(string? value)
    {
        _messages.Add(value);
    }
}