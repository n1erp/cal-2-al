using System;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class SynthesizedIntrinsicOperatorSymbol : MethodSymbol
{
	private sealed class SynthesizedOperatorParameterSymbol : SynthesizedParameterSymbol
	{
		public SynthesizedOperatorParameterSymbol(SynthesizedIntrinsicOperatorSymbol container, TypeSymbol type, int ordinal, string name)
			: base(container, type, ordinal, isVar: false, name)
		{
		}

		public override bool Equals(object obj)
		{
			if (obj == this)
			{
				return true;
			}
			if (!(obj is SynthesizedOperatorParameterSymbol synthesizedOperatorParameterSymbol))
			{
				return false;
			}
			if (Ordinal == synthesizedOperatorParameterSymbol.Ordinal)
			{
				return ContainingSymbol == synthesizedOperatorParameterSymbol.ContainingSymbol;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(ContainingSymbol.GetHashCode(), Ordinal.GetHashCode());
		}
	}

	private readonly TypeSymbol containingType;

	private readonly string name;

	private readonly ImmutableArray<ParameterSymbol> parameters;

	private readonly TypeSymbol returnType;

	public override string Name => name;

	public override Accessibility DeclaredAccessibility => Accessibility.Public;

	public override ReturnValueSymbol ReturnValue { get; }

	public override bool IsLocal => false;

	public override SymbolKind Kind => SymbolKind.Method;

	public override MethodKind MethodKind => MethodKind.BuiltInOperator;

	public override bool IsImplicitlyDeclared => true;

	internal override Compilation DeclaringCompilation => null;

	public override ImmutableArray<ParameterSymbol> Parameters => parameters;

	public override ImmutableArray<AttributeSymbol> Attributes => ImmutableArray<AttributeSymbol>.Empty;

	public override Symbol ContainingSymbol => containingType;

	public override TypeSymbol ContainingType => containingType;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override ImmutableArray<VariableSymbol> LocalVariables => ImmutableArray<VariableSymbol>.Empty;

	public override bool IsStatic => true;

	public SynthesizedIntrinsicOperatorSymbol(TypeSymbol leftType, string name, TypeSymbol rightType, TypeSymbol returnType)
	{
		if (leftType.Equals(rightType))
		{
			containingType = leftType;
		}
		else if (rightType.Equals(returnType))
		{
			containingType = rightType;
		}
		else
		{
			containingType = leftType;
		}
		this.name = name;
		this.returnType = returnType;
		parameters = new ParameterSymbol[2]
		{
			new SynthesizedOperatorParameterSymbol(this, leftType, 0, "left"),
			new SynthesizedOperatorParameterSymbol(this, rightType, 1, "right")
		}.AsImmutableOrNull();
	}

	public SynthesizedIntrinsicOperatorSymbol(TypeSymbol container, string name, TypeSymbol returnType)
	{
		containingType = container;
		this.name = name;
		this.returnType = returnType;
		parameters = new ParameterSymbol[1]
		{
			new SynthesizedOperatorParameterSymbol(this, container, 0, "value")
		}.AsImmutableOrNull();
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is SynthesizedIntrinsicOperatorSymbol synthesizedIntrinsicOperatorSymbol))
		{
			return false;
		}
		if (parameters.Length == synthesizedIntrinsicOperatorSymbol.parameters.Length && string.Equals(name, synthesizedIntrinsicOperatorSymbol.name, StringComparison.Ordinal) && containingType == synthesizedIntrinsicOperatorSymbol.containingType && returnType == synthesizedIntrinsicOperatorSymbol.returnType)
		{
			for (int i = 0; i < parameters.Length; i++)
			{
				if (parameters[i].Type != synthesizedIntrinsicOperatorSymbol.parameters[i].Type)
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
		return Hash.Combine(name.GetHashCode(), Hash.Combine(containingType.GetHashCode(), parameters.Length));
	}
}
