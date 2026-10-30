using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ParenthesizedVariableDesignationSyntax : VariableDesignationSyntax
{
	internal readonly SyntaxToken openParenToken;

	internal readonly GreenNode variables;

	internal readonly SyntaxToken closeParenToken;

	public SyntaxToken OpenParenToken => openParenToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<VariableDesignationSyntax> Variables => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<VariableDesignationSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(variables));

	public SyntaxToken CloseParenToken => closeParenToken;

	internal ParenthesizedVariableDesignationSyntax(SyntaxKind kind, SyntaxToken openParenToken, GreenNode variables, SyntaxToken closeParenToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		if (variables != null)
		{
			AdjustFlagsAndWidth(variables);
			this.variables = variables;
		}
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal ParenthesizedVariableDesignationSyntax(SyntaxKind kind, SyntaxToken openParenToken, GreenNode variables, SyntaxToken closeParenToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		if (variables != null)
		{
			AdjustFlagsAndWidth(variables);
			this.variables = variables;
		}
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal ParenthesizedVariableDesignationSyntax(SyntaxKind kind, SyntaxToken openParenToken, GreenNode variables, SyntaxToken closeParenToken)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		if (variables != null)
		{
			AdjustFlagsAndWidth(variables);
			this.variables = variables;
		}
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => openParenToken, 
			1 => variables, 
			2 => closeParenToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedVariableDesignationSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitParenthesizedVariableDesignation(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitParenthesizedVariableDesignation(this);
	}

	public ParenthesizedVariableDesignationSyntax Update(SyntaxToken openParenToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<VariableDesignationSyntax> variables, SyntaxToken closeParenToken)
	{
		if (openParenToken != OpenParenToken || variables != Variables || closeParenToken != CloseParenToken)
		{
			ParenthesizedVariableDesignationSyntax parenthesizedVariableDesignationSyntax = SyntaxFactory.ParenthesizedVariableDesignation(openParenToken, variables, closeParenToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				parenthesizedVariableDesignationSyntax = parenthesizedVariableDesignationSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				parenthesizedVariableDesignationSyntax = parenthesizedVariableDesignationSyntax.WithAnnotationsGreen(annotations);
			}
			return parenthesizedVariableDesignationSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ParenthesizedVariableDesignationSyntax(Kind, openParenToken, variables, closeParenToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ParenthesizedVariableDesignationSyntax(Kind, openParenToken, variables, closeParenToken, GetDiagnostics(), annotations);
	}

	internal ParenthesizedVariableDesignationSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			openParenToken = syntaxToken;
		}
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			variables = greenNode;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			closeParenToken = syntaxToken2;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(openParenToken);
		writer.WriteValue(variables);
		writer.WriteValue(closeParenToken);
	}

	static ParenthesizedVariableDesignationSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ParenthesizedVariableDesignationSyntax), (ObjectReader r) => new ParenthesizedVariableDesignationSyntax(r));
	}
}
