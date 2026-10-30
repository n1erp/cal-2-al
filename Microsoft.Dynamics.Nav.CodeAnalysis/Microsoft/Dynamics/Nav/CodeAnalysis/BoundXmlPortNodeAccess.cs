using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundXmlPortNodeAccess : BoundExpression, IXmlPortNodeAccess, IOperation
{
	private readonly LookupResultKind _ResultKind;

	public override Symbol ExpressionSymbol => XmlPortNodeSymbol;

	public BoundExpression ReceiverOpt { get; private set; }

	public SourceXmlPortNodeSymbol XmlPortNodeSymbol { get; private set; }

	public override LookupResultKind ResultKind => _ResultKind;

	IOperation IXmlPortNodeAccess.Instance => ReceiverOpt;

	IXmlPortNodeSymbol IXmlPortNodeAccess.XmlPortNodeSymbol => XmlPortNodeSymbol;

	protected override OperationKind ExpressionKind => OperationKind.ReportDataItemAccess;

	public BoundXmlPortNodeAccess(SyntaxNode syntax, BoundExpression receiverOpt, SourceXmlPortNodeSymbol xmlPortNodeSymbol, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.XmlPortNodeAccess, syntax, type, hasErrors || receiverOpt.HasErrors())
	{
		ReceiverOpt = receiverOpt;
		XmlPortNodeSymbol = xmlPortNodeSymbol;
		_ResultKind = resultKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitXmlPortNodeAccess(this);
	}

	public BoundXmlPortNodeAccess Update(BoundExpression receiverOpt, SourceXmlPortNodeSymbol xmlPortNodeSymbol, LookupResultKind resultKind, TypeSymbol type)
	{
		if (receiverOpt != ReceiverOpt || xmlPortNodeSymbol != XmlPortNodeSymbol || resultKind != ResultKind || type != Type)
		{
			return new BoundXmlPortNodeAccess(Syntax, receiverOpt, xmlPortNodeSymbol, resultKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitXmlPortNodeAccess(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitReportDataItemAccess(this, argument);
	}
}
