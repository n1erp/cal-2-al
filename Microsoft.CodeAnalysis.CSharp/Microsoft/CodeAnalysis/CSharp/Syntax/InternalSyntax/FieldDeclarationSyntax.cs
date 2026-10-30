using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class FieldDeclarationSyntax : BaseFieldDeclarationSyntax
{
	internal readonly GreenNode attributeLists;

	internal readonly GreenNode modifiers;

	internal readonly VariableDeclarationSyntax declaration;

	internal readonly SyntaxToken semicolonToken;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Modifiers => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(modifiers);

	public override VariableDeclarationSyntax Declaration => declaration;

	public override SyntaxToken SemicolonToken => semicolonToken;

	internal FieldDeclarationSyntax(SyntaxKind kind, GreenNode attributeLists, GreenNode modifiers, VariableDeclarationSyntax declaration, SyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (modifiers != null)
		{
			AdjustFlagsAndWidth(modifiers);
			this.modifiers = modifiers;
		}
		AdjustFlagsAndWidth(declaration);
		this.declaration = declaration;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal FieldDeclarationSyntax(SyntaxKind kind, GreenNode attributeLists, GreenNode modifiers, VariableDeclarationSyntax declaration, SyntaxToken semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 4;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (modifiers != null)
		{
			AdjustFlagsAndWidth(modifiers);
			this.modifiers = modifiers;
		}
		AdjustFlagsAndWidth(declaration);
		this.declaration = declaration;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal FieldDeclarationSyntax(SyntaxKind kind, GreenNode attributeLists, GreenNode modifiers, VariableDeclarationSyntax declaration, SyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 4;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (modifiers != null)
		{
			AdjustFlagsAndWidth(modifiers);
			this.modifiers = modifiers;
		}
		AdjustFlagsAndWidth(declaration);
		this.declaration = declaration;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			1 => modifiers, 
			2 => declaration, 
			3 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFieldDeclaration(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitFieldDeclaration(this);
	}

	public FieldDeclarationSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, VariableDeclarationSyntax declaration, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || declaration != Declaration || semicolonToken != SemicolonToken)
		{
			FieldDeclarationSyntax fieldDeclarationSyntax = SyntaxFactory.FieldDeclaration(attributeLists, modifiers, declaration, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				fieldDeclarationSyntax = fieldDeclarationSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				fieldDeclarationSyntax = fieldDeclarationSyntax.WithAnnotationsGreen(annotations);
			}
			return fieldDeclarationSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new FieldDeclarationSyntax(Kind, attributeLists, modifiers, declaration, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new FieldDeclarationSyntax(Kind, attributeLists, modifiers, declaration, semicolonToken, GetDiagnostics(), annotations);
	}

	internal FieldDeclarationSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 4;
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			attributeLists = greenNode;
		}
		GreenNode greenNode2 = (GreenNode)reader.ReadValue();
		if (greenNode2 != null)
		{
			AdjustFlagsAndWidth(greenNode2);
			modifiers = greenNode2;
		}
		VariableDeclarationSyntax variableDeclarationSyntax = (VariableDeclarationSyntax)reader.ReadValue();
		if (variableDeclarationSyntax != null)
		{
			AdjustFlagsAndWidth(variableDeclarationSyntax);
			declaration = variableDeclarationSyntax;
		}
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			semicolonToken = syntaxToken;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(attributeLists);
		writer.WriteValue(modifiers);
		writer.WriteValue(declaration);
		writer.WriteValue(semicolonToken);
	}

	static FieldDeclarationSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(FieldDeclarationSyntax), (ObjectReader r) => new FieldDeclarationSyntax(r));
	}
}
