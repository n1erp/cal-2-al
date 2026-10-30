using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalFieldGroupExtensionListSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _fieldGroupsKeyword;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalSyntaxNode _changes;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => FieldGroupExtensionListSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken FieldGroupsKeyword => _fieldGroupsKeyword;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public InternalSyntaxList<InternalFieldGroupChangeBaseSyntax> Changes => new InternalSyntaxList<InternalFieldGroupChangeBaseSyntax>(_changes);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalFieldGroupExtensionListSyntax(SyntaxKind kind, InternalSyntaxToken fieldGroupsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode changes, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(fieldGroupsKeyword);
		_fieldGroupsKeyword = fieldGroupsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (changes != null)
		{
			AdjustFlagsAndWidth(changes);
			_changes = changes;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalFieldGroupExtensionListSyntax(SyntaxKind kind, InternalSyntaxToken fieldGroupsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode changes, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(fieldGroupsKeyword);
		_fieldGroupsKeyword = fieldGroupsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (changes != null)
		{
			AdjustFlagsAndWidth(changes);
			_changes = changes;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalFieldGroupExtensionListSyntax(SyntaxKind kind, InternalSyntaxToken fieldGroupsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode changes, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(fieldGroupsKeyword);
		_fieldGroupsKeyword = fieldGroupsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (changes != null)
		{
			AdjustFlagsAndWidth(changes);
			_changes = changes;
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
			2 => _changes, 
			3 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new FieldGroupExtensionListSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFieldGroupExtensionList(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitFieldGroupExtensionList(this);
	}

	public InternalFieldGroupExtensionListSyntax Update(InternalSyntaxToken fieldGroupsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalFieldGroupChangeBaseSyntax> changes, InternalSyntaxToken closeBraceToken)
	{
		if (fieldGroupsKeyword != FieldGroupsKeyword || openBraceToken != OpenBraceToken || changes != Changes || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.FieldGroupExtensionList(fieldGroupsKeyword, openBraceToken, changes, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalFieldGroupExtensionListSyntax(Kind, _fieldGroupsKeyword, _openBraceToken, _changes, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalFieldGroupExtensionListSyntax(Kind, _fieldGroupsKeyword, _openBraceToken, _changes, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
