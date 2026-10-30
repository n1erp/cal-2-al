using System;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class SynthesizedPropertySymbol : MethodSymbol
{
	private readonly TypeSymbol containingType;

	private readonly string name;

	private readonly TypeSymbol type;

	public override string Name => name;

	public override Accessibility DeclaredAccessibility => Accessibility.Public;

	public override ReturnValueSymbol ReturnValue { get; }

	public override bool IsLocal => false;

	public override bool IsStatic => false;

	public override SymbolKind Kind => SymbolKind.Method;

	public override MethodKind MethodKind => MethodKind.Property;

	public override bool IsImplicitlyDeclared => true;

	public override ImmutableArray<ParameterSymbol> Parameters { get; }

	public override ImmutableArray<AttributeSymbol> Attributes => ImmutableArray<AttributeSymbol>.Empty;

	public override Symbol ContainingSymbol => containingType;

	public override TypeSymbol ContainingType => containingType;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override ImmutableArray<VariableSymbol> LocalVariables => ImmutableArray<VariableSymbol>.Empty;

	public SynthesizedPropertySymbol(TypeSymbol container, string name, TypeSymbol type)
	{
		containingType = container;
		this.name = name;
		this.type = type;
		ReturnValue = new SynthesizedReturnValueSymbol(this, type);
		Parameters = ImmutableArray<ParameterSymbol>.Empty;
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is SynthesizedPropertySymbol synthesizedPropertySymbol))
		{
			return false;
		}
		if (Parameters.Length == synthesizedPropertySymbol.Parameters.Length && string.Equals(name, synthesizedPropertySymbol.name, StringComparison.Ordinal) && containingType == synthesizedPropertySymbol.containingType && type == synthesizedPropertySymbol.type)
		{
			for (int i = 0; i < Parameters.Length; i++)
			{
				if (Parameters[i].Type != synthesizedPropertySymbol.Parameters[i].Type)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(name.GetHashCode(), Hash.Combine(containingType.GetHashCode(), ReturnValue.GetHashCode()));
	}
}
