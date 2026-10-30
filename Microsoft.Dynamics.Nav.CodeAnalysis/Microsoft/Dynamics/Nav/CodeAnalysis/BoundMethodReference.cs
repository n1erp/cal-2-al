using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundMethodReference : BoundExpression
{
	public MethodSymbol Method { get; private set; }

	protected override OperationKind ExpressionKind => OperationKind.None;

	public BoundMethodReference(SyntaxNode syntax, MethodSymbol method, TypeSymbol type, bool hasErrors)
		: base(BoundKind.MethodReference, syntax, type, hasErrors)
	{
		Method = method;
	}

	public BoundMethodReference(SyntaxNode syntax, MethodSymbol method, TypeSymbol type)
		: base(BoundKind.MethodReference, syntax, type)
	{
		Method = method;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitMethodReference(this);
	}

	public BoundMethodReference Update(MethodSymbol method, TypeSymbol type)
	{
		if (method != Method || type != Type)
		{
			return new BoundMethodReference(Syntax, method, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
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
