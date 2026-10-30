using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalDotNetAssemblySyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _assemblyKeyword;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalNameSyntax _assemblyName;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalPropertyListSyntax _propertyList;

	internal readonly InternalSyntaxNode _typeDeclarations;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => DotNetAssemblySyntaxDefinition.Instance.Value;

	public InternalSyntaxToken AssemblyKeyword => _assemblyKeyword;

	public InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalNameSyntax AssemblyName => _assemblyName;

	public InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public InternalPropertyListSyntax PropertyList => _propertyList;

	public InternalSyntaxList<InternalDotNetTypeDeclarationSyntax> TypeDeclarations => new InternalSyntaxList<InternalDotNetTypeDeclarationSyntax>(_typeDeclarations);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalDotNetAssemblySyntax(SyntaxKind kind, InternalSyntaxToken assemblyKeyword, InternalSyntaxToken openParenthesisToken, InternalNameSyntax assemblyName, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode typeDeclarations, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 8;
		AdjustFlagsAndWidth(assemblyKeyword);
		_assemblyKeyword = assemblyKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(assemblyName);
		_assemblyName = assemblyName;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (typeDeclarations != null)
		{
			AdjustFlagsAndWidth(typeDeclarations);
			_typeDeclarations = typeDeclarations;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalDotNetAssemblySyntax(SyntaxKind kind, InternalSyntaxToken assemblyKeyword, InternalSyntaxToken openParenthesisToken, InternalNameSyntax assemblyName, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode typeDeclarations, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 8;
		AdjustFlagsAndWidth(assemblyKeyword);
		_assemblyKeyword = assemblyKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(assemblyName);
		_assemblyName = assemblyName;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (typeDeclarations != null)
		{
			AdjustFlagsAndWidth(typeDeclarations);
			_typeDeclarations = typeDeclarations;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalDotNetAssemblySyntax(SyntaxKind kind, InternalSyntaxToken assemblyKeyword, InternalSyntaxToken openParenthesisToken, InternalNameSyntax assemblyName, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode typeDeclarations, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 8;
		AdjustFlagsAndWidth(assemblyKeyword);
		_assemblyKeyword = assemblyKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(assemblyName);
		_assemblyName = assemblyName;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (typeDeclarations != null)
		{
			AdjustFlagsAndWidth(typeDeclarations);
			_typeDeclarations = typeDeclarations;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _assemblyKeyword, 
			1 => _openParenthesisToken, 
			2 => _assemblyName, 
			3 => _closeParenthesisToken, 
			4 => _openBraceToken, 
			5 => _propertyList, 
			6 => _typeDeclarations, 
			7 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new DotNetAssemblySyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDotNetAssembly(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitDotNetAssembly(this);
	}

	public InternalDotNetAssemblySyntax Update(InternalSyntaxToken assemblyKeyword, InternalSyntaxToken openParenthesisToken, InternalNameSyntax assemblyName, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalDotNetTypeDeclarationSyntax> typeDeclarations, InternalSyntaxToken closeBraceToken)
	{
		if (assemblyKeyword != AssemblyKeyword || openParenthesisToken != OpenParenthesisToken || assemblyName != AssemblyName || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || typeDeclarations != TypeDeclarations || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.DotNetAssembly(assemblyKeyword, openParenthesisToken, assemblyName, closeParenthesisToken, openBraceToken, propertyList, typeDeclarations, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalDotNetAssemblySyntax(Kind, _assemblyKeyword, _openParenthesisToken, _assemblyName, _closeParenthesisToken, _openBraceToken, _propertyList, _typeDeclarations, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalDotNetAssemblySyntax(Kind, _assemblyKeyword, _openParenthesisToken, _assemblyName, _closeParenthesisToken, _openBraceToken, _propertyList, _typeDeclarations, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
