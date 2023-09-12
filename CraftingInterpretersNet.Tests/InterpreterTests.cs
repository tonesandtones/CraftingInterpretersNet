using System.Net.Security;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CraftingInterpretersNet.Tests;

public class InterpreterTests
{
    private readonly CollectingErrorReporter _errorReporter = new();
    private readonly CollectingRuntimeErrorReporter _runtimeReporter = new();

    private readonly ITestOutputHelper _outputHelper;

    public InterpreterTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [MemberData(nameof(SimpleExpressionTestData))]
    [Theory]
    public void InterpreterProducesExpectedOutput(string lox, string expectedResult)
    {
        var actualResult = Interpret(lox);

        if (actualResult is null)
        {
            PrintInterpretErrors();
        }

        actualResult.Should().Be(expectedResult);
    }

    public static IEnumerable<object[]> SimpleExpressionTestData()
    {
        yield return TestCase("1", "1");
        yield return TestCase("1 + 1", "2");
        yield return TestCase("1.0 + 1.0", "2");
        yield return TestCase("1.1 - 0.1", "1");
        yield return TestCase("1 > 2", "false");
        yield return TestCase("1 + 2 < 5", "true");
        yield return TestCase("\"abc\" + \"def\"", "abcdef");
        yield return TestCase("1 + 2 >= 3", "true");
        yield return TestCase("1 >= 1 ? \"iamtrue\" : \"iamfalse\"", "iamtrue");
        yield return TestCase("1 > 2 ? \"iamtrue\" : \"iamfalse\"", "iamfalse");
        yield return TestCase("1 == 1 ? \"a\" + \"b\" : 123", "ab");
        yield return TestCase("-1 + 1", "0");
        yield return TestCase("!false", "true");
        yield return TestCase("1 + 2 * 3", "7");
        yield return TestCase("(2 + 2) * 3", "12");
    }

    [MemberData(nameof(RuntimeErrorTestData))]
    [Theory]
    public void RuntimeErrorTests(string lox, string expectedError)
    {
        var result = Interpret(lox);
        result.Should().BeNull();

        _runtimeReporter.HasRuntimeError.Should().BeTrue();
        _runtimeReporter.ReceivedErrors.Should().HaveCount(1)
            .And.Subject.First()
            .Message
            .Should().Be(expectedError);
    }

    public static IEnumerable<object[]> RuntimeErrorTestData()
    {
        yield return TestCase("\"a\" + 1", "Both operands must be the same type and both strings or numbers. Left is String, right is Double");
        yield return TestCase("\"a\" + nil", "Both operands must be the same type and both strings or numbers. Left is String, right is nil");
        yield return TestCase("nil * 1", "Operands must be numbers.");
        yield return TestCase("-nil", "Operand must be a number.");
    }

    private string? Interpret(string lox)
    {
        Scanner s = new(lox, _errorReporter);
        var tokens = s.ScanTokens();
        Parser p = new(tokens, _errorReporter);
        var parsedExpr = p.Parse();

        if (parsedExpr is null)
        {
            _outputHelper.WriteLine("Did not receive output from Parser.");
            if (!_errorReporter.HasReceivedError)
            {
                _outputHelper.WriteLine("Error reporter did not receive any errors.");
            }
            else
            {
                _outputHelper.WriteLine("Error reporter had these errors:");
                foreach (var errorEvent in _errorReporter.ReceivedErrors)
                {
                    _outputHelper.WriteLine($"{errorEvent.Message} : {errorEvent.Where}");
                }
            }
            Assert.Fail("Did not receive any output from Parser");
        }
        
        Interpreter i = new(_runtimeReporter);
        return i.Interpret(parsedExpr);
    }

    private void PrintInterpretErrors()
    {
        if (!_runtimeReporter.HasRuntimeError)
        {
            _outputHelper.WriteLine("Received no result, but Runtime Error Reporter has no errors");
            return;
        }

        _outputHelper.WriteLine("Received no result and runtime error reporter has these errors");
        foreach (var error in _runtimeReporter.ReceivedErrors)
        {
            _outputHelper.WriteLine($"Message = {error.Message}. Line = {error.Line}");
        }
    }

    private static object[] TestCase(string lox, string expectedResult)
    {
        return new object[] { lox, expectedResult };
    }
}