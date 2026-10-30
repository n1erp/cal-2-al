using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalMemberAttributeSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _openBracketToken;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalAttributeArgumentListSyntax _argumentList;

	internal readonly InternalSyntaxToken _closeBracketToken;

	internal override SyntaxNodeDefinition Definition => MemberAttributeSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken OpenBracketToken => _openBracketToken;

	public InternalIdentifierNameSyntax Name => _name;

	public InternalAttributeArgumentListSyntax ArgumentList => _argumentList;

	public InternalSyntaxToken CloseBracketToken => _closeBracketToken;

	internal InternalMemberAttributeSyntax(SyntaxKind kind, InternalSyntaxToken openBracketToken, InternalIdentifierNameSyntax name, InternalAttributeArgumentListSyntax argumentList, InternalSyntaxToken closeBracketToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(openBracketToken);
		_openBracketToken = openBracketToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		if (argumentList != null)
		{
			AdjustFlagsAndWidth(argumentList);
			_argumentList = argumentList;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		_closeBracketToken = closeBracketToken;
	}

	internal InternalMemberAttributeSyntax(SyntaxKind kind, InternalSyntaxToken openBracketToken, InternalIdentifierNameSyntax name, InternalAttributeArgumentListSyntax argumentList, InternalSyntaxToken closeBracketToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(openBracketToken);
		_openBracketToken = openBracketToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		if (argumentList != null)
		{
			AdjustFlagsAndWidth(argumentList);
			_argumentList = argumentList;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		_closeBracketToken = closeBracketToken;
	}

	internal InternalMemberAttributeSyntax(SyntaxKind kind, InternalSyntaxToken openBracketToken, InternalIdentifierNameSyntax name, InternalAttributeArgumentListSyntax argumentList, InternalSyntaxToken closeBracketToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(openBracketToken);
		_openBracketToken = openBracketToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		if (argumentList != null)
		{
			AdjustFlagsAndWidth(argumentList);
			_argumentList = argumentList;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		_closeBracketToken = closeBracketToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_openBracketToken, 
			1 => _name, 
			2 => _argumentList, 
			3 => _closeBracketToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new MemberAttributeSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitMemberAttribute(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitMemberAttribute(this);
	}

	public InternalMemberAttributeSyntax Update(InternalSyntaxToken openBracketToken, InternalIdentifierNameSyntax name, InternalAttributeArgumentListSyntax argumentList, InternalSyntaxToken closeBracketToken)
	{
		if (openBracketToken != OpenBracketToken || name != Name || argumentList != ArgumentList || closeBracketToken != CloseBracketToken)
		{
			return InternalSyntaxFactory.MemberAttribute(openBracketToken, name, argumentList, closeBracketToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalMemberAttributeSyntax(Kind, _openBracketToken, _name, _argumentList, _closeBracketToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalMemberAttributeSyntax(Kind, _openBracketToken, _name, _argumentList, _closeBracketToken, GetDiagnostics(), annotations);
	}
}
