using CraftingInterpretersNet.Abstractions;

namespace CraftingInterpretersNet;

public static class Defaults
{
    public static IErrorReporter DefaultErrorReporter = new ConsoleErrorReporter();
    public static IRuntimeErrorReporter DefaultRuntimeErrorReporter = new ConsoleRuntimeErrorReporter();
    public static IOutputSink DefaultOutputSink = new ConsoleSink();
}