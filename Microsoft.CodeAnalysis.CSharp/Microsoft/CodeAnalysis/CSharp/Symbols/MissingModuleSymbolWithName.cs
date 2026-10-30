using System;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal class MissingModuleSymbolWithName : MissingModuleSymbol
{
	private readonly string _name;

	public override string Name => _name;

	public MissingModuleSymbolWithName(AssemblySymbol assembly, string name)
		: base(assembly, -1)
	{
		_name = name;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(assembly.GetHashCode(), StringComparer.OrdinalIgnoreCase.GetHashCode(_name));
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (obj is MissingModuleSymbolWithName missingModuleSymbolWithName && assembly.Equals(missingModuleSymbolWithName.assembly))
		{
			return string.Equals(_name, missingModuleSymbolWithName._name, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}
}
