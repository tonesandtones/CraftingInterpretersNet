using CraftingInterpretersNet.Abstractions;
using Xunit.Abstractions;

namespace CraftingInterpretersNet.Tests;

public class TestOutputHelperSink : IOutputSink
{
    private readonly ITestOutputHelper _outputHelper;

    public TestOutputHelperSink(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    public void Print(string? value)
    {
        _outputHelper.WriteLine(value);
    }
}