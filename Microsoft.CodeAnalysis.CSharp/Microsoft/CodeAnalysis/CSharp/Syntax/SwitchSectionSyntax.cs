using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class SwitchSectionSyntax : CSharpSyntaxNode
{
	private SyntaxNode labels;

	private SyntaxNode statements;

	public SyntaxList<SwitchLabelSyntax> Labels => new SyntaxList<SwitchLabelSyntax>(GetRed(ref labels, 0));

	public SyntaxList<StatementSyntax> Statements => new SyntaxList<StatementSyntax>(GetRed(ref statements, 1));

	internal SwitchSectionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref labels), 
			1 => GetRed(ref statements, 1), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => labels, 
			1 => statements, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSwitchSection(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitSwitchSection(this);
	}

	public SwitchSectionSyntax Update(SyntaxList<SwitchLabelSyntax> labels, SyntaxList<StatementSyntax> statements)
	{
		if (labels != Labels || statements != Statements)
		{
			SwitchSectionSyntax switchSectionSyntax = SyntaxFactory.SwitchSection(labels, statements);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return switchSectionSyntax.WithAnnotations(annotations);
			}
			return switchSectionSyntax;
		}
		return this;
	}

	public SwitchSectionSyntax WithLabels(SyntaxList<SwitchLabelSyntax> labels)
	{
		return Update(labels, Statements);
	}

	public SwitchSectionSyntax WithStatements(SyntaxList<StatementSyntax> statements)
	{
		return Update(Labels, statements);
	}

	public SwitchSectionSyntax AddLabels(params SwitchLabelSyntax[] items)
	{
		return WithLabels(Labels.AddRange(items));
	}

	public SwitchSectionSyntax AddStatements(params StatementSyntax[] items)
	{
		return WithStatements(Statements.AddRange(items));
	}
}
