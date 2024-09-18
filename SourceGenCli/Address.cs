using SourceGenGenerators;

namespace SourceGen;

internal partial class Address
{
    public Address(string city, string line)
    {
        City = city;
        Line = line;
    }

    public string Line { get; set; }
    
    protected string City { get; set; }
}