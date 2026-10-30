using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal sealed class SimpleNavTypeSymbol : NavTypeSymbol
{
	private readonly Func<TypeSymbol> getBaseType;

	public override string Name { get; }

	public override string MetadataName { get; }

	internal override TypeSymbol BaseType => getBaseType?.Invoke();

	public override TypeCategoryKind TypeCategory
	{
		get
		{
			if (BaseType != null)
			{
				return BaseType.TypeCategory;
			}
			if (NavTypeKind == NavTypeKind.Option)
			{
				return TypeCategoryKind.Option;
			}
			return TypeCategoryKind.Simple;
		}
	}

	public override VersionCompatibility Compatibility => BaseType?.Compatibility;

	public override SymbolCompilationScope CompilationScope => BaseType?.CompilationScope ?? base.CompilationScope;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitSimpleNavType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitSimpleNavType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitSimpleNavType(this, a);
	}

	internal SimpleNavTypeSymbol(NavTypeKind navTypeKind, Func<TypeSymbol> baseType = null, string name = null)
		: base(navTypeKind)
	{
		getBaseType = baseType;
		Name = name ?? navTypeKind.ToString();
		MetadataName = GetMetadataName(Name);
	}

	private static string GetMetadataName(string name)
	{
		if (name == NavTypeKind.Guid.ToString())
		{
			return "GUID";
		}
		if (name == NavTypeKind.RecordId.ToString())
		{
			return "RecordID";
		}
		return name;
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return ImmutableArray<Symbol>.Empty;
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return ImmutableArray<Symbol>.Empty;
	}
}
