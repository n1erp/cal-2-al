using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class BuckStopsHereBinder : Binder
{
	internal override ImportChain ImportChain => null;

	internal override QuickAttributeChecker QuickAttributeChecker => QuickAttributeChecker.Predefined;

	internal override uint LocalScopeDepth => 0u;

	internal override ConstantFieldsInProgress ConstantFieldsInProgress => ConstantFieldsInProgress.Empty;

	internal override ConsList<FieldSymbol> FieldsBeingBound => ConsList<FieldSymbol>.Empty;

	internal override LocalSymbol LocalInProgress => null;

	internal override bool IsInMethodBody => false;

	internal override bool IsDirectlyInIterator => false;

	internal override bool IsIndirectlyInIterator => false;

	internal override GeneratedLabelSymbol BreakLabel => null;

	internal override GeneratedLabelSymbol ContinueLabel => null;

	internal override BoundExpression ConditionalReceiverExpression => null;

	internal override Symbol ContainingMemberOrLambda => null;

	internal override ImmutableHashSet<Symbol> LockedOrDisposedVariables => ImmutableHashSet.Create<Symbol>();

	internal BuckStopsHereBinder(CSharpCompilation compilation)
		: base(compilation)
	{
	}

	internal override Imports GetImports(ConsList<Symbol> basesBeingResolved)
	{
		return Imports.Empty;
	}

	protected override SourceLocalSymbol LookupLocal(SyntaxToken nameToken)
	{
		return null;
	}

	protected override LocalFunctionSymbol LookupLocalFunction(SyntaxToken nameToken)
	{
		return null;
	}

	internal override bool IsAccessibleHelper(Symbol symbol, TypeSymbol accessThroughType, out bool failedThroughTypeCheck, ref HashSet<DiagnosticInfo> useSiteDiagnostics, ConsList<Symbol> basesBeingResolved)
	{
		failedThroughTypeCheck = false;
		return Binder.IsSymbolAccessibleConditional(symbol, Compilation.Assembly, ref useSiteDiagnostics);
	}

	protected override bool IsUnboundTypeAllowed(GenericNameSyntax syntax)
	{
		return false;
	}

	internal override TypeSymbol GetIteratorElementType(YieldStatementSyntax node, DiagnosticBag diagnostics)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override Binder GetBinder(SyntaxNode node)
	{
		return null;
	}

	internal override ImmutableArray<LocalSymbol> GetDeclaredLocalsForScope(SyntaxNode scopeDesignator)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override ImmutableArray<LocalFunctionSymbol> GetDeclaredLocalFunctionsForScope(CSharpSyntaxNode scopeDesignator)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override BoundStatement BindSwitchExpressionAndSections(SwitchStatementSyntax node, Binder originalBinder, DiagnosticBag diagnostics)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override void BindPatternSwitchLabelForInference(CasePatternSwitchLabelSyntax node, DiagnosticBag diagnostics)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override BoundForStatement BindForParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override BoundStatement BindForEachParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override BoundStatement BindForEachDeconstruction(DiagnosticBag diagnostics, Binder originalBinder)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override BoundWhileStatement BindWhileParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override BoundDoStatement BindDoParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override BoundStatement BindUsingStatementParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override BoundStatement BindLockStatementParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		throw ExceptionUtilities.Unreachable;
	}
}
