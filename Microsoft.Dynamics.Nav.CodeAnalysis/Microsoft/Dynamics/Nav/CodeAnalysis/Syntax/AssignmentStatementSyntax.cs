using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class AssignmentStatementSyntax : StatementSyntax
{
	private CodeExpressionSyntax _target;

	private CodeExpressionSyntax _source;

	public CodeExpressionSyntax Target => GetRedAtZero(ref _target);

	public SyntaxToken AssignmentToken => new SyntaxToken(this, ((InternalAssignmentStatementSyntax)Node)._assignmentToken, GetChildPosition(1), GetChildIndex(1));

	public CodeExpressionSyntax Source => GetRed(ref _source, 2);

	public SyntaxToken SemicolonToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalAssignmentStatementSyntax)Node)._semicolonToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(3), GetChildIndex(3));
			}
			return default;
		}
	}

	internal AssignmentStatementSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref _target), 
			2 => GetRed(ref _source, 2), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)_target, 
			2 => _source, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitAssignmentStatement(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitAssignmentStatement(this);
	}

	public AssignmentStatementSyntax Update(CodeExpressionSyntax target, SyntaxToken assignmentToken, CodeExpressionSyntax source, SyntaxToken semicolonToken)
	{
		if (target != Target || assignmentToken != AssignmentToken || source != Source || semicolonToken != SemicolonToken)
		{
			AssignmentStatementSyntax assignmentStatementSyntax = SyntaxFactory.AssignmentStatement(target, assignmentToken, source, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return assignmentStatementSyntax.WithAnnotations(annotations);
			}
			return assignmentStatementSyntax;
		}
		return this;
	}

	public AssignmentStatementSyntax WithTarget(CodeExpressionSyntax target)
	{
		return Update(target, AssignmentToken, Source, SemicolonToken);
	}

	public AssignmentStatementSyntax WithAssignmentToken(SyntaxToken assignmentToken)
	{
		return Update(Target, assignmentToken, Source, SemicolonToken);
	}

	public AssignmentStatementSyntax WithSource(CodeExpressionSyntax source)
	{
		return Update(Target, AssignmentToken, source, SemicolonToken);
	}

	public AssignmentStatementSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(Target, AssignmentToken, Source, semicolonToken);
	}
}
