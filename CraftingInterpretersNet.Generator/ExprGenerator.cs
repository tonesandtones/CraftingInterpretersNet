using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CraftingInterpretersNet.Generator;

[Generator]
public class ExprGenerator : IIncrementalGenerator
{
  private const string HelloClass = """
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
    context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
      "Hello.g",
      SourceText.From(HelloClass, Encoding.UTF8)));
    
    //TODO generate the Expr classes. https://craftinginterpreters.com/representing-code.html#metaprogramming-the-trees
  }
}