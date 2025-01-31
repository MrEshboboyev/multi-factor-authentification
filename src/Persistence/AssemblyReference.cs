using System.Reflection;

namespace Persistence;

public static class AssemblyReference
{
    // Holds a reference to the current assembly
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}