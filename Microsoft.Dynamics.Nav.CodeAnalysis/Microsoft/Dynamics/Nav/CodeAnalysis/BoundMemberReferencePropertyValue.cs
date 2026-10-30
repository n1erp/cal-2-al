using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundMemberReferencePropertyValue : BoundPropertyValue
{
	public TypeSymbol MemberReference { get; private set; }

	internal override object ValueObject => MemberReference;

	internal override string ValueText => MemberReference.Name;

	public BoundMemberReferencePropertyValue(SyntaxNode syntax, TypeSymbol memberReference, TypeSymbol type, bool hasErrors)
		: base(BoundKind.MemberReferencePropertyValue, syntax, type, hasErrors)
	{
		MemberReference = memberReference;
	}

	public BoundMemberReferencePropertyValue(SyntaxNode syntax, TypeSymbol memberReference, TypeSymbol type)
		: base(BoundKind.MemberReferencePropertyValue, syntax, type)
	{
		MemberReference = memberReference;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitMemberReferencePropertyValue(this);
	}

	public BoundMemberReferencePropertyValue Update(TypeSymbol memberReference, TypeSymbol type)
	{
		if (memberReference != MemberReference || type != Type)
		{
			return new BoundMemberReferencePropertyValue(Syntax, memberReference, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
