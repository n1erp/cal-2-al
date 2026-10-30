using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalGenericNamedDataTypeSyntax : InternalDataTypeSyntax
{
	internal readonly InternalSyntaxToken _typeName;

	internal readonly InternalSyntaxToken _ofKeyword;

	internal readonly InternalSyntaxToken _openBracketToken;

	internal readonly InternalSyntaxNode _typeArguments;

	internal readonly InternalSyntaxToken _closeBracketToken;

	internal override SyntaxNodeDefinition Definition => GenericNamedDataTypeSyntaxDefinition.Instance.Value;

	public override InternalSyntaxToken TypeName => _typeName;

	public InternalSyntaxToken OfKeyword => _ofKeyword;

	public InternalSyntaxToken OpenBracketToken => _openBracketToken;

	public InternalSeparatedSyntaxList<InternalDataTypeSyntax> TypeArguments => new InternalSeparatedSyntaxList<InternalDataTypeSyntax>(new InternalSyntaxList<InternalSyntaxNode>(_typeArguments));

	public InternalSyntaxToken CloseBracketToken => _closeBracketToken;

	internal InternalGenericNamedDataTypeSyntax(SyntaxKind kind, InternalSyntaxToken typeName, InternalSyntaxToken ofKeyword, InternalSyntaxToken openBracketToken, InternalSyntaxNode typeArguments, InternalSyntaxToken closeBracketToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 5;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
		AdjustFlagsAndWidth(ofKeyword);
		_ofKeyword = ofKeyword;
		if (openBracketToken != null)
		{
			AdjustFlagsAndWidth(openBracketToken);
			_openBracketToken = openBracketToken;
		}
		if (typeArguments != null)
		{
			AdjustFlagsAndWidth(typeArguments);
			_typeArguments = typeArguments;
		}
		if (closeBracketToken != null)
		{
			AdjustFlagsAndWidth(closeBracketToken);
			_closeBracketToken = closeBracketToken;
		}
	}

	internal InternalGenericNamedDataTypeSyntax(SyntaxKind kind, InternalSyntaxToken typeName, InternalSyntaxToken ofKeyword, InternalSyntaxToken openBracketToken, InternalSyntaxNode typeArguments, InternalSyntaxToken closeBracketToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 5;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
		AdjustFlagsAndWidth(ofKeyword);
		_ofKeyword = ofKeyword;
		if (openBracketToken != null)
		{
			AdjustFlagsAndWidth(openBracketToken);
			_openBracketToken = openBracketToken;
		}
		if (typeArguments != null)
		{
			AdjustFlagsAndWidth(typeArguments);
			_typeArguments = typeArguments;
		}
		if (closeBracketToken != null)
		{
			AdjustFlagsAndWidth(closeBracketToken);
			_closeBracketToken = closeBracketToken;
		}
	}

	internal InternalGenericNamedDataTypeSyntax(SyntaxKind kind, InternalSyntaxToken typeName, InternalSyntaxToken ofKeyword, InternalSyntaxToken openBracketToken, InternalSyntaxNode typeArguments, InternalSyntaxToken closeBracketToken)
		: base(kind)
	{
		SlotCount = 5;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
		AdjustFlagsAndWidth(ofKeyword);
		_ofKeyword = ofKeyword;
		if (openBracketToken != null)
		{
			AdjustFlagsAndWidth(openBracketToken);
			_openBracketToken = openBracketToken;
		}
		if (typeArguments != null)
		{
			AdjustFlagsAndWidth(typeArguments);
			_typeArguments = typeArguments;
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
			0 => _typeName, 
			1 => _ofKeyword, 
			2 => _openBracketToken, 
			3 => _typeArguments, 
			4 => _closeBracketToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new GenericNamedDataTypeSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitGenericNamedDataType(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitGenericNamedDataType(this);
	}

	public InternalGenericNamedDataTypeSyntax Update(InternalSyntaxToken typeName, InternalSyntaxToken ofKeyword, InternalSyntaxToken openBracketToken, InternalSeparatedSyntaxList<InternalDataTypeSyntax> typeArguments, InternalSyntaxToken closeBracketToken)
	{
		if (typeName != TypeName || ofKeyword != OfKeyword || openBracketToken != OpenBracketToken || typeArguments != TypeArguments || closeBracketToken != CloseBracketToken)
		{
			return InternalSyntaxFactory.GenericNamedDataType(typeName, ofKeyword, openBracketToken, typeArguments, closeBracketToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalGenericNamedDataTypeSyntax(Kind, _typeName, _ofKeyword, _openBracketToken, _typeArguments, _closeBracketToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalGenericNamedDataTypeSyntax(Kind, _typeName, _ofKeyword, _openBracketToken, _typeArguments, _closeBracketToken, GetDiagnostics(), annotations);
	}
}
