using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalVariableDeclarationSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxNode _attributes;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _colonToken;

	internal readonly InternalTypeReferenceBaseSyntax _type;

	internal readonly InternalSyntaxToken _semicolonToken;

	internal override SyntaxNodeDefinition Definition => VariableDeclarationSyntaxDefinition.Instance.Value;

	public InternalSyntaxList<InternalMemberAttributeSyntax> Attributes => new InternalSyntaxList<InternalMemberAttributeSyntax>(_attributes);

	public InternalIdentifierNameSyntax Name => _name;

	public InternalSyntaxToken ColonToken => _colonToken;

	public InternalTypeReferenceBaseSyntax Type => _type;

	public InternalSyntaxToken SemicolonToken => _semicolonToken;

	internal InternalVariableDeclarationSyntax(SyntaxKind kind, InternalSyntaxNode attributes, InternalIdentifierNameSyntax name, InternalSyntaxToken colonToken, InternalTypeReferenceBaseSyntax type, InternalSyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 5;
		if (attributes != null)
		{
			AdjustFlagsAndWidth(attributes);
			_attributes = attributes;
		}
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(colonToken);
		_colonToken = colonToken;
		AdjustFlagsAndWidth(type);
		_type = type;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
	}

	internal InternalVariableDeclarationSyntax(SyntaxKind kind, InternalSyntaxNode attributes, InternalIdentifierNameSyntax name, InternalSyntaxToken colonToken, InternalTypeReferenceBaseSyntax type, InternalSyntaxToken semicolonToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 5;
		if (attributes != null)
		{
			AdjustFlagsAndWidth(attributes);
			_attributes = attributes;
		}
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(colonToken);
		_colonToken = colonToken;
		AdjustFlagsAndWidth(type);
		_type = type;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
	}

	internal InternalVariableDeclarationSyntax(SyntaxKind kind, InternalSyntaxNode attributes, InternalIdentifierNameSyntax name, InternalSyntaxToken colonToken, InternalTypeReferenceBaseSyntax type, InternalSyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 5;
		if (attributes != null)
		{
			AdjustFlagsAndWidth(attributes);
			_attributes = attributes;
		}
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(colonToken);
		_colonToken = colonToken;
		AdjustFlagsAndWidth(type);
		_type = type;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _attributes, 
			1 => _name, 
			2 => _colonToken, 
			3 => _type, 
			4 => _semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new VariableDeclarationSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitVariableDeclaration(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitVariableDeclaration(this);
	}

	public InternalVariableDeclarationSyntax Update(InternalSyntaxList<InternalMemberAttributeSyntax> attributes, InternalIdentifierNameSyntax name, InternalSyntaxToken colonToken, InternalTypeReferenceBaseSyntax type, InternalSyntaxToken semicolonToken)
	{
		if (attributes != Attributes || name != Name || colonToken != ColonToken || type != Type || semicolonToken != SemicolonToken)
		{
			return InternalSyntaxFactory.VariableDeclaration(attributes, name, colonToken, type, semicolonToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalVariableDeclarationSyntax(Kind, _attributes, _name, _colonToken, _type, _semicolonToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalVariableDeclarationSyntax(Kind, _attributes, _name, _colonToken, _type, _semicolonToken, GetDiagnostics(), annotations);
	}
}
