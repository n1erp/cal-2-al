using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class DeclarationPatternSyntax : PatternSyntax
{
	internal readonly TypeSyntax type;

	internal readonly VariableDesignationSyntax designation;

	public TypeSyntax Type => type;

	public VariableDesignationSyntax Designation => designation;

	internal DeclarationPatternSyntax(SyntaxKind kind, TypeSyntax type, VariableDesignationSyntax designation, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(type);
		this.type = type;
		AdjustFlagsAndWidth(designation);
		this.designation = designation;
	}

	internal DeclarationPatternSyntax(SyntaxKind kind, TypeSyntax type, VariableDesignationSyntax designation, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(type);
		this.type = type;
		AdjustFlagsAndWidth(designation);
		this.designation = designation;
	}

	internal DeclarationPatternSyntax(SyntaxKind kind, TypeSyntax type, VariableDesignationSyntax designation)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(type);
		this.type = type;
		AdjustFlagsAndWidth(designation);
		this.designation = designation;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)type, 
			1 => designation, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.DeclarationPatternSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDeclarationPattern(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitDeclarationPattern(this);
	}

	public DeclarationPatternSyntax Update(TypeSyntax type, VariableDesignationSyntax designation)
	{
		if (type != Type || designation != Designation)
		{
			DeclarationPatternSyntax declarationPatternSyntax = SyntaxFactory.DeclarationPattern(type, designation);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				declarationPatternSyntax = declarationPatternSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				declarationPatternSyntax = declarationPatternSyntax.WithAnnotationsGreen(annotations);
			}
			return declarationPatternSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new DeclarationPatternSyntax(Kind, type, designation, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new DeclarationPatternSyntax(Kind, type, designation, GetDiagnostics(), annotations);
	}

	internal DeclarationPatternSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		TypeSyntax typeSyntax = (TypeSyntax)reader.ReadValue();
		if (typeSyntax != null)
		{
			AdjustFlagsAndWidth(typeSyntax);
			type = typeSyntax;
		}
		VariableDesignationSyntax variableDesignationSyntax = (VariableDesignationSyntax)reader.ReadValue();
		if (variableDesignationSyntax != null)
		{
			AdjustFlagsAndWidth(variableDesignationSyntax);
			designation = variableDesignationSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(type);
		writer.WriteValue(designation);
	}

	static DeclarationPatternSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(DeclarationPatternSyntax), (ObjectReader r) => new DeclarationPatternSyntax(r));
	}
}
