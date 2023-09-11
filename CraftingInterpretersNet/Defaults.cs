using CraftingInterpretersNet.Abstractions;

namespace CraftingInterpretersNet;

public static class Defaults
{
    public static IErrorReporter DefaultErrorReporter => new ConsoleErrorReporter();
}