using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundPropertyAccess : BoundExpression
{
	private readonly LookupResultKind _ResultKind;

	public override Symbol ExpressionSymbol => PropertySymbol;

	public BoundExpression ReceiverOpt { get; }

	public PropertySymbol PropertySymbol { get; }

	public override LookupResultKind ResultKind => _ResultKind;

	public BoundPropertyAccess(SyntaxNode syntax, BoundExpression receiverOpt, PropertySymbol propertySymbol, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.PropertyAccess, syntax, type, hasErrors || receiverOpt.HasErrors())
	{
		ReceiverOpt = receiverOpt;
		PropertySymbol = propertySymbol;
		_ResultKind = resultKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitPropertyAccess(this);
	}

	public BoundPropertyAccess Update(BoundExpression receiverOpt, PropertySymbol propertySymbol, LookupResultKind resultKind, TypeSymbol type)
	{
		if (receiverOpt != ReceiverOpt || propertySymbol != PropertySymbol || resultKind != ResultKind || type != Type)
		{
			return new BoundPropertyAccess(Syntax, receiverOpt, propertySymbol, resultKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
