using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class EventTriggerDeclarationSyntax : MethodOrTriggerDeclarationSyntax
{
	private SyntaxNode _attributes;

	private IdentifierNameSyntax _publisher;

	private IdentifierNameSyntax _name;

	private ParameterListSyntax _parameterList;

	private ReturnValueSyntax _returnValue;

	private VarSectionSyntax _variables;

	private BlockSyntax _body;

	public override SyntaxList<MemberAttributeSyntax> Attributes => new SyntaxList<MemberAttributeSyntax>(GetRed(ref _attributes, 0));

	public SyntaxToken TriggerKeyword => new SyntaxToken(this, ((InternalEventTriggerDeclarationSyntax)Node)._triggerKeyword, GetChildPosition(1), GetChildIndex(1));

	public IdentifierNameSyntax Publisher => GetRed(ref _publisher, 2);

	public SyntaxToken ColonColonToken => new SyntaxToken(this, ((InternalEventTriggerDeclarationSyntax)Node)._colonColonToken, GetChildPosition(3), GetChildIndex(3));

	public override IdentifierNameSyntax Name => GetRed(ref _name, 4);

	public override ParameterListSyntax ParameterList => GetRed(ref _parameterList, 5);

	public override ReturnValueSyntax ReturnValue => GetRed(ref _returnValue, 6);

	public override SyntaxToken SemicolonToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalEventTriggerDeclarationSyntax)Node)._semicolonToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(7), GetChildIndex(7));
			}
			return default;
		}
	}

	public override VarSectionSyntax Variables => GetRed(ref _variables, 8);

	public override BlockSyntax Body => GetRed(ref _body, 9);

	internal EventTriggerDeclarationSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref _attributes), 
			2 => GetRed(ref _publisher, 2), 
			4 => GetRed(ref _name, 4), 
			5 => GetRed(ref _parameterList, 5), 
			6 => GetRed(ref _returnValue, 6), 
			8 => GetRed(ref _variables, 8), 
			9 => GetRed(ref _body, 9), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => _attributes, 
			2 => _publisher, 
			4 => _name, 
			5 => _parameterList, 
			6 => _returnValue, 
			8 => _variables, 
			9 => _body, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitEventTriggerDeclaration(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitEventTriggerDeclaration(this);
	}

	public EventTriggerDeclarationSyntax Update(SyntaxList<MemberAttributeSyntax> attributes, SyntaxToken triggerKeyword, IdentifierNameSyntax publisher, SyntaxToken colonColonToken, IdentifierNameSyntax name, ParameterListSyntax parameterList, ReturnValueSyntax returnValue, SyntaxToken semicolonToken, VarSectionSyntax variables, BlockSyntax body)
	{
		if (attributes != Attributes || triggerKeyword != TriggerKeyword || publisher != Publisher || colonColonToken != ColonColonToken || name != Name || parameterList != ParameterList || returnValue != ReturnValue || semicolonToken != SemicolonToken || variables != Variables || body != Body)
		{
			EventTriggerDeclarationSyntax eventTriggerDeclarationSyntax = SyntaxFactory.EventTriggerDeclaration(attributes, triggerKeyword, publisher, colonColonToken, name, parameterList, returnValue, semicolonToken, variables, body);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return eventTriggerDeclarationSyntax.WithAnnotations(annotations);
			}
			return eventTriggerDeclarationSyntax;
		}
		return this;
	}

	public EventTriggerDeclarationSyntax WithAttributes(SyntaxList<MemberAttributeSyntax> attributes)
	{
		return Update(attributes, TriggerKeyword, Publisher, ColonColonToken, Name, ParameterList, ReturnValue, SemicolonToken, Variables, Body);
	}

	public EventTriggerDeclarationSyntax WithTriggerKeyword(SyntaxToken triggerKeyword)
	{
		return Update(Attributes, triggerKeyword, Publisher, ColonColonToken, Name, ParameterList, ReturnValue, SemicolonToken, Variables, Body);
	}

	public EventTriggerDeclarationSyntax WithPublisher(IdentifierNameSyntax publisher)
	{
		return Update(Attributes, TriggerKeyword, publisher, ColonColonToken, Name, ParameterList, ReturnValue, SemicolonToken, Variables, Body);
	}

	public EventTriggerDeclarationSyntax WithColonColonToken(SyntaxToken colonColonToken)
	{
		return Update(Attributes, TriggerKeyword, Publisher, colonColonToken, Name, ParameterList, ReturnValue, SemicolonToken, Variables, Body);
	}

	public EventTriggerDeclarationSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(Attributes, TriggerKeyword, Publisher, ColonColonToken, name, ParameterList, ReturnValue, SemicolonToken, Variables, Body);
	}

	public EventTriggerDeclarationSyntax WithParameterList(ParameterListSyntax parameterList)
	{
		return Update(Attributes, TriggerKeyword, Publisher, ColonColonToken, Name, parameterList, ReturnValue, SemicolonToken, Variables, Body);
	}

	public EventTriggerDeclarationSyntax WithReturnValue(ReturnValueSyntax returnValue)
	{
		return Update(Attributes, TriggerKeyword, Publisher, ColonColonToken, Name, ParameterList, returnValue, SemicolonToken, Variables, Body);
	}

	public EventTriggerDeclarationSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(Attributes, TriggerKeyword, Publisher, ColonColonToken, Name, ParameterList, ReturnValue, semicolonToken, Variables, Body);
	}

	public EventTriggerDeclarationSyntax WithVariables(VarSectionSyntax variables)
	{
		return Update(Attributes, TriggerKeyword, Publisher, ColonColonToken, Name, ParameterList, ReturnValue, SemicolonToken, variables, Body);
	}

	public EventTriggerDeclarationSyntax WithBody(BlockSyntax body)
	{
		return Update(Attributes, TriggerKeyword, Publisher, ColonColonToken, Name, ParameterList, ReturnValue, SemicolonToken, Variables, body);
	}

	public EventTriggerDeclarationSyntax AddAttributes(params MemberAttributeSyntax[] items)
	{
		return WithAttributes(Attributes.AddRange(items));
	}

	public EventTriggerDeclarationSyntax AddParameterListParameters(params ParameterSyntax[] items)
	{
		return WithParameterList(ParameterList.WithParameters(ParameterList.Parameters.AddRange(items)));
	}

	public EventTriggerDeclarationSyntax AddVariablesVariables(params VariableDeclarationSyntax[] items)
	{
		VarSectionSyntax varSectionSyntax = Variables ?? SyntaxFactory.VarSection();
		return WithVariables(varSectionSyntax.WithVariables(varSectionSyntax.Variables.AddRange(items)));
	}

	public EventTriggerDeclarationSyntax AddBodyStatements(params StatementSyntax[] items)
	{
		return WithBody(Body.WithStatements(Body.Statements.AddRange(items)));
	}
}
