using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class TriggerDeclarationSyntax : MethodOrTriggerDeclarationSyntax
{
	private SyntaxNode _attributes;

	private IdentifierNameSyntax _name;

	private ParameterListSyntax _parameterList;

	private ReturnValueSyntax _returnValue;

	private VarSectionSyntax _variables;

	private BlockSyntax _body;

	public override SyntaxList<MemberAttributeSyntax> Attributes => new SyntaxList<MemberAttributeSyntax>(GetRed(ref _attributes, 0));

	public SyntaxToken TriggerKeyword => new SyntaxToken(this, ((InternalTriggerDeclarationSyntax)Node)._triggerKeyword, GetChildPosition(1), GetChildIndex(1));

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public override ParameterListSyntax ParameterList => GetRed(ref _parameterList, 3);

	public override ReturnValueSyntax ReturnValue => GetRed(ref _returnValue, 4);

	public override SyntaxToken SemicolonToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalTriggerDeclarationSyntax)Node)._semicolonToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(5), GetChildIndex(5));
			}
			return default;
		}
	}

	public override VarSectionSyntax Variables => GetRed(ref _variables, 6);

	public override BlockSyntax Body => GetRed(ref _body, 7);

	internal TriggerDeclarationSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref _attributes), 
			2 => GetRed(ref _name, 2), 
			3 => GetRed(ref _parameterList, 3), 
			4 => GetRed(ref _returnValue, 4), 
			6 => GetRed(ref _variables, 6), 
			7 => GetRed(ref _body, 7), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => _attributes, 
			2 => _name, 
			3 => _parameterList, 
			4 => _returnValue, 
			6 => _variables, 
			7 => _body, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTriggerDeclaration(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitTriggerDeclaration(this);
	}

	public TriggerDeclarationSyntax Update(SyntaxList<MemberAttributeSyntax> attributes, SyntaxToken triggerKeyword, IdentifierNameSyntax name, ParameterListSyntax parameterList, ReturnValueSyntax returnValue, SyntaxToken semicolonToken, VarSectionSyntax variables, BlockSyntax body)
	{
		if (attributes != Attributes || triggerKeyword != TriggerKeyword || name != Name || parameterList != ParameterList || returnValue != ReturnValue || semicolonToken != SemicolonToken || variables != Variables || body != Body)
		{
			TriggerDeclarationSyntax triggerDeclarationSyntax = SyntaxFactory.TriggerDeclaration(attributes, triggerKeyword, name, parameterList, returnValue, semicolonToken, variables, body);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return triggerDeclarationSyntax.WithAnnotations(annotations);
			}
			return triggerDeclarationSyntax;
		}
		return this;
	}

	public TriggerDeclarationSyntax WithAttributes(SyntaxList<MemberAttributeSyntax> attributes)
	{
		return Update(attributes, TriggerKeyword, Name, ParameterList, ReturnValue, SemicolonToken, Variables, Body);
	}

	public TriggerDeclarationSyntax WithTriggerKeyword(SyntaxToken triggerKeyword)
	{
		return Update(Attributes, triggerKeyword, Name, ParameterList, ReturnValue, SemicolonToken, Variables, Body);
	}

	public TriggerDeclarationSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(Attributes, TriggerKeyword, name, ParameterList, ReturnValue, SemicolonToken, Variables, Body);
	}

	public TriggerDeclarationSyntax WithParameterList(ParameterListSyntax parameterList)
	{
		return Update(Attributes, TriggerKeyword, Name, parameterList, ReturnValue, SemicolonToken, Variables, Body);
	}

	public TriggerDeclarationSyntax WithReturnValue(ReturnValueSyntax returnValue)
	{
		return Update(Attributes, TriggerKeyword, Name, ParameterList, returnValue, SemicolonToken, Variables, Body);
	}

	public TriggerDeclarationSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(Attributes, TriggerKeyword, Name, ParameterList, ReturnValue, semicolonToken, Variables, Body);
	}

	public TriggerDeclarationSyntax WithVariables(VarSectionSyntax variables)
	{
		return Update(Attributes, TriggerKeyword, Name, ParameterList, ReturnValue, SemicolonToken, variables, Body);
	}

	public TriggerDeclarationSyntax WithBody(BlockSyntax body)
	{
		return Update(Attributes, TriggerKeyword, Name, ParameterList, ReturnValue, SemicolonToken, Variables, body);
	}

	public TriggerDeclarationSyntax AddAttributes(params MemberAttributeSyntax[] items)
	{
		return WithAttributes(Attributes.AddRange(items));
	}

	public TriggerDeclarationSyntax AddParameterListParameters(params ParameterSyntax[] items)
	{
		return WithParameterList(ParameterList.WithParameters(ParameterList.Parameters.AddRange(items)));
	}

	public TriggerDeclarationSyntax AddVariablesVariables(params VariableDeclarationSyntax[] items)
	{
		VarSectionSyntax varSectionSyntax = Variables ?? SyntaxFactory.VarSection();
		return WithVariables(varSectionSyntax.WithVariables(varSectionSyntax.Variables.AddRange(items)));
	}

	public TriggerDeclarationSyntax AddBodyStatements(params StatementSyntax[] items)
	{
		return WithBody(Body.WithStatements(Body.Statements.AddRange(items)));
	}
}
