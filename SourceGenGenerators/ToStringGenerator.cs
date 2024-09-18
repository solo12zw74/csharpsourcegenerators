using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenGenerators;

[Generator]
public class ToStringGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classes = context.SyntaxProvider.CreateSyntaxProvider(
                static (syntaxNode, _) => IsTargetNode(syntaxNode),
                static (ctx, _) => GetSyntaxNodeNodeForTransform(ctx))
            .Where(v => v != null);

        context.RegisterSourceOutput(classes, Execute!);
        context.RegisterPostInitializationOutput(PostInitializationOutput);
    }

    private static bool IsTargetNode(SyntaxNode syntaxNode)
    {
        return syntaxNode is ClassDeclarationSyntax classDeclarationSyntax
               && classDeclarationSyntax.AttributeLists.Any();
    }

    private static ClassToGenerate? GetSyntaxNodeNodeForTransform(GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);

        if (classSymbol == null) return null;

        var attributeSymbol =
            context.SemanticModel.Compilation.GetTypeByMetadataName("SourceGenGenerators.GenerateToStringAttribute");

        if (attributeSymbol == null) return null;

        return classSymbol.GetAttributes()
            .Any(a => attributeSymbol.Equals(a.AttributeClass, SymbolEqualityComparer.Default))
            ? new ClassToGenerate(classSymbol.ContainingNamespace.ToDisplayString(), classSymbol.Name, GetProperties(classDeclarationSyntax))
            : null;
    }

    private static void PostInitializationOutput(IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource("SourceGenGenerators.GenerateToStringAttribute.g.cs",
            """
            namespace SourceGenGenerators;
            [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
            internal sealed class GenerateToStringAttribute : System.Attribute { }
            """);
    }

    private static void Execute(SourceProductionContext context, ClassToGenerate classToGenerate)
    {
        var namespaceName = classToGenerate. NamespaceName;
        var className = classToGenerate.ClassName;
        var fileName = $"{namespaceName}.{className}.g.cs";

        var sb = new StringBuilder();
        sb.Append(
            $$"""
              namespace {{namespaceName}};
              partial class {{className}}
              {
                  public override string ToString()
                  {
              """);
        var toStringValue = string.Join(", ", classToGenerate.Properties
            .Select(property => $"{property} = {{{property}}}"));

        sb.AppendLine();
        sb.Append(
            $$"""
                      return $"{{className}}: {{toStringValue}}";
                  }
              }
              """);
        context.AddSource(fileName, sb.ToString());
    }

    private static IEnumerable<string> GetProperties(ClassDeclarationSyntax source)
    {
        return source.Members.OfType<PropertyDeclarationSyntax>()
            .Where(pds => pds.Modifiers.Any(SyntaxKind.PublicKeyword))
            .Select(p => p.Identifier.Text);
    }
}