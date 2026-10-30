using System.Diagnostics;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal struct AnonymousTypeField(string name, Location location, TypeSymbol type)
{
	public readonly string Name = name;

	public readonly Location Location = location;

	public readonly TypeSymbol Type = type;

	[Conditional("DEBUG")]
	internal void AssertIsGood()
	{
	}
}
