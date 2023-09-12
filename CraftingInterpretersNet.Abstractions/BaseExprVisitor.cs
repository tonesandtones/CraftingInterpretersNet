namespace CraftingInterpretersNet.Abstractions;

public abstract class BaseExprVisitor<T> : Expr.Visitor<T?>
{
    public virtual T? VisitConditionalExpr(Expr.Conditional expr)
    {
        return default;
    }

    public virtual T? VisitAssignExpr(Expr.Assign expr)
    {
        return default;
    }

    public virtual T? VisitBinaryExpr(Expr.Binary expr)
    {
        return default;
    }

    public virtual T? VisitCallExpr(Expr.Call expr)
    {
        return default;
    }

    public virtual T? VisitGetExpr(Expr.Get expr)
    {
        return default;
    }

    public virtual T? VisitGroupingExpr(Expr.Grouping expr)
    {
        return default;
    }

    public virtual T? VisitLiteralExpr(Expr.Literal expr)
    {
        return default;
    }

    public virtual T? VisitLogicalExpr(Expr.Logical expr)
    {
        return default;
    }

    public virtual T? VisitSetExpr(Expr.Set expr)
    {
        return default;
    }

    public virtual T? VisitSuperExpr(Expr.Super expr)
    {
        return default;
    }

    public virtual T? VisitThisExpr(Expr.This expr)
    {
        return default;
    }

    public virtual T? VisitUnaryExpr(Expr.Unary expr)
    {
        return default;
    }

    public virtual T? VisitVariableExpr(Expr.Variable expr)
    {
        return default;
    }
}