# Crafting Interpreters .Net

A Lox implementation in C# written while following along to https://craftinginterpreters.com/

## Building

```
> dotnet build
> dotnet test

-- To run mutation tests and generate the report
> dotnet tool restore
> cd CraftingInterpretersNet.Tests
> dotnet stryker
```

## Additional features
* Support for Conditional (ternary) `? :` operator
  * Chapter 6, challenge 2 https://craftinginterpreters.com/parsing-expressions.html#challenges
  * Hat-tip to [https://github.com/HanKruiger/jlox](https://github.com/HanKruiger/jlox/blob/master/com/craftinginterpreters/lox/Parser.java#L403-L416) for the inspiration when I got stuck.

## Implementation differences
* `Expr` and `Stmt` classes generated with a C# Source Generator, `CraftingInterpretersNet.Generator`

## Where I'm up to (what's next to work on)

* `jlox` implementation is done!
* Next up, the bytecode-based implementation (if I ever get around to it 😬)