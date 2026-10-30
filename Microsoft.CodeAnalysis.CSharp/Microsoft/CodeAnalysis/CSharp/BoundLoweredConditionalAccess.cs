using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundLoweredConditionalAccess : BoundExpression
{
	public BoundExpression Receiver { get; }

	public MethodSymbol HasValueMethodOpt { get; }

	public BoundExpression WhenNotNull { get; }

	public BoundExpression WhenNullOpt { get; }

	public int Id { get; }

	public BoundLoweredConditionalAccess(SyntaxNode syntax, BoundExpression receiver, MethodSymbol hasValueMethodOpt, BoundExpression whenNotNull, BoundExpression whenNullOpt, int id, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.LoweredConditionalAccess, syntax, type, hasErrors || receiver.HasErrors() || whenNotNull.HasErrors() || whenNullOpt.HasErrors())
	{
		Receiver = receiver;
		HasValueMethodOpt = hasValueMethodOpt;
		WhenNotNull = whenNotNull;
		WhenNullOpt = whenNullOpt;
		Id = id;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitLoweredConditionalAccess(this);
	}

	public BoundLoweredConditionalAccess Update(BoundExpression receiver, MethodSymbol hasValueMethodOpt, BoundExpression whenNotNull, BoundExpression whenNullOpt, int id, TypeSymbol type)
	{
		if (receiver != Receiver || hasValueMethodOpt != HasValueMethodOpt || whenNotNull != WhenNotNull || whenNullOpt != WhenNullOpt || id != Id || type != Type)
		{
			return new BoundLoweredConditionalAccess(Syntax, receiver, hasValueMethodOpt, whenNotNull, whenNullOpt, id, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
