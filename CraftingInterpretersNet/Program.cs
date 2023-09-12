using System;
using System.Collections.Generic;
using System.IO;
using CraftingInterpretersNet.Abstractions;

namespace CraftingInterpretersNet;

internal class Program
{
    private static bool HadParseError => Defaults.DefaultErrorReporter.HasReceivedError;
    private static bool HadRuntimeError => Defaults.DefaultRuntimeErrorReporter.HasRuntimeError;

    public static void Main(string[] args)
    {
        if (args.Length > 1)
        {
            Console.WriteLine("Usage loxnet [script_file]");
            Environment.Exit(64);
        }

        if (args.Length == 1)
            RunFile(args[0]);
        else
            RunPrompt();
    }

    private static void RunFile(string path)
    {
        var contents = File.ReadAllText(path);
        Run(contents);

        if (HadParseError) Environment.Exit(65);
        if (HadRuntimeError) Environment.Exit(70);
    }

    private static void RunPrompt()
    {
        Console.WriteLine("Welcome to Lox.Net REPL");
        for (;;)
        {
            Console.Out.Write("> ");
            var line = Console.ReadLine();
            if (line == null) break;
            Run(line);
            Defaults.DefaultErrorReporter.ClearErrorState();
            Defaults.DefaultRuntimeErrorReporter.ClearErrorState();
        }
    }

    private static void Run(string source)
    {
        Scanner scanner = new(source);
        List<Token> tokens = scanner.ScanTokens();
        Parser parser = new(tokens);
        Expr? expression = parser.Parse();
        Interpreter interpreter = new();
        
        if (HadParseError || expression is null) return;
        // Console.WriteLine(new AstPrinter().Print(expression));
        string? result = interpreter.Interpret(expression);
        Console.WriteLine(result);
        
        // foreach (var token in tokens)
        // {
        //     Console.WriteLine(token);
        // }
    }
}