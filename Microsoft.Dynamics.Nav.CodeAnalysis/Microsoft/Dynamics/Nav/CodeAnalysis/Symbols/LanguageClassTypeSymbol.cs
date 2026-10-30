using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class LanguageClassTypeSymbol : TypeSymbol, IClassTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>
{
	private readonly Lazy<ImmutableArray<Symbol>> lazyMembers;

	public override string Name { get; }

	public override TypeCategoryKind TypeCategory { get; }

	public override NavTypeKind NavTypeKind { get; }

	public override SymbolCompilationScope CompilationScope { get; }

	public bool ImplicitMemberAccess { get; }

	public override VersionCompatibility Compatibility { get; }

	public override Accessibility DeclaredAccessibility => Accessibility.Public;

	public override SymbolKind Kind { get; }

	public override Symbol ContainingSymbol => NavCorLib.Instance;

	public override TypeSymbol ContainingType => ContainingSymbol as TypeSymbol;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitLanguageClassType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitLanguageClassType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitLanguageClassType(this, a);
	}

	internal LanguageClassTypeSymbol(string name, bool implicitMemberAccess, TypeCategoryKind typeCategory, NavTypeKind kind, SymbolCompilationScope compilationScope, Func<Symbol, Symbol[]> getMethods, VersionCompatibility compatibility = null)
	{
		LanguageClassTypeSymbol arg = this;
		Name = name;
		ImplicitMemberAccess = implicitMemberAccess;
		Kind = SymbolKind.Class;
		TypeCategory = typeCategory;
		NavTypeKind = kind;
		CompilationScope = compilationScope;
		Compatibility = compatibility;
		lazyMembers = new Lazy<ImmutableArray<Symbol>>(() => getMethods(arg).AsImmutableOrEmpty());
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return lazyMembers.Value;
	}
}
