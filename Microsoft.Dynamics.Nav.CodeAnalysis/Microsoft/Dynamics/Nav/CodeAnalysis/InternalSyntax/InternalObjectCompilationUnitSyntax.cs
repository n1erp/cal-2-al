using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalObjectCompilationUnitSyntax : InternalCompilationUnitSyntax
{
	internal readonly InternalSyntaxNode _objects;

	internal readonly InternalSyntaxToken _endOfFileToken;

	internal override SyntaxNodeDefinition Definition => ObjectCompilationUnitSyntaxDefinition.Instance.Value;

	public InternalSyntaxList<InternalObjectSyntax> Objects => new InternalSyntaxList<InternalObjectSyntax>(_objects);

	public InternalSyntaxToken EndOfFileToken => _endOfFileToken;

	internal InternalObjectCompilationUnitSyntax(SyntaxKind kind, InternalSyntaxNode objects, InternalSyntaxToken endOfFileToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		if (objects != null)
		{
			AdjustFlagsAndWidth(objects);
			_objects = objects;
		}
		AdjustFlagsAndWidth(endOfFileToken);
		_endOfFileToken = endOfFileToken;
	}

	internal InternalObjectCompilationUnitSyntax(SyntaxKind kind, InternalSyntaxNode objects, InternalSyntaxToken endOfFileToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 2;
		if (objects != null)
		{
			AdjustFlagsAndWidth(objects);
			_objects = objects;
		}
		AdjustFlagsAndWidth(endOfFileToken);
		_endOfFileToken = endOfFileToken;
	}

	internal InternalObjectCompilationUnitSyntax(SyntaxKind kind, InternalSyntaxNode objects, InternalSyntaxToken endOfFileToken)
		: base(kind)
	{
		SlotCount = 2;
		if (objects != null)
		{
			AdjustFlagsAndWidth(objects);
			_objects = objects;
		}
		AdjustFlagsAndWidth(endOfFileToken);
		_endOfFileToken = endOfFileToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _objects, 
			1 => _endOfFileToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ObjectCompilationUnitSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitObjectCompilationUnit(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitObjectCompilationUnit(this);
	}

	public InternalObjectCompilationUnitSyntax Update(InternalSyntaxList<InternalObjectSyntax> objects, InternalSyntaxToken endOfFileToken)
	{
		if (objects != Objects || endOfFileToken != EndOfFileToken)
		{
			return InternalSyntaxFactory.ObjectCompilationUnit(objects, endOfFileToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalObjectCompilationUnitSyntax(Kind, _objects, _endOfFileToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalObjectCompilationUnitSyntax(Kind, _objects, _endOfFileToken, GetDiagnostics(), annotations);
	}
}
