using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundTestFieldAccess : BoundExpression, ITestFieldAccess, IOperation
{
	private readonly LookupResultKind _ResultKind;

	public override Symbol ExpressionSymbol => FieldSymbol.ControlSymbol;

	public BoundExpression ReceiverOpt { get; private set; }

	public TestFieldSymbol FieldSymbol { get; private set; }

	public override LookupResultKind ResultKind => _ResultKind;

	IOperation ITestFieldAccess.Instance => ReceiverOpt;

	IControlSymbol ITestFieldAccess.ControlSymbol => FieldSymbol.ControlSymbol;

	protected override OperationKind ExpressionKind => OperationKind.TestFieldAccess;

	public BoundTestFieldAccess(SyntaxNode syntax, BoundExpression receiverOpt, TestFieldSymbol fieldSymbol, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.TestFieldAccess, syntax, type, hasErrors || receiverOpt.HasErrors())
	{
		ReceiverOpt = receiverOpt;
		FieldSymbol = fieldSymbol;
		_ResultKind = resultKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitTestFieldAccess(this);
	}

	public BoundTestFieldAccess Update(BoundExpression receiverOpt, TestFieldSymbol fieldSymbol, LookupResultKind resultKind, TypeSymbol type)
	{
		if (receiverOpt != ReceiverOpt || fieldSymbol != FieldSymbol || resultKind != ResultKind || type != Type)
		{
			return new BoundTestFieldAccess(Syntax, receiverOpt, fieldSymbol, resultKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitTestFieldAccess(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitTestFieldAccess(this, argument);
	}
}
