using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class AliasSymbol : Symbol, IAliasSymbol, ISymbol, IEquatable<ISymbol>
{
	private readonly SyntaxToken _aliasName;

	private readonly Binder _binder;

	private SymbolCompletionState _state;

	private NamespaceOrTypeSymbol _aliasTarget;

	private readonly ImmutableArray<Location> _locations;

	private readonly NameSyntax _aliasTargetName;

	private readonly bool _isExtern;

	private DiagnosticBag _aliasTargetDiagnostics;

	public override string Name => _aliasName.ValueText;

	public override SymbolKind Kind => SymbolKind.Alias;

	public NamespaceOrTypeSymbol Target => GetAliasTarget(null);

	public override ImmutableArray<Location> Locations => _locations;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => Symbol.GetDeclaringSyntaxReferenceHelper<UsingDirectiveSyntax>(_locations);

	public override bool IsExtern => _isExtern;

	public override bool IsSealed => false;

	public override bool IsAbstract => false;

	public override bool IsOverride => false;

	public override bool IsVirtual => false;

	public override bool IsStatic => false;

	internal sealed override ObsoleteAttributeData ObsoleteAttributeData => null;

	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public override Symbol ContainingSymbol => _binder.ContainingMemberOrLambda;

	internal DiagnosticBag AliasTargetDiagnostics
	{
		get
		{
			GetAliasTarget(null);
			return _aliasTargetDiagnostics;
		}
	}

	internal override bool RequiresCompletion => true;

	INamespaceOrTypeSymbol IAliasSymbol.Target => Target;

	private AliasSymbol(Binder binder, NamespaceOrTypeSymbol target, SyntaxToken aliasName, ImmutableArray<Location> locations)
	{
		_aliasName = aliasName;
		_locations = locations;
		_aliasTarget = target;
		_binder = binder;
		_state.NotePartComplete(CompletionPart.StartBaseType);
	}

	private AliasSymbol(Binder binder, SyntaxToken aliasName)
	{
		_aliasName = aliasName;
		_locations = ImmutableArray.Create(aliasName.GetLocation());
		_binder = binder;
	}

	internal AliasSymbol(Binder binder, UsingDirectiveSyntax syntax)
		: this(binder, syntax.Alias.Name.Identifier)
	{
		_aliasTargetName = syntax.Name;
	}

	internal AliasSymbol(Binder binder, ExternAliasDirectiveSyntax syntax)
		: this(binder, syntax.Identifier)
	{
		_isExtern = true;
	}

	internal static AliasSymbol CreateGlobalNamespaceAlias(NamespaceSymbol globalNamespace, Binder globalNamespaceBinder)
	{
		SyntaxToken aliasName = SyntaxFactory.Identifier(SyntaxFactory.TriviaList(), SyntaxKind.GlobalKeyword, "global", "global", SyntaxFactory.TriviaList());
		return new AliasSymbol(globalNamespaceBinder, globalNamespace, aliasName, ImmutableArray<Location>.Empty);
	}

	internal static AliasSymbol CreateCustomDebugInfoAlias(NamespaceOrTypeSymbol targetSymbol, SyntaxToken aliasToken, Binder binder)
	{
		return new AliasSymbol(binder, targetSymbol, aliasToken, ImmutableArray.Create(aliasToken.GetLocation()));
	}

	internal AliasSymbol ToNewSubmission(CSharpCompilation compilation)
	{
		NamespaceOrTypeSymbol aliasTarget = GetAliasTarget(null);
		if (aliasTarget.Kind != SymbolKind.Namespace)
		{
			return this;
		}
		NamespaceSymbol globalNamespace = compilation.GlobalNamespace;
		NamespaceSymbol target = Imports.ExpandPreviousSubmissionNamespace((NamespaceSymbol)aliasTarget, globalNamespace);
		return new AliasSymbol(new InContainerBinder(globalNamespace, new BuckStopsHereBinder(compilation)), target, _aliasName, _locations);
	}

	internal override TResult Accept<TArg, TResult>(CSharpSymbolVisitor<TArg, TResult> visitor, TArg a)
	{
		return visitor.VisitAlias(this, a);
	}

	public override void Accept(CSharpSymbolVisitor visitor)
	{
		visitor.VisitAlias(this);
	}

	public override TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)
	{
		return visitor.VisitAlias(this);
	}

	internal NamespaceOrTypeSymbol GetAliasTarget(ConsList<Symbol> basesBeingResolved)
	{
		if (!_state.HasComplete(CompletionPart.StartBaseType))
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			NamespaceOrTypeSymbol value = (IsExtern ? ResolveExternAliasTarget(instance) : ResolveAliasTarget(_binder, _aliasTargetName, instance, basesBeingResolved));
			if ((object)Interlocked.CompareExchange(ref _aliasTarget, value, null) == null)
			{
				Interlocked.Exchange(ref _aliasTargetDiagnostics, instance);
				_state.NotePartComplete(CompletionPart.StartBaseType);
			}
			else
			{
				instance.Free();
				_state.SpinWaitComplete(CompletionPart.StartBaseType, default);
			}
		}
		return _aliasTarget;
	}

	internal void CheckConstraints(DiagnosticBag diagnostics)
	{
		if (Target is TypeSymbol type && _locations.Length > 0)
		{
			TypeConversions conversions = new TypeConversions(ContainingAssembly.CorLibrary);
			type.CheckAllConstraints(conversions, _locations[0], diagnostics);
		}
	}

	private NamespaceSymbol ResolveExternAliasTarget(DiagnosticBag diagnostics)
	{
		if (!_binder.Compilation.GetExternAliasTarget(_aliasName.ValueText, out var @namespace))
		{
			diagnostics.Add(ErrorCode.ERR_BadExternAlias, _aliasName.GetLocation(), _aliasName.ValueText);
		}
		return @namespace;
	}

	private static NamespaceOrTypeSymbol ResolveAliasTarget(Binder binder, NameSyntax syntax, DiagnosticBag diagnostics, ConsList<Symbol> basesBeingResolved)
	{
		return binder.WithAdditionalFlags(BinderFlags.SuppressConstraintChecks | BinderFlags.SuppressObsoleteChecks).BindNamespaceOrTypeSymbol(syntax, diagnostics, basesBeingResolved);
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (obj == null)
		{
			return false;
		}
		if (obj is AliasSymbol aliasSymbol && object.Equals(Locations.FirstOrDefault(), aliasSymbol.Locations.FirstOrDefault()))
		{
			return ContainingAssembly == aliasSymbol.ContainingAssembly;
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (Locations.Length > 0)
		{
			return Locations.First().GetHashCode();
		}
		return Name.GetHashCode();
	}

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitAlias(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitAlias(this);
	}
}
