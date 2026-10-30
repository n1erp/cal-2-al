using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalCaseLineSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxNode _expressions;

	internal readonly InternalSyntaxToken _colonToken;

	internal readonly InternalStatementSyntax _statement;

	internal override SyntaxNodeDefinition Definition => CaseLineSyntaxDefinition.Instance.Value;

	public InternalSeparatedSyntaxList<InternalCodeExpressionSyntax> Expressions => new InternalSeparatedSyntaxList<InternalCodeExpressionSyntax>(new InternalSyntaxList<InternalSyntaxNode>(_expressions));

	public InternalSyntaxToken ColonToken => _colonToken;

	public InternalStatementSyntax Statement => _statement;

	internal InternalCaseLineSyntax(SyntaxKind kind, InternalSyntaxNode expressions, InternalSyntaxToken colonToken, InternalStatementSyntax statement, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		if (expressions != null)
		{
			AdjustFlagsAndWidth(expressions);
			_expressions = expressions;
		}
		AdjustFlagsAndWidth(colonToken);
		_colonToken = colonToken;
		AdjustFlagsAndWidth(statement);
		_statement = statement;
	}

	internal InternalCaseLineSyntax(SyntaxKind kind, InternalSyntaxNode expressions, InternalSyntaxToken colonToken, InternalStatementSyntax statement, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 3;
		if (expressions != null)
		{
			AdjustFlagsAndWidth(expressions);
			_expressions = expressions;
		}
		AdjustFlagsAndWidth(colonToken);
		_colonToken = colonToken;
		AdjustFlagsAndWidth(statement);
		_statement = statement;
	}

	internal InternalCaseLineSyntax(SyntaxKind kind, InternalSyntaxNode expressions, InternalSyntaxToken colonToken, InternalStatementSyntax statement)
		: base(kind)
	{
		SlotCount = 3;
		if (expressions != null)
		{
			AdjustFlagsAndWidth(expressions);
			_expressions = expressions;
		}
		AdjustFlagsAndWidth(colonToken);
		_colonToken = colonToken;
		AdjustFlagsAndWidth(statement);
		_statement = statement;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _expressions, 
			1 => _colonToken, 
			2 => _statement, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new CaseLineSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCaseLine(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitCaseLine(this);
	}

	public InternalCaseLineSyntax Update(InternalSeparatedSyntaxList<InternalCodeExpressionSyntax> expressions, InternalSyntaxToken colonToken, InternalStatementSyntax statement)
	{
		if (expressions != Expressions || colonToken != ColonToken || statement != Statement)
		{
			return InternalSyntaxFactory.CaseLine(expressions, colonToken, statement).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalCaseLineSyntax(Kind, _expressions, _colonToken, _statement, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalCaseLineSyntax(Kind, _expressions, _colonToken, _statement, GetDiagnostics(), annotations);
	}
}
