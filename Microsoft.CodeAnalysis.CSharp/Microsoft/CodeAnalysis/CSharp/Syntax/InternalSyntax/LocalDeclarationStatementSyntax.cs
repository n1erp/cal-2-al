using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class LocalDeclarationStatementSyntax : StatementSyntax
{
	internal readonly GreenNode modifiers;

	internal readonly VariableDeclarationSyntax declaration;

	internal readonly SyntaxToken semicolonToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Modifiers => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(modifiers);

	public VariableDeclarationSyntax Declaration => declaration;

	public SyntaxToken SemicolonToken => semicolonToken;

	internal LocalDeclarationStatementSyntax(SyntaxKind kind, GreenNode modifiers, VariableDeclarationSyntax declaration, SyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
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

	internal LocalDeclarationStatementSyntax(SyntaxKind kind, GreenNode modifiers, VariableDeclarationSyntax declaration, SyntaxToken semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
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

	internal LocalDeclarationStatementSyntax(SyntaxKind kind, GreenNode modifiers, VariableDeclarationSyntax declaration, SyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 3;
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
			0 => modifiers, 
			1 => declaration, 
			2 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLocalDeclarationStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitLocalDeclarationStatement(this);
	}

	public LocalDeclarationStatementSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, VariableDeclarationSyntax declaration, SyntaxToken semicolonToken)
	{
		if (modifiers != Modifiers || declaration != Declaration || semicolonToken != SemicolonToken)
		{
			LocalDeclarationStatementSyntax localDeclarationStatementSyntax = SyntaxFactory.LocalDeclarationStatement(modifiers, declaration, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				localDeclarationStatementSyntax = localDeclarationStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				localDeclarationStatementSyntax = localDeclarationStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return localDeclarationStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new LocalDeclarationStatementSyntax(Kind, modifiers, declaration, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new LocalDeclarationStatementSyntax(Kind, modifiers, declaration, semicolonToken, GetDiagnostics(), annotations);
	}

	internal LocalDeclarationStatementSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			modifiers = greenNode;
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
		writer.WriteValue(modifiers);
		writer.WriteValue(declaration);
		writer.WriteValue(semicolonToken);
	}

	static LocalDeclarationStatementSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(LocalDeclarationStatementSyntax), (ObjectReader r) => new LocalDeclarationStatementSyntax(r));
	}
}
