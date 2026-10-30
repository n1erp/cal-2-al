using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class QueryColumnSemanticModel : MemberSemanticModel
{
	public override bool IsSpeculativeSemanticModel => false;

	public override int OriginalPositionForSpeculation => 0;

	private QueryColumnSemanticModel(Compilation compilation, Symbol owner, Binder rootBinder, SyntaxNode syntax)
		: base(compilation, syntax, owner, rootBinder)
	{
		DebugAssertHelper.Assert((object)owner != null);
		DebugAssertHelper.Assert(owner.Kind == SymbolKind.QueryColumn);
		DebugAssertHelper.Assert(syntax != null);
	}

	internal static QueryColumnSemanticModel Create(Compilation compilation, TypeSymbol owner, Binder rootBinder, QueryColumnSyntax syntax)
	{
		ExecutableCodeBinder rootBinder2 = new ExecutableCodeBinder(syntax, owner, rootBinder);
		return new QueryColumnSemanticModel(compilation, owner, rootBinder2, syntax);
	}

	internal override BoundNode Bind(Binder binder, SyntaxNode node, DiagnosticBag diagnostics)
	{
		if (node.Kind != SyntaxKind.QueryColumn)
		{
			return base.Bind(binder, node, diagnostics);
		}
		IdentifierNameSyntax relatedField = ((QueryColumnSyntax)node).RelatedField;
		if (relatedField == null)
		{
			return base.Bind(binder, node, diagnostics);
		}
		return binder.BindExpression(relatedField, diagnostics);
	}

	internal override bool TryGetSpeculativeSemanticModelForMethodBodyCore(SyntaxTreeSemanticModel parentModel, int position, MemberSyntax method, out SemanticModel speculativeModel)
	{
		speculativeModel = null;
		return false;
	}

	protected override SyntaxNode GetBindingRoot(SyntaxNode node)
	{
		return Root;
	}

	internal override SymbolInfo GetSymbolInfoWorker(SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (node.Kind == SyntaxKind.QueryColumn)
		{
			return new SymbolInfo(MemberSymbol);
		}
		return base.GetSymbolInfoWorker(node, cancellationToken);
	}
}
