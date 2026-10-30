using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundReportLabelAccess : BoundExpression, IReportLabelAccess, IOperation
{
	private readonly LookupResultKind _ResultKind;

	public override Symbol ExpressionSymbol => Label;

	public BoundExpression ReceiverOpt { get; private set; }

	public ReportLabelSymbol Label { get; private set; }

	public override LookupResultKind ResultKind => _ResultKind;

	IOperation IReportLabelAccess.Instance => ReceiverOpt;

	protected override OperationKind ExpressionKind => OperationKind.ReportLabelAccess;

	public BoundReportLabelAccess(SyntaxNode syntax, BoundExpression receiverOpt, ReportLabelSymbol label, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ReportLabelAccess, syntax, type, hasErrors || receiverOpt.HasErrors())
	{
		ReceiverOpt = receiverOpt;
		Label = label;
		_ResultKind = resultKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitReportLabelAccess(this);
	}

	public BoundReportLabelAccess Update(BoundExpression receiverOpt, ReportLabelSymbol label, LookupResultKind resultKind, TypeSymbol type)
	{
		if (receiverOpt != ReceiverOpt || label != Label || resultKind != ResultKind || type != Type)
		{
			return new BoundReportLabelAccess(Syntax, receiverOpt, label, resultKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitReportLabelAccess(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitReportLabelAccess(this, argument);
	}
}
