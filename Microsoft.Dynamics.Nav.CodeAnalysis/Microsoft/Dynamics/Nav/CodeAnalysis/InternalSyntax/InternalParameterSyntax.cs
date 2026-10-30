using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalParameterSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _varKeyword;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _colonToken;

	internal readonly InternalTypeReferenceBaseSyntax _type;

	internal override SyntaxNodeDefinition Definition => ParameterSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken VarKeyword => _varKeyword;

	public InternalIdentifierNameSyntax Name => _name;

	public InternalSyntaxToken ColonToken => _colonToken;

	public InternalTypeReferenceBaseSyntax Type => _type;

	internal InternalParameterSyntax(SyntaxKind kind, InternalSyntaxToken varKeyword, InternalIdentifierNameSyntax name, InternalSyntaxToken colonToken, InternalTypeReferenceBaseSyntax type, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		if (varKeyword != null)
		{
			AdjustFlagsAndWidth(varKeyword);
			_varKeyword = varKeyword;
		}
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(colonToken);
		_colonToken = colonToken;
		AdjustFlagsAndWidth(type);
		_type = type;
	}

	internal InternalParameterSyntax(SyntaxKind kind, InternalSyntaxToken varKeyword, InternalIdentifierNameSyntax name, InternalSyntaxToken colonToken, InternalTypeReferenceBaseSyntax type, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		if (varKeyword != null)
		{
			AdjustFlagsAndWidth(varKeyword);
			_varKeyword = varKeyword;
		}
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(colonToken);
		_colonToken = colonToken;
		AdjustFlagsAndWidth(type);
		_type = type;
	}

	internal InternalParameterSyntax(SyntaxKind kind, InternalSyntaxToken varKeyword, InternalIdentifierNameSyntax name, InternalSyntaxToken colonToken, InternalTypeReferenceBaseSyntax type)
		: base(kind)
	{
		SlotCount = 4;
		if (varKeyword != null)
		{
			AdjustFlagsAndWidth(varKeyword);
			_varKeyword = varKeyword;
		}
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(colonToken);
		_colonToken = colonToken;
		AdjustFlagsAndWidth(type);
		_type = type;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_varKeyword, 
			1 => _name, 
			2 => _colonToken, 
			3 => _type, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ParameterSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitParameter(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitParameter(this);
	}

	public InternalParameterSyntax Update(InternalSyntaxToken varKeyword, InternalIdentifierNameSyntax name, InternalSyntaxToken colonToken, InternalTypeReferenceBaseSyntax type)
	{
		if (varKeyword != VarKeyword || name != Name || colonToken != ColonToken || type != Type)
		{
			return InternalSyntaxFactory.Parameter(varKeyword, name, colonToken, type).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalParameterSyntax(Kind, _varKeyword, _name, _colonToken, _type, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalParameterSyntax(Kind, _varKeyword, _name, _colonToken, _type, GetDiagnostics(), annotations);
	}
}
