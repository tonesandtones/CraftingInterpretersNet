using System;
using System.Collections.Generic;
using System.IO;
using CraftingInterpretersNet.Abstractions;

namespace CraftingInterpretersNet;

internal class Program
{
  private static bool hadError = false;
  
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
    
    if (hadError) Environment.Exit(65);
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
      hadError = false;
    }
  }

  private static void Run(string source)
  {
    Scanner scanner = new(source);
    List<Token> tokens = scanner.ScanTokens();
    Parser parser = new(tokens);
    Expr? expression = parser.Parse();

    if (hadError) return;
    Console.WriteLine(new AstPrinter().Print(expression));
    // foreach (var token in tokens)
    // {
    //   Console.WriteLine(token);
    // }
  }
  
  public static void Error(int line, String message) {
    Report(line, "", message);
  }
  
  public static void Error(Token token, String message) {
    if (token.Type == TokenType.EOF) {
      Report(token.Line, " at end", message);
    } else {
      Report(token.Line, " at '" + token.Lexeme + "'", message);
    }
  }
  
  private static void Report(int line, String where, String message) {
    var outMessage = $"[line {line}] Error{where}: {message}";
    Console.WriteLine(outMessage);
    hadError = true;
  }
}