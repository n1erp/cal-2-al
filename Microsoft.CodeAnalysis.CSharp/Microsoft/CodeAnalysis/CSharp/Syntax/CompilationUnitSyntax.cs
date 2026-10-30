using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class CompilationUnitSyntax : CSharpSyntaxNode, ICompilationUnitSyntax
{
	private SyntaxNode externs;

	private SyntaxNode usings;

	private SyntaxNode attributeLists;

	private SyntaxNode members;

	public SyntaxList<ExternAliasDirectiveSyntax> Externs => new SyntaxList<ExternAliasDirectiveSyntax>(GetRed(ref externs, 0));

	public SyntaxList<UsingDirectiveSyntax> Usings => new SyntaxList<UsingDirectiveSyntax>(GetRed(ref usings, 1));

	public SyntaxList<AttributeListSyntax> AttributeLists => new SyntaxList<AttributeListSyntax>(GetRed(ref attributeLists, 2));

	public SyntaxList<MemberDeclarationSyntax> Members => new SyntaxList<MemberDeclarationSyntax>(GetRed(ref members, 3));

	public SyntaxToken EndOfFileToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CompilationUnitSyntax)Green).endOfFileToken, GetChildPosition(4), GetChildIndex(4));

	internal CompilationUnitSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref externs), 
			1 => GetRed(ref usings, 1), 
			2 => GetRed(ref attributeLists, 2), 
			3 => GetRed(ref members, 3), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => externs, 
			1 => usings, 
			2 => attributeLists, 
			3 => members, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCompilationUnit(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitCompilationUnit(this);
	}

	public CompilationUnitSyntax Update(SyntaxList<ExternAliasDirectiveSyntax> externs, SyntaxList<UsingDirectiveSyntax> usings, SyntaxList<AttributeListSyntax> attributeLists, SyntaxList<MemberDeclarationSyntax> members, SyntaxToken endOfFileToken)
	{
		if (externs != Externs || usings != Usings || attributeLists != AttributeLists || members != Members || endOfFileToken != EndOfFileToken)
		{
			CompilationUnitSyntax compilationUnitSyntax = SyntaxFactory.CompilationUnit(externs, usings, attributeLists, members, endOfFileToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return compilationUnitSyntax.WithAnnotations(annotations);
			}
			return compilationUnitSyntax;
		}
		return this;
	}

	public CompilationUnitSyntax WithExterns(SyntaxList<ExternAliasDirectiveSyntax> externs)
	{
		return Update(externs, Usings, AttributeLists, Members, EndOfFileToken);
	}

	public CompilationUnitSyntax WithUsings(SyntaxList<UsingDirectiveSyntax> usings)
	{
		return Update(Externs, usings, AttributeLists, Members, EndOfFileToken);
	}

	public CompilationUnitSyntax WithAttributeLists(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return Update(Externs, Usings, attributeLists, Members, EndOfFileToken);
	}

	public CompilationUnitSyntax WithMembers(SyntaxList<MemberDeclarationSyntax> members)
	{
		return Update(Externs, Usings, AttributeLists, members, EndOfFileToken);
	}

	public CompilationUnitSyntax WithEndOfFileToken(SyntaxToken endOfFileToken)
	{
		return Update(Externs, Usings, AttributeLists, Members, endOfFileToken);
	}

	public CompilationUnitSyntax AddExterns(params ExternAliasDirectiveSyntax[] items)
	{
		return WithExterns(Externs.AddRange(items));
	}

	public CompilationUnitSyntax AddUsings(params UsingDirectiveSyntax[] items)
	{
		return WithUsings(Usings.AddRange(items));
	}

	public CompilationUnitSyntax AddAttributeLists(params AttributeListSyntax[] items)
	{
		return WithAttributeLists(AttributeLists.AddRange(items));
	}

	public CompilationUnitSyntax AddMembers(params MemberDeclarationSyntax[] items)
	{
		return WithMembers(Members.AddRange(items));
	}

	public IList<ReferenceDirectiveTriviaSyntax> GetReferenceDirectives()
	{
		return GetReferenceDirectives(null);
	}

	internal IList<ReferenceDirectiveTriviaSyntax> GetReferenceDirectives(Func<ReferenceDirectiveTriviaSyntax, bool> filter)
	{
		return ((SyntaxNodeOrToken)GetFirstToken(includeZeroWidth: true)).GetDirectives(filter);
	}

	public IList<LoadDirectiveTriviaSyntax> GetLoadDirectives()
	{
		return ((SyntaxNodeOrToken)GetFirstToken(includeZeroWidth: true)).GetDirectives<LoadDirectiveTriviaSyntax>();
	}

	internal DirectiveStack GetConditionalDirectivesStack()
	{
		IList<DirectiveTriviaSyntax> directives = GetDirectives(IsActiveConditionalDirective);
		DirectiveStack directiveStack = DirectiveStack.Empty;
		foreach (DirectiveTriviaSyntax item in directives)
		{
			directiveStack = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.DirectiveTriviaSyntax)item.Green).ApplyDirectives(directiveStack);
		}
		return directiveStack;
	}

	private static bool IsActiveConditionalDirective(DirectiveTriviaSyntax directive)
	{
		return directive.Kind() switch
		{
			SyntaxKind.DefineDirectiveTrivia => ((DefineDirectiveTriviaSyntax)directive).IsActive, 
			SyntaxKind.UndefDirectiveTrivia => ((UndefDirectiveTriviaSyntax)directive).IsActive, 
			_ => false, 
		};
	}
}
