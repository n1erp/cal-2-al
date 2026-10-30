using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalOptionDataTypeSyntax : InternalDataTypeSyntax
{
	internal readonly InternalSyntaxToken _typeName;

	internal readonly InternalOptionValuesSyntax _optionValues;

	internal override SyntaxNodeDefinition Definition => OptionDataTypeSyntaxDefinition.Instance.Value;

	public override InternalSyntaxToken TypeName => _typeName;

	public InternalOptionValuesSyntax OptionValues => _optionValues;

	internal InternalOptionDataTypeSyntax(SyntaxKind kind, InternalSyntaxToken typeName, InternalOptionValuesSyntax optionValues, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
		if (optionValues != null)
		{
			AdjustFlagsAndWidth(optionValues);
			_optionValues = optionValues;
		}
	}

	internal InternalOptionDataTypeSyntax(SyntaxKind kind, InternalSyntaxToken typeName, InternalOptionValuesSyntax optionValues, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
		if (optionValues != null)
		{
			AdjustFlagsAndWidth(optionValues);
			_optionValues = optionValues;
		}
	}

	internal InternalOptionDataTypeSyntax(SyntaxKind kind, InternalSyntaxToken typeName, InternalOptionValuesSyntax optionValues)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
		if (optionValues != null)
		{
			AdjustFlagsAndWidth(optionValues);
			_optionValues = optionValues;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_typeName, 
			1 => _optionValues, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new OptionDataTypeSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitOptionDataType(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitOptionDataType(this);
	}

	public InternalOptionDataTypeSyntax Update(InternalSyntaxToken typeName, InternalOptionValuesSyntax optionValues)
	{
		if (typeName != TypeName || optionValues != OptionValues)
		{
			return InternalSyntaxFactory.OptionDataType(typeName, optionValues).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalOptionDataTypeSyntax(Kind, _typeName, _optionValues, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalOptionDataTypeSyntax(Kind, _typeName, _optionValues, GetDiagnostics(), annotations);
	}
}
