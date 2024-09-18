# SourceGen Project

This project is developed as part of the Pluralsight course [C# 10: Developing Source Generators](https://app.pluralsight.com/library/courses/c-sharp-10-developing-source-generators/table-of-contents).

## Project Structure

- `NuGet.Config`: Configuration for NuGet package sources.
- `SourceGenGenerators/SourceGenGenerators.csproj`: Project file for the source generators.
- `SourceGenCli/Person.cs`: Example usage of the source generator.
- `SourceGen.Person.g.cs`: Generated code for the `Person` class.

## Description

The project demonstrates the creation and usage of C# source generators. The `SourceGenGenerators` project contains the source generator logic, while the `SourceGenCli` project shows how to use the generated code.

## Features

- **Source Generators**: Automatically generate code at compile time.
- **NuGet Configuration**: Custom package sources and package restore settings.
- **Post-Build Events**: Copy generated NuGet package to a specific directory.

## Usage

1. Clone the repository.
2. Open the solution in JetBrains Rider.
3. Build the solution to generate the source code.
4. Explore the generated code in `SourceGen.Person.g.cs`.

## Dependencies

- `Microsoft.CodeAnalysis.Analyzers` (v3.3.4)
- `Microsoft.CodeAnalysis.CSharp` (v4.10.0)