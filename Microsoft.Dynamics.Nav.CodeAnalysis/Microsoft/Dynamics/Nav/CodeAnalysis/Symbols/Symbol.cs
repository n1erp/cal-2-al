using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal abstract class Symbol : ISymbol, IEquatable<ISymbol>
{
	internal virtual bool RequiresCompletion => false;

	public virtual string Name => string.Empty;

	public virtual string MetadataName => Name;

	public abstract SymbolKind Kind { get; }

	public abstract Symbol ContainingSymbol { get; }

	public virtual TypeSymbol ContainingType
	{
		get
		{
			Symbol containingSymbol = ContainingSymbol;
			TypeSymbol typeSymbol = containingSymbol as TypeSymbol;
			if ((object)typeSymbol == containingSymbol)
			{
				return typeSymbol;
			}
			return containingSymbol.ContainingType;
		}
	}

	internal virtual Compilation DeclaringCompilation
	{
		get
		{
			if (ContainingModule is SourceModuleSymbol sourceModuleSymbol)
			{
				return sourceModuleSymbol.DeclaringCompilation;
			}
			return null;
		}
	}

	internal virtual ModuleSymbol ContainingModule => ContainingSymbol?.ContainingModule;

	public Symbol OriginalDefinition => OriginalSymbolDefinition;

	protected virtual Symbol OriginalSymbolDefinition => this;

	public bool IsDefinition => (object)this == OriginalDefinition;

	public virtual bool IsDeprecated => false;

	public virtual bool IsObsolete => false;

	public virtual string ObsoleteReason => null;

	public virtual VersionCompatibility Compatibility => null;

	public abstract Accessibility DeclaredAccessibility { get; }

	public abstract ImmutableArray<Location> Locations { get; }

	public abstract ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

	public virtual SymbolCompilationScope CompilationScope => SymbolCompilationScope.Personalization;

	public virtual bool IsImplicitlyDeclared => false;

	internal bool HasUseSiteError
	{
		get
		{
			DiagnosticInfo useSiteDiagnostic = GetUseSiteDiagnostic();
			if (useSiteDiagnostic != null)
			{
				return useSiteDiagnostic.Severity == DiagnosticSeverity.Error;
			}
			return false;
		}
	}

	ISymbol ISymbol.ContainingSymbol => ContainingSymbol;

	ITypeSymbol ISymbol.ContainingType => ContainingType;

	public virtual IDictionary<string, PropertyTypeInfo> PropertyTypeInfos { get; } = ImmutableDictionary<string, PropertyTypeInfo>.Empty;

	public virtual ImmutableArray<PropertySymbol> Properties { get; } = ImmutableArray<PropertySymbol>.Empty;

	ImmutableArray<IPropertySymbol> ISymbol.Properties => StaticCast<IPropertySymbol>.From(Properties);

	ISymbol ISymbol.OriginalDefinition => OriginalDefinition;

	internal virtual void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		DebugAssertHelper.Assert(!RequiresCompletion);
	}

	internal virtual bool HasComplete(CompletionPart part)
	{
		DebugAssertHelper.Assert(!RequiresCompletion);
		return true;
	}

	internal string GetDebuggerDisplay()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0} {1}", Kind, ToDisplayString(SymbolDisplayFormat.TestFormat));
	}

	internal virtual LexicalSortKey GetLexicalSortKey()
	{
		ImmutableArray<Location> locations = Locations;
		Compilation declaringCompilation = DeclaringCompilation;
		DebugAssertHelper.Assert(declaringCompilation != null);
		if (locations.Length <= 0)
		{
			return LexicalSortKey.NotInSource;
		}
		return new LexicalSortKey(locations[0], declaringCompilation);
	}

	internal virtual SyntaxNode GetFirstSyntax()
	{
		return DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
	}

	internal static ImmutableArray<SyntaxReference> GetDeclaringSyntaxReferenceHelper<TNode>(ImmutableArray<Location> locations) where TNode : SyntaxNode
	{
		if (locations.IsEmpty)
		{
			return ImmutableArray<SyntaxReference>.Empty;
		}
		ArrayBuilder<SyntaxReference> instance = ArrayBuilder<SyntaxReference>.GetInstance();
		foreach (Location item in locations)
		{
			if (!item.IsInSource)
			{
				continue;
			}
			SyntaxToken syntaxToken = item.SourceTree.GetRoot().FindToken(item.SourceSpan.Start);
			if (syntaxToken.Kind != SyntaxKind.None)
			{
				SyntaxNode syntaxNode = syntaxToken.Parent.FirstAncestorOrSelf<TNode>();
				if (syntaxNode != null)
				{
					instance.Add(syntaxNode.GetReference());
				}
			}
		}
		return instance.ToImmutableAndFree();
	}

	public static bool operator ==(Symbol left, Symbol right)
	{
		if ((object)right == null)
		{
			return (object)left == null;
		}
		if ((object)left != right)
		{
			return right.Equals(left);
		}
		return true;
	}

	public static bool operator !=(Symbol left, Symbol right)
	{
		if ((object)right == null)
		{
			return (object)left != null;
		}
		if ((object)left != right)
		{
			return !right.Equals(left);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		return this == obj;
	}

	public bool Equals(ISymbol other)
	{
		return Equals((object)other);
	}

	public override int GetHashCode()
	{
		return RuntimeHelpers.GetHashCode(this);
	}

	public sealed override string ToString()
	{
		return ToDisplayString();
	}

	internal abstract TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a);

	internal Symbol()
	{
	}

	internal virtual bool IsDefinedInSourceTree(SyntaxTree tree, TextSpan? definedWithinSpan, CancellationToken cancellationToken = default(CancellationToken))
	{
		ImmutableArray<SyntaxReference> declaringSyntaxReferences = DeclaringSyntaxReferences;
		if (IsImplicitlyDeclared && declaringSyntaxReferences.Length == 0)
		{
			return ContainingSymbol.IsDefinedInSourceTree(tree, definedWithinSpan, cancellationToken);
		}
		foreach (SyntaxReference item in declaringSyntaxReferences)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (item.SyntaxTree == tree && (!definedWithinSpan.HasValue || item.Span.IntersectsWith(definedWithinSpan.Value)))
			{
				return true;
			}
		}
		return false;
	}

	internal void AddDeclarationDiagnostics(DiagnosticBag diagnostics)
	{
		if (!diagnostics.IsEmptyWithoutResolution)
		{
			Compilation declaringCompilation = DeclaringCompilation;
			DebugAssertHelper.Assert(declaringCompilation != null);
			declaringCompilation.DeclarationDiagnostics.AddRange(diagnostics);
		}
	}

	internal virtual DiagnosticInfo GetUseSiteDiagnostic()
	{
		return null;
	}

	internal DiagnosticInfo GetUseSiteDiagnosticForSymbolOrContainingType()
	{
		DiagnosticInfo useSiteDiagnostic = GetUseSiteDiagnostic();
		if (useSiteDiagnostic != null && useSiteDiagnostic.Severity == DiagnosticSeverity.Error)
		{
			return useSiteDiagnostic;
		}
		return ContainingType.GetUseSiteDiagnostic() ?? useSiteDiagnostic;
	}

	internal static bool ReportUseSiteDiagnostic(DiagnosticInfo info, DiagnosticBag diagnostics, Location location)
	{
		diagnostics.Add(info, location);
		return info.Severity == DiagnosticSeverity.Error;
	}

	public ImmutableArray<SymbolDisplayPart> ToDisplayParts(SymbolDisplayFormat format = null)
	{
		return SymbolDisplay.ToDisplayParts(this, format);
	}

	public string ToDisplayString(SymbolDisplayFormat format = null)
	{
		return SymbolDisplay.ToDisplayString(this, format);
	}

	public string ToMinimalDisplayString(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null)
	{
		return SymbolDisplay.ToMinimalDisplayString(this, semanticModel, position, format);
	}

	public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null)
	{
		return SymbolDisplay.ToMinimalDisplayParts(this, semanticModel, position, format);
	}

	public abstract void Accept(SymbolVisitor visitor);

	public abstract TResult Accept<TResult>(SymbolVisitor<TResult> visitor);

	protected void LazySetSymbol(Action<DiagnosticBag> setSymbol, CompletionPart completionValue, ref SymbolCompletionState state)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		try
		{
			setSymbol(instance);
			AddDeclarationDiagnostics(instance);
		}
		finally
		{
			state.NotePartComplete(completionValue);
			instance.Free();
		}
	}

	protected void NotifyCompilationAboutSymbolCompletion(ref SymbolCompletionState state)
	{
		try
		{
			DeclaringCompilation.SymbolDeclaredEvent(this);
		}
		finally
		{
			state.NotePartComplete(CompletionPart.SymbolCompleted);
		}
	}

	public virtual string GetDocumentation()
	{
		return string.Empty;
	}

	public PropertyTypeInfo GetPropertyTypeInfo(string name)
	{
		PropertyTypeInfo value = null;
		IDictionary<string, PropertyTypeInfo> propertyTypeInfos = PropertyTypeInfos;
		if (propertyTypeInfos != null && propertyTypeInfos.TryGetValue(name.ToUpperInvariant(), out value))
		{
			return value;
		}
		return null;
	}

	public PropertySymbol GetProperty(PropertyKind kind)
	{
		for (int i = 0; i < Properties.Length; i++)
		{
			if (Properties[i].PropertyKind == kind)
			{
				return Properties[i];
			}
		}
		return null;
	}

	IPropertySymbol ISymbol.GetProperty(PropertyKind kind)
	{
		return GetProperty(kind);
	}
}
