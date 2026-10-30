using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ReportLabelSyntax : ReportLabelBaseSyntax
{
	private IdentifierNameSyntax _name;

	private LabelSyntax _label;

	public override IdentifierNameSyntax Name => GetRedAtZero(ref _name);

	public SyntaxToken EqualsToken => new SyntaxToken(this, ((InternalReportLabelSyntax)Node)._equalsToken, GetChildPosition(1), GetChildIndex(1));

	public LabelSyntax Label => GetRed(ref _label, 2);

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((InternalReportLabelSyntax)Node)._semicolonToken, GetChildPosition(3), GetChildIndex(3));

	internal ReportLabelSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref _name), 
			2 => GetRed(ref _label, 2), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)_name, 
			2 => _label, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitReportLabel(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitReportLabel(this);
	}

	public ReportLabelSyntax Update(IdentifierNameSyntax name, SyntaxToken equalsToken, LabelSyntax label, SyntaxToken semicolonToken)
	{
		if (name != Name || equalsToken != EqualsToken || label != Label || semicolonToken != SemicolonToken)
		{
			ReportLabelSyntax reportLabelSyntax = SyntaxFactory.ReportLabel(name, equalsToken, label, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return reportLabelSyntax.WithAnnotations(annotations);
			}
			return reportLabelSyntax;
		}
		return this;
	}

	public ReportLabelSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(name, EqualsToken, Label, SemicolonToken);
	}

	public ReportLabelSyntax WithEqualsToken(SyntaxToken equalsToken)
	{
		return Update(Name, equalsToken, Label, SemicolonToken);
	}

	public ReportLabelSyntax WithLabel(LabelSyntax label)
	{
		return Update(Name, EqualsToken, label, SemicolonToken);
	}

	public ReportLabelSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(Name, EqualsToken, Label, semicolonToken);
	}
}
