using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalLengthDataTypeSyntax : InternalDataTypeSyntax
{
	internal readonly InternalSyntaxToken _typeName;

	internal readonly InternalSyntaxToken _openBracketToken;

	internal readonly InternalSyntaxToken _length;

	internal readonly InternalSyntaxToken _closeBracketToken;

	internal override SyntaxNodeDefinition Definition => LengthDataTypeSyntaxDefinition.Instance.Value;

	public override InternalSyntaxToken TypeName => _typeName;

	public InternalSyntaxToken OpenBracketToken => _openBracketToken;

	public InternalSyntaxToken Length => _length;

	public InternalSyntaxToken CloseBracketToken => _closeBracketToken;

	internal InternalLengthDataTypeSyntax(SyntaxKind kind, InternalSyntaxToken typeName, InternalSyntaxToken openBracketToken, InternalSyntaxToken length, InternalSyntaxToken closeBracketToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
		if (openBracketToken != null)
		{
			AdjustFlagsAndWidth(openBracketToken);
			_openBracketToken = openBracketToken;
		}
		if (length != null)
		{
			AdjustFlagsAndWidth(length);
			_length = length;
		}
		if (closeBracketToken != null)
		{
			AdjustFlagsAndWidth(closeBracketToken);
			_closeBracketToken = closeBracketToken;
		}
	}

	internal InternalLengthDataTypeSyntax(SyntaxKind kind, InternalSyntaxToken typeName, InternalSyntaxToken openBracketToken, InternalSyntaxToken length, InternalSyntaxToken closeBracketToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
		if (openBracketToken != null)
		{
			AdjustFlagsAndWidth(openBracketToken);
			_openBracketToken = openBracketToken;
		}
		if (length != null)
		{
			AdjustFlagsAndWidth(length);
			_length = length;
		}
		if (closeBracketToken != null)
		{
			AdjustFlagsAndWidth(closeBracketToken);
			_closeBracketToken = closeBracketToken;
		}
	}

	internal InternalLengthDataTypeSyntax(SyntaxKind kind, InternalSyntaxToken typeName, InternalSyntaxToken openBracketToken, InternalSyntaxToken length, InternalSyntaxToken closeBracketToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
		if (openBracketToken != null)
		{
			AdjustFlagsAndWidth(openBracketToken);
			_openBracketToken = openBracketToken;
		}
		if (length != null)
		{
			AdjustFlagsAndWidth(length);
			_length = length;
		}
		if (closeBracketToken != null)
		{
			AdjustFlagsAndWidth(closeBracketToken);
			_closeBracketToken = closeBracketToken;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_typeName, 
			1 => _openBracketToken, 
			2 => _length, 
			3 => _closeBracketToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new LengthDataTypeSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLengthDataType(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitLengthDataType(this);
	}

	public InternalLengthDataTypeSyntax Update(InternalSyntaxToken typeName, InternalSyntaxToken openBracketToken, InternalSyntaxToken length, InternalSyntaxToken closeBracketToken)
	{
		if (typeName != TypeName || openBracketToken != OpenBracketToken || length != Length || closeBracketToken != CloseBracketToken)
		{
			return InternalSyntaxFactory.LengthDataType(typeName, openBracketToken, length, closeBracketToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalLengthDataTypeSyntax(Kind, _typeName, _openBracketToken, _length, _closeBracketToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalLengthDataTypeSyntax(Kind, _typeName, _openBracketToken, _length, _closeBracketToken, GetDiagnostics(), annotations);
	}
}
