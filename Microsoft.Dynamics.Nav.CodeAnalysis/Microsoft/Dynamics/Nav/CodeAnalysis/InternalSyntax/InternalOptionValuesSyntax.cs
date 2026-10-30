using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalOptionValuesSyntax : InternalLiteralValueSyntax
{
	internal readonly InternalSyntaxNode _options;

	internal override SyntaxNodeDefinition Definition => OptionValuesSyntaxDefinition.Instance.Value;

	public InternalSeparatedSyntaxList<InternalIdentifierNameOrEmptySyntax> Options => new InternalSeparatedSyntaxList<InternalIdentifierNameOrEmptySyntax>(new InternalSyntaxList<InternalSyntaxNode>(_options));

	internal InternalOptionValuesSyntax(SyntaxKind kind, InternalSyntaxNode options, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		if (options != null)
		{
			AdjustFlagsAndWidth(options);
			_options = options;
		}
	}

	internal InternalOptionValuesSyntax(SyntaxKind kind, InternalSyntaxNode options, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		if (options != null)
		{
			AdjustFlagsAndWidth(options);
			_options = options;
		}
	}

	internal InternalOptionValuesSyntax(SyntaxKind kind, InternalSyntaxNode options)
		: base(kind)
	{
		SlotCount = 1;
		if (options != null)
		{
			AdjustFlagsAndWidth(options);
			_options = options;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		if (index == 0)
		{
			return _options;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new OptionValuesSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitOptionValues(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitOptionValues(this);
	}

	public InternalOptionValuesSyntax Update(InternalSeparatedSyntaxList<InternalIdentifierNameOrEmptySyntax> options)
	{
		if (options != Options)
		{
			return InternalSyntaxFactory.OptionValues(options).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalOptionValuesSyntax(Kind, _options, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalOptionValuesSyntax(Kind, _options, GetDiagnostics(), annotations);
	}
}
