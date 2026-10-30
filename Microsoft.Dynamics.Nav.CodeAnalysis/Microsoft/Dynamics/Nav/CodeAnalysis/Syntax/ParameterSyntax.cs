using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ParameterSyntax : SyntaxNode
{
	private IdentifierNameSyntax _name;

	private TypeReferenceBaseSyntax _type;

	public SyntaxToken VarKeyword
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalParameterSyntax)Node)._varKeyword;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, Position, 0);
			}
			return default;
		}
	}

	public IdentifierNameSyntax Name => GetRed(ref _name, 1);

	public SyntaxToken ColonToken => new SyntaxToken(this, ((InternalParameterSyntax)Node)._colonToken, GetChildPosition(2), GetChildIndex(2));

	[SuppressMessage("Microsoft.Naming", "CA1721: PropertyNamesShouldNotMatchGetMethods")]
	public TypeReferenceBaseSyntax Type => GetRed(ref _type, 3);

	internal ParameterSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)GetRed(ref _name, 1), 
			3 => GetRed(ref _type, 3), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)_name, 
			3 => _type, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitParameter(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitParameter(this);
	}

	public ParameterSyntax Update(SyntaxToken varKeyword, IdentifierNameSyntax name, SyntaxToken colonToken, TypeReferenceBaseSyntax type)
	{
		if (varKeyword != VarKeyword || name != Name || colonToken != ColonToken || type != Type)
		{
			ParameterSyntax parameterSyntax = SyntaxFactory.Parameter(varKeyword, name, colonToken, type);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return parameterSyntax.WithAnnotations(annotations);
			}
			return parameterSyntax;
		}
		return this;
	}

	public ParameterSyntax WithVarKeyword(SyntaxToken varKeyword)
	{
		return Update(varKeyword, Name, ColonToken, Type);
	}

	public ParameterSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(VarKeyword, name, ColonToken, Type);
	}

	public ParameterSyntax WithColonToken(SyntaxToken colonToken)
	{
		return Update(VarKeyword, Name, colonToken, Type);
	}

	public ParameterSyntax WithType(TypeReferenceBaseSyntax type)
	{
		return Update(VarKeyword, Name, ColonToken, type);
	}
}
