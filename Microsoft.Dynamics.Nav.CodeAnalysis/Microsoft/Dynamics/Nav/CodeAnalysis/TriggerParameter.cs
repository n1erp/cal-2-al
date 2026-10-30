using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class TriggerParameter
{
	internal string Name { get; }

	internal TypeSymbol Type { get; }

	internal bool IsVar { get; }

	public TriggerParameter(string name, TypeSymbol parameterType, bool isVar)
	{
		Name = name;
		Type = parameterType;
		IsVar = isVar;
	}
}
