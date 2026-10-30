using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class ReferenceOptionTypeSymbol : OptionTypeSymbol
{
	private ImmutableArray<Symbol> members;

	public override string OptionMembers { get; }

	public override Symbol ContainingSymbol { get; }

	public override int EffectiveOptionsCount { get; }

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public static ReferenceOptionTypeSymbol Create(Symbol owner, string[] optionMembers)
	{
		return new ReferenceOptionTypeSymbol(owner, optionMembers);
	}

	private ReferenceOptionTypeSymbol(Symbol owner, string[] optionMembers)
	{
		ContainingSymbol = owner;
		EffectiveOptionsCount = optionMembers?.Length ?? 0;
		BuildMembers(optionMembers);
		OptionMembers = optionMembers.Concatenate(',');
	}

	private void BuildMembers(string[] optionMembers)
	{
		if (optionMembers == null || optionMembers.Length == 0)
		{
			members = ImmutableArray<Symbol>.Empty;
			return;
		}
		ImmutableArray<int> immutableArray = BuildOrdinalValues(ContainingSymbol);
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(optionMembers.Length);
		for (int i = 0; i < optionMembers.Length; i++)
		{
			if (!string.IsNullOrEmpty(optionMembers[i]))
			{
				int ordinal = i;
				if (immutableArray.Length == optionMembers.Length)
				{
					ordinal = immutableArray[i];
				}
				string name = optionMembers[i].UnquoteIdentifier();
				instance.Add(ReferenceOptionSymbol.Create(this, name, ordinal));
			}
		}
		members = instance.ToImmutableAndFree();
	}

	private static ImmutableArray<int> BuildOrdinalValues(Symbol containingSymbol)
	{
		if ((object)containingSymbol != null && containingSymbol.Kind == SymbolKind.Field && SystemTables.IsOptionalOrdinalValuesAllowed(containingSymbol.GetContainingSymbolOfType<ApplicationObjectTypeSymbol>().Id))
		{
			PropertySymbol property = containingSymbol.GetProperty(PropertyKind.OptionOrdinalValues);
			if (property != null)
			{
				string valueText = property.ValueText;
				if (!string.IsNullOrWhiteSpace(valueText))
				{
					string[] array = valueText.Split(',');
					ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance();
					string[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						if (int.TryParse(array2[i], out var result))
						{
							instance.Add(result);
						}
					}
					return instance.ToImmutableAndFree();
				}
			}
		}
		return ImmutableArray<int>.Empty;
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return members;
	}
}
