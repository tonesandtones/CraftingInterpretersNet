namespace CraftingInterpretersNet.Abstractions;

public interface IErrorReporter
{
    public bool HasReceivedError { get; }
    void Error(int line, string message);
    void Error(Token token, string message);
    void ClearErrorState();
}