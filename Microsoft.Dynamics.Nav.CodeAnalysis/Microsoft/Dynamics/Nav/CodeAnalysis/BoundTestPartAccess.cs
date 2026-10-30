using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundTestPartAccess : BoundExpression, ITestPartAccess, IOperation
{
	private readonly LookupResultKind _ResultKind;

	public override Symbol ExpressionSymbol => PartSymbol;

	public BoundExpression ReceiverOpt { get; private set; }

	public TestPartSymbol PartSymbol { get; private set; }

	public override LookupResultKind ResultKind => _ResultKind;

	IOperation ITestPartAccess.Instance => ReceiverOpt;

	IControlSymbol ITestPartAccess.ControlSymbol => PartSymbol.ControlSymbol;

	protected override OperationKind ExpressionKind => OperationKind.TestPartAccess;

	public BoundTestPartAccess(SyntaxNode syntax, BoundExpression receiverOpt, TestPartSymbol partSymbol, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.TestPartAccess, syntax, type, hasErrors || receiverOpt.HasErrors())
	{
		ReceiverOpt = receiverOpt;
		PartSymbol = partSymbol;
		_ResultKind = resultKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitTestPartAccess(this);
	}

	public BoundTestPartAccess Update(BoundExpression receiverOpt, TestPartSymbol partSymbol, LookupResultKind resultKind, TypeSymbol type)
	{
		if (receiverOpt != ReceiverOpt || partSymbol != PartSymbol || resultKind != ResultKind || type != Type)
		{
			return new BoundTestPartAccess(Syntax, receiverOpt, partSymbol, resultKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitTestPartAccess(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitTestPartAccess(this, argument);
	}
}
