using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalDotNetPackageSyntax : InternalObjectSyntax
{
	internal readonly InternalSyntaxToken _dotNetKeyword;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalPropertyListSyntax _propertyList;

	internal readonly InternalSyntaxNode _assemblies;

	internal readonly InternalSyntaxNode _members;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => DotNetPackageSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken DotNetKeyword => _dotNetKeyword;

	public override InternalIdentifierNameSyntax Name => _name;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public override InternalPropertyListSyntax PropertyList => _propertyList;

	public InternalSyntaxList<InternalDotNetAssemblySyntax> Assemblies => new InternalSyntaxList<InternalDotNetAssemblySyntax>(_assemblies);

	public override InternalSyntaxList<InternalMemberSyntax> Members => new InternalSyntaxList<InternalMemberSyntax>(_members);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalDotNetPackageSyntax(SyntaxKind kind, InternalSyntaxToken dotNetKeyword, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode assemblies, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 7;
		AdjustFlagsAndWidth(dotNetKeyword);
		_dotNetKeyword = dotNetKeyword;
		if (name != null)
		{
			AdjustFlagsAndWidth(name);
			_name = name;
		}
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (assemblies != null)
		{
			AdjustFlagsAndWidth(assemblies);
			_assemblies = assemblies;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			_members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalDotNetPackageSyntax(SyntaxKind kind, InternalSyntaxToken dotNetKeyword, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode assemblies, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 7;
		AdjustFlagsAndWidth(dotNetKeyword);
		_dotNetKeyword = dotNetKeyword;
		if (name != null)
		{
			AdjustFlagsAndWidth(name);
			_name = name;
		}
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (assemblies != null)
		{
			AdjustFlagsAndWidth(assemblies);
			_assemblies = assemblies;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			_members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalDotNetPackageSyntax(SyntaxKind kind, InternalSyntaxToken dotNetKeyword, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode assemblies, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 7;
		AdjustFlagsAndWidth(dotNetKeyword);
		_dotNetKeyword = dotNetKeyword;
		if (name != null)
		{
			AdjustFlagsAndWidth(name);
			_name = name;
		}
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (assemblies != null)
		{
			AdjustFlagsAndWidth(assemblies);
			_assemblies = assemblies;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			_members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _dotNetKeyword, 
			1 => _name, 
			2 => _openBraceToken, 
			3 => _propertyList, 
			4 => _assemblies, 
			5 => _members, 
			6 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new DotNetPackageSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDotNetPackage(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitDotNetPackage(this);
	}

	public InternalDotNetPackageSyntax Update(InternalSyntaxToken dotNetKeyword, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalDotNetAssemblySyntax> assemblies, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		if (dotNetKeyword != DotNetKeyword || name != Name || openBraceToken != OpenBraceToken || propertyList != PropertyList || assemblies != Assemblies || members != Members || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.DotNetPackage(dotNetKeyword, name, openBraceToken, propertyList, assemblies, members, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalDotNetPackageSyntax(Kind, _dotNetKeyword, _name, _openBraceToken, _propertyList, _assemblies, _members, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalDotNetPackageSyntax(Kind, _dotNetKeyword, _name, _openBraceToken, _propertyList, _assemblies, _members, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
