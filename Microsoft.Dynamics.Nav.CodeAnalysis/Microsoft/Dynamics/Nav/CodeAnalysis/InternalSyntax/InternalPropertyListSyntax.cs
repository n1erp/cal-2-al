using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalPropertyListSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxNode _properties;

	internal override SyntaxNodeDefinition Definition => PropertyListSyntaxDefinition.Instance.Value;

	public InternalSyntaxList<InternalPropertySyntaxOrEmpty> Properties => new InternalSyntaxList<InternalPropertySyntaxOrEmpty>(_properties);

	internal InternalPropertyListSyntax(SyntaxKind kind, InternalSyntaxNode properties, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		if (properties != null)
		{
			AdjustFlagsAndWidth(properties);
			_properties = properties;
		}
	}

	internal InternalPropertyListSyntax(SyntaxKind kind, InternalSyntaxNode properties, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		if (properties != null)
		{
			AdjustFlagsAndWidth(properties);
			_properties = properties;
		}
	}

	internal InternalPropertyListSyntax(SyntaxKind kind, InternalSyntaxNode properties)
		: base(kind)
	{
		SlotCount = 1;
		if (properties != null)
		{
			AdjustFlagsAndWidth(properties);
			_properties = properties;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		if (index == 0)
		{
			return _properties;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new PropertyListSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPropertyList(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitPropertyList(this);
	}

	public InternalPropertyListSyntax Update(InternalSyntaxList<InternalPropertySyntaxOrEmpty> properties)
	{
		if (properties != Properties)
		{
			return InternalSyntaxFactory.PropertyList(properties).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalPropertyListSyntax(Kind, _properties, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalPropertyListSyntax(Kind, _properties, GetDiagnostics(), annotations);
	}
}
