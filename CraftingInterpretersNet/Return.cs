using System;

namespace CraftingInterpretersNet;

/// <summary>
/// Let's abuse exception handling to do control flow management in a deeply recursive interpreter 😀
/// </summary>
internal class Return : Exception
{
    public object? Value { get; }

    public Return(object? value)
    {
        Value = value;
    }
}