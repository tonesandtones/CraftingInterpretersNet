using System.Collections.Generic;

namespace CraftingInterpretersNet.Generator;

public static class GenerationTargets
{
    public record Target(string Name, IEnumerable<Argument> ArgumentList);

    public record Argument(string Type, string Name);

    public record Ast(string Name, IEnumerable<Target> Targets);

    public static readonly IEnumerable<Ast> PastryAst = new[]
    {
        AstOf("Pastry",
            TargetOf("Doughnut",
                ArgOf("string", "Colour"),
                ArgOf("bool", "Holey")
            ),
            TargetOf("Croissant",
                ArgOf("List<string>", "Fillings")))
    };

    public static readonly IEnumerable<Ast> DefinedAsts = new[]
    {
        AstOf("Expr",
            TargetOf("Assign", ArgOf("Token", "name"), ArgOf("Expr", "value")),
            TargetOf("Binary", ArgOf("Expr", "left"), ArgOf("Token", "oper"), ArgOf("Expr", "right")),
            TargetOf("Conditional", ArgOf("Expr", "condition"), ArgOf("Token", "leftOperator"), ArgOf("Expr", "left"),
                ArgOf("Token", "rightOperator"), ArgOf("Expr", "right")),
            TargetOf("Call", ArgOf("Expr", "callee"), ArgOf("Token", "paren"), ArgOf("List<Expr>", "arguments")),
            TargetOf("Get", ArgOf("Expr", "obj"), ArgOf("Token", "name")),
            TargetOf("Grouping", ArgOf("Expr", "expression")),
            TargetOf("Literal", ArgOf("object", "value")),
            TargetOf("Logical", ArgOf("Expr", "left"), ArgOf("Token", "oper"), ArgOf("Expr", "right")),
            TargetOf("Set", ArgOf("Expr", "obj"), ArgOf("Token", "name"), ArgOf("Expr", "value")),
            TargetOf("Super", ArgOf("Token", "keyword"), ArgOf("Token", "method")),
            TargetOf("This", ArgOf("Token", "keyword")),
            TargetOf("Unary", ArgOf("Token", "oper"), ArgOf("Expr", "right")),
            TargetOf("Variable", ArgOf("Token", "name"))
        ),
        AstOf("Stmt",
            TargetOf("Block", ArgOf("List<Stmt>", "statements")),
            TargetOf("Class", ArgOf("Token", "name"), ArgOf("Expr.Variable", "superclass"),
                ArgOf("List<Stmt.Function>", "methods")),
            TargetOf("ExpressionStmt", ArgOf("Expr", "expression")),
            TargetOf("Function", ArgOf("Token", "name"), ArgOf("List<Token>", "par"), ArgOf("List<Stmt>", "body")),
            TargetOf("If", ArgOf("Expr", "condition"), ArgOf("Stmt", "thenBranch"), ArgOf("Stmt", "elseBranch")),
            TargetOf("Print", ArgOf("Expr", "expression")),
            TargetOf("Return", ArgOf("Token", "keyword"), ArgOf("Expr", "value")),
            TargetOf("Var", ArgOf("Token", "name"), ArgOf("Expr", "initializer")),
            TargetOf("While", ArgOf("Expr", "condition"), ArgOf("Stmt", "body"))
        )
    };

    private static Ast AstOf(string name, params Target[] targets)
    {
        return new Ast(name, targets);
    }

    private static Target TargetOf(string name, params Argument[] arguments)
    {
        return new Target(name, arguments);
    }

    private static Argument ArgOf(string type, string name)
    {
        return new Argument(type, name);
    }
}