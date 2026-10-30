using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalCommaSeparatedIdentifierEqualsStringListSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxNode _values;

	internal override SyntaxNodeDefinition Definition => CommaSeparatedIdentifierEqualsStringListSyntaxDefinition.Instance.Value;

	public InternalSeparatedSyntaxList<InternalIdentifierEqualsStringSyntax> Values => new InternalSeparatedSyntaxList<InternalIdentifierEqualsStringSyntax>(new InternalSyntaxList<InternalSyntaxNode>(_values));

	internal InternalCommaSeparatedIdentifierEqualsStringListSyntax(SyntaxKind kind, InternalSyntaxNode values, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		if (values != null)
		{
			AdjustFlagsAndWidth(values);
			_values = values;
		}
	}

	internal InternalCommaSeparatedIdentifierEqualsStringListSyntax(SyntaxKind kind, InternalSyntaxNode values, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		if (values != null)
		{
			AdjustFlagsAndWidth(values);
			_values = values;
		}
	}

	internal InternalCommaSeparatedIdentifierEqualsStringListSyntax(SyntaxKind kind, InternalSyntaxNode values)
		: base(kind)
	{
		SlotCount = 1;
		if (values != null)
		{
			AdjustFlagsAndWidth(values);
			_values = values;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		if (index == 0)
		{
			return _values;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new CommaSeparatedIdentifierEqualsStringListSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCommaSeparatedIdentifierEqualsStringList(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitCommaSeparatedIdentifierEqualsStringList(this);
	}

	public InternalCommaSeparatedIdentifierEqualsStringListSyntax Update(InternalSeparatedSyntaxList<InternalIdentifierEqualsStringSyntax> values)
	{
		if (values != Values)
		{
			return InternalSyntaxFactory.CommaSeparatedIdentifierEqualsStringList(values).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalCommaSeparatedIdentifierEqualsStringListSyntax(Kind, _values, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalCommaSeparatedIdentifierEqualsStringListSyntax(Kind, _values, GetDiagnostics(), annotations);
	}
}
