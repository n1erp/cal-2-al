using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalDotNetTypeDeclarationSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _type;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalNameSyntax _typeName;

	internal readonly InternalSyntaxToken _semicolonToken;

	internal readonly InternalIdentifierNameSyntax _typeAlias;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalPropertyListSyntax _propertyList;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => DotNetTypeDeclarationSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken Type => _type;

	public InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalNameSyntax TypeName => _typeName;

	public InternalSyntaxToken SemicolonToken => _semicolonToken;

	public InternalIdentifierNameSyntax TypeAlias => _typeAlias;

	public InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public InternalPropertyListSyntax PropertyList => _propertyList;

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalDotNetTypeDeclarationSyntax(SyntaxKind kind, InternalSyntaxToken type, InternalSyntaxToken openParenthesisToken, InternalNameSyntax typeName, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax typeAlias, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(type);
		_type = type;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
		if (typeAlias != null)
		{
			AdjustFlagsAndWidth(typeAlias);
			_typeAlias = typeAlias;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalDotNetTypeDeclarationSyntax(SyntaxKind kind, InternalSyntaxToken type, InternalSyntaxToken openParenthesisToken, InternalNameSyntax typeName, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax typeAlias, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(type);
		_type = type;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
		if (typeAlias != null)
		{
			AdjustFlagsAndWidth(typeAlias);
			_typeAlias = typeAlias;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalDotNetTypeDeclarationSyntax(SyntaxKind kind, InternalSyntaxToken type, InternalSyntaxToken openParenthesisToken, InternalNameSyntax typeName, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax typeAlias, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(type);
		_type = type;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
		if (typeAlias != null)
		{
			AdjustFlagsAndWidth(typeAlias);
			_typeAlias = typeAlias;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_type, 
			1 => _openParenthesisToken, 
			2 => _typeName, 
			3 => _semicolonToken, 
			4 => _typeAlias, 
			5 => _closeParenthesisToken, 
			6 => _openBraceToken, 
			7 => _propertyList, 
			8 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new DotNetTypeDeclarationSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDotNetTypeDeclaration(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitDotNetTypeDeclaration(this);
	}

	public InternalDotNetTypeDeclarationSyntax Update(InternalSyntaxToken type, InternalSyntaxToken openParenthesisToken, InternalNameSyntax typeName, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax typeAlias, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		if (type != Type || openParenthesisToken != OpenParenthesisToken || typeName != TypeName || semicolonToken != SemicolonToken || typeAlias != TypeAlias || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.DotNetTypeDeclaration(type, openParenthesisToken, typeName, semicolonToken, typeAlias, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalDotNetTypeDeclarationSyntax(Kind, _type, _openParenthesisToken, _typeName, _semicolonToken, _typeAlias, _closeParenthesisToken, _openBraceToken, _propertyList, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalDotNetTypeDeclarationSyntax(Kind, _type, _openParenthesisToken, _typeName, _semicolonToken, _typeAlias, _closeParenthesisToken, _openBraceToken, _propertyList, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
