using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalReportLabelSyntax : InternalReportLabelBaseSyntax
{
	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _equalsToken;

	internal readonly InternalLabelSyntax _label;

	internal readonly InternalSyntaxToken _semicolonToken;

	internal override SyntaxNodeDefinition Definition => ReportLabelSyntaxDefinition.Instance.Value;

	public override InternalIdentifierNameSyntax Name => _name;

	public InternalSyntaxToken EqualsToken => _equalsToken;

	public InternalLabelSyntax Label => _label;

	public InternalSyntaxToken SemicolonToken => _semicolonToken;

	internal InternalReportLabelSyntax(SyntaxKind kind, InternalIdentifierNameSyntax name, InternalSyntaxToken equalsToken, InternalLabelSyntax label, InternalSyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(label);
		_label = label;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
	}

	internal InternalReportLabelSyntax(SyntaxKind kind, InternalIdentifierNameSyntax name, InternalSyntaxToken equalsToken, InternalLabelSyntax label, InternalSyntaxToken semicolonToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(label);
		_label = label;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
	}

	internal InternalReportLabelSyntax(SyntaxKind kind, InternalIdentifierNameSyntax name, InternalSyntaxToken equalsToken, InternalLabelSyntax label, InternalSyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(label);
		_label = label;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_name, 
			1 => _equalsToken, 
			2 => _label, 
			3 => _semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ReportLabelSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitReportLabel(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitReportLabel(this);
	}

	public InternalReportLabelSyntax Update(InternalIdentifierNameSyntax name, InternalSyntaxToken equalsToken, InternalLabelSyntax label, InternalSyntaxToken semicolonToken)
	{
		if (name != Name || equalsToken != EqualsToken || label != Label || semicolonToken != SemicolonToken)
		{
			return InternalSyntaxFactory.ReportLabel(name, equalsToken, label, semicolonToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalReportLabelSyntax(Kind, _name, _equalsToken, _label, _semicolonToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalReportLabelSyntax(Kind, _name, _equalsToken, _label, _semicolonToken, GetDiagnostics(), annotations);
	}
}
