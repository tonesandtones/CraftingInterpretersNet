﻿using System;
using System.IO;
using System.Linq;

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
            System.Environment.Exit(64);
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

        if (HadParseError) System.Environment.Exit(65);
        if (HadRuntimeError) System.Environment.Exit(70);
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
        var tokens = scanner.ScanTokens();
        Parser parser = new(tokens);
        var statements = parser.Parse().ToList();
        if (HadParseError) return;
        
        Interpreter interpreter = new();
        Resolver resolver = new(interpreter);
        resolver.Resolve(statements);
        if (HadParseError) return;

        // Console.WriteLine(new AstPrinter().Print(expression));
        interpreter.Interpret(statements);
    }
}