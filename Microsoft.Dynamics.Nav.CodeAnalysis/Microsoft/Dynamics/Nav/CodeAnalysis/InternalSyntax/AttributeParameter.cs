using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class AttributeParameter
{
	public string Name { get; }

	public TypeSymbol Type { get; }

	public bool IsVar { get; }

	public bool IsOptional { get; }

	public AttributeParameter(string name, TypeSymbol type, bool isVar, bool isOptional)
	{
		Name = name;
		Type = type;
		IsVar = isVar;
		IsOptional = isOptional;
	}
}
