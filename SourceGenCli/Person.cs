using SourceGenGenerators;

namespace SourceGen;

[GenerateToString]
public partial class Person(string firstName, string? middleName, string lastName)
{
    public string FirstName { get; } = firstName;
    public string LastName { get; } = lastName;
    public string MiddleName { get; } = middleName ?? "undefined";
}

public partial class Person
{
    public int Age { get; set; } = 20;
}