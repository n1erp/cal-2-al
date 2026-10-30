using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class BoundPropertyValue : BoundExpression
{
	internal virtual object ValueObject => null;

	internal virtual string ValueText => string.Empty;

	protected override OperationKind ExpressionKind => OperationKind.None;

	protected BoundPropertyValue(BoundKind kind, SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(kind, syntax, type, hasErrors)
	{
	}

	protected BoundPropertyValue(BoundKind kind, SyntaxNode syntax, TypeSymbol type)
		: base(kind, syntax, type)
	{
	}

	internal PropertyTypeInfo GetPropertyTypeInfo()
	{
		return (Syntax as PropertyValueSyntax).GetPropertyTypeInfo();
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitNoneOperation(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitNoneOperation(this, argument);
	}
}
