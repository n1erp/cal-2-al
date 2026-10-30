using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalFieldExtensionListSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _fieldsKeyword;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalSyntaxNode _fields;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => FieldExtensionListSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken FieldsKeyword => _fieldsKeyword;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public InternalSyntaxList<InternalFieldBaseSyntax> Fields => new InternalSyntaxList<InternalFieldBaseSyntax>(_fields);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalFieldExtensionListSyntax(SyntaxKind kind, InternalSyntaxToken fieldsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode fields, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(fieldsKeyword);
		_fieldsKeyword = fieldsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (fields != null)
		{
			AdjustFlagsAndWidth(fields);
			_fields = fields;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalFieldExtensionListSyntax(SyntaxKind kind, InternalSyntaxToken fieldsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode fields, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(fieldsKeyword);
		_fieldsKeyword = fieldsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (fields != null)
		{
			AdjustFlagsAndWidth(fields);
			_fields = fields;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalFieldExtensionListSyntax(SyntaxKind kind, InternalSyntaxToken fieldsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode fields, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(fieldsKeyword);
		_fieldsKeyword = fieldsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (fields != null)
		{
			AdjustFlagsAndWidth(fields);
			_fields = fields;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _fieldsKeyword, 
			1 => _openBraceToken, 
			2 => _fields, 
			3 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new FieldExtensionListSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFieldExtensionList(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitFieldExtensionList(this);
	}

	public InternalFieldExtensionListSyntax Update(InternalSyntaxToken fieldsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalFieldBaseSyntax> fields, InternalSyntaxToken closeBraceToken)
	{
		if (fieldsKeyword != FieldsKeyword || openBraceToken != OpenBraceToken || fields != Fields || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.FieldExtensionList(fieldsKeyword, openBraceToken, fields, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalFieldExtensionListSyntax(Kind, _fieldsKeyword, _openBraceToken, _fields, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalFieldExtensionListSyntax(Kind, _fieldsKeyword, _openBraceToken, _fields, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
