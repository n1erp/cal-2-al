using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class CSharpCustomModifier : CustomModifier, ICustomModifier
{
	private class OptionalCustomModifier : CSharpCustomModifier
	{
		public override bool IsOptional => true;

		public OptionalCustomModifier(NamedTypeSymbol modifier)
			: base(modifier)
		{
		}

		public override int GetHashCode()
		{
			return modifier.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			if (obj is OptionalCustomModifier optionalCustomModifier)
			{
				return optionalCustomModifier.modifier.Equals(modifier);
			}
			return false;
		}
	}

	private class RequiredCustomModifier : CSharpCustomModifier
	{
		public override bool IsOptional => false;

		public RequiredCustomModifier(NamedTypeSymbol modifier)
			: base(modifier)
		{
		}

		public override int GetHashCode()
		{
			return modifier.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			if (obj is RequiredCustomModifier requiredCustomModifier)
			{
				return requiredCustomModifier.modifier.Equals(modifier);
			}
			return false;
		}
	}

	protected readonly NamedTypeSymbol modifier;

	bool ICustomModifier.IsOptional => IsOptional;

	public override INamedTypeSymbol Modifier => modifier;

	ITypeReference ICustomModifier.GetModifier(EmitContext context)
	{
		return ((PEModuleBuilder)context.Module).Translate(Modifier, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
	}

	private CSharpCustomModifier(NamedTypeSymbol modifier)
	{
		this.modifier = modifier;
	}

	public abstract override int GetHashCode();

	public abstract override bool Equals(object obj);

	internal static CustomModifier CreateOptional(NamedTypeSymbol modifier)
	{
		return new OptionalCustomModifier(modifier);
	}

	internal static CustomModifier CreateRequired(NamedTypeSymbol modifier)
	{
		return new RequiredCustomModifier(modifier);
	}

	internal static ImmutableArray<CustomModifier> Convert(ImmutableArray<ModifierInfo<TypeSymbol>> customModifiers)
	{
		if (customModifiers.IsDefault)
		{
			return ImmutableArray<CustomModifier>.Empty;
		}
		return customModifiers.SelectAsArray(Convert);
	}

	private static CustomModifier Convert(ModifierInfo<TypeSymbol> customModifier)
	{
		NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)customModifier.Modifier;
		if (!customModifier.IsOptional)
		{
			return CreateRequired(namedTypeSymbol);
		}
		return CreateOptional(namedTypeSymbol);
	}
}
