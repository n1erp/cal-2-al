using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalReturnValueSyntax : InternalSyntaxNode
{
	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _colonToken;

	internal readonly InternalDataTypeSyntax _dataType;

	internal override SyntaxNodeDefinition Definition => ReturnValueSyntaxDefinition.Instance.Value;

	public InternalIdentifierNameSyntax Name => _name;

	public InternalSyntaxToken ColonToken => _colonToken;

	public InternalDataTypeSyntax DataType => _dataType;

	internal InternalReturnValueSyntax(SyntaxKind kind, InternalIdentifierNameSyntax name, InternalSyntaxToken colonToken, InternalDataTypeSyntax dataType, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		if (name != null)
		{
			AdjustFlagsAndWidth(name);
			_name = name;
		}
		AdjustFlagsAndWidth(colonToken);
		_colonToken = colonToken;
		AdjustFlagsAndWidth(dataType);
		_dataType = dataType;
	}

	internal InternalReturnValueSyntax(SyntaxKind kind, InternalIdentifierNameSyntax name, InternalSyntaxToken colonToken, InternalDataTypeSyntax dataType, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 3;
		if (name != null)
		{
			AdjustFlagsAndWidth(name);
			_name = name;
		}
		AdjustFlagsAndWidth(colonToken);
		_colonToken = colonToken;
		AdjustFlagsAndWidth(dataType);
		_dataType = dataType;
	}

	internal InternalReturnValueSyntax(SyntaxKind kind, InternalIdentifierNameSyntax name, InternalSyntaxToken colonToken, InternalDataTypeSyntax dataType)
		: base(kind)
	{
		SlotCount = 3;
		if (name != null)
		{
			AdjustFlagsAndWidth(name);
			_name = name;
		}
		AdjustFlagsAndWidth(colonToken);
		_colonToken = colonToken;
		AdjustFlagsAndWidth(dataType);
		_dataType = dataType;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_name, 
			1 => _colonToken, 
			2 => _dataType, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ReturnValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitReturnValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitReturnValue(this);
	}

	public InternalReturnValueSyntax Update(InternalIdentifierNameSyntax name, InternalSyntaxToken colonToken, InternalDataTypeSyntax dataType)
	{
		if (name != Name || colonToken != ColonToken || dataType != DataType)
		{
			return InternalSyntaxFactory.ReturnValue(name, colonToken, dataType).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalReturnValueSyntax(Kind, _name, _colonToken, _dataType, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalReturnValueSyntax(Kind, _name, _colonToken, _dataType, GetDiagnostics(), annotations);
	}
}
