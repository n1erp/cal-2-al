using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalQueryDataItemLinkExpressionSyntax : InternalExpressionSyntax
{
	internal readonly InternalIdentifierNameSyntax _sourceField;

	internal readonly InternalSyntaxToken _equalsToken;

	internal readonly InternalMemberAccessExpressionSyntax _relatedField;

	internal override SyntaxNodeDefinition Definition => QueryDataItemLinkExpressionSyntaxDefinition.Instance.Value;

	public InternalIdentifierNameSyntax SourceField => _sourceField;

	public InternalSyntaxToken EqualsToken => _equalsToken;

	public InternalMemberAccessExpressionSyntax RelatedField => _relatedField;

	internal InternalQueryDataItemLinkExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax sourceField, InternalSyntaxToken equalsToken, InternalMemberAccessExpressionSyntax relatedField, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(sourceField);
		_sourceField = sourceField;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(relatedField);
		_relatedField = relatedField;
	}

	internal InternalQueryDataItemLinkExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax sourceField, InternalSyntaxToken equalsToken, InternalMemberAccessExpressionSyntax relatedField, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(sourceField);
		_sourceField = sourceField;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(relatedField);
		_relatedField = relatedField;
	}

	internal InternalQueryDataItemLinkExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax sourceField, InternalSyntaxToken equalsToken, InternalMemberAccessExpressionSyntax relatedField)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(sourceField);
		_sourceField = sourceField;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(relatedField);
		_relatedField = relatedField;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_sourceField, 
			1 => _equalsToken, 
			2 => _relatedField, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new QueryDataItemLinkExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitQueryDataItemLinkExpression(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitQueryDataItemLinkExpression(this);
	}

	public InternalQueryDataItemLinkExpressionSyntax Update(InternalIdentifierNameSyntax sourceField, InternalSyntaxToken equalsToken, InternalMemberAccessExpressionSyntax relatedField)
	{
		if (sourceField != SourceField || equalsToken != EqualsToken || relatedField != RelatedField)
		{
			return InternalSyntaxFactory.QueryDataItemLinkExpression(sourceField, equalsToken, relatedField).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalQueryDataItemLinkExpressionSyntax(Kind, _sourceField, _equalsToken, _relatedField, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalQueryDataItemLinkExpressionSyntax(Kind, _sourceField, _equalsToken, _relatedField, GetDiagnostics(), annotations);
	}
}
