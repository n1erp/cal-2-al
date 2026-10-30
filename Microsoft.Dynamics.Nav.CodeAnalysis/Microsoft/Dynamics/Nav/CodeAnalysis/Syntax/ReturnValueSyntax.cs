using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ReturnValueSyntax : SyntaxNode
{
	private IdentifierNameSyntax _name;

	private DataTypeSyntax _dataType;

	public IdentifierNameSyntax Name => GetRedAtZero(ref _name);

	public SyntaxToken ColonToken => new SyntaxToken(this, ((InternalReturnValueSyntax)Node)._colonToken, GetChildPosition(1), GetChildIndex(1));

	public DataTypeSyntax DataType => GetRed(ref _dataType, 2);

	internal ReturnValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref _name), 
			2 => GetRed(ref _dataType, 2), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)_name, 
			2 => _dataType, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitReturnValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitReturnValue(this);
	}

	public ReturnValueSyntax Update(IdentifierNameSyntax name, SyntaxToken colonToken, DataTypeSyntax dataType)
	{
		if (name != Name || colonToken != ColonToken || dataType != DataType)
		{
			ReturnValueSyntax returnValueSyntax = SyntaxFactory.ReturnValue(name, colonToken, dataType);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return returnValueSyntax.WithAnnotations(annotations);
			}
			return returnValueSyntax;
		}
		return this;
	}

	public ReturnValueSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(name, ColonToken, DataType);
	}

	public ReturnValueSyntax WithColonToken(SyntaxToken colonToken)
	{
		return Update(Name, colonToken, DataType);
	}

	public ReturnValueSyntax WithDataType(DataTypeSyntax dataType)
	{
		return Update(Name, ColonToken, dataType);
	}
}
