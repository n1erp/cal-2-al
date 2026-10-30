using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundControlAccess : BoundExpression, IPagePartAccess, IOperation
{
	private readonly LookupResultKind _ResultKind;

	public override Symbol ExpressionSymbol => ControlSymbol;

	public BoundExpression ReceiverOpt { get; private set; }

	public ControlSymbol ControlSymbol { get; private set; }

	public override LookupResultKind ResultKind => _ResultKind;

	IOperation IPagePartAccess.Instance => ReceiverOpt;

	IControlSymbol IPagePartAccess.ControlSymbol => ControlSymbol;

	IApplicationObjectTypeSymbol IPagePartAccess.PageSymbol => ControlSymbol.RelatedPartSymbol;

	protected override OperationKind ExpressionKind => OperationKind.PagePartAccess;

	public BoundControlAccess(SyntaxNode syntax, BoundExpression receiverOpt, ControlSymbol controlSymbol, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ControlAccess, syntax, type, hasErrors || receiverOpt.HasErrors())
	{
		ReceiverOpt = receiverOpt;
		ControlSymbol = controlSymbol;
		_ResultKind = resultKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitControlAccess(this);
	}

	public BoundControlAccess Update(BoundExpression receiverOpt, ControlSymbol controlSymbol, LookupResultKind resultKind, TypeSymbol type)
	{
		if (receiverOpt != ReceiverOpt || controlSymbol != ControlSymbol || resultKind != ResultKind || type != Type)
		{
			return new BoundControlAccess(Syntax, receiverOpt, controlSymbol, resultKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitPagePartAccess(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitPagePartAccess(this, argument);
	}
}
