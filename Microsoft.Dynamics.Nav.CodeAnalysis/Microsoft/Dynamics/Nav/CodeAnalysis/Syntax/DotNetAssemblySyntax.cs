using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class DotNetAssemblySyntax : SyntaxNode
{
	private NameSyntax _assemblyName;

	private PropertyListSyntax _propertyList;

	private SyntaxNode _typeDeclarations;

	public SyntaxToken AssemblyKeyword => new SyntaxToken(this, ((InternalDotNetAssemblySyntax)Node)._assemblyKeyword, Position, 0);

	public SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalDotNetAssemblySyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public NameSyntax AssemblyName => GetRed(ref _assemblyName, 2);

	public SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalDotNetAssemblySyntax)Node)._closeParenthesisToken, GetChildPosition(3), GetChildIndex(3));

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalDotNetAssemblySyntax)Node)._openBraceToken, GetChildPosition(4), GetChildIndex(4));

	public PropertyListSyntax PropertyList => GetRed(ref _propertyList, 5);

	public SyntaxList<DotNetTypeDeclarationSyntax> TypeDeclarations => new SyntaxList<DotNetTypeDeclarationSyntax>(GetRed(ref _typeDeclarations, 6));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalDotNetAssemblySyntax)Node)._closeBraceToken, GetChildPosition(7), GetChildIndex(7));

	internal DotNetAssemblySyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => GetRed(ref _assemblyName, 2), 
			5 => GetRed(ref _propertyList, 5), 
			6 => GetRed(ref _typeDeclarations, 6), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => _assemblyName, 
			5 => _propertyList, 
			6 => _typeDeclarations, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDotNetAssembly(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitDotNetAssembly(this);
	}

	public DotNetAssemblySyntax Update(SyntaxToken assemblyKeyword, SyntaxToken openParenthesisToken, NameSyntax assemblyName, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<DotNetTypeDeclarationSyntax> typeDeclarations, SyntaxToken closeBraceToken)
	{
		if (assemblyKeyword != AssemblyKeyword || openParenthesisToken != OpenParenthesisToken || assemblyName != AssemblyName || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || typeDeclarations != TypeDeclarations || closeBraceToken != CloseBraceToken)
		{
			DotNetAssemblySyntax dotNetAssemblySyntax = SyntaxFactory.DotNetAssembly(assemblyKeyword, openParenthesisToken, assemblyName, closeParenthesisToken, openBraceToken, propertyList, typeDeclarations, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return dotNetAssemblySyntax.WithAnnotations(annotations);
			}
			return dotNetAssemblySyntax;
		}
		return this;
	}

	public DotNetAssemblySyntax WithAssemblyKeyword(SyntaxToken assemblyKeyword)
	{
		return Update(assemblyKeyword, OpenParenthesisToken, AssemblyName, CloseParenthesisToken, OpenBraceToken, PropertyList, TypeDeclarations, CloseBraceToken);
	}

	public DotNetAssemblySyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(AssemblyKeyword, openParenthesisToken, AssemblyName, CloseParenthesisToken, OpenBraceToken, PropertyList, TypeDeclarations, CloseBraceToken);
	}

	public DotNetAssemblySyntax WithAssemblyName(NameSyntax assemblyName)
	{
		return Update(AssemblyKeyword, OpenParenthesisToken, assemblyName, CloseParenthesisToken, OpenBraceToken, PropertyList, TypeDeclarations, CloseBraceToken);
	}

	public DotNetAssemblySyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(AssemblyKeyword, OpenParenthesisToken, AssemblyName, closeParenthesisToken, OpenBraceToken, PropertyList, TypeDeclarations, CloseBraceToken);
	}

	public DotNetAssemblySyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(AssemblyKeyword, OpenParenthesisToken, AssemblyName, CloseParenthesisToken, openBraceToken, PropertyList, TypeDeclarations, CloseBraceToken);
	}

	public DotNetAssemblySyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(AssemblyKeyword, OpenParenthesisToken, AssemblyName, CloseParenthesisToken, OpenBraceToken, propertyList, TypeDeclarations, CloseBraceToken);
	}

	public DotNetAssemblySyntax WithTypeDeclarations(SyntaxList<DotNetTypeDeclarationSyntax> typeDeclarations)
	{
		return Update(AssemblyKeyword, OpenParenthesisToken, AssemblyName, CloseParenthesisToken, OpenBraceToken, PropertyList, typeDeclarations, CloseBraceToken);
	}

	public DotNetAssemblySyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(AssemblyKeyword, OpenParenthesisToken, AssemblyName, CloseParenthesisToken, OpenBraceToken, PropertyList, TypeDeclarations, closeBraceToken);
	}

	public DotNetAssemblySyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public DotNetAssemblySyntax AddTypeDeclarations(params DotNetTypeDeclarationSyntax[] items)
	{
		return WithTypeDeclarations(TypeDeclarations.AddRange(items));
	}
}
