using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class VarSectionSyntax : MemberSyntax
{
	private SyntaxNode _variables;

	public SyntaxToken VarKeyword => new SyntaxToken(this, ((InternalVarSectionSyntax)Node)._varKeyword, Position, 0);

	public SyntaxList<VariableDeclarationSyntax> Variables => new SyntaxList<VariableDeclarationSyntax>(GetRed(ref _variables, 1));

	internal VarSectionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref _variables, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return _variables;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitVarSection(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitVarSection(this);
	}

	public VarSectionSyntax Update(SyntaxToken varKeyword, SyntaxList<VariableDeclarationSyntax> variables)
	{
		if (varKeyword != VarKeyword || variables != Variables)
		{
			VarSectionSyntax varSectionSyntax = SyntaxFactory.VarSection(varKeyword, variables);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return varSectionSyntax.WithAnnotations(annotations);
			}
			return varSectionSyntax;
		}
		return this;
	}

	public VarSectionSyntax WithVarKeyword(SyntaxToken varKeyword)
	{
		return Update(varKeyword, Variables);
	}

	public VarSectionSyntax WithVariables(SyntaxList<VariableDeclarationSyntax> variables)
	{
		return Update(VarKeyword, variables);
	}

	public VarSectionSyntax AddVariables(params VariableDeclarationSyntax[] items)
	{
		return WithVariables(Variables.AddRange(items));
	}
}
