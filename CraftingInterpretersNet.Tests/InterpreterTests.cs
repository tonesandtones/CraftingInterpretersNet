using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CraftingInterpretersNet.Tests;

public class InterpreterTests
{
    private readonly CollectingErrorReporter _errorReporter = new();
    private readonly CollectingRuntimeErrorReporter _runtimeReporter = new();
    private readonly CollectingOutputSink _sink = new();

    private readonly ITestOutputHelper _outputHelper;

    public InterpreterTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [MemberData(nameof(SimpleExpressionTestData))]
    [Theory]
    public void InterpreterProducesExpectedOutput(string lox, params string[] expectedResults)
    {
        Interpret(lox);

        if (_runtimeReporter.HasRuntimeError)
        {
            PrintInterpretErrors();
        }

        _sink.Messages.Should().BeEquivalentTo(expectedResults);
    }

    public static IEnumerable<object[]> SimpleExpressionTestData()
    {
        yield return TestCase("print 1;", "1");
        yield return TestCase("print 1 + 1;", "2");
        yield return TestCase("print 1.0 + 1.0;", "2");
        yield return TestCase("print 1.1 - 0.1;", "1");
        yield return TestCase("print 1 > 2;", "false");
        yield return TestCase("print 1 + 2 < 5;", "true");
        yield return TestCase("print \"abc\" + \"def\";", "abcdef");
        yield return TestCase("print 1 + 2 >= 3;", "true");
        yield return TestCase("print 1 >= 1 ? \"iamtrue\" : \"iamfalse\";", "iamtrue");
        yield return TestCase("print 1 > 2 ? \"iamtrue\" : \"iamfalse\";", "iamfalse");
        yield return TestCase("print 1 == 1 ? \"a\" + \"b\" : 123;", "ab");
        yield return TestCase("print -1 + 1;", "0");
        yield return TestCase("print !false;", "true");
        yield return TestCase("print 1 + 2 * 3;", "7");
        yield return TestCase("print (2 + 2) * 3;", "12");

        yield return TestCase("print 1; print 2;", "1", "2");
    }

    [MemberData(nameof(RuntimeErrorTestData))]
    [Theory]
    public void RuntimeErrorTests(string lox, params string[] expectedError)
    {
        Interpret(lox);

        _runtimeReporter.HasRuntimeError.Should().BeTrue();
        _runtimeReporter.ReceivedErrors.Should().HaveCount(1)
            .And.Subject.Single()
            .Message
            .Should().Be(expectedError.Single());
    }

    public static IEnumerable<object[]> RuntimeErrorTestData()
    {
        yield return TestCase("\"a\" + 1;", "Both operands must be the same type and both strings or numbers. Left is String, right is Double");
        yield return TestCase("\"a\" + nil;", "Both operands must be the same type and both strings or numbers. Left is String, right is nil");
        yield return TestCase("nil * 1;", "Operands must be numbers.");
        yield return TestCase("-nil;", "Operand must be a number.");
    }

    private void Interpret(string lox)
    {
        Scanner s = new(lox, _errorReporter);
        var tokens = s.ScanTokens();
        Parser p = new(tokens, _errorReporter);
        var parsedExpr = p.Parse().ToList();

        if (parsedExpr is { Count: 0 })
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
        
        Interpreter i = new(_runtimeReporter, _sink);
        i.Interpret(parsedExpr);
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

    private static object[] TestCase(string lox, params string[] expectedResult)
    {
        return new object[] { lox, expectedResult };
    }
}