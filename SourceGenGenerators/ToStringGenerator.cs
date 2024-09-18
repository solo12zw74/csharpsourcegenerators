using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenGenerators;

[Generator]
public class ToStringGenerator : IIncrementalGenerator
{
    private static readonly IDictionary<string, int> _countPerFilename = new Dictionary<string, int>();

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classes = context.SyntaxProvider.CreateSyntaxProvider(
                static (syntaxNode, _) => IsTargetNode(syntaxNode),
                static (ctx, _) => GetSyntaxNodeNodeForTransform(ctx))
            .Where(v => v != null)
            .Collect();

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
            ? new ClassToGenerate(classSymbol.ContainingNamespace.ToDisplayString(), classSymbol.Name,
                GetProperties(classSymbol))
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

    private static void Execute(SourceProductionContext context, ImmutableArray<ClassToGenerate> classesToGenerates)
    {
        foreach (var classToGenerate in classesToGenerates)
        {
            var namespaceName = classToGenerate.NamespaceName;
            var className = classToGenerate.ClassName;
            var fileName = $"{namespaceName}.{className}.g.cs";

            if (_countPerFilename.ContainsKey(fileName))
                _countPerFilename[fileName]++;
            else
                _countPerFilename[fileName] = 1;

            var sb = new StringBuilder();
            sb.Append(
                $$"""
                  // Generation count {{_countPerFilename[fileName]}}
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
    }

    private static IEnumerable<string> GetProperties(INamedTypeSymbol source)
    {
        return source.GetMembers()
            .Where(pds => pds.Kind == SymbolKind.Property && pds.DeclaredAccessibility == Accessibility.Public)
            .Select(p => p.Name);
    }
}