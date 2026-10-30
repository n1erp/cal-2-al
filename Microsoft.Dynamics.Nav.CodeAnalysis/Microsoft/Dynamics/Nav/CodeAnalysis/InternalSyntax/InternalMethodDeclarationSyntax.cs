using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalMethodDeclarationSyntax : InternalMethodOrTriggerDeclarationSyntax
{
	internal readonly InternalSyntaxNode _attributes;

	internal readonly InternalSyntaxToken _localKeyword;

	internal readonly InternalSyntaxToken _procedureKeyword;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalParameterListSyntax _parameterList;

	internal readonly InternalReturnValueSyntax _returnValue;

	internal readonly InternalSyntaxToken _semicolonToken;

	internal readonly InternalVarSectionSyntax _variables;

	internal readonly InternalBlockSyntax _body;

	internal override SyntaxNodeDefinition Definition => MethodDeclarationSyntaxDefinition.Instance.Value;

	public override InternalSyntaxList<InternalMemberAttributeSyntax> Attributes => new InternalSyntaxList<InternalMemberAttributeSyntax>(_attributes);

	public InternalSyntaxToken LocalKeyword => _localKeyword;

	public InternalSyntaxToken ProcedureKeyword => _procedureKeyword;

	public override InternalIdentifierNameSyntax Name => _name;

	public override InternalParameterListSyntax ParameterList => _parameterList;

	public override InternalReturnValueSyntax ReturnValue => _returnValue;

	public override InternalSyntaxToken SemicolonToken => _semicolonToken;

	public override InternalVarSectionSyntax Variables => _variables;

	public override InternalBlockSyntax Body => _body;

	internal InternalMethodDeclarationSyntax(SyntaxKind kind, InternalSyntaxNode attributes, InternalSyntaxToken localKeyword, InternalSyntaxToken procedureKeyword, InternalIdentifierNameSyntax name, InternalParameterListSyntax parameterList, InternalReturnValueSyntax returnValue, InternalSyntaxToken semicolonToken, InternalVarSectionSyntax variables, InternalBlockSyntax body, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 9;
		if (attributes != null)
		{
			AdjustFlagsAndWidth(attributes);
			_attributes = attributes;
		}
		if (localKeyword != null)
		{
			AdjustFlagsAndWidth(localKeyword);
			_localKeyword = localKeyword;
		}
		AdjustFlagsAndWidth(procedureKeyword);
		_procedureKeyword = procedureKeyword;
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
		if (body != null)
		{
			AdjustFlagsAndWidth(body);
			_body = body;
		}
	}

	internal InternalMethodDeclarationSyntax(SyntaxKind kind, InternalSyntaxNode attributes, InternalSyntaxToken localKeyword, InternalSyntaxToken procedureKeyword, InternalIdentifierNameSyntax name, InternalParameterListSyntax parameterList, InternalReturnValueSyntax returnValue, InternalSyntaxToken semicolonToken, InternalVarSectionSyntax variables, InternalBlockSyntax body, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 9;
		if (attributes != null)
		{
			AdjustFlagsAndWidth(attributes);
			_attributes = attributes;
		}
		if (localKeyword != null)
		{
			AdjustFlagsAndWidth(localKeyword);
			_localKeyword = localKeyword;
		}
		AdjustFlagsAndWidth(procedureKeyword);
		_procedureKeyword = procedureKeyword;
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
		if (body != null)
		{
			AdjustFlagsAndWidth(body);
			_body = body;
		}
	}

	internal InternalMethodDeclarationSyntax(SyntaxKind kind, InternalSyntaxNode attributes, InternalSyntaxToken localKeyword, InternalSyntaxToken procedureKeyword, InternalIdentifierNameSyntax name, InternalParameterListSyntax parameterList, InternalReturnValueSyntax returnValue, InternalSyntaxToken semicolonToken, InternalVarSectionSyntax variables, InternalBlockSyntax body)
		: base(kind)
	{
		SlotCount = 9;
		if (attributes != null)
		{
			AdjustFlagsAndWidth(attributes);
			_attributes = attributes;
		}
		if (localKeyword != null)
		{
			AdjustFlagsAndWidth(localKeyword);
			_localKeyword = localKeyword;
		}
		AdjustFlagsAndWidth(procedureKeyword);
		_procedureKeyword = procedureKeyword;
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
		if (body != null)
		{
			AdjustFlagsAndWidth(body);
			_body = body;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _attributes, 
			1 => _localKeyword, 
			2 => _procedureKeyword, 
			3 => _name, 
			4 => _parameterList, 
			5 => _returnValue, 
			6 => _semicolonToken, 
			7 => _variables, 
			8 => _body, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new MethodDeclarationSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitMethodDeclaration(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitMethodDeclaration(this);
	}

	public InternalMethodDeclarationSyntax Update(InternalSyntaxList<InternalMemberAttributeSyntax> attributes, InternalSyntaxToken localKeyword, InternalSyntaxToken procedureKeyword, InternalIdentifierNameSyntax name, InternalParameterListSyntax parameterList, InternalReturnValueSyntax returnValue, InternalSyntaxToken semicolonToken, InternalVarSectionSyntax variables, InternalBlockSyntax body)
	{
		if (attributes != Attributes || localKeyword != LocalKeyword || procedureKeyword != ProcedureKeyword || name != Name || parameterList != ParameterList || returnValue != ReturnValue || semicolonToken != SemicolonToken || variables != Variables || body != Body)
		{
			return InternalSyntaxFactory.MethodDeclaration(attributes, localKeyword, procedureKeyword, name, parameterList, returnValue, semicolonToken, variables, body).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalMethodDeclarationSyntax(Kind, _attributes, _localKeyword, _procedureKeyword, _name, _parameterList, _returnValue, _semicolonToken, _variables, _body, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalMethodDeclarationSyntax(Kind, _attributes, _localKeyword, _procedureKeyword, _name, _parameterList, _returnValue, _semicolonToken, _variables, _body, GetDiagnostics(), annotations);
	}
}
