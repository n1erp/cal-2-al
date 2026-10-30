using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class MethodDeclarationSyntax : MethodOrTriggerDeclarationSyntax
{
	private SyntaxNode _attributes;

	private IdentifierNameSyntax _name;

	private ParameterListSyntax _parameterList;

	private ReturnValueSyntax _returnValue;

	private VarSectionSyntax _variables;

	private BlockSyntax _body;

	public override SyntaxList<MemberAttributeSyntax> Attributes => new SyntaxList<MemberAttributeSyntax>(GetRed(ref _attributes, 0));

	public SyntaxToken LocalKeyword
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalMethodDeclarationSyntax)Node)._localKeyword;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(1), GetChildIndex(1));
			}
			return default;
		}
	}

	public SyntaxToken ProcedureKeyword => new SyntaxToken(this, ((InternalMethodDeclarationSyntax)Node)._procedureKeyword, GetChildPosition(2), GetChildIndex(2));

	public override IdentifierNameSyntax Name => GetRed(ref _name, 3);

	public override ParameterListSyntax ParameterList => GetRed(ref _parameterList, 4);

	public override ReturnValueSyntax ReturnValue => GetRed(ref _returnValue, 5);

	public override SyntaxToken SemicolonToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalMethodDeclarationSyntax)Node)._semicolonToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(6), GetChildIndex(6));
			}
			return default;
		}
	}

	public override VarSectionSyntax Variables => GetRed(ref _variables, 7);

	public override BlockSyntax Body => GetRed(ref _body, 8);

	internal MethodDeclarationSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref _attributes), 
			3 => GetRed(ref _name, 3), 
			4 => GetRed(ref _parameterList, 4), 
			5 => GetRed(ref _returnValue, 5), 
			7 => GetRed(ref _variables, 7), 
			8 => GetRed(ref _body, 8), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => _attributes, 
			3 => _name, 
			4 => _parameterList, 
			5 => _returnValue, 
			7 => _variables, 
			8 => _body, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitMethodDeclaration(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitMethodDeclaration(this);
	}

	public MethodDeclarationSyntax Update(SyntaxList<MemberAttributeSyntax> attributes, SyntaxToken localKeyword, SyntaxToken procedureKeyword, IdentifierNameSyntax name, ParameterListSyntax parameterList, ReturnValueSyntax returnValue, SyntaxToken semicolonToken, VarSectionSyntax variables, BlockSyntax body)
	{
		if (attributes != Attributes || localKeyword != LocalKeyword || procedureKeyword != ProcedureKeyword || name != Name || parameterList != ParameterList || returnValue != ReturnValue || semicolonToken != SemicolonToken || variables != Variables || body != Body)
		{
			MethodDeclarationSyntax methodDeclarationSyntax = SyntaxFactory.MethodDeclaration(attributes, localKeyword, procedureKeyword, name, parameterList, returnValue, semicolonToken, variables, body);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return methodDeclarationSyntax.WithAnnotations(annotations);
			}
			return methodDeclarationSyntax;
		}
		return this;
	}

	public MethodDeclarationSyntax WithAttributes(SyntaxList<MemberAttributeSyntax> attributes)
	{
		return Update(attributes, LocalKeyword, ProcedureKeyword, Name, ParameterList, ReturnValue, SemicolonToken, Variables, Body);
	}

	public MethodDeclarationSyntax WithLocalKeyword(SyntaxToken localKeyword)
	{
		return Update(Attributes, localKeyword, ProcedureKeyword, Name, ParameterList, ReturnValue, SemicolonToken, Variables, Body);
	}

	public MethodDeclarationSyntax WithProcedureKeyword(SyntaxToken procedureKeyword)
	{
		return Update(Attributes, LocalKeyword, procedureKeyword, Name, ParameterList, ReturnValue, SemicolonToken, Variables, Body);
	}

	public MethodDeclarationSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(Attributes, LocalKeyword, ProcedureKeyword, name, ParameterList, ReturnValue, SemicolonToken, Variables, Body);
	}

	public MethodDeclarationSyntax WithParameterList(ParameterListSyntax parameterList)
	{
		return Update(Attributes, LocalKeyword, ProcedureKeyword, Name, parameterList, ReturnValue, SemicolonToken, Variables, Body);
	}

	public MethodDeclarationSyntax WithReturnValue(ReturnValueSyntax returnValue)
	{
		return Update(Attributes, LocalKeyword, ProcedureKeyword, Name, ParameterList, returnValue, SemicolonToken, Variables, Body);
	}

	public MethodDeclarationSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(Attributes, LocalKeyword, ProcedureKeyword, Name, ParameterList, ReturnValue, semicolonToken, Variables, Body);
	}

	public MethodDeclarationSyntax WithVariables(VarSectionSyntax variables)
	{
		return Update(Attributes, LocalKeyword, ProcedureKeyword, Name, ParameterList, ReturnValue, SemicolonToken, variables, Body);
	}

	public MethodDeclarationSyntax WithBody(BlockSyntax body)
	{
		return Update(Attributes, LocalKeyword, ProcedureKeyword, Name, ParameterList, ReturnValue, SemicolonToken, Variables, body);
	}

	public MethodDeclarationSyntax AddAttributes(params MemberAttributeSyntax[] items)
	{
		return WithAttributes(Attributes.AddRange(items));
	}

	public MethodDeclarationSyntax AddParameterListParameters(params ParameterSyntax[] items)
	{
		return WithParameterList(ParameterList.WithParameters(ParameterList.Parameters.AddRange(items)));
	}

	public MethodDeclarationSyntax AddVariablesVariables(params VariableDeclarationSyntax[] items)
	{
		VarSectionSyntax varSectionSyntax = Variables ?? SyntaxFactory.VarSection();
		return WithVariables(varSectionSyntax.WithVariables(varSectionSyntax.Variables.AddRange(items)));
	}

	public MethodDeclarationSyntax AddBodyStatements(params StatementSyntax[] items)
	{
		BlockSyntax blockSyntax = Body ?? SyntaxFactory.Block();
		return WithBody(blockSyntax.WithStatements(blockSyntax.Statements.AddRange(items)));
	}
}
