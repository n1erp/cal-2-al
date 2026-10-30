using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundOptionAccess : BoundExpression, IOptionAccess, IOperation
{
	private readonly LookupResultKind _ResultKind;

	public override Symbol ExpressionSymbol => OptionSymbol;

	public BoundExpression ReceiverOpt { get; private set; }

	public OptionSymbol OptionSymbol { get; private set; }

	public override LookupResultKind ResultKind => _ResultKind;

	IOperation IOptionAccess.Instance => ReceiverOpt;

	IOptionSymbol IOptionAccess.OptionSymbol => OptionSymbol;

	protected override OperationKind ExpressionKind => OperationKind.OptionAccess;

	public BoundOptionAccess(SyntaxNode syntax, BoundExpression receiverOpt, OptionSymbol optionSymbol, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.OptionAccess, syntax, type, hasErrors || receiverOpt.HasErrors())
	{
		ReceiverOpt = receiverOpt;
		OptionSymbol = optionSymbol;
		_ResultKind = resultKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitOptionAccess(this);
	}

	public BoundOptionAccess Update(BoundExpression receiverOpt, OptionSymbol optionSymbol, LookupResultKind resultKind, TypeSymbol type)
	{
		if (receiverOpt != ReceiverOpt || optionSymbol != OptionSymbol || resultKind != ResultKind || type != Type)
		{
			return new BoundOptionAccess(Syntax, receiverOpt, optionSymbol, resultKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitOptionAccess(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitOptionAccess(this, argument);
	}
}
