using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class TypeParameterConstraintClauseSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken whereKeyword;

	internal readonly IdentifierNameSyntax name;

	internal readonly SyntaxToken colonToken;

	internal readonly GreenNode constraints;

	public SyntaxToken WhereKeyword => whereKeyword;

	public IdentifierNameSyntax Name => name;

	public SyntaxToken ColonToken => colonToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TypeParameterConstraintSyntax> Constraints => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TypeParameterConstraintSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(constraints));

	internal TypeParameterConstraintClauseSyntax(SyntaxKind kind, SyntaxToken whereKeyword, IdentifierNameSyntax name, SyntaxToken colonToken, GreenNode constraints, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(whereKeyword);
		this.whereKeyword = whereKeyword;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		if (constraints != null)
		{
			AdjustFlagsAndWidth(constraints);
			this.constraints = constraints;
		}
	}

	internal TypeParameterConstraintClauseSyntax(SyntaxKind kind, SyntaxToken whereKeyword, IdentifierNameSyntax name, SyntaxToken colonToken, GreenNode constraints, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 4;
		AdjustFlagsAndWidth(whereKeyword);
		this.whereKeyword = whereKeyword;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		if (constraints != null)
		{
			AdjustFlagsAndWidth(constraints);
			this.constraints = constraints;
		}
	}

	internal TypeParameterConstraintClauseSyntax(SyntaxKind kind, SyntaxToken whereKeyword, IdentifierNameSyntax name, SyntaxToken colonToken, GreenNode constraints)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(whereKeyword);
		this.whereKeyword = whereKeyword;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		if (constraints != null)
		{
			AdjustFlagsAndWidth(constraints);
			this.constraints = constraints;
		}
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => whereKeyword, 
			1 => name, 
			2 => colonToken, 
			3 => constraints, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterConstraintClauseSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTypeParameterConstraintClause(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitTypeParameterConstraintClause(this);
	}

	public TypeParameterConstraintClauseSyntax Update(SyntaxToken whereKeyword, IdentifierNameSyntax name, SyntaxToken colonToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TypeParameterConstraintSyntax> constraints)
	{
		if (whereKeyword != WhereKeyword || name != Name || colonToken != ColonToken || constraints != Constraints)
		{
			TypeParameterConstraintClauseSyntax typeParameterConstraintClauseSyntax = SyntaxFactory.TypeParameterConstraintClause(whereKeyword, name, colonToken, constraints);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				typeParameterConstraintClauseSyntax = typeParameterConstraintClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				typeParameterConstraintClauseSyntax = typeParameterConstraintClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return typeParameterConstraintClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new TypeParameterConstraintClauseSyntax(Kind, whereKeyword, name, colonToken, constraints, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new TypeParameterConstraintClauseSyntax(Kind, whereKeyword, name, colonToken, constraints, GetDiagnostics(), annotations);
	}

	internal TypeParameterConstraintClauseSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 4;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			whereKeyword = syntaxToken;
		}
		IdentifierNameSyntax identifierNameSyntax = (IdentifierNameSyntax)reader.ReadValue();
		if (identifierNameSyntax != null)
		{
			AdjustFlagsAndWidth(identifierNameSyntax);
			name = identifierNameSyntax;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			colonToken = syntaxToken2;
		}
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			constraints = greenNode;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(whereKeyword);
		writer.WriteValue(name);
		writer.WriteValue(colonToken);
		writer.WriteValue(constraints);
	}

	static TypeParameterConstraintClauseSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(TypeParameterConstraintClauseSyntax), (ObjectReader r) => new TypeParameterConstraintClauseSyntax(r));
	}
}
