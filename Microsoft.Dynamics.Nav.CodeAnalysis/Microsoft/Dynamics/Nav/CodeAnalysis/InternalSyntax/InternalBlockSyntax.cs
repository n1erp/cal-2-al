using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalBlockSyntax : InternalStatementSyntax
{
	internal readonly InternalSyntaxToken _beginKeywordToken;

	internal readonly InternalSyntaxNode _statements;

	internal readonly InternalSyntaxToken _endKeywordToken;

	internal readonly InternalSyntaxToken _semicolonToken;

	internal override SyntaxNodeDefinition Definition => BlockSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken BeginKeywordToken => _beginKeywordToken;

	public InternalSyntaxList<InternalStatementSyntax> Statements => new InternalSyntaxList<InternalStatementSyntax>(_statements);

	public InternalSyntaxToken EndKeywordToken => _endKeywordToken;

	public InternalSyntaxToken SemicolonToken => _semicolonToken;

	internal InternalBlockSyntax(SyntaxKind kind, InternalSyntaxToken beginKeywordToken, InternalSyntaxNode statements, InternalSyntaxToken endKeywordToken, InternalSyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(beginKeywordToken);
		_beginKeywordToken = beginKeywordToken;
		if (statements != null)
		{
			AdjustFlagsAndWidth(statements);
			_statements = statements;
		}
		AdjustFlagsAndWidth(endKeywordToken);
		_endKeywordToken = endKeywordToken;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
	}

	internal InternalBlockSyntax(SyntaxKind kind, InternalSyntaxToken beginKeywordToken, InternalSyntaxNode statements, InternalSyntaxToken endKeywordToken, InternalSyntaxToken semicolonToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(beginKeywordToken);
		_beginKeywordToken = beginKeywordToken;
		if (statements != null)
		{
			AdjustFlagsAndWidth(statements);
			_statements = statements;
		}
		AdjustFlagsAndWidth(endKeywordToken);
		_endKeywordToken = endKeywordToken;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
	}

	internal InternalBlockSyntax(SyntaxKind kind, InternalSyntaxToken beginKeywordToken, InternalSyntaxNode statements, InternalSyntaxToken endKeywordToken, InternalSyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(beginKeywordToken);
		_beginKeywordToken = beginKeywordToken;
		if (statements != null)
		{
			AdjustFlagsAndWidth(statements);
			_statements = statements;
		}
		AdjustFlagsAndWidth(endKeywordToken);
		_endKeywordToken = endKeywordToken;
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
			0 => _beginKeywordToken, 
			1 => _statements, 
			2 => _endKeywordToken, 
			3 => _semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new BlockSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitBlock(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitBlock(this);
	}

	public InternalBlockSyntax Update(InternalSyntaxToken beginKeywordToken, InternalSyntaxList<InternalStatementSyntax> statements, InternalSyntaxToken endKeywordToken, InternalSyntaxToken semicolonToken)
	{
		if (beginKeywordToken != BeginKeywordToken || statements != Statements || endKeywordToken != EndKeywordToken || semicolonToken != SemicolonToken)
		{
			return InternalSyntaxFactory.Block(beginKeywordToken, statements, endKeywordToken, semicolonToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalBlockSyntax(Kind, _beginKeywordToken, _statements, _endKeywordToken, _semicolonToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalBlockSyntax(Kind, _beginKeywordToken, _statements, _endKeywordToken, _semicolonToken, GetDiagnostics(), annotations);
	}
}
