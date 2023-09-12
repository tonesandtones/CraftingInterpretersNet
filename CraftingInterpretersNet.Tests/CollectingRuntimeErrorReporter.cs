using CraftingInterpretersNet.Abstractions;

namespace CraftingInterpretersNet.Tests;

public class CollectingRuntimeErrorReporter : BaseRuntimeErrorReporter
{
    public record ErrorEvent(string Message, int Line);

    private readonly List<ErrorEvent> _receivedErrors = new();

    public IEnumerable<ErrorEvent> ReceivedErrors => _receivedErrors;
    protected override void Report(string message, int line)
    {
        _receivedErrors.Add(new ErrorEvent(message, line));
    }
}