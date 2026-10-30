using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class DestructorDeclarationSyntax : BaseMethodDeclarationSyntax
{
	internal readonly GreenNode attributeLists;

	internal readonly GreenNode modifiers;

	internal readonly SyntaxToken tildeToken;

	internal readonly SyntaxToken identifier;

	internal readonly ParameterListSyntax parameterList;

	internal readonly BlockSyntax body;

	internal readonly ArrowExpressionClauseSyntax expressionBody;

	internal readonly SyntaxToken semicolonToken;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Modifiers => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(modifiers);

	public SyntaxToken TildeToken => tildeToken;

	public SyntaxToken Identifier => identifier;

	public override ParameterListSyntax ParameterList => parameterList;

	public override BlockSyntax Body => body;

	public override ArrowExpressionClauseSyntax ExpressionBody => expressionBody;

	public override SyntaxToken SemicolonToken => semicolonToken;

	internal DestructorDeclarationSyntax(SyntaxKind kind, GreenNode attributeLists, GreenNode modifiers, SyntaxToken tildeToken, SyntaxToken identifier, ParameterListSyntax parameterList, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 8;
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
		AdjustFlagsAndWidth(tildeToken);
		this.tildeToken = tildeToken;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(parameterList);
		this.parameterList = parameterList;
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

	internal DestructorDeclarationSyntax(SyntaxKind kind, GreenNode attributeLists, GreenNode modifiers, SyntaxToken tildeToken, SyntaxToken identifier, ParameterListSyntax parameterList, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 8;
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
		AdjustFlagsAndWidth(tildeToken);
		this.tildeToken = tildeToken;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(parameterList);
		this.parameterList = parameterList;
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

	internal DestructorDeclarationSyntax(SyntaxKind kind, GreenNode attributeLists, GreenNode modifiers, SyntaxToken tildeToken, SyntaxToken identifier, ParameterListSyntax parameterList, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 8;
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
		AdjustFlagsAndWidth(tildeToken);
		this.tildeToken = tildeToken;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(parameterList);
		this.parameterList = parameterList;
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
			2 => tildeToken, 
			3 => identifier, 
			4 => parameterList, 
			5 => body, 
			6 => expressionBody, 
			7 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.DestructorDeclarationSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDestructorDeclaration(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitDestructorDeclaration(this);
	}

	public DestructorDeclarationSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken tildeToken, SyntaxToken identifier, ParameterListSyntax parameterList, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || tildeToken != TildeToken || identifier != Identifier || parameterList != ParameterList || body != Body || expressionBody != ExpressionBody || semicolonToken != SemicolonToken)
		{
			DestructorDeclarationSyntax destructorDeclarationSyntax = SyntaxFactory.DestructorDeclaration(attributeLists, modifiers, tildeToken, identifier, parameterList, body, expressionBody, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				destructorDeclarationSyntax = destructorDeclarationSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				destructorDeclarationSyntax = destructorDeclarationSyntax.WithAnnotationsGreen(annotations);
			}
			return destructorDeclarationSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new DestructorDeclarationSyntax(Kind, attributeLists, modifiers, tildeToken, identifier, parameterList, body, expressionBody, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new DestructorDeclarationSyntax(Kind, attributeLists, modifiers, tildeToken, identifier, parameterList, body, expressionBody, semicolonToken, GetDiagnostics(), annotations);
	}

	internal DestructorDeclarationSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 8;
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
			tildeToken = syntaxToken;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			identifier = syntaxToken2;
		}
		ParameterListSyntax parameterListSyntax = (ParameterListSyntax)reader.ReadValue();
		if (parameterListSyntax != null)
		{
			AdjustFlagsAndWidth(parameterListSyntax);
			parameterList = parameterListSyntax;
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
		SyntaxToken syntaxToken3 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken3 != null)
		{
			AdjustFlagsAndWidth(syntaxToken3);
			semicolonToken = syntaxToken3;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(attributeLists);
		writer.WriteValue(modifiers);
		writer.WriteValue(tildeToken);
		writer.WriteValue(identifier);
		writer.WriteValue(parameterList);
		writer.WriteValue(body);
		writer.WriteValue(expressionBody);
		writer.WriteValue(semicolonToken);
	}

	static DestructorDeclarationSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(DestructorDeclarationSyntax), (ObjectReader r) => new DestructorDeclarationSyntax(r));
	}
}
