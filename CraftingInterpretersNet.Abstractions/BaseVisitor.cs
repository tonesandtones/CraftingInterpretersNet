namespace CraftingInterpretersNet.Abstractions;

public class BaseVisitor<TExpr, TStmt> : Expr.Visitor<TExpr?>, Stmt.Visitor<TStmt?>
{
    public virtual TExpr? VisitAssignExpr(Expr.Assign expr)
    {
        return default;
    }

    public virtual TExpr? VisitBinaryExpr(Expr.Binary expr)
    {
        return default;
    }

    public virtual TExpr? VisitConditionalExpr(Expr.Conditional expr)
    {
        return default;
    }

    public virtual TExpr? VisitCallExpr(Expr.Call expr)
    {
        return default;
    }

    public virtual TExpr? VisitGetExpr(Expr.Get expr)
    {
        return default;
    }

    public virtual TExpr? VisitGroupingExpr(Expr.Grouping expr)
    {
        return default;
    }

    public virtual TExpr? VisitLiteralExpr(Expr.Literal expr)
    {
        return default;
    }

    public virtual TExpr? VisitLogicalExpr(Expr.Logical expr)
    {
        return default;
    }

    public virtual TExpr? VisitSetExpr(Expr.Set expr)
    {
        return default;
    }

    public virtual TExpr? VisitSuperExpr(Expr.Super expr)
    {
        return default;
    }

    public virtual TExpr? VisitThisExpr(Expr.This expr)
    {
        return default;
    }

    public virtual TExpr? VisitUnaryExpr(Expr.Unary expr)
    {
        return default;
    }

    public virtual TExpr? VisitVariableExpr(Expr.Variable expr)
    {
        return default;
    }

    public virtual TStmt? VisitBlockStmt(Stmt.Block stmt)
    {
        return default;
    }

    public virtual TStmt? VisitClassStmt(Stmt.Class stmt)
    {
        return default;
    }

    public virtual TStmt? VisitExpressionStmt(Stmt.Expression stmt)
    {
        return default;
    }

    public virtual TStmt? VisitFunctionStmt(Stmt.Function stmt)
    {
        return default;
    }

    public virtual TStmt? VisitIfStmt(Stmt.If stmt)
    {
        return default;
    }

    public virtual TStmt? VisitPrintStmt(Stmt.Print stmt)
    {
        return default;
    }

    public virtual TStmt? VisitReturnStmt(Stmt.Return stmt)
    {
        return default;
    }

    public virtual TStmt? VisitVarStmt(Stmt.Var stmt)
    {
        return default;
    }

    public virtual TStmt? VisitWhileStmt(Stmt.While stmt)
    {
        return default;
    }
}