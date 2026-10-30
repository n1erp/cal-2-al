using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class CompilationScopeOnBoundPropertyValueVisitor : BoundTreeWalkerWithStackGuard
{
	private readonly CompilationTarget compilationTarget;

	private readonly DiagnosticBag diagnostics;

	private readonly Location location;

	private bool reportedOnce;

	internal CompilationScopeOnBoundPropertyValueVisitor(Location location, CompilationTarget compilationTarget, DiagnosticBag diagnostics)
	{
		this.location = location;
		this.compilationTarget = compilationTarget;
		this.diagnostics = diagnostics;
	}

	public override BoundNode VisitFieldAccess(BoundFieldAccess node)
	{
		if (node != null)
		{
			CheckCompilationScope(node.FieldSymbol?.ContainingSymbol);
		}
		return base.VisitFieldAccess(node);
	}

	public override BoundNode VisitApplicationObjectAccess(BoundApplicationObjectAccess node)
	{
		if (node != null)
		{
			CheckCompilationScope(node.ApplicationObjectTypeSymbol);
		}
		return base.VisitApplicationObjectAccess(node);
	}

	public override BoundNode VisitApplicationObjectReferencePropertyValue(BoundApplicationObjectReferencePropertyValue node)
	{
		if (node != null)
		{
			CheckCompilationScope(node.Value);
		}
		return base.VisitApplicationObjectReferencePropertyValue(node);
	}

	public override BoundNode VisitMemberReferencePropertyValue(BoundMemberReferencePropertyValue node)
	{
		if (node != null)
		{
			CheckCompilationScope(node.Type);
		}
		return base.VisitMemberReferencePropertyValue(node);
	}

	private void CheckCompilationScope(Symbol containingSymbol)
	{
		if (!(containingSymbol == null) && !reportedOnce)
		{
			if (!containingSymbol.CanCompileWithTarget(compilationTarget))
			{
				diagnostics.Add(ErrorCode.ERR_SymbolCannotBeUsedInThisContext, location, containingSymbol.Name, compilationTarget.ToString());
				reportedOnce = true;
			}
			else if (containingSymbol.Kind != SymbolKind.Table)
			{
				TableTypeSymbol relatedTableSymbol = containingSymbol.GetRelatedTableSymbol();
				CheckCompilationScope(relatedTableSymbol);
			}
		}
	}
}
