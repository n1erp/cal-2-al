using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class LabelSyntax : SyntaxNode
{
	private StringLiteralValueSyntax _labelText;

	private CommaSeparatedIdentifierEqualsLiteralListSyntax _properties;

	public StringLiteralValueSyntax LabelText => GetRedAtZero(ref _labelText);

	public SyntaxToken CommaToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalLabelSyntax)Node)._commaToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(1), GetChildIndex(1));
			}
			return default;
		}
	}

	public CommaSeparatedIdentifierEqualsLiteralListSyntax Properties => GetRed(ref _properties, 2);

	internal LabelSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref _labelText), 
			2 => GetRed(ref _properties, 2), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)_labelText, 
			2 => _properties, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLabel(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitLabel(this);
	}

	public LabelSyntax Update(StringLiteralValueSyntax labelText, SyntaxToken commaToken, CommaSeparatedIdentifierEqualsLiteralListSyntax properties)
	{
		if (labelText != LabelText || commaToken != CommaToken || properties != Properties)
		{
			LabelSyntax labelSyntax = SyntaxFactory.Label(labelText, commaToken, properties);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return labelSyntax.WithAnnotations(annotations);
			}
			return labelSyntax;
		}
		return this;
	}

	public LabelSyntax WithLabelText(StringLiteralValueSyntax labelText)
	{
		return Update(labelText, CommaToken, Properties);
	}

	public LabelSyntax WithCommaToken(SyntaxToken commaToken)
	{
		return Update(LabelText, commaToken, Properties);
	}

	public LabelSyntax WithProperties(CommaSeparatedIdentifierEqualsLiteralListSyntax properties)
	{
		return Update(LabelText, CommaToken, properties);
	}

	public LabelSyntax AddPropertiesValues(params IdentifierEqualsLiteralSyntax[] items)
	{
		CommaSeparatedIdentifierEqualsLiteralListSyntax commaSeparatedIdentifierEqualsLiteralListSyntax = Properties ?? SyntaxFactory.CommaSeparatedIdentifierEqualsLiteralList();
		return WithProperties(commaSeparatedIdentifierEqualsLiteralListSyntax.WithValues(commaSeparatedIdentifierEqualsLiteralListSyntax.Values.AddRange(items)));
	}
}
