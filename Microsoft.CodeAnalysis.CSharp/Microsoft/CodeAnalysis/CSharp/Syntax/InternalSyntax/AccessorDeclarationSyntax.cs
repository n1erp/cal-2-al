using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class AccessorDeclarationSyntax : CSharpSyntaxNode
{
	internal readonly GreenNode attributeLists;

	internal readonly GreenNode modifiers;

	internal readonly SyntaxToken keyword;

	internal readonly BlockSyntax body;

	internal readonly ArrowExpressionClauseSyntax expressionBody;

	internal readonly SyntaxToken semicolonToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Modifiers => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(modifiers);

	public SyntaxToken Keyword => keyword;

	public BlockSyntax Body => body;

	public ArrowExpressionClauseSyntax ExpressionBody => expressionBody;

	public SyntaxToken SemicolonToken => semicolonToken;

	internal AccessorDeclarationSyntax(SyntaxKind kind, GreenNode attributeLists, GreenNode modifiers, SyntaxToken keyword, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 6;
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
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		if (body != null)
		{
			AdjustFlagsAndWidth(body);
			this.body = body;
		}
		if (expressionBody != null)
		{
			AdjustFlagsAndWidth(expressionBody);
			this.expressionBody = expressionBody;
		}
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			this.semicolonToken = semicolonToken;
		}
	}

	internal AccessorDeclarationSyntax(SyntaxKind kind, GreenNode attributeLists, GreenNode modifiers, SyntaxToken keyword, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 6;
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
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		if (body != null)
		{
			AdjustFlagsAndWidth(body);
			this.body = body;
		}
		if (expressionBody != null)
		{
			AdjustFlagsAndWidth(expressionBody);
			this.expressionBody = expressionBody;
		}
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			this.semicolonToken = semicolonToken;
		}
	}

	internal AccessorDeclarationSyntax(SyntaxKind kind, GreenNode attributeLists, GreenNode modifiers, SyntaxToken keyword, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 6;
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
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		if (body != null)
		{
			AdjustFlagsAndWidth(body);
			this.body = body;
		}
		if (expressionBody != null)
		{
			AdjustFlagsAndWidth(expressionBody);
			this.expressionBody = expressionBody;
		}
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			this.semicolonToken = semicolonToken;
		}
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			1 => modifiers, 
			2 => keyword, 
			3 => body, 
			4 => expressionBody, 
			5 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitAccessorDeclaration(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitAccessorDeclaration(this);
	}

	public AccessorDeclarationSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken keyword, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || keyword != Keyword || body != Body || expressionBody != ExpressionBody || semicolonToken != SemicolonToken)
		{
			AccessorDeclarationSyntax accessorDeclarationSyntax = SyntaxFactory.AccessorDeclaration(Kind, attributeLists, modifiers, keyword, body, expressionBody, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				accessorDeclarationSyntax = accessorDeclarationSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				accessorDeclarationSyntax = accessorDeclarationSyntax.WithAnnotationsGreen(annotations);
			}
			return accessorDeclarationSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new AccessorDeclarationSyntax(Kind, attributeLists, modifiers, keyword, body, expressionBody, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new AccessorDeclarationSyntax(Kind, attributeLists, modifiers, keyword, body, expressionBody, semicolonToken, GetDiagnostics(), annotations);
	}

	internal AccessorDeclarationSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 6;
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
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			keyword = syntaxToken;
		}
		BlockSyntax blockSyntax = (BlockSyntax)reader.ReadValue();
		if (blockSyntax != null)
		{
			AdjustFlagsAndWidth(blockSyntax);
			body = blockSyntax;
		}
		ArrowExpressionClauseSyntax arrowExpressionClauseSyntax = (ArrowExpressionClauseSyntax)reader.ReadValue();
		if (arrowExpressionClauseSyntax != null)
		{
			AdjustFlagsAndWidth(arrowExpressionClauseSyntax);
			expressionBody = arrowExpressionClauseSyntax;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			semicolonToken = syntaxToken2;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(attributeLists);
		writer.WriteValue(modifiers);
		writer.WriteValue(keyword);
		writer.WriteValue(body);
		writer.WriteValue(expressionBody);
		writer.WriteValue(semicolonToken);
	}

	static AccessorDeclarationSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(AccessorDeclarationSyntax), (ObjectReader r) => new AccessorDeclarationSyntax(r));
	}
}
