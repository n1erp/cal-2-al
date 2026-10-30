using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class NamespaceOrTypeSymbol : Symbol, INamespaceOrTypeSymbol, ISymbol, IEquatable<ISymbol>
{
	public bool IsNamespace => Kind == SymbolKind.Namespace;

	public bool IsType => !IsNamespace;

	public sealed override bool IsVirtual => false;

	public sealed override bool IsOverride => false;

	public sealed override bool IsExtern => false;

	internal NamespaceOrTypeSymbol()
	{
	}

	public abstract ImmutableArray<Symbol> GetMembers();

	internal virtual ImmutableArray<Symbol> GetMembersUnordered()
	{
		return GetMembers();
	}

	public abstract ImmutableArray<Symbol> GetMembers(string name);

	internal virtual ImmutableArray<NamedTypeSymbol> GetTypeMembersUnordered()
	{
		return GetTypeMembers();
	}

	public abstract ImmutableArray<NamedTypeSymbol> GetTypeMembers();

	public abstract ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name);

	public virtual ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name, int arity)
	{
		return GetTypeMembers(name).WhereAsArray((NamedTypeSymbol t) => t.Arity == arity);
	}

	internal SourceNamedTypeSymbol GetSourceTypeMember(TypeDeclarationSyntax syntax)
	{
		return GetSourceTypeMember(syntax.Identifier.ValueText, syntax.Arity, syntax.Kind(), syntax);
	}

	internal SourceNamedTypeSymbol GetSourceTypeMember(DelegateDeclarationSyntax syntax)
	{
		return GetSourceTypeMember(syntax.Identifier.ValueText, syntax.Arity, syntax.Kind(), syntax);
	}

	internal SourceNamedTypeSymbol GetSourceTypeMember(string name, int arity, SyntaxKind kind, CSharpSyntaxNode syntax)
	{
		TypeKind typeKind = kind.ToDeclarationKind().ToTypeKind();
		foreach (NamedTypeSymbol typeMember in GetTypeMembers(name, arity))
		{
			if (!(typeMember is SourceNamedTypeSymbol sourceNamedTypeSymbol) || sourceNamedTypeSymbol.TypeKind != typeKind)
			{
				continue;
			}
			if (syntax != null)
			{
				foreach (Location location in sourceNamedTypeSymbol.Locations)
				{
					if (location.IsInSource && location.SourceTree == syntax.SyntaxTree && syntax.Span.Contains(location.SourceSpan))
					{
						return sourceNamedTypeSymbol;
					}
				}
				continue;
			}
			return sourceNamedTypeSymbol;
		}
		return null;
	}

	internal virtual NamedTypeSymbol LookupMetadataType(ref MetadataTypeName emittedTypeName)
	{
		if (Kind == SymbolKind.ErrorType)
		{
			return new MissingMetadataTypeSymbol.Nested((NamedTypeSymbol)this, ref emittedTypeName);
		}
		NamedTypeSymbol namedTypeSymbol = null;
		bool isNamespace = IsNamespace;
		if (emittedTypeName.IsMangled && (emittedTypeName.ForcedArity == -1 || emittedTypeName.ForcedArity == emittedTypeName.InferredArity))
		{
			foreach (NamedTypeSymbol typeMember in GetTypeMembers(emittedTypeName.UnmangledTypeName))
			{
				if (emittedTypeName.InferredArity == typeMember.Arity && typeMember.MangleName)
				{
					if ((object)namedTypeSymbol != null)
					{
						namedTypeSymbol = null;
						break;
					}
					namedTypeSymbol = typeMember;
				}
			}
		}
		int num = emittedTypeName.ForcedArity;
		if (emittedTypeName.UseCLSCompliantNameArityEncoding)
		{
			if (emittedTypeName.InferredArity > 0)
			{
				goto IL_0100;
			}
			if (num == -1)
			{
				num = 0;
			}
			else if (num != 0)
			{
				goto IL_0100;
			}
		}
		foreach (NamedTypeSymbol typeMember2 in GetTypeMembers(emittedTypeName.TypeName))
		{
			if (!typeMember2.MangleName && (num == -1 || num == typeMember2.Arity))
			{
				if ((object)namedTypeSymbol != null)
				{
					namedTypeSymbol = null;
					break;
				}
				namedTypeSymbol = typeMember2;
			}
		}
		goto IL_0100;
		IL_0100:
		if ((object)namedTypeSymbol == null)
		{
			if (isNamespace)
			{
				return new MissingMetadataTypeSymbol.TopLevel(ContainingModule, ref emittedTypeName);
			}
			return new MissingMetadataTypeSymbol.Nested((NamedTypeSymbol)this, ref emittedTypeName);
		}
		return namedTypeSymbol;
	}

	internal IEnumerable<NamespaceOrTypeSymbol> GetNamespaceOrTypeByQualifiedName(IEnumerable<string> qualifiedName)
	{
		NamespaceOrTypeSymbol namespaceOrTypeSymbol = this;
		IEnumerable<NamespaceOrTypeSymbol> enumerable = null;
		foreach (string item in qualifiedName)
		{
			if (enumerable != null)
			{
				namespaceOrTypeSymbol = enumerable.OfMinimalArity();
				if ((object)namespaceOrTypeSymbol == null)
				{
					return SpecializedCollections.EmptyEnumerable<NamespaceOrTypeSymbol>();
				}
			}
			enumerable = namespaceOrTypeSymbol.GetMembers(item).OfType<NamespaceOrTypeSymbol>();
		}
		return enumerable;
	}

	ImmutableArray<ISymbol> INamespaceOrTypeSymbol.GetMembers()
	{
		return StaticCast<ISymbol>.From(GetMembers());
	}

	ImmutableArray<ISymbol> INamespaceOrTypeSymbol.GetMembers(string name)
	{
		return StaticCast<ISymbol>.From(GetMembers(name));
	}

	ImmutableArray<INamedTypeSymbol> INamespaceOrTypeSymbol.GetTypeMembers()
	{
		return StaticCast<INamedTypeSymbol>.From(GetTypeMembers());
	}

	ImmutableArray<INamedTypeSymbol> INamespaceOrTypeSymbol.GetTypeMembers(string name)
	{
		return StaticCast<INamedTypeSymbol>.From(GetTypeMembers(name));
	}

	ImmutableArray<INamedTypeSymbol> INamespaceOrTypeSymbol.GetTypeMembers(string name, int arity)
	{
		return StaticCast<INamedTypeSymbol>.From(GetTypeMembers(name, arity));
	}
}
