using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalAssignmentStatementSyntax : InternalStatementSyntax
{
	internal readonly InternalCodeExpressionSyntax _target;

	internal readonly InternalSyntaxToken _assignmentToken;

	internal readonly InternalCodeExpressionSyntax _source;

	internal readonly InternalSyntaxToken _semicolonToken;

	internal override SyntaxNodeDefinition Definition => AssignmentStatementSyntaxDefinition.Instance.Value;

	public InternalCodeExpressionSyntax Target => _target;

	public InternalSyntaxToken AssignmentToken => _assignmentToken;

	public InternalCodeExpressionSyntax Source => _source;

	public InternalSyntaxToken SemicolonToken => _semicolonToken;

	internal InternalAssignmentStatementSyntax(SyntaxKind kind, InternalCodeExpressionSyntax target, InternalSyntaxToken assignmentToken, InternalCodeExpressionSyntax source, InternalSyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(target);
		_target = target;
		AdjustFlagsAndWidth(assignmentToken);
		_assignmentToken = assignmentToken;
		AdjustFlagsAndWidth(source);
		_source = source;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
	}

	internal InternalAssignmentStatementSyntax(SyntaxKind kind, InternalCodeExpressionSyntax target, InternalSyntaxToken assignmentToken, InternalCodeExpressionSyntax source, InternalSyntaxToken semicolonToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(target);
		_target = target;
		AdjustFlagsAndWidth(assignmentToken);
		_assignmentToken = assignmentToken;
		AdjustFlagsAndWidth(source);
		_source = source;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
	}

	internal InternalAssignmentStatementSyntax(SyntaxKind kind, InternalCodeExpressionSyntax target, InternalSyntaxToken assignmentToken, InternalCodeExpressionSyntax source, InternalSyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(target);
		_target = target;
		AdjustFlagsAndWidth(assignmentToken);
		_assignmentToken = assignmentToken;
		AdjustFlagsAndWidth(source);
		_source = source;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_target, 
			1 => _assignmentToken, 
			2 => _source, 
			3 => _semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new AssignmentStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitAssignmentStatement(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitAssignmentStatement(this);
	}

	public InternalAssignmentStatementSyntax Update(InternalCodeExpressionSyntax target, InternalSyntaxToken assignmentToken, InternalCodeExpressionSyntax source, InternalSyntaxToken semicolonToken)
	{
		if (target != Target || assignmentToken != AssignmentToken || source != Source || semicolonToken != SemicolonToken)
		{
			return InternalSyntaxFactory.AssignmentStatement(target, assignmentToken, source, semicolonToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalAssignmentStatementSyntax(Kind, _target, _assignmentToken, _source, _semicolonToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalAssignmentStatementSyntax(Kind, _target, _assignmentToken, _source, _semicolonToken, GetDiagnostics(), annotations);
	}
}
