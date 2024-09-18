namespace SourceGenGenerators;

public sealed class ClassToGenerate(string namespaceName, string className, IEnumerable<string> properties)
{
    public string NamespaceName { get; } = namespaceName;
    public string ClassName { get; } = className;
    public IEnumerable<string> Properties { get; } = properties;
}