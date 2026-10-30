using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalTextConstDataTypeSyntax : InternalDataTypeSyntax
{
	internal readonly InternalSyntaxToken _typeName;

	internal readonly InternalCommaSeparatedIdentifierEqualsStringListSyntax _multilanguage;

	internal override SyntaxNodeDefinition Definition => TextConstDataTypeSyntaxDefinition.Instance.Value;

	public override InternalSyntaxToken TypeName => _typeName;

	public InternalCommaSeparatedIdentifierEqualsStringListSyntax Multilanguage => _multilanguage;

	internal InternalTextConstDataTypeSyntax(SyntaxKind kind, InternalSyntaxToken typeName, InternalCommaSeparatedIdentifierEqualsStringListSyntax multilanguage, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
		if (multilanguage != null)
		{
			AdjustFlagsAndWidth(multilanguage);
			_multilanguage = multilanguage;
		}
	}

	internal InternalTextConstDataTypeSyntax(SyntaxKind kind, InternalSyntaxToken typeName, InternalCommaSeparatedIdentifierEqualsStringListSyntax multilanguage, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
		if (multilanguage != null)
		{
			AdjustFlagsAndWidth(multilanguage);
			_multilanguage = multilanguage;
		}
	}

	internal InternalTextConstDataTypeSyntax(SyntaxKind kind, InternalSyntaxToken typeName, InternalCommaSeparatedIdentifierEqualsStringListSyntax multilanguage)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
		if (multilanguage != null)
		{
			AdjustFlagsAndWidth(multilanguage);
			_multilanguage = multilanguage;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_typeName, 
			1 => _multilanguage, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new TextConstDataTypeSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTextConstDataType(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitTextConstDataType(this);
	}

	public InternalTextConstDataTypeSyntax Update(InternalSyntaxToken typeName, InternalCommaSeparatedIdentifierEqualsStringListSyntax multilanguage)
	{
		if (typeName != TypeName || multilanguage != Multilanguage)
		{
			return InternalSyntaxFactory.TextConstDataType(typeName, multilanguage).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalTextConstDataTypeSyntax(Kind, _typeName, _multilanguage, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalTextConstDataTypeSyntax(Kind, _typeName, _multilanguage, GetDiagnostics(), annotations);
	}
}
