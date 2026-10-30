using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalFieldGroupListSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _fieldGroupsKeyword;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalSyntaxNode _fieldGroups;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => FieldGroupListSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken FieldGroupsKeyword => _fieldGroupsKeyword;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public InternalSyntaxList<InternalFieldGroupSyntax> FieldGroups => new InternalSyntaxList<InternalFieldGroupSyntax>(_fieldGroups);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalFieldGroupListSyntax(SyntaxKind kind, InternalSyntaxToken fieldGroupsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode fieldGroups, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(fieldGroupsKeyword);
		_fieldGroupsKeyword = fieldGroupsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (fieldGroups != null)
		{
			AdjustFlagsAndWidth(fieldGroups);
			_fieldGroups = fieldGroups;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalFieldGroupListSyntax(SyntaxKind kind, InternalSyntaxToken fieldGroupsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode fieldGroups, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(fieldGroupsKeyword);
		_fieldGroupsKeyword = fieldGroupsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (fieldGroups != null)
		{
			AdjustFlagsAndWidth(fieldGroups);
			_fieldGroups = fieldGroups;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalFieldGroupListSyntax(SyntaxKind kind, InternalSyntaxToken fieldGroupsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode fieldGroups, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(fieldGroupsKeyword);
		_fieldGroupsKeyword = fieldGroupsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (fieldGroups != null)
		{
			AdjustFlagsAndWidth(fieldGroups);
			_fieldGroups = fieldGroups;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _fieldGroupsKeyword, 
			1 => _openBraceToken, 
			2 => _fieldGroups, 
			3 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new FieldGroupListSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFieldGroupList(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitFieldGroupList(this);
	}

	public InternalFieldGroupListSyntax Update(InternalSyntaxToken fieldGroupsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalFieldGroupSyntax> fieldGroups, InternalSyntaxToken closeBraceToken)
	{
		if (fieldGroupsKeyword != FieldGroupsKeyword || openBraceToken != OpenBraceToken || fieldGroups != FieldGroups || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.FieldGroupList(fieldGroupsKeyword, openBraceToken, fieldGroups, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalFieldGroupListSyntax(Kind, _fieldGroupsKeyword, _openBraceToken, _fieldGroups, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalFieldGroupListSyntax(Kind, _fieldGroupsKeyword, _openBraceToken, _fieldGroups, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
