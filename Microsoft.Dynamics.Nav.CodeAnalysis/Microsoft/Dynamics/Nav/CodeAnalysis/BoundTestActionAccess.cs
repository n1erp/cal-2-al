using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundTestActionAccess : BoundExpression, ITestActionAccess, IOperation
{
	private readonly LookupResultKind _ResultKind;

	public override Symbol ExpressionSymbol => ActionSymbol.ActionSymbol;

	public BoundExpression ReceiverOpt { get; private set; }

	public TestActionSymbol ActionSymbol { get; private set; }

	public override LookupResultKind ResultKind => _ResultKind;

	IOperation ITestActionAccess.Instance => ReceiverOpt;

	IActionSymbol ITestActionAccess.ActionSymbol => ActionSymbol.ActionSymbol;

	protected override OperationKind ExpressionKind => OperationKind.TestActionAccess;

	public BoundTestActionAccess(SyntaxNode syntax, BoundExpression receiverOpt, TestActionSymbol actionSymbol, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.TestActionAccess, syntax, type, hasErrors || receiverOpt.HasErrors())
	{
		ReceiverOpt = receiverOpt;
		ActionSymbol = actionSymbol;
		_ResultKind = resultKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitTestActionAccess(this);
	}

	public BoundTestActionAccess Update(BoundExpression receiverOpt, TestActionSymbol actionSymbol, LookupResultKind resultKind, TypeSymbol type)
	{
		if (receiverOpt != ReceiverOpt || actionSymbol != ActionSymbol || resultKind != ResultKind || type != Type)
		{
			return new BoundTestActionAccess(Syntax, receiverOpt, actionSymbol, resultKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitTestActionAccess(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitTestActionAccess(this, argument);
	}
}
