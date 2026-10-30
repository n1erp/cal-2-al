using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class ViewExpressionVisitor : ExpressionVisitor
{
	private readonly ViewSymbol viewSymbol;

	private readonly PropertySymbol property;

	private readonly DiagnosticBag diagnostics;

	public ViewExpressionVisitor(ViewSymbol viewSymbol, PropertySymbol property, DiagnosticBag diagnostics)
	{
		this.diagnostics = diagnostics;
		this.viewSymbol = viewSymbol;
		this.property = property;
	}

	internal override void VisitGlobal(BoundGlobal node)
	{
		diagnostics.Add(ErrorCode.ERR_ViewsBooleanExpressionsShouldNotUseVariables, node.Syntax.GetLocation(), node.GlobalSymbol.Name, property.Name, viewSymbol.Name);
	}
}
