using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class VariableDeclarationSyntax : CSharpSyntaxNode
{
	internal readonly TypeSyntax type;

	internal readonly GreenNode variables;

	public TypeSyntax Type => type;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<VariableDeclaratorSyntax> Variables => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<VariableDeclaratorSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(variables));

	internal VariableDeclarationSyntax(SyntaxKind kind, TypeSyntax type, GreenNode variables, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (variables != null)
		{
			AdjustFlagsAndWidth(variables);
			this.variables = variables;
		}
	}

	internal VariableDeclarationSyntax(SyntaxKind kind, TypeSyntax type, GreenNode variables, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (variables != null)
		{
			AdjustFlagsAndWidth(variables);
			this.variables = variables;
		}
	}

	internal VariableDeclarationSyntax(SyntaxKind kind, TypeSyntax type, GreenNode variables)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (variables != null)
		{
			AdjustFlagsAndWidth(variables);
			this.variables = variables;
		}
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => type, 
			1 => variables, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitVariableDeclaration(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitVariableDeclaration(this);
	}

	public VariableDeclarationSyntax Update(TypeSyntax type, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<VariableDeclaratorSyntax> variables)
	{
		if (type != Type || variables != Variables)
		{
			VariableDeclarationSyntax variableDeclarationSyntax = SyntaxFactory.VariableDeclaration(type, variables);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				variableDeclarationSyntax = variableDeclarationSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				variableDeclarationSyntax = variableDeclarationSyntax.WithAnnotationsGreen(annotations);
			}
			return variableDeclarationSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new VariableDeclarationSyntax(Kind, type, variables, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new VariableDeclarationSyntax(Kind, type, variables, GetDiagnostics(), annotations);
	}

	internal VariableDeclarationSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		TypeSyntax typeSyntax = (TypeSyntax)reader.ReadValue();
		if (typeSyntax != null)
		{
			AdjustFlagsAndWidth(typeSyntax);
			type = typeSyntax;
		}
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			variables = greenNode;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(type);
		writer.WriteValue(variables);
	}

	static VariableDeclarationSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(VariableDeclarationSyntax), (ObjectReader r) => new VariableDeclarationSyntax(r));
	}
}
