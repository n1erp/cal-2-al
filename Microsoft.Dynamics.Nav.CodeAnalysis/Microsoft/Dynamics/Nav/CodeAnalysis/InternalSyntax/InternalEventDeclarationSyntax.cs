using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalEventDeclarationSyntax : InternalMemberSyntax
{
	internal readonly InternalSyntaxNode _attributes;

	internal readonly InternalSyntaxToken _eventKeyword;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalParameterListSyntax _parameterList;

	internal readonly InternalSyntaxToken _semicolonToken;

	internal override SyntaxNodeDefinition Definition => EventDeclarationSyntaxDefinition.Instance.Value;

	public InternalSyntaxList<InternalMemberAttributeSyntax> Attributes => new InternalSyntaxList<InternalMemberAttributeSyntax>(_attributes);

	public InternalSyntaxToken EventKeyword => _eventKeyword;

	public InternalIdentifierNameSyntax Name => _name;

	public InternalParameterListSyntax ParameterList => _parameterList;

	public InternalSyntaxToken SemicolonToken => _semicolonToken;

	internal InternalEventDeclarationSyntax(SyntaxKind kind, InternalSyntaxNode attributes, InternalSyntaxToken eventKeyword, InternalIdentifierNameSyntax name, InternalParameterListSyntax parameterList, InternalSyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 5;
		if (attributes != null)
		{
			AdjustFlagsAndWidth(attributes);
			_attributes = attributes;
		}
		AdjustFlagsAndWidth(eventKeyword);
		_eventKeyword = eventKeyword;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(parameterList);
		_parameterList = parameterList;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
	}

	internal InternalEventDeclarationSyntax(SyntaxKind kind, InternalSyntaxNode attributes, InternalSyntaxToken eventKeyword, InternalIdentifierNameSyntax name, InternalParameterListSyntax parameterList, InternalSyntaxToken semicolonToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 5;
		if (attributes != null)
		{
			AdjustFlagsAndWidth(attributes);
			_attributes = attributes;
		}
		AdjustFlagsAndWidth(eventKeyword);
		_eventKeyword = eventKeyword;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(parameterList);
		_parameterList = parameterList;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
	}

	internal InternalEventDeclarationSyntax(SyntaxKind kind, InternalSyntaxNode attributes, InternalSyntaxToken eventKeyword, InternalIdentifierNameSyntax name, InternalParameterListSyntax parameterList, InternalSyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 5;
		if (attributes != null)
		{
			AdjustFlagsAndWidth(attributes);
			_attributes = attributes;
		}
		AdjustFlagsAndWidth(eventKeyword);
		_eventKeyword = eventKeyword;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(parameterList);
		_parameterList = parameterList;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _attributes, 
			1 => _eventKeyword, 
			2 => _name, 
			3 => _parameterList, 
			4 => _semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new EventDeclarationSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitEventDeclaration(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitEventDeclaration(this);
	}

	public InternalEventDeclarationSyntax Update(InternalSyntaxList<InternalMemberAttributeSyntax> attributes, InternalSyntaxToken eventKeyword, InternalIdentifierNameSyntax name, InternalParameterListSyntax parameterList, InternalSyntaxToken semicolonToken)
	{
		if (attributes != Attributes || eventKeyword != EventKeyword || name != Name || parameterList != ParameterList || semicolonToken != SemicolonToken)
		{
			return InternalSyntaxFactory.EventDeclaration(attributes, eventKeyword, name, parameterList, semicolonToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalEventDeclarationSyntax(Kind, _attributes, _eventKeyword, _name, _parameterList, _semicolonToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalEventDeclarationSyntax(Kind, _attributes, _eventKeyword, _name, _parameterList, _semicolonToken, GetDiagnostics(), annotations);
	}
}
