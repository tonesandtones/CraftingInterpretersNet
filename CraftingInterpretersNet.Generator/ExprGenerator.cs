using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using static CraftingInterpretersNet.Generator.HelperExtensions;

namespace CraftingInterpretersNet.Generator;

[Generator]
internal partial class ExprGenerator : IIncrementalGenerator
{
    private const string HelloClass = """
//lang=c#
namespace CraftingInterpretersNet
{
  public static class HelloConstants
  {
    public const string Hello = "Hello";
  }
}
""";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(PostInitialisationCallback);

        //TODO generate the Expr classes. https://craftinginterpreters.com/representing-code.html#metaprogramming-the-trees
        context.RegisterPostInitializationOutput(PastryGenerator);
    }

    private static void PostInitialisationCallback(IncrementalGeneratorPostInitializationContext ctx)
    {
        ctx.AddSource(
            "Hello.g.cs",
            SourceText.From(HelloClass, Encoding.UTF8));
    }

    private static void PastryGenerator(IncrementalGeneratorPostInitializationContext context)
    {
        foreach (var kind in GenerationTargets.PastryAst)
        {
            context.AddSource($"{kind.Name}.g.cs", MakeSourceForAst(kind));
        }
    }

    private static string MakeSourceForAst(GenerationTargets.Ast kind)
    {
        var source = $$"""
{{Preamble}}{{Environment.NewLine}}
{{GeneratedCodeAttribute}}
public abstract class {{kind.Name}}
{
    public abstract T Accept<T>(Visitor<T> visitor);{{Environment.NewLine}}
    {{GeneratedCodeAttribute}}
    public interface Visitor<out T>
    {
{{kind.Targets.Select(x => $"{MakeIndent(2)}T Visit{x.Name}({x.Name} {kind.Name.ToCamelCase()});").Join(Environment.NewLine)}}
     }
     
{{kind.Targets.Select(x => MakeEntityClass(x, kind.Name)).Join(Environment.NewLine).BlockIndent(1)}}
}

""";
        return source;
    }

    private static string MakeEntityClass(GenerationTargets.Target target, string baseClassName)
    {
        var source = $$"""
{{GeneratedCodeAttribute}}
public class {{target.Name}} : {{baseClassName}}
{
{{target.ArgumentList.Select(x => $"public {x.Type} {x.Name} {{ get; }}").Join(Environment.NewLine).BlockIndent(1)}}
    public {{target.Name}}({{target.ArgumentList.Select(x => $"{x.Type} {x.Name.ToCamelCase()}").Join(", ")}})
    {
{{target.ArgumentList.Select(x => $"{x.Name} = {x.Name.ToCamelCase()};").Join(Environment.NewLine).BlockIndent(2)}}
    }
    
    public override T Accept<T>(Visitor<T> visitor)
    {
        return visitor.Visit{{target.Name}}(this);
    }
}{{Environment.NewLine}}
""";
        return source;
    }
}

public static class HelperExtensions
{
    public static string MakeIndent(int indentLevel)
    {
        return new string(' ', indentLevel * 4);
    }

    public static string BlockIndent(this string s, int level)
    {
        return string.Join(Environment.NewLine, s
            .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
            .Select(x => $"{MakeIndent(level)}{x}"));
    }

    public static string ToCamelCase(this string s)
    {
        if (s is null or { Length: 0 }) return "";
        return char.ToLower(s[0]) + s.Substring(1);
    }

    public static string Join(this IEnumerable<string> enumerable, string delim)
    {
        return string.Join(delim, enumerable);
    }
    
}