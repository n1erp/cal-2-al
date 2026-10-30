using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ClassOrStructConstraintSyntax : TypeParameterConstraintSyntax
{
	internal readonly SyntaxToken classOrStructKeyword;

	public SyntaxToken ClassOrStructKeyword => classOrStructKeyword;

	internal ClassOrStructConstraintSyntax(SyntaxKind kind, SyntaxToken classOrStructKeyword, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(classOrStructKeyword);
		this.classOrStructKeyword = classOrStructKeyword;
	}

	internal ClassOrStructConstraintSyntax(SyntaxKind kind, SyntaxToken classOrStructKeyword, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 1;
		AdjustFlagsAndWidth(classOrStructKeyword);
		this.classOrStructKeyword = classOrStructKeyword;
	}

	internal ClassOrStructConstraintSyntax(SyntaxKind kind, SyntaxToken classOrStructKeyword)
		: base(kind)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(classOrStructKeyword);
		this.classOrStructKeyword = classOrStructKeyword;
	}

	internal override GreenNode GetSlot(int index)
	{
		if (index == 0)
		{
			return classOrStructKeyword;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ClassOrStructConstraintSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitClassOrStructConstraint(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitClassOrStructConstraint(this);
	}

	public ClassOrStructConstraintSyntax Update(SyntaxToken classOrStructKeyword)
	{
		if (classOrStructKeyword != ClassOrStructKeyword)
		{
			ClassOrStructConstraintSyntax classOrStructConstraintSyntax = SyntaxFactory.ClassOrStructConstraint(Kind, classOrStructKeyword);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				classOrStructConstraintSyntax = classOrStructConstraintSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				classOrStructConstraintSyntax = classOrStructConstraintSyntax.WithAnnotationsGreen(annotations);
			}
			return classOrStructConstraintSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ClassOrStructConstraintSyntax(Kind, classOrStructKeyword, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ClassOrStructConstraintSyntax(Kind, classOrStructKeyword, GetDiagnostics(), annotations);
	}

	internal ClassOrStructConstraintSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 1;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			classOrStructKeyword = syntaxToken;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(classOrStructKeyword);
	}

	static ClassOrStructConstraintSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ClassOrStructConstraintSyntax), (ObjectReader r) => new ClassOrStructConstraintSyntax(r));
	}
}
