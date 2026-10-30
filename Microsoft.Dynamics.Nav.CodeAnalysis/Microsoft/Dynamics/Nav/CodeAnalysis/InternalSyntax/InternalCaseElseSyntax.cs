using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalCaseElseSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _elseKeywordToken;

	internal readonly InternalSyntaxNode _elseStatements;

	internal override SyntaxNodeDefinition Definition => CaseElseSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken ElseKeywordToken => _elseKeywordToken;

	public InternalSyntaxList<InternalStatementSyntax> ElseStatements => new InternalSyntaxList<InternalStatementSyntax>(_elseStatements);

	internal InternalCaseElseSyntax(SyntaxKind kind, InternalSyntaxToken elseKeywordToken, InternalSyntaxNode elseStatements, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(elseKeywordToken);
		_elseKeywordToken = elseKeywordToken;
		if (elseStatements != null)
		{
			AdjustFlagsAndWidth(elseStatements);
			_elseStatements = elseStatements;
		}
	}

	internal InternalCaseElseSyntax(SyntaxKind kind, InternalSyntaxToken elseKeywordToken, InternalSyntaxNode elseStatements, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(elseKeywordToken);
		_elseKeywordToken = elseKeywordToken;
		if (elseStatements != null)
		{
			AdjustFlagsAndWidth(elseStatements);
			_elseStatements = elseStatements;
		}
	}

	internal InternalCaseElseSyntax(SyntaxKind kind, InternalSyntaxToken elseKeywordToken, InternalSyntaxNode elseStatements)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(elseKeywordToken);
		_elseKeywordToken = elseKeywordToken;
		if (elseStatements != null)
		{
			AdjustFlagsAndWidth(elseStatements);
			_elseStatements = elseStatements;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _elseKeywordToken, 
			1 => _elseStatements, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new CaseElseSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCaseElse(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitCaseElse(this);
	}

	public InternalCaseElseSyntax Update(InternalSyntaxToken elseKeywordToken, InternalSyntaxList<InternalStatementSyntax> elseStatements)
	{
		if (elseKeywordToken != ElseKeywordToken || elseStatements != ElseStatements)
		{
			return InternalSyntaxFactory.CaseElse(elseKeywordToken, elseStatements).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalCaseElseSyntax(Kind, _elseKeywordToken, _elseStatements, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalCaseElseSyntax(Kind, _elseKeywordToken, _elseStatements, GetDiagnostics(), annotations);
	}
}
