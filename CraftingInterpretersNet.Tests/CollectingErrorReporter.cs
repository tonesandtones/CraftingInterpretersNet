using CraftingInterpretersNet.Abstractions;

namespace CraftingInterpretersNet.Tests;

public class CollectingErrorReporter : BaseErrorReporter
{
    private readonly List<Event> _receivedErrors = new();

    public record Event(int Line, string Where, string Message);

    public IEnumerable<Event> ReceivedErrors => _receivedErrors;

    protected override void Report(int line, string where, string message)
    {
        _receivedErrors.Add(new Event(line, where, message));
    }
}