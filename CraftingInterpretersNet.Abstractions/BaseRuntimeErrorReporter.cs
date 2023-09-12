namespace CraftingInterpretersNet.Abstractions;

public abstract class BaseRuntimeErrorReporter: IRuntimeErrorReporter
{
    public virtual bool HasRuntimeError { get; protected set; }
    public void Error(RuntimeError error)
    {
        HasRuntimeError = true;
        Report(error.Message, error.Token.Line);
    }

    public virtual void ClearErrorState()
    {
        HasRuntimeError = false;
    }

    protected abstract void Report(string message, int line);
}