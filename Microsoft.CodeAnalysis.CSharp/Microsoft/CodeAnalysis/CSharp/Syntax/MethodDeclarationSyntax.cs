using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class MethodDeclarationSyntax : BaseMethodDeclarationSyntax
{
	private SyntaxNode attributeLists;

	private TypeSyntax returnType;

	private ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier;

	private TypeParameterListSyntax typeParameterList;

	private ParameterListSyntax parameterList;

	private SyntaxNode constraintClauses;

	private BlockSyntax body;

	private ArrowExpressionClauseSyntax expressionBody;

	public override SyntaxList<AttributeListSyntax> AttributeLists => new SyntaxList<AttributeListSyntax>(GetRed(ref attributeLists, 0));

	public override SyntaxTokenList Modifiers
	{
		get
		{
			GreenNode slot = Green.GetSlot(1);
			if (slot != null)
			{
				return new SyntaxTokenList(this, slot, GetChildPosition(1), GetChildIndex(1));
			}
			return default;
		}
	}

	public TypeSyntax ReturnType => GetRed(ref returnType, 2);

	public ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier => GetRed(ref explicitInterfaceSpecifier, 3);

	public SyntaxToken Identifier => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.MethodDeclarationSyntax)Green).identifier, GetChildPosition(4), GetChildIndex(4));

	public TypeParameterListSyntax TypeParameterList => GetRed(ref typeParameterList, 5);

	public override ParameterListSyntax ParameterList => GetRed(ref parameterList, 6);

	public SyntaxList<TypeParameterConstraintClauseSyntax> ConstraintClauses => new SyntaxList<TypeParameterConstraintClauseSyntax>(GetRed(ref constraintClauses, 7));

	public override BlockSyntax Body => GetRed(ref body, 8);

	public override ArrowExpressionClauseSyntax ExpressionBody => GetRed(ref expressionBody, 9);

	public override SyntaxToken SemicolonToken
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken semicolonToken = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.MethodDeclarationSyntax)Green).semicolonToken;
			if (semicolonToken != null)
			{
				return new SyntaxToken(this, semicolonToken, GetChildPosition(10), GetChildIndex(10));
			}
			return default;
		}
	}

	public int Arity
	{
		get
		{
			if (TypeParameterList != null)
			{
				return TypeParameterList.Parameters.Count;
			}
			return 0;
		}
	}

	internal MethodDeclarationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref attributeLists), 
			2 => GetRed(ref returnType, 2), 
			3 => GetRed(ref explicitInterfaceSpecifier, 3), 
			5 => GetRed(ref typeParameterList, 5), 
			6 => GetRed(ref parameterList, 6), 
			7 => GetRed(ref constraintClauses, 7), 
			8 => GetRed(ref body, 8), 
			9 => GetRed(ref expressionBody, 9), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			2 => returnType, 
			3 => explicitInterfaceSpecifier, 
			5 => typeParameterList, 
			6 => parameterList, 
			7 => constraintClauses, 
			8 => body, 
			9 => expressionBody, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitMethodDeclaration(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitMethodDeclaration(this);
	}

	public MethodDeclarationSyntax Update(SyntaxList<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, TypeSyntax returnType, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken identifier, TypeParameterListSyntax typeParameterList, ParameterListSyntax parameterList, SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || returnType != ReturnType || explicitInterfaceSpecifier != ExplicitInterfaceSpecifier || identifier != Identifier || typeParameterList != TypeParameterList || parameterList != ParameterList || constraintClauses != ConstraintClauses || body != Body || expressionBody != ExpressionBody || semicolonToken != SemicolonToken)
		{
			MethodDeclarationSyntax methodDeclarationSyntax = SyntaxFactory.MethodDeclaration(attributeLists, modifiers, returnType, explicitInterfaceSpecifier, identifier, typeParameterList, parameterList, constraintClauses, body, expressionBody, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return methodDeclarationSyntax.WithAnnotations(annotations);
			}
			return methodDeclarationSyntax;
		}
		return this;
	}

	internal override BaseMethodDeclarationSyntax WithAttributeListsCore(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return WithAttributeLists(attributeLists);
	}

	public new MethodDeclarationSyntax WithAttributeLists(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return Update(attributeLists, Modifiers, ReturnType, ExplicitInterfaceSpecifier, Identifier, TypeParameterList, ParameterList, ConstraintClauses, Body, ExpressionBody, SemicolonToken);
	}

	internal override BaseMethodDeclarationSyntax WithModifiersCore(SyntaxTokenList modifiers)
	{
		return WithModifiers(modifiers);
	}

	public new MethodDeclarationSyntax WithModifiers(SyntaxTokenList modifiers)
	{
		return Update(AttributeLists, modifiers, ReturnType, ExplicitInterfaceSpecifier, Identifier, TypeParameterList, ParameterList, ConstraintClauses, Body, ExpressionBody, SemicolonToken);
	}

	public MethodDeclarationSyntax WithReturnType(TypeSyntax returnType)
	{
		return Update(AttributeLists, Modifiers, returnType, ExplicitInterfaceSpecifier, Identifier, TypeParameterList, ParameterList, ConstraintClauses, Body, ExpressionBody, SemicolonToken);
	}

	public MethodDeclarationSyntax WithExplicitInterfaceSpecifier(ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier)
	{
		return Update(AttributeLists, Modifiers, ReturnType, explicitInterfaceSpecifier, Identifier, TypeParameterList, ParameterList, ConstraintClauses, Body, ExpressionBody, SemicolonToken);
	}

	public MethodDeclarationSyntax WithIdentifier(SyntaxToken identifier)
	{
		return Update(AttributeLists, Modifiers, ReturnType, ExplicitInterfaceSpecifier, identifier, TypeParameterList, ParameterList, ConstraintClauses, Body, ExpressionBody, SemicolonToken);
	}

	public MethodDeclarationSyntax WithTypeParameterList(TypeParameterListSyntax typeParameterList)
	{
		return Update(AttributeLists, Modifiers, ReturnType, ExplicitInterfaceSpecifier, Identifier, typeParameterList, ParameterList, ConstraintClauses, Body, ExpressionBody, SemicolonToken);
	}

	internal override BaseMethodDeclarationSyntax WithParameterListCore(ParameterListSyntax parameterList)
	{
		return WithParameterList(parameterList);
	}

	public new MethodDeclarationSyntax WithParameterList(ParameterListSyntax parameterList)
	{
		return Update(AttributeLists, Modifiers, ReturnType, ExplicitInterfaceSpecifier, Identifier, TypeParameterList, parameterList, ConstraintClauses, Body, ExpressionBody, SemicolonToken);
	}

	public MethodDeclarationSyntax WithConstraintClauses(SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses)
	{
		return Update(AttributeLists, Modifiers, ReturnType, ExplicitInterfaceSpecifier, Identifier, TypeParameterList, ParameterList, constraintClauses, Body, ExpressionBody, SemicolonToken);
	}

	internal override BaseMethodDeclarationSyntax WithBodyCore(BlockSyntax body)
	{
		return WithBody(body);
	}

	public new MethodDeclarationSyntax WithBody(BlockSyntax body)
	{
		return Update(AttributeLists, Modifiers, ReturnType, ExplicitInterfaceSpecifier, Identifier, TypeParameterList, ParameterList, ConstraintClauses, body, ExpressionBody, SemicolonToken);
	}

	internal override BaseMethodDeclarationSyntax WithExpressionBodyCore(ArrowExpressionClauseSyntax expressionBody)
	{
		return WithExpressionBody(expressionBody);
	}

	public new MethodDeclarationSyntax WithExpressionBody(ArrowExpressionClauseSyntax expressionBody)
	{
		return Update(AttributeLists, Modifiers, ReturnType, ExplicitInterfaceSpecifier, Identifier, TypeParameterList, ParameterList, ConstraintClauses, Body, expressionBody, SemicolonToken);
	}

	internal override BaseMethodDeclarationSyntax WithSemicolonTokenCore(SyntaxToken semicolonToken)
	{
		return WithSemicolonToken(semicolonToken);
	}

	public new MethodDeclarationSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(AttributeLists, Modifiers, ReturnType, ExplicitInterfaceSpecifier, Identifier, TypeParameterList, ParameterList, ConstraintClauses, Body, ExpressionBody, semicolonToken);
	}

	internal override BaseMethodDeclarationSyntax AddAttributeListsCore(params AttributeListSyntax[] items)
	{
		return AddAttributeLists(items);
	}

	public new MethodDeclarationSyntax AddAttributeLists(params AttributeListSyntax[] items)
	{
		return WithAttributeLists(AttributeLists.AddRange(items));
	}

	internal override BaseMethodDeclarationSyntax AddModifiersCore(params SyntaxToken[] items)
	{
		return AddModifiers(items);
	}

	public new MethodDeclarationSyntax AddModifiers(params SyntaxToken[] items)
	{
		return WithModifiers(Modifiers.AddRange(items));
	}

	public MethodDeclarationSyntax AddTypeParameterListParameters(params TypeParameterSyntax[] items)
	{
		TypeParameterListSyntax typeParameterListSyntax = TypeParameterList ?? SyntaxFactory.TypeParameterList();
		return WithTypeParameterList(typeParameterListSyntax.WithParameters(typeParameterListSyntax.Parameters.AddRange(items)));
	}

	internal override BaseMethodDeclarationSyntax AddParameterListParametersCore(params ParameterSyntax[] items)
	{
		return AddParameterListParameters(items);
	}

	public new MethodDeclarationSyntax AddParameterListParameters(params ParameterSyntax[] items)
	{
		return WithParameterList(ParameterList.WithParameters(ParameterList.Parameters.AddRange(items)));
	}

	public MethodDeclarationSyntax AddConstraintClauses(params TypeParameterConstraintClauseSyntax[] items)
	{
		return WithConstraintClauses(ConstraintClauses.AddRange(items));
	}

	internal override BaseMethodDeclarationSyntax AddBodyStatementsCore(params StatementSyntax[] items)
	{
		return AddBodyStatements(items);
	}

	public new MethodDeclarationSyntax AddBodyStatements(params StatementSyntax[] items)
	{
		BlockSyntax blockSyntax = Body ?? SyntaxFactory.Block();
		return WithBody(blockSyntax.WithStatements(blockSyntax.Statements.AddRange(items)));
	}
}
