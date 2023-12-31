﻿using CraftingInterpretersNet.Abstractions;
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
        yield return TestCase("""var a = "a"; print a;""", "a");
        yield return TestCase("var a; a = 1; print a;", "1");
        yield return TestCase("""
                              var a = 1;
                              var b;
                              b = a >=2 ? "yes" : "no";
                              print a;
                              print b;
                              """, "1", "no");
        yield return TestCase(
            """
            var a = "global a";
            var b = "global b";
            var c = "global c";
            {
              var a = "outer a";
              var b = "outer b";
              {
                var a = "inner a";
                print a;
                print b;
                print c;
              }
              print a;
              print b;
              print c;
            }
            print a;
            print b;
            print c;
            """,
            "inner a", "outer b", "global c",
            "outer a", "outer b", "global c",
            "global a", "global b", "global c");
        yield return TestCase(
            """
            var a = 1;
            var b;
            if (a == 1) { b = "truthy"; } else { b = "falsey"; }
            print b;
            """,
            "truthy");
        yield return TestCase("""print "hi" or 2;""", "hi");
        yield return TestCase("""print nil or "yes";""", "yes");
        yield return TestCase(
            """
            var a = 0;
            while (a <= 2) print a = a + 1;
            """,
            "1", "2", "3");
        yield return TestCase(
            """
            for (var i = 0; i < 5; i=i+1) {
              print i;
            }
            """,
            "0", "1", "2", "3", "4");
        yield return TestCase( // Fibonacci !!!
            """
            var a = 0;
            var temp;
            for (var b = 1; a < 10000; b = temp + b) {
              print a;
              temp = a;
              a = b;
            }
            """,
            "0", "1", "1", "2", "3", "5", "8", "13", "21", "34", "55", "89", "144", "233", "377", "610", "987", "1597",
            "2584", "4181", "6765");
        yield return TestCase("print clock;", "<native fn>");
        yield return TestCase("fun a(){ print \"a\"; }\nprint a;", "<fn a>");
        yield return TestCase("fun a(){ print \"abc\"; } a();", "abc");
        yield return TestCase("""fun HelloWorld(Name) { print "Hello " + Name + "!"; } HelloWorld("Tones"); """,
            "Hello Tones!");
        yield return TestCase("""fun a(){ return "abc"; } print a();""", "abc");
        yield return TestCase( // Recursive fibonacci !! // slow...🐌
            """
            fun fib(n) {
              if (n <= 1) return n;
              return fib(n - 2) + fib(n - 1);
            }

            for (var i = 0; i < 20; i = i + 1) {
              print fib(i);
            }
            """,
            "0", "1", "1", "2", "3", "5", "8", "13", "21", "34", "55", "89", "144", "233", "377", "610", "987", "1597",
            "2584", "4181");
        yield return TestCase( //nested functions and scopes
            """
            fun makeCounter() {
              var i = 0;
              fun count() {
                i = i + 1;
                print i;
              }
            
              return count;
            }

            var counter = makeCounter();
            counter(); // "1".
            counter(); // "2".
            """,
            "1", "2");
        yield return TestCase(
            //showA() should resolve the same value of a each time it's run, even though a is redefined between runs.
            """
            var a = "global";
            {
              fun showA() {
                print a;
              }
            
              showA();
              var a = "block";
              showA();
            }
            """,
            "global", "global");
        yield return TestCase("""class Bacon{eat(){print "om nom nom";}} Bacon().eat();""", "om nom nom");
        yield return TestCase("""class Bacon{eat(){print "om nom nom";}} print Bacon().eat;""", "<fn eat>");
        yield return TestCase(
            """
            class Say {
              init() {
                this.something = "I Am Tones";
              }
              SayIt() {
                print ("Hello, " + this.something);
              }
            }

            var a = Say();
            a.SayIt();
            """,
            "Hello, I Am Tones");
        yield return TestCase(
            """
            class Foo {
              init() {
                print this;
              }
            }

            var foo = Foo();
            print foo.init();
            """,
            "Foo instance",  //when foo is constructed and init() is called by the interpreter
            "Foo instance",  //when foo.init() is called on the last line (and returns the foo.this)
            "Foo instance"); //when we print the result of foo.init()
        yield return TestCase(
            """
            class Foo {
              init() {
                print "a";
                return;    //early return ...
                print "b"; //... should not print "b"
              }
            }
            Foo();
            """,
            "a");
        yield return TestCase(
            """
            class Doughnut {
              cook() {
                print "Fry until golden brown.";
              }
            }
            
            class BostonCream < Doughnut {}
            
            BostonCream().cook();
            """,
            "Fry until golden brown.");
         yield return TestCase(
             """
             class Doughnut {
               cook() {
                 print "Fry until golden brown.";
               }
             }

             class BostonCream < Doughnut {
               cook() {
                 super.cook();
                 print "Pipe full of custard and coat with chocolate.";
               }
             }

             BostonCream().cook();
             """,
             "Fry until golden brown.", "Pipe full of custard and coat with chocolate.");
        yield return TestCase( //correct resolution of super. target
            """
            class A {
              method() {
                print "A method";
              }
            }

            class B < A {
              method() {
                print "B method";
              }
            
              test() {
                super.method();
              }
            }

            class C < B {}

            C().test();
            """,
            "A method");
    }

    [Fact]
    public void ClockReturnsCurrentTime()
    {
        Interpret("print clock();");
        _sink.Messages.Should().HaveCount(1);
        var actualTimeStr = _sink.Messages.Single();
        long.TryParse(actualTimeStr, out var actualTime).Should().BeTrue();
        actualTime.Should().BeCloseTo(DateTimeOffset.Now.ToUnixTimeMilliseconds(), 50);
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
        yield return TestCase("\"a\" + 1;",
            "Both operands must be the same type and both strings or numbers. Left is String, right is Double");
        yield return TestCase("\"a\" + nil;",
            "Both operands must be the same type and both strings or numbers. Left is String, right is nil");
        yield return TestCase("nil * 1;", "Operands must be numbers.");
        yield return TestCase("-nil;", "Operand must be a number.");
        yield return TestCase("xyz = 1;", "Undefined variable 'xyz'.");
        yield return TestCase(
            """
            var Abc = 123;
            class Def < Abc {
            }
            """,
            "Superclass must be a class.");
    }

    private void Interpret(string lox)
    {
        Scanner s = new(lox, _errorReporter);
        var tokens = s.ScanTokens();
        Parser p = new(tokens, _errorReporter);
        var parsedExpr = p.Parse().ToList();

        FailIfErrors(parsedExpr, "parse");

        Interpreter i = new(_runtimeReporter, new MultiSink(_sink, new TestOutputHelperSink(_outputHelper)));
        Resolver resolver = new(i, _errorReporter);
        resolver.Resolve(parsedExpr);
        FailIfErrors(parsedExpr, "resolve");

        i.Interpret(parsedExpr);
    }

    private void FailIfErrors(List<Stmt> parsedExpr, string stage)
    {
        if (parsedExpr is { Count: 0 } || _errorReporter.HasReceivedError)
        {
            _outputHelper.WriteLine($"Did not receive output from {stage}.");
            if (!_errorReporter.HasReceivedError)
            {
                _outputHelper.WriteLine("Error reporter did not receive any errors.");
            }
            else
            {
                _outputHelper.WriteLine("Error reporter had these errors:");
                foreach (var errorEvent in _errorReporter.ReceivedErrors)
                {
                    _outputHelper.WriteLine($"{errorEvent.Message} : {errorEvent.Where} : line {errorEvent.Line}");
                }
            }

            Assert.Fail($"Did not receive any output from {stage}");
        }
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