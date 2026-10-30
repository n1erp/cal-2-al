using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ConstructorConstraintSyntax : TypeParameterConstraintSyntax
{
	internal readonly SyntaxToken newKeyword;

	internal readonly SyntaxToken openParenToken;

	internal readonly SyntaxToken closeParenToken;

	public SyntaxToken NewKeyword => newKeyword;

	public SyntaxToken OpenParenToken => openParenToken;

	public SyntaxToken CloseParenToken => closeParenToken;

	internal ConstructorConstraintSyntax(SyntaxKind kind, SyntaxToken newKeyword, SyntaxToken openParenToken, SyntaxToken closeParenToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(newKeyword);
		this.newKeyword = newKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal ConstructorConstraintSyntax(SyntaxKind kind, SyntaxToken newKeyword, SyntaxToken openParenToken, SyntaxToken closeParenToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(newKeyword);
		this.newKeyword = newKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal ConstructorConstraintSyntax(SyntaxKind kind, SyntaxToken newKeyword, SyntaxToken openParenToken, SyntaxToken closeParenToken)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(newKeyword);
		this.newKeyword = newKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)newKeyword, 
			1 => openParenToken, 
			2 => closeParenToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorConstraintSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitConstructorConstraint(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitConstructorConstraint(this);
	}

	public ConstructorConstraintSyntax Update(SyntaxToken newKeyword, SyntaxToken openParenToken, SyntaxToken closeParenToken)
	{
		if (newKeyword != NewKeyword || openParenToken != OpenParenToken || closeParenToken != CloseParenToken)
		{
			ConstructorConstraintSyntax constructorConstraintSyntax = SyntaxFactory.ConstructorConstraint(newKeyword, openParenToken, closeParenToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				constructorConstraintSyntax = constructorConstraintSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				constructorConstraintSyntax = constructorConstraintSyntax.WithAnnotationsGreen(annotations);
			}
			return constructorConstraintSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ConstructorConstraintSyntax(Kind, newKeyword, openParenToken, closeParenToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ConstructorConstraintSyntax(Kind, newKeyword, openParenToken, closeParenToken, GetDiagnostics(), annotations);
	}

	internal ConstructorConstraintSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			newKeyword = syntaxToken;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			openParenToken = syntaxToken2;
		}
		SyntaxToken syntaxToken3 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken3 != null)
		{
			AdjustFlagsAndWidth(syntaxToken3);
			closeParenToken = syntaxToken3;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(newKeyword);
		writer.WriteValue(openParenToken);
		writer.WriteValue(closeParenToken);
	}

	static ConstructorConstraintSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ConstructorConstraintSyntax), (ObjectReader r) => new ConstructorConstraintSyntax(r));
	}
}
