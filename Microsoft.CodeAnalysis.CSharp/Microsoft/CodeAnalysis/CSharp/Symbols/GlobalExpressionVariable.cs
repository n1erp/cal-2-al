using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal class GlobalExpressionVariable : SourceMemberFieldSymbol
{
	private class InferrableGlobalExpressionVariable : GlobalExpressionVariable
	{
		private readonly FieldSymbol _containingFieldOpt;

		private readonly SyntaxReference _nodeToBind;

		internal InferrableGlobalExpressionVariable(SourceMemberContainerTypeSymbol containingType, DeclarationModifiers modifiers, TypeSyntax typeSyntax, string name, SyntaxReference syntax, Location location, FieldSymbol containingFieldOpt, SyntaxNode nodeToBind)
			: base(containingType, modifiers, typeSyntax, name, syntax, location)
		{
			_containingFieldOpt = containingFieldOpt;
			_nodeToBind = nodeToBind.GetReference();
		}

		protected override void InferFieldType(ConsList<FieldSymbol> fieldsBeingBound, Binder binder)
		{
			SyntaxNode syntax = _nodeToBind.GetSyntax();
			if ((object)_containingFieldOpt != null && syntax.Kind() != SyntaxKind.VariableDeclarator)
			{
				binder = binder.WithContainingMemberOrLambda(_containingFieldOpt).WithAdditionalFlags(BinderFlags.FieldInitializer);
			}
			fieldsBeingBound = new ConsList<FieldSymbol>(this, fieldsBeingBound);
			binder = new ImplicitlyTypedFieldBinder(binder, fieldsBeingBound);
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			SyntaxKind syntaxKind = syntax.Kind();
			if (syntaxKind == SyntaxKind.VariableDeclarator)
			{
				binder.BindDeclaratorArguments((VariableDeclaratorSyntax)syntax, instance);
			}
			else
			{
				binder.BindExpression((ExpressionSyntax)syntax, instance);
			}
			instance.Free();
		}
	}

	private TypeSymbol _lazyType;

	private SyntaxReference _typeSyntax;

	protected override SyntaxList<AttributeListSyntax> AttributeDeclarationSyntaxList => default;

	protected override TypeSyntax TypeSyntax => (TypeSyntax)_typeSyntax.GetSyntax();

	protected override SyntaxTokenList ModifiersTokenList => default;

	public override bool HasInitializer => false;

	internal GlobalExpressionVariable(SourceMemberContainerTypeSymbol containingType, DeclarationModifiers modifiers, TypeSyntax typeSyntax, string name, SyntaxReference syntax, Location location)
		: base(containingType, modifiers, name, syntax, location)
	{
		_typeSyntax = typeSyntax.GetReference();
	}

	internal static GlobalExpressionVariable Create(SourceMemberContainerTypeSymbol containingType, DeclarationModifiers modifiers, TypeSyntax typeSyntax, string name, SyntaxNode syntax, Location location, FieldSymbol containingFieldOpt, SyntaxNode nodeToBind)
	{
		SyntaxReference reference = syntax.GetReference();
		if (!typeSyntax.IsVar)
		{
			return new GlobalExpressionVariable(containingType, modifiers, typeSyntax, name, reference, location);
		}
		return new InferrableGlobalExpressionVariable(containingType, modifiers, typeSyntax, name, reference, location, containingFieldOpt, nodeToBind);
	}

	protected override ConstantValue MakeConstantValue(HashSet<SourceFieldSymbolWithSyntaxReference> dependencies, bool earlyDecodingWellKnownAttributes, DiagnosticBag diagnostics)
	{
		return null;
	}

	internal override TypeSymbol GetFieldType(ConsList<FieldSymbol> fieldsBeingBound)
	{
		if ((object)_lazyType != null)
		{
			return _lazyType;
		}
		TypeSyntax typeSyntax = TypeSyntax;
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		Binder binder = declaringCompilation.GetBinderFactory(SyntaxTree).GetBinder(typeSyntax);
		TypeSymbol type = binder.BindTypeOrVarKeyword(typeSyntax, instance, out var isVar);
		if (isVar && !fieldsBeingBound.ContainsReference(this))
		{
			InferFieldType(fieldsBeingBound, binder);
		}
		else
		{
			if (isVar)
			{
				instance.Add(ErrorCode.ERR_RecursivelyTypedVariable, ErrorLocation, this);
				type = binder.CreateErrorType("var");
			}
			SetType(declaringCompilation, instance, type);
		}
		instance.Free();
		return _lazyType;
	}

	private TypeSymbol SetType(CSharpCompilation compilation, DiagnosticBag diagnostics, TypeSymbol type)
	{
		_ = _lazyType;
		if ((object)Interlocked.CompareExchange(ref _lazyType, type, null) == null)
		{
			TypeChecks(type, diagnostics);
			compilation.DeclarationDiagnostics.AddRange(diagnostics);
			state.NotePartComplete(CompletionPart.Type);
		}
		return _lazyType;
	}

	internal TypeSymbol SetType(TypeSymbol type, DiagnosticBag diagnostics)
	{
		return SetType(DeclaringCompilation, diagnostics, type);
	}

	protected virtual void InferFieldType(ConsList<FieldSymbol> fieldsBeingBound, Binder binder)
	{
		throw ExceptionUtilities.Unreachable;
	}
}
