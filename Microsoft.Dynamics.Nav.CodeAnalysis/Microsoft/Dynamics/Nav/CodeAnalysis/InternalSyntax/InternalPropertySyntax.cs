using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalPropertySyntax : InternalPropertySyntaxOrEmpty
{
	internal readonly InternalPropertyNameSyntax _name;

	internal readonly InternalSyntaxToken _equalsToken;

	internal readonly InternalPropertyValueSyntax _value;

	internal readonly InternalSyntaxToken _semicolonToken;

	internal override SyntaxNodeDefinition Definition => PropertySyntaxDefinition.Instance.Value;

	public InternalPropertyNameSyntax Name => _name;

	public InternalSyntaxToken EqualsToken => _equalsToken;

	public InternalPropertyValueSyntax Value => _value;

	public InternalSyntaxToken SemicolonToken => _semicolonToken;

	internal InternalPropertySyntax(SyntaxKind kind, InternalPropertyNameSyntax name, InternalSyntaxToken equalsToken, InternalPropertyValueSyntax value, InternalSyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		if (value != null)
		{
			AdjustFlagsAndWidth(value);
			_value = value;
		}
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
	}

	internal InternalPropertySyntax(SyntaxKind kind, InternalPropertyNameSyntax name, InternalSyntaxToken equalsToken, InternalPropertyValueSyntax value, InternalSyntaxToken semicolonToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		if (value != null)
		{
			AdjustFlagsAndWidth(value);
			_value = value;
		}
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
	}

	internal InternalPropertySyntax(SyntaxKind kind, InternalPropertyNameSyntax name, InternalSyntaxToken equalsToken, InternalPropertyValueSyntax value, InternalSyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		if (value != null)
		{
			AdjustFlagsAndWidth(value);
			_value = value;
		}
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_name, 
			1 => _equalsToken, 
			2 => _value, 
			3 => _semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new PropertySyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitProperty(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitProperty(this);
	}

	public InternalPropertySyntax Update(InternalPropertyNameSyntax name, InternalSyntaxToken equalsToken, InternalPropertyValueSyntax value, InternalSyntaxToken semicolonToken)
	{
		if (name != Name || equalsToken != EqualsToken || value != Value || semicolonToken != SemicolonToken)
		{
			return InternalSyntaxFactory.Property(name, equalsToken, value, semicolonToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalPropertySyntax(Kind, _name, _equalsToken, _value, _semicolonToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalPropertySyntax(Kind, _name, _equalsToken, _value, _semicolonToken, GetDiagnostics(), annotations);
	}
}
