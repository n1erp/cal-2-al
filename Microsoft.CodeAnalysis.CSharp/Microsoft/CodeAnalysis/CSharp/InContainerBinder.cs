using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class InContainerBinder : Binder
{
	private readonly NamespaceOrTypeSymbol _container;

	private readonly Func<ConsList<Symbol>, Imports> _computeImports;

	private Imports _lazyImports;

	private ImportChain _lazyImportChain;

	private QuickAttributeChecker _lazyQuickAttributeChecker;

	private readonly SyntaxList<UsingDirectiveSyntax> _usingsSyntax;

	internal NamespaceOrTypeSymbol Container => _container;

	internal override ImportChain ImportChain
	{
		get
		{
			if (_lazyImportChain == null)
			{
				ImportChain importChain = Next.ImportChain;
				if ((object)_container == null || _container.Kind == SymbolKind.Namespace)
				{
					importChain = new ImportChain(GetImports(null), importChain);
				}
				Interlocked.CompareExchange(ref _lazyImportChain, importChain, null);
			}
			return _lazyImportChain;
		}
	}

	internal override QuickAttributeChecker QuickAttributeChecker
	{
		get
		{
			if (_lazyQuickAttributeChecker == null)
			{
				QuickAttributeChecker quickAttributeChecker = Next.QuickAttributeChecker;
				if ((object)_container == null || _container.Kind == SymbolKind.Namespace)
				{
					quickAttributeChecker = quickAttributeChecker.AddAliasesIfAny(_usingsSyntax);
				}
				_lazyQuickAttributeChecker = quickAttributeChecker;
			}
			return _lazyQuickAttributeChecker;
		}
	}

	internal override Symbol ContainingMemberOrLambda
	{
		get
		{
			if (!(_container is MergedNamespaceSymbol mergedNamespaceSymbol))
			{
				return _container;
			}
			return mergedNamespaceSymbol.GetConstituentForCompilation(Compilation);
		}
	}

	private bool IsSubmissionClass
	{
		get
		{
			NamespaceOrTypeSymbol container = _container;
			if ((object)container != null && container.Kind == SymbolKind.NamedType)
			{
				return ((NamedTypeSymbol)_container).IsSubmissionClass;
			}
			return false;
		}
	}

	private bool IsScriptClass
	{
		get
		{
			NamespaceOrTypeSymbol container = _container;
			if ((object)container != null && container.Kind == SymbolKind.NamedType)
			{
				return ((NamedTypeSymbol)_container).IsScriptClass;
			}
			return false;
		}
	}

	internal override bool SupportsExtensionMethods => true;

	internal override uint LocalScopeDepth => 0u;

	internal InContainerBinder(NamespaceOrTypeSymbol container, Binder next, CSharpSyntaxNode declarationSyntax, bool inUsing)
		: base(next)
	{
		InContainerBinder binder = this;
		_container = container;
		_computeImports = (ConsList<Symbol> basesBeingResolved) => Imports.FromSyntax(declarationSyntax, binder, basesBeingResolved, inUsing);
		if (!inUsing)
		{
			if (declarationSyntax.Kind() == SyntaxKind.CompilationUnit)
			{
				CompilationUnitSyntax compilationUnitSyntax = (CompilationUnitSyntax)declarationSyntax;
				_usingsSyntax = compilationUnitSyntax.Usings;
			}
			else if (declarationSyntax.Kind() == SyntaxKind.NamespaceDeclaration)
			{
				NamespaceDeclarationSyntax namespaceDeclarationSyntax = (NamespaceDeclarationSyntax)declarationSyntax;
				_usingsSyntax = namespaceDeclarationSyntax.Usings;
			}
		}
	}

	internal InContainerBinder(NamespaceOrTypeSymbol container, Binder next, Imports imports = null)
		: base(next)
	{
		_container = container;
		_lazyImports = imports ?? Imports.Empty;
	}

	internal InContainerBinder(Binder next, Func<ConsList<Symbol>, Imports> computeImports)
		: base(next)
	{
		_container = null;
		_computeImports = computeImports;
	}

	internal override Imports GetImports(ConsList<Symbol> basesBeingResolved)
	{
		if (_lazyImports == null)
		{
			Interlocked.CompareExchange(ref _lazyImports, _computeImports(basesBeingResolved), null);
		}
		return _lazyImports;
	}

	internal override bool IsAccessibleHelper(Symbol symbol, TypeSymbol accessThroughType, out bool failedThroughTypeCheck, ref HashSet<DiagnosticInfo> useSiteDiagnostics, ConsList<Symbol> basesBeingResolved)
	{
		if (_container is NamedTypeSymbol within)
		{
			return IsSymbolAccessibleConditional(symbol, within, accessThroughType, out failedThroughTypeCheck, ref useSiteDiagnostics);
		}
		return Next.IsAccessibleHelper(symbol, accessThroughType, out failedThroughTypeCheck, ref useSiteDiagnostics, basesBeingResolved);
	}

	internal override void GetCandidateExtensionMethods(bool searchUsingsNotNamespace, ArrayBuilder<MethodSymbol> methods, string name, int arity, LookupOptions options, Binder originalBinder)
	{
		if (searchUsingsNotNamespace)
		{
			GetImports(null).LookupExtensionMethodsInUsings(methods, name, arity, options, originalBinder);
			return;
		}
		NamespaceOrTypeSymbol container = _container;
		if ((object)container != null && container.Kind == SymbolKind.Namespace)
		{
			((NamespaceSymbol)_container).GetExtensionMethods(methods, name, arity, options);
		}
		else if (IsSubmissionClass)
		{
			for (CSharpCompilation cSharpCompilation = Compilation; cSharpCompilation != null; cSharpCompilation = cSharpCompilation.PreviousSubmission)
			{
				cSharpCompilation.ScriptClass?.GetExtensionMethods(methods, name, arity, options);
			}
		}
	}

	internal override TypeSymbol GetIteratorElementType(YieldStatementSyntax node, DiagnosticBag diagnostics)
	{
		if (IsScriptClass)
		{
			return Compilation.GetSpecialType(SpecialType.System_Object);
		}
		return Next.GetIteratorElementType(node, diagnostics);
	}

	internal override void LookupSymbolsInSingleBinder(LookupResult result, string name, int arity, ConsList<Symbol> basesBeingResolved, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (IsSubmissionClass)
		{
			LookupMembersInternal(result, _container, name, arity, basesBeingResolved, options, originalBinder, diagnose, ref useSiteDiagnostics);
			return;
		}
		Imports imports = GetImports(basesBeingResolved);
		if ((options & LookupOptions.NamespaceAliasesOnly) == 0 && _container != null)
		{
			LookupMembersInternal(result, _container, name, arity, basesBeingResolved, options, originalBinder, diagnose, ref useSiteDiagnostics);
			if (result.IsMultiViable)
			{
				if (arity == 0 && imports.IsUsingAlias(name, originalBinder.IsSemanticModelBinder))
				{
					CSDiagnosticInfo errorInfo = new CSDiagnosticInfo(ErrorCode.ERR_ConflictAliasAndMember, name, _container);
					ExtendedErrorTypeSymbol symbol = new ExtendedErrorTypeSymbol((NamespaceOrTypeSymbol)null, name, arity, (DiagnosticInfo)errorInfo, true, false);
					result.SetFrom(LookupResult.Good(symbol));
				}
				return;
			}
		}
		imports.LookupSymbol(originalBinder, result, name, arity, basesBeingResolved, options, diagnose, ref useSiteDiagnostics);
	}

	protected override void AddLookupSymbolsInfoInSingleBinder(LookupSymbolsInfo result, LookupOptions options, Binder originalBinder)
	{
		if (_container != null)
		{
			AddMemberLookupSymbolsInfo(result, _container, options, originalBinder);
		}
		if (!IsSubmissionClass && (options & LookupOptions.LabelsOnly) == 0)
		{
			GetImports(null).AddLookupSymbolsInfo(result, options, originalBinder);
		}
	}

	protected override SourceLocalSymbol LookupLocal(SyntaxToken nameToken)
	{
		return null;
	}

	protected override LocalFunctionSymbol LookupLocalFunction(SyntaxToken nameToken)
	{
		return null;
	}
}
