namespace CraftingInterpretersNet.Abstractions;

public abstract class BaseErrorReporter : IErrorReporter
{
    public bool HasReceivedError { get; protected set; }

    public virtual void Error(int line, string message)
    {
        Report(line, "", message);
    }

    public virtual void Error(Token token, string message)
    {
        if (token.Type == TokenType.EOF)
        {
            Report(token.Line, " at end", message);
        }
        else
        {
            Report(token.Line, " at '" + token.Lexeme + "'", message);
        }
    }

    protected abstract void Report(int line, string where, string message);


    public void ClearErrorState()
    {
        HasReceivedError = false;
    }
}