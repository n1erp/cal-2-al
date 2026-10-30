using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundDynamicObjectInitializerMember : BoundExpression
{
	public string MemberName { get; }

	public TypeSymbol ReceiverType { get; }

	public BoundDynamicObjectInitializerMember(SyntaxNode syntax, string memberName, TypeSymbol receiverType, TypeSymbol type, bool hasErrors)
		: base(BoundKind.DynamicObjectInitializerMember, syntax, type, hasErrors)
	{
		MemberName = memberName;
		ReceiverType = receiverType;
	}

	public BoundDynamicObjectInitializerMember(SyntaxNode syntax, string memberName, TypeSymbol receiverType, TypeSymbol type)
		: base(BoundKind.DynamicObjectInitializerMember, syntax, type)
	{
		MemberName = memberName;
		ReceiverType = receiverType;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDynamicObjectInitializerMember(this);
	}

	public BoundDynamicObjectInitializerMember Update(string memberName, TypeSymbol receiverType, TypeSymbol type)
	{
		if (memberName != MemberName || receiverType != ReceiverType || type != Type)
		{
			return new BoundDynamicObjectInitializerMember(Syntax, memberName, receiverType, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
