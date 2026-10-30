using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalLabelSyntax : InternalSyntaxNode
{
	internal readonly InternalStringLiteralValueSyntax _labelText;

	internal readonly InternalSyntaxToken _commaToken;

	internal readonly InternalCommaSeparatedIdentifierEqualsLiteralListSyntax _properties;

	internal override SyntaxNodeDefinition Definition => LabelSyntaxDefinition.Instance.Value;

	public InternalStringLiteralValueSyntax LabelText => _labelText;

	public InternalSyntaxToken CommaToken => _commaToken;

	public InternalCommaSeparatedIdentifierEqualsLiteralListSyntax Properties => _properties;

	internal InternalLabelSyntax(SyntaxKind kind, InternalStringLiteralValueSyntax labelText, InternalSyntaxToken commaToken, InternalCommaSeparatedIdentifierEqualsLiteralListSyntax properties, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(labelText);
		_labelText = labelText;
		if (commaToken != null)
		{
			AdjustFlagsAndWidth(commaToken);
			_commaToken = commaToken;
		}
		if (properties != null)
		{
			AdjustFlagsAndWidth(properties);
			_properties = properties;
		}
	}

	internal InternalLabelSyntax(SyntaxKind kind, InternalStringLiteralValueSyntax labelText, InternalSyntaxToken commaToken, InternalCommaSeparatedIdentifierEqualsLiteralListSyntax properties, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(labelText);
		_labelText = labelText;
		if (commaToken != null)
		{
			AdjustFlagsAndWidth(commaToken);
			_commaToken = commaToken;
		}
		if (properties != null)
		{
			AdjustFlagsAndWidth(properties);
			_properties = properties;
		}
	}

	internal InternalLabelSyntax(SyntaxKind kind, InternalStringLiteralValueSyntax labelText, InternalSyntaxToken commaToken, InternalCommaSeparatedIdentifierEqualsLiteralListSyntax properties)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(labelText);
		_labelText = labelText;
		if (commaToken != null)
		{
			AdjustFlagsAndWidth(commaToken);
			_commaToken = commaToken;
		}
		if (properties != null)
		{
			AdjustFlagsAndWidth(properties);
			_properties = properties;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_labelText, 
			1 => _commaToken, 
			2 => _properties, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new LabelSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLabel(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitLabel(this);
	}

	public InternalLabelSyntax Update(InternalStringLiteralValueSyntax labelText, InternalSyntaxToken commaToken, InternalCommaSeparatedIdentifierEqualsLiteralListSyntax properties)
	{
		if (labelText != LabelText || commaToken != CommaToken || properties != Properties)
		{
			return InternalSyntaxFactory.Label(labelText, commaToken, properties).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalLabelSyntax(Kind, _labelText, _commaToken, _properties, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalLabelSyntax(Kind, _labelText, _commaToken, _properties, GetDiagnostics(), annotations);
	}
}
