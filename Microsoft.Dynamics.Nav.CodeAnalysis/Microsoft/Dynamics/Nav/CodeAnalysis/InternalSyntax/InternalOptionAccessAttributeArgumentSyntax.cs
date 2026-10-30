using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalOptionAccessAttributeArgumentSyntax : InternalAttributeArgumentSyntax
{
	internal readonly InternalOptionAccessExpressionSyntax _optionAccess;

	internal override SyntaxNodeDefinition Definition => OptionAccessAttributeArgumentSyntaxDefinition.Instance.Value;

	public InternalOptionAccessExpressionSyntax OptionAccess => _optionAccess;

	internal InternalOptionAccessAttributeArgumentSyntax(SyntaxKind kind, InternalOptionAccessExpressionSyntax optionAccess, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(optionAccess);
		_optionAccess = optionAccess;
	}

	internal InternalOptionAccessAttributeArgumentSyntax(SyntaxKind kind, InternalOptionAccessExpressionSyntax optionAccess, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(optionAccess);
		_optionAccess = optionAccess;
	}

	internal InternalOptionAccessAttributeArgumentSyntax(SyntaxKind kind, InternalOptionAccessExpressionSyntax optionAccess)
		: base(kind)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(optionAccess);
		_optionAccess = optionAccess;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		if (index == 0)
		{
			return _optionAccess;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new OptionAccessAttributeArgumentSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitOptionAccessAttributeArgument(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitOptionAccessAttributeArgument(this);
	}

	public InternalOptionAccessAttributeArgumentSyntax Update(InternalOptionAccessExpressionSyntax optionAccess)
	{
		if (optionAccess != OptionAccess)
		{
			return InternalSyntaxFactory.OptionAccessAttributeArgument(optionAccess).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalOptionAccessAttributeArgumentSyntax(Kind, _optionAccess, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalOptionAccessAttributeArgumentSyntax(Kind, _optionAccess, GetDiagnostics(), annotations);
	}
}
