using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalVarSectionSyntax : InternalMemberSyntax
{
	internal readonly InternalSyntaxToken _varKeyword;

	internal readonly InternalSyntaxNode _variables;

	internal override SyntaxNodeDefinition Definition => VarSectionSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken VarKeyword => _varKeyword;

	public InternalSyntaxList<InternalVariableDeclarationSyntax> Variables => new InternalSyntaxList<InternalVariableDeclarationSyntax>(_variables);

	internal InternalVarSectionSyntax(SyntaxKind kind, InternalSyntaxToken varKeyword, InternalSyntaxNode variables, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(varKeyword);
		_varKeyword = varKeyword;
		if (variables != null)
		{
			AdjustFlagsAndWidth(variables);
			_variables = variables;
		}
	}

	internal InternalVarSectionSyntax(SyntaxKind kind, InternalSyntaxToken varKeyword, InternalSyntaxNode variables, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(varKeyword);
		_varKeyword = varKeyword;
		if (variables != null)
		{
			AdjustFlagsAndWidth(variables);
			_variables = variables;
		}
	}

	internal InternalVarSectionSyntax(SyntaxKind kind, InternalSyntaxToken varKeyword, InternalSyntaxNode variables)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(varKeyword);
		_varKeyword = varKeyword;
		if (variables != null)
		{
			AdjustFlagsAndWidth(variables);
			_variables = variables;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _varKeyword, 
			1 => _variables, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new VarSectionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitVarSection(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitVarSection(this);
	}

	public InternalVarSectionSyntax Update(InternalSyntaxToken varKeyword, InternalSyntaxList<InternalVariableDeclarationSyntax> variables)
	{
		if (varKeyword != VarKeyword || variables != Variables)
		{
			return InternalSyntaxFactory.VarSection(varKeyword, variables).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalVarSectionSyntax(Kind, _varKeyword, _variables, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalVarSectionSyntax(Kind, _varKeyword, _variables, GetDiagnostics(), annotations);
	}
}
