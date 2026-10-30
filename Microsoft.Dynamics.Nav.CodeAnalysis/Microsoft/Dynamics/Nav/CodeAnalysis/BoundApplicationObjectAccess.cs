using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundApplicationObjectAccess : BoundExpression, IApplicationObjectAccess, IOperation
{
	private readonly LookupResultKind _ResultKind;

	public override Symbol ExpressionSymbol => ApplicationObjectTypeSymbol;

	public BoundExpression ReceiverOpt { get; private set; }

	public ApplicationObjectTypeSymbol ApplicationObjectTypeSymbol { get; private set; }

	public override LookupResultKind ResultKind => _ResultKind;

	IOperation IApplicationObjectAccess.Instance => ReceiverOpt;

	IApplicationObjectTypeSymbol IApplicationObjectAccess.ApplicationObjectTypeSymbol => ApplicationObjectTypeSymbol;

	protected override OperationKind ExpressionKind => OperationKind.FieldAccess;

	public BoundApplicationObjectAccess(SyntaxNode syntax, BoundExpression receiverOpt, ApplicationObjectTypeSymbol applicationObjectTypeSymbol, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ApplicationObjectAccess, syntax, type, hasErrors || receiverOpt.HasErrors())
	{
		ReceiverOpt = receiverOpt;
		ApplicationObjectTypeSymbol = applicationObjectTypeSymbol;
		_ResultKind = resultKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitApplicationObjectAccess(this);
	}

	public BoundApplicationObjectAccess Update(BoundExpression receiverOpt, ApplicationObjectTypeSymbol applicationObjectTypeSymbol, LookupResultKind resultKind, TypeSymbol type)
	{
		if (receiverOpt != ReceiverOpt || applicationObjectTypeSymbol != ApplicationObjectTypeSymbol || resultKind != ResultKind || type != Type)
		{
			return new BoundApplicationObjectAccess(Syntax, receiverOpt, applicationObjectTypeSymbol, resultKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitApplicationObjectAccess(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitApplicationObjectAccess(this, argument);
	}
}
