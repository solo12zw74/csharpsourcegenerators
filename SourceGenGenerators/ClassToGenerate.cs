namespace SourceGenGenerators;

public sealed class ClassToGenerate(string namespaceName, string className, IEnumerable<string> properties)
    : IEquatable<ClassToGenerate>
{
    public string NamespaceName { get; } = namespaceName;
    public string ClassName { get; } = className;
    public IEnumerable<string> Properties { get; } = properties;

    public bool Equals(ClassToGenerate? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return NamespaceName == other.NamespaceName && ClassName == other.ClassName &&
               Properties.SequenceEqual(other.Properties);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is ClassToGenerate other && Equals(other));
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (NamespaceName.GetHashCode() * 397) ^ ClassName.GetHashCode();
        }
    }
}