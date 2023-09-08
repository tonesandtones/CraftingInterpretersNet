using System.Collections.Generic;

namespace CraftingInterpretersNet.Generator;

public static class GenerationTargets
{
    public record Target(string Name, IEnumerable<Argument> ArgumentList);

    public record Argument(string Type, string Name);

    public record Ast(string Name, IEnumerable<Target> Targets);

    public static readonly IEnumerable<Ast> PastryAst = new Ast[]
    {
        new("Pastry", new Target[]
        {
            new("Doughnut",
                new Argument[]
                {
                    new("string", "Colour"),
                    new("bool", "Holey")
                }),
            new("Croissant", new Argument[]
            {
                new("List<string>", "Fillings")
            })
        })
    };

    // public static readonly IEnumerable<Ast> DefinedAsts = new[]
    // {
    //     new Ast("Expr", new[]
    //     {
    //         new Target("Assign", "Token name, Expr value"),
    //         new Target("Binary", "Expr left, Token operator, Expr right"),
    //         new Target("Call", "Expr callee, Token paren, List<Expr> arguments"),
    //         new Target("Get", "Expr object, Token name"),
    //         new Target("Grouping", "Expr expression"),
    //         new Target("Literal", "Object value"),
    //         new Target("Logical", "Expr left, Token operator, Expr right"),
    //         new Target("Set", "Expr object, Token name, Expr value"),
    //         new Target("Super", "Token keyword, Token method"),
    //         new Target("This", "Token keyword"),
    //         new Target("Unary", "Token operator, Expr right"),
    //         new Target("Variable", "Token name")
    //     }),
    //     new Ast("Stmt", new []
    //     {
    //         new Target("Block", "List<Stmt> statements"),
    //         new Target("Class", "Token name, Expr.Variable superclass, List<Stmt.Function> methods"),
    //         new Target("Expression", "Expr expression"),
    //         new Target("Function", "Token name, List<Token> params, List<Stmt> body"),
    //         new Target("If", "Expr condition, Stmt thenBranch, Stmt elseBranch"),
    //         new Target("Print", "Expr expression"),
    //         new Target("Return", "Token keyword, Expr value"),
    //         new Target("Var", "Token name, Expr initializer"),
    //         new Target("While", "Expr condition, Stmt body")
    //     })
    // };
}