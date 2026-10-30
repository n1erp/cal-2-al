using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class PropertyDeclarationSyntax : BasePropertyDeclarationSyntax
{
	internal readonly GreenNode attributeLists;

	internal readonly GreenNode modifiers;

	internal readonly TypeSyntax type;

	internal readonly ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier;

	internal readonly SyntaxToken identifier;

	internal readonly AccessorListSyntax accessorList;

	internal readonly ArrowExpressionClauseSyntax expressionBody;

	internal readonly EqualsValueClauseSyntax initializer;

	internal readonly SyntaxToken semicolonToken;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Modifiers => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(modifiers);

	public override TypeSyntax Type => type;

	public override ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier => explicitInterfaceSpecifier;

	public SyntaxToken Identifier => identifier;

	public override AccessorListSyntax AccessorList => accessorList;

	public ArrowExpressionClauseSyntax ExpressionBody => expressionBody;

	public EqualsValueClauseSyntax Initializer => initializer;

	public SyntaxToken SemicolonToken => semicolonToken;

	internal PropertyDeclarationSyntax(SyntaxKind kind, GreenNode attributeLists, GreenNode modifiers, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken identifier, AccessorListSyntax accessorList, ArrowExpressionClauseSyntax expressionBody, EqualsValueClauseSyntax initializer, SyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 9;
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
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (explicitInterfaceSpecifier != null)
		{
			AdjustFlagsAndWidth(explicitInterfaceSpecifier);
			this.explicitInterfaceSpecifier = explicitInterfaceSpecifier;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		if (accessorList != null)
		{
			AdjustFlagsAndWidth(accessorList);
			this.accessorList = accessorList;
		}
		if (expressionBody != null)
		{
			AdjustFlagsAndWidth(expressionBody);
			this.expressionBody = expressionBody;
		}
		if (initializer != null)
		{
			AdjustFlagsAndWidth(initializer);
			this.initializer = initializer;
		}
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			this.semicolonToken = semicolonToken;
		}
	}

	internal PropertyDeclarationSyntax(SyntaxKind kind, GreenNode attributeLists, GreenNode modifiers, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken identifier, AccessorListSyntax accessorList, ArrowExpressionClauseSyntax expressionBody, EqualsValueClauseSyntax initializer, SyntaxToken semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 9;
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
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (explicitInterfaceSpecifier != null)
		{
			AdjustFlagsAndWidth(explicitInterfaceSpecifier);
			this.explicitInterfaceSpecifier = explicitInterfaceSpecifier;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		if (accessorList != null)
		{
			AdjustFlagsAndWidth(accessorList);
			this.accessorList = accessorList;
		}
		if (expressionBody != null)
		{
			AdjustFlagsAndWidth(expressionBody);
			this.expressionBody = expressionBody;
		}
		if (initializer != null)
		{
			AdjustFlagsAndWidth(initializer);
			this.initializer = initializer;
		}
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			this.semicolonToken = semicolonToken;
		}
	}

	internal PropertyDeclarationSyntax(SyntaxKind kind, GreenNode attributeLists, GreenNode modifiers, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken identifier, AccessorListSyntax accessorList, ArrowExpressionClauseSyntax expressionBody, EqualsValueClauseSyntax initializer, SyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 9;
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
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (explicitInterfaceSpecifier != null)
		{
			AdjustFlagsAndWidth(explicitInterfaceSpecifier);
			this.explicitInterfaceSpecifier = explicitInterfaceSpecifier;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		if (accessorList != null)
		{
			AdjustFlagsAndWidth(accessorList);
			this.accessorList = accessorList;
		}
		if (expressionBody != null)
		{
			AdjustFlagsAndWidth(expressionBody);
			this.expressionBody = expressionBody;
		}
		if (initializer != null)
		{
			AdjustFlagsAndWidth(initializer);
			this.initializer = initializer;
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
			2 => type, 
			3 => explicitInterfaceSpecifier, 
			4 => identifier, 
			5 => accessorList, 
			6 => expressionBody, 
			7 => initializer, 
			8 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPropertyDeclaration(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitPropertyDeclaration(this);
	}

	public PropertyDeclarationSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken identifier, AccessorListSyntax accessorList, ArrowExpressionClauseSyntax expressionBody, EqualsValueClauseSyntax initializer, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || type != Type || explicitInterfaceSpecifier != ExplicitInterfaceSpecifier || identifier != Identifier || accessorList != AccessorList || expressionBody != ExpressionBody || initializer != Initializer || semicolonToken != SemicolonToken)
		{
			PropertyDeclarationSyntax propertyDeclarationSyntax = SyntaxFactory.PropertyDeclaration(attributeLists, modifiers, type, explicitInterfaceSpecifier, identifier, accessorList, expressionBody, initializer, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				propertyDeclarationSyntax = propertyDeclarationSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				propertyDeclarationSyntax = propertyDeclarationSyntax.WithAnnotationsGreen(annotations);
			}
			return propertyDeclarationSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new PropertyDeclarationSyntax(Kind, attributeLists, modifiers, type, explicitInterfaceSpecifier, identifier, accessorList, expressionBody, initializer, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new PropertyDeclarationSyntax(Kind, attributeLists, modifiers, type, explicitInterfaceSpecifier, identifier, accessorList, expressionBody, initializer, semicolonToken, GetDiagnostics(), annotations);
	}

	internal PropertyDeclarationSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 9;
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
		TypeSyntax typeSyntax = (TypeSyntax)reader.ReadValue();
		if (typeSyntax != null)
		{
			AdjustFlagsAndWidth(typeSyntax);
			type = typeSyntax;
		}
		ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifierSyntax = (ExplicitInterfaceSpecifierSyntax)reader.ReadValue();
		if (explicitInterfaceSpecifierSyntax != null)
		{
			AdjustFlagsAndWidth(explicitInterfaceSpecifierSyntax);
			explicitInterfaceSpecifier = explicitInterfaceSpecifierSyntax;
		}
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			identifier = syntaxToken;
		}
		AccessorListSyntax accessorListSyntax = (AccessorListSyntax)reader.ReadValue();
		if (accessorListSyntax != null)
		{
			AdjustFlagsAndWidth(accessorListSyntax);
			accessorList = accessorListSyntax;
		}
		ArrowExpressionClauseSyntax arrowExpressionClauseSyntax = (ArrowExpressionClauseSyntax)reader.ReadValue();
		if (arrowExpressionClauseSyntax != null)
		{
			AdjustFlagsAndWidth(arrowExpressionClauseSyntax);
			expressionBody = arrowExpressionClauseSyntax;
		}
		EqualsValueClauseSyntax equalsValueClauseSyntax = (EqualsValueClauseSyntax)reader.ReadValue();
		if (equalsValueClauseSyntax != null)
		{
			AdjustFlagsAndWidth(equalsValueClauseSyntax);
			initializer = equalsValueClauseSyntax;
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
		writer.WriteValue(type);
		writer.WriteValue(explicitInterfaceSpecifier);
		writer.WriteValue(identifier);
		writer.WriteValue(accessorList);
		writer.WriteValue(expressionBody);
		writer.WriteValue(initializer);
		writer.WriteValue(semicolonToken);
	}

	static PropertyDeclarationSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(PropertyDeclarationSyntax), (ObjectReader r) => new PropertyDeclarationSyntax(r));
	}
}
