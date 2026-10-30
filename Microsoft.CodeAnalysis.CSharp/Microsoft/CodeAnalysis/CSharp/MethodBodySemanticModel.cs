using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class MethodBodySemanticModel : MemberSemanticModel
{
	private MethodBodySemanticModel(Symbol owner, Binder rootBinder, CSharpSyntaxNode syntax, SyntaxTreeSemanticModel containingSemanticModelOpt = null, SyntaxTreeSemanticModel parentSemanticModelOpt = null, int speculatedPosition = 0)
		: base(syntax, owner, rootBinder, containingSemanticModelOpt, parentSemanticModelOpt, speculatedPosition)
	{
	}

	internal static MethodBodySemanticModel Create(SyntaxTreeSemanticModel containingSemanticModel, MethodSymbol owner, ExecutableCodeBinder executableCodeBinder, CSharpSyntaxNode syntax, BoundNode boundNode = null)
	{
		MethodBodySemanticModel methodBodySemanticModel = new MethodBodySemanticModel(owner, executableCodeBinder, syntax, containingSemanticModel);
		if (boundNode != null)
		{
			methodBodySemanticModel.UnguardedAddBoundTreeForStandaloneSyntax(syntax, boundNode);
		}
		return methodBodySemanticModel;
	}

	internal override BoundNode Bind(Binder binder, CSharpSyntaxNode node, DiagnosticBag diagnostics)
	{
		switch (node.Kind())
		{
		case SyntaxKind.ArrowExpressionClause:
			return binder.BindExpressionBodyAsBlock((ArrowExpressionClauseSyntax)node, diagnostics);
		case SyntaxKind.BaseConstructorInitializer:
		case SyntaxKind.ThisConstructorInitializer:
			return binder.BindConstructorInitializer((ConstructorInitializerSyntax)node, diagnostics);
		case SyntaxKind.MethodDeclaration:
		case SyntaxKind.OperatorDeclaration:
		case SyntaxKind.ConversionOperatorDeclaration:
		case SyntaxKind.ConstructorDeclaration:
		case SyntaxKind.DestructorDeclaration:
		case SyntaxKind.GetAccessorDeclaration:
		case SyntaxKind.SetAccessorDeclaration:
		case SyntaxKind.AddAccessorDeclaration:
		case SyntaxKind.RemoveAccessorDeclaration:
			return binder.BindMethodBody(node, diagnostics);
		default:
			return base.Bind(binder, node, diagnostics);
		}
	}

	internal static MethodBodySemanticModel CreateSpeculative(SyntaxTreeSemanticModel parentSemanticModel, MethodSymbol owner, StatementSyntax syntax, Binder rootBinder, int position)
	{
		return new MethodBodySemanticModel(owner, rootBinder, syntax, null, parentSemanticModel, position);
	}

	internal static MethodBodySemanticModel CreateSpeculative(SyntaxTreeSemanticModel parentSemanticModel, MethodSymbol owner, ArrowExpressionClauseSyntax syntax, Binder rootBinder, int position)
	{
		return new MethodBodySemanticModel(owner, rootBinder, syntax, null, parentSemanticModel, position);
	}

	internal static MethodBodySemanticModel CreateSpeculative(SyntaxTreeSemanticModel parentSemanticModel, MethodSymbol owner, ConstructorInitializerSyntax syntax, Binder rootBinder, int position)
	{
		return new MethodBodySemanticModel(owner, rootBinder, syntax, null, parentSemanticModel, position);
	}

	internal override bool TryGetSpeculativeSemanticModelForMethodBodyCore(SyntaxTreeSemanticModel parentModel, int position, BaseMethodDeclarationSyntax method, out SemanticModel speculativeModel)
	{
		return GetSpeculativeSemanticModelForMethodBody(parentModel, position, method.Body, out speculativeModel);
	}

	private bool GetSpeculativeSemanticModelForMethodBody(SyntaxTreeSemanticModel parentModel, int position, BlockSyntax body, out SemanticModel speculativeModel)
	{
		position = CheckAndAdjustPosition(position);
		MethodSymbol methodSymbol = (MethodSymbol)MemberSymbol;
		Binder binder = RootBinder;
		do
		{
			if (binder is ExecutableCodeBinder)
			{
				binder = binder.Next;
				break;
			}
			binder = binder.Next;
		}
		while (binder != null);
		Binder rootBinder = new ExecutableCodeBinder(body, methodSymbol, binder ?? RootBinder).GetBinder(body).WithAdditionalFlags(GetSemanticModelBinderFlags());
		speculativeModel = CreateSpeculative(parentModel, methodSymbol, body, rootBinder, position);
		return true;
	}

	internal override bool TryGetSpeculativeSemanticModelForMethodBodyCore(SyntaxTreeSemanticModel parentModel, int position, AccessorDeclarationSyntax accessor, out SemanticModel speculativeModel)
	{
		return GetSpeculativeSemanticModelForMethodBody(parentModel, position, accessor.Body, out speculativeModel);
	}

	internal override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, StatementSyntax statement, out SemanticModel speculativeModel)
	{
		position = CheckAndAdjustPosition(position);
		Binder enclosingBinder = GetEnclosingBinder(position);
		if (enclosingBinder == null)
		{
			speculativeModel = null;
			return false;
		}
		MethodSymbol methodSymbol = (MethodSymbol)MemberSymbol;
		enclosingBinder = new ExecutableCodeBinder(statement, methodSymbol, enclosingBinder);
		speculativeModel = CreateSpeculative(parentModel, methodSymbol, statement, enclosingBinder, position);
		return true;
	}

	internal override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, ArrowExpressionClauseSyntax expressionBody, out SemanticModel speculativeModel)
	{
		position = CheckAndAdjustPosition(position);
		Binder enclosingBinder = GetEnclosingBinder(position);
		if (enclosingBinder == null)
		{
			speculativeModel = null;
			return false;
		}
		MethodSymbol methodSymbol = (MethodSymbol)MemberSymbol;
		enclosingBinder = new ExecutableCodeBinder(expressionBody, methodSymbol, enclosingBinder);
		speculativeModel = CreateSpeculative(parentModel, methodSymbol, expressionBody, enclosingBinder, position);
		return true;
	}

	internal override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, ConstructorInitializerSyntax constructorInitializer, out SemanticModel speculativeModel)
	{
		MethodSymbol obj = MemberSymbol as MethodSymbol;
		if ((object)obj != null && obj.MethodKind == MethodKind.Constructor)
		{
			Binder enclosingBinder = GetEnclosingBinder(position);
			if (enclosingBinder != null)
			{
				MethodSymbol methodSymbol = (MethodSymbol)MemberSymbol;
				enclosingBinder = new ExecutableCodeBinder(constructorInitializer, methodSymbol, enclosingBinder);
				speculativeModel = CreateSpeculative(parentModel, methodSymbol, constructorInitializer, enclosingBinder, position);
				return true;
			}
		}
		speculativeModel = null;
		return false;
	}

	internal override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, EqualsValueClauseSyntax initializer, out SemanticModel speculativeModel)
	{
		speculativeModel = null;
		return false;
	}
}
