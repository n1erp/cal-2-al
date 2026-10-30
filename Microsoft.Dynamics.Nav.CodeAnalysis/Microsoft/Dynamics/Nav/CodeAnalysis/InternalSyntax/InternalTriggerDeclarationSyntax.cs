using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalTriggerDeclarationSyntax : InternalMethodOrTriggerDeclarationSyntax
{
	internal readonly InternalSyntaxNode _attributes;

	internal readonly InternalSyntaxToken _triggerKeyword;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalParameterListSyntax _parameterList;

	internal readonly InternalReturnValueSyntax _returnValue;

	internal readonly InternalSyntaxToken _semicolonToken;

	internal readonly InternalVarSectionSyntax _variables;

	internal readonly InternalBlockSyntax _body;

	internal override SyntaxNodeDefinition Definition => TriggerDeclarationSyntaxDefinition.Instance.Value;

	public override InternalSyntaxList<InternalMemberAttributeSyntax> Attributes => new InternalSyntaxList<InternalMemberAttributeSyntax>(_attributes);

	public InternalSyntaxToken TriggerKeyword => _triggerKeyword;

	public override InternalIdentifierNameSyntax Name => _name;

	public override InternalParameterListSyntax ParameterList => _parameterList;

	public override InternalReturnValueSyntax ReturnValue => _returnValue;

	public override InternalSyntaxToken SemicolonToken => _semicolonToken;

	public override InternalVarSectionSyntax Variables => _variables;

	public override InternalBlockSyntax Body => _body;

	internal InternalTriggerDeclarationSyntax(SyntaxKind kind, InternalSyntaxNode attributes, InternalSyntaxToken triggerKeyword, InternalIdentifierNameSyntax name, InternalParameterListSyntax parameterList, InternalReturnValueSyntax returnValue, InternalSyntaxToken semicolonToken, InternalVarSectionSyntax variables, InternalBlockSyntax body, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 8;
		if (attributes != null)
		{
			AdjustFlagsAndWidth(attributes);
			_attributes = attributes;
		}
		AdjustFlagsAndWidth(triggerKeyword);
		_triggerKeyword = triggerKeyword;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(parameterList);
		_parameterList = parameterList;
		if (returnValue != null)
		{
			AdjustFlagsAndWidth(returnValue);
			_returnValue = returnValue;
		}
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
		if (variables != null)
		{
			AdjustFlagsAndWidth(variables);
			_variables = variables;
		}
		AdjustFlagsAndWidth(body);
		_body = body;
	}

	internal InternalTriggerDeclarationSyntax(SyntaxKind kind, InternalSyntaxNode attributes, InternalSyntaxToken triggerKeyword, InternalIdentifierNameSyntax name, InternalParameterListSyntax parameterList, InternalReturnValueSyntax returnValue, InternalSyntaxToken semicolonToken, InternalVarSectionSyntax variables, InternalBlockSyntax body, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 8;
		if (attributes != null)
		{
			AdjustFlagsAndWidth(attributes);
			_attributes = attributes;
		}
		AdjustFlagsAndWidth(triggerKeyword);
		_triggerKeyword = triggerKeyword;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(parameterList);
		_parameterList = parameterList;
		if (returnValue != null)
		{
			AdjustFlagsAndWidth(returnValue);
			_returnValue = returnValue;
		}
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
		if (variables != null)
		{
			AdjustFlagsAndWidth(variables);
			_variables = variables;
		}
		AdjustFlagsAndWidth(body);
		_body = body;
	}

	internal InternalTriggerDeclarationSyntax(SyntaxKind kind, InternalSyntaxNode attributes, InternalSyntaxToken triggerKeyword, InternalIdentifierNameSyntax name, InternalParameterListSyntax parameterList, InternalReturnValueSyntax returnValue, InternalSyntaxToken semicolonToken, InternalVarSectionSyntax variables, InternalBlockSyntax body)
		: base(kind)
	{
		SlotCount = 8;
		if (attributes != null)
		{
			AdjustFlagsAndWidth(attributes);
			_attributes = attributes;
		}
		AdjustFlagsAndWidth(triggerKeyword);
		_triggerKeyword = triggerKeyword;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(parameterList);
		_parameterList = parameterList;
		if (returnValue != null)
		{
			AdjustFlagsAndWidth(returnValue);
			_returnValue = returnValue;
		}
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
		if (variables != null)
		{
			AdjustFlagsAndWidth(variables);
			_variables = variables;
		}
		AdjustFlagsAndWidth(body);
		_body = body;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _attributes, 
			1 => _triggerKeyword, 
			2 => _name, 
			3 => _parameterList, 
			4 => _returnValue, 
			5 => _semicolonToken, 
			6 => _variables, 
			7 => _body, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new TriggerDeclarationSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTriggerDeclaration(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitTriggerDeclaration(this);
	}

	public InternalTriggerDeclarationSyntax Update(InternalSyntaxList<InternalMemberAttributeSyntax> attributes, InternalSyntaxToken triggerKeyword, InternalIdentifierNameSyntax name, InternalParameterListSyntax parameterList, InternalReturnValueSyntax returnValue, InternalSyntaxToken semicolonToken, InternalVarSectionSyntax variables, InternalBlockSyntax body)
	{
		if (attributes != Attributes || triggerKeyword != TriggerKeyword || name != Name || parameterList != ParameterList || returnValue != ReturnValue || semicolonToken != SemicolonToken || variables != Variables || body != Body)
		{
			return InternalSyntaxFactory.TriggerDeclaration(attributes, triggerKeyword, name, parameterList, returnValue, semicolonToken, variables, body).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalTriggerDeclarationSyntax(Kind, _attributes, _triggerKeyword, _name, _parameterList, _returnValue, _semicolonToken, _variables, _body, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalTriggerDeclarationSyntax(Kind, _attributes, _triggerKeyword, _name, _parameterList, _returnValue, _semicolonToken, _variables, _body, GetDiagnostics(), annotations);
	}
}
