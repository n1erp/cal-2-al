using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalCommaSeparatedPropertyValueSyntax : InternalSeparatedListPropertyValueSyntax
{
	internal readonly InternalSyntaxNode _values;

	internal override SyntaxNodeDefinition Definition => CommaSeparatedPropertyValueSyntaxDefinition.Instance.Value;

	public InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> Values => new InternalSeparatedSyntaxList<InternalIdentifierNameSyntax>(new InternalSyntaxList<InternalSyntaxNode>(_values));

	internal InternalCommaSeparatedPropertyValueSyntax(SyntaxKind kind, InternalSyntaxNode values, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		if (values != null)
		{
			AdjustFlagsAndWidth(values);
			_values = values;
		}
	}

	internal InternalCommaSeparatedPropertyValueSyntax(SyntaxKind kind, InternalSyntaxNode values, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		if (values != null)
		{
			AdjustFlagsAndWidth(values);
			_values = values;
		}
	}

	internal InternalCommaSeparatedPropertyValueSyntax(SyntaxKind kind, InternalSyntaxNode values)
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
		return new CommaSeparatedPropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCommaSeparatedPropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitCommaSeparatedPropertyValue(this);
	}

	public InternalCommaSeparatedPropertyValueSyntax Update(InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> values)
	{
		if (values != Values)
		{
			return InternalSyntaxFactory.CommaSeparatedPropertyValue(values).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalCommaSeparatedPropertyValueSyntax(Kind, _values, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalCommaSeparatedPropertyValueSyntax(Kind, _values, GetDiagnostics(), annotations);
	}
}
