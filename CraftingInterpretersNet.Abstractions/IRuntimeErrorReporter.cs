namespace CraftingInterpretersNet.Abstractions;

public interface IRuntimeErrorReporter
{
    public bool HasRuntimeError { get; }
    void Error(RuntimeError error);
    void ClearErrorState();
}