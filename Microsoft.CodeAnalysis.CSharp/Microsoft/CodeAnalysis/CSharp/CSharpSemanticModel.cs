using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class CSharpSemanticModel : SemanticModel
{
	[Flags]
	internal enum SymbolInfoOptions
	{
		PreferTypeToConstructors = 1,
		PreferConstructorsToType = 2,
		ResolveAliases = 4,
		PreserveAliases = 8,
		DefaultOptions = PreferConstructorsToType | ResolveAliases
	}

	public new abstract CSharpCompilation Compilation { get; }

	internal new abstract CSharpSyntaxNode Root { get; }

	public new abstract CSharpSemanticModel ParentModel { get; }

	public new abstract SyntaxTree SyntaxTree { get; }

	public sealed override string Language => "C#";

	protected sealed override Compilation CompilationCore => Compilation;

	protected sealed override SemanticModel ParentModelCore => ParentModel;

	protected sealed override SyntaxTree SyntaxTreeCore => SyntaxTree;

	protected sealed override SyntaxNode RootCore => Root;

	internal static bool CanGetSemanticInfo(CSharpSyntaxNode node, bool allowNamedArgumentName = false, bool isSpeculative = false)
	{
		if (!isSpeculative && IsInStructuredTriviaOtherThanCrefOrNameAttribute(node))
		{
			return false;
		}
		switch (node.Kind())
		{
		case SyntaxKind.ObjectInitializerExpression:
		case SyntaxKind.CollectionInitializerExpression:
			return false;
		case SyntaxKind.ComplexElementInitializerExpression:
			return false;
		case SyntaxKind.IdentifierName:
			if (!isSpeculative && node.Parent != null && node.Parent.Kind() == SyntaxKind.NameEquals && node.Parent.Parent.Kind() == SyntaxKind.UsingDirective)
			{
				return false;
			}
			break;
		case SyntaxKind.OmittedTypeArgument:
		case SyntaxKind.RefExpression:
		case SyntaxKind.RefType:
			return false;
		}
		if (node.IsMissing)
		{
			return false;
		}
		if ((!(node is ExpressionSyntax) || (!(isSpeculative | allowNamedArgumentName) && SyntaxFacts.IsNamedArgumentName(node))) && !(node is ConstructorInitializerSyntax) && !(node is AttributeSyntax))
		{
			return node is CrefSyntax;
		}
		return true;
	}

	internal abstract SymbolInfo GetSymbolInfoWorker(CSharpSyntaxNode node, SymbolInfoOptions options, CancellationToken cancellationToken = default(CancellationToken));

	internal abstract SymbolInfo GetCollectionInitializerSymbolInfoWorker(InitializerExpressionSyntax collectionInitializer, ExpressionSyntax node, CancellationToken cancellationToken = default(CancellationToken));

	internal abstract CSharpTypeInfo GetTypeInfoWorker(CSharpSyntaxNode node, CancellationToken cancellationToken = default(CancellationToken));

	internal abstract ImmutableArray<Symbol> GetMemberGroupWorker(CSharpSyntaxNode node, SymbolInfoOptions options, CancellationToken cancellationToken = default(CancellationToken));

	internal abstract ImmutableArray<PropertySymbol> GetIndexerGroupWorker(CSharpSyntaxNode node, SymbolInfoOptions options, CancellationToken cancellationToken = default(CancellationToken));

	internal abstract Optional<object> GetConstantValueWorker(CSharpSyntaxNode node, CancellationToken cancellationToken = default(CancellationToken));

	internal Binder GetSpeculativeBinder(int position, ExpressionSyntax expression, SpeculativeBindingOption bindingOption)
	{
		position = CheckAndAdjustPosition(position);
		if (bindingOption == SpeculativeBindingOption.BindAsTypeOrNamespace && !(expression is TypeSyntax))
		{
			return null;
		}
		Binder binder = GetEnclosingBinder(position);
		if (binder == null)
		{
			return null;
		}
		if (bindingOption == SpeculativeBindingOption.BindAsTypeOrNamespace && IsInTypeofExpression(position))
		{
			binder = new TypeofBinder(expression, binder);
		}
		return new ExecutableCodeBinder(expression, binder.ContainingMemberOrLambda, binder).GetBinder(expression);
	}

	private Binder GetSpeculativeBinderForAttribute(int position, AttributeSyntax attribute)
	{
		position = CheckAndAdjustPositionForSpeculativeAttribute(position);
		Binder enclosingBinder = GetEnclosingBinder(position);
		if (enclosingBinder == null)
		{
			return null;
		}
		return new ExecutableCodeBinder(attribute, enclosingBinder.ContainingMemberOrLambda, enclosingBinder).GetBinder(attribute);
	}

	private static BoundExpression GetSpeculativelyBoundExpressionHelper(Binder binder, ExpressionSyntax expression, SpeculativeBindingOption bindingOption, DiagnosticBag diagnostics)
	{
		if (bindingOption == SpeculativeBindingOption.BindAsTypeOrNamespace || binder.Flags.Includes(BinderFlags.CrefParameterOrReturnType))
		{
			return binder.BindNamespaceOrType(expression, diagnostics);
		}
		return binder.BindExpression(expression, diagnostics);
	}

	private BoundExpression GetSpeculativelyBoundExpression(int position, ExpressionSyntax expression, SpeculativeBindingOption bindingOption, out Binder binder, out ImmutableArray<Symbol> crefSymbols)
	{
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		crefSymbols = default;
		expression = SyntaxFactory.GetStandaloneExpression(expression);
		binder = GetSpeculativeBinder(position, expression, bindingOption);
		if (binder == null)
		{
			return null;
		}
		if (binder.Flags.Includes(BinderFlags.CrefParameterOrReturnType))
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			crefSymbols = ImmutableArray.Create((Symbol)binder.BindType(expression, instance));
			instance.Free();
			return null;
		}
		if (binder.InCref)
		{
			if (expression.IsKind(SyntaxKind.QualifiedName))
			{
				QualifiedNameSyntax qualifiedNameSyntax = (QualifiedNameSyntax)expression;
				QualifiedCrefSyntax crefSyntax = SyntaxFactory.QualifiedCref(qualifiedNameSyntax.Left, SyntaxFactory.NameMemberCref(qualifiedNameSyntax.Right));
				crefSymbols = BindCref(crefSyntax, binder);
			}
			else if (expression is TypeSyntax typeSyntax)
			{
				CrefSyntax crefSyntax2 = ((typeSyntax is PredefinedTypeSyntax) ? ((CrefSyntax)SyntaxFactory.TypeCref(typeSyntax)) : ((CrefSyntax)SyntaxFactory.NameMemberCref(typeSyntax)));
				crefSymbols = BindCref(crefSyntax2, binder);
			}
			return null;
		}
		DiagnosticBag instance2 = DiagnosticBag.GetInstance();
		BoundExpression speculativelyBoundExpressionHelper = GetSpeculativelyBoundExpressionHelper(binder, expression, bindingOption, instance2);
		instance2.Free();
		return speculativelyBoundExpressionHelper;
	}

	internal static ImmutableArray<Symbol> BindCref(CrefSyntax crefSyntax, Binder binder)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		ImmutableArray<Symbol> result = binder.BindCref(crefSyntax, out var _, instance);
		instance.Free();
		return result;
	}

	internal SymbolInfo GetCrefSymbolInfo(int position, CrefSyntax crefSyntax, SymbolInfoOptions options, bool hasParameterList)
	{
		Binder enclosingBinder = GetEnclosingBinder(position);
		if (enclosingBinder != null && enclosingBinder.InCref)
		{
			return GetCrefSymbolInfo(BindCref(crefSyntax, enclosingBinder), options, hasParameterList);
		}
		return SymbolInfo.None;
	}

	internal static bool HasParameterList(CrefSyntax crefSyntax)
	{
		while (crefSyntax.Kind() == SyntaxKind.QualifiedCref)
		{
			crefSyntax = ((QualifiedCrefSyntax)crefSyntax).Member;
		}
		return crefSyntax.Kind() switch
		{
			SyntaxKind.NameMemberCref => ((NameMemberCrefSyntax)crefSyntax).Parameters != null, 
			SyntaxKind.IndexerMemberCref => ((IndexerMemberCrefSyntax)crefSyntax).Parameters != null, 
			SyntaxKind.OperatorMemberCref => ((OperatorMemberCrefSyntax)crefSyntax).Parameters != null, 
			SyntaxKind.ConversionOperatorMemberCref => ((ConversionOperatorMemberCrefSyntax)crefSyntax).Parameters != null, 
			_ => false, 
		};
	}

	private static SymbolInfo GetCrefSymbolInfo(ImmutableArray<Symbol> symbols, SymbolInfoOptions options, bool hasParameterList)
	{
		switch (symbols.Length)
		{
		case 0:
			return SymbolInfo.None;
		case 1:
			return GetSymbolInfoForSymbol(symbols[0], options);
		default:
		{
			if ((options & SymbolInfoOptions.ResolveAliases) == SymbolInfoOptions.ResolveAliases)
			{
				symbols = UnwrapAliases(symbols);
			}
			LookupResultKind resultKind = LookupResultKind.Ambiguous;
			SymbolKind firstCandidateKind = symbols[0].Kind;
			if (hasParameterList && symbols.All((Symbol s) => s.Kind == firstCandidateKind))
			{
				resultKind = LookupResultKind.OverloadResolutionFailure;
			}
			return SymbolInfoFactory.Create(symbols, resultKind, isDynamic: false);
		}
		}
	}

	private BoundAttribute GetSpeculativelyBoundAttribute(int position, AttributeSyntax attribute, out Binder binder)
	{
		if (attribute == null)
		{
			throw new ArgumentNullException("attribute");
		}
		binder = GetSpeculativeBinderForAttribute(position, attribute);
		if (binder == null)
		{
			return null;
		}
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		NamedTypeSymbol attributeType = (NamedTypeSymbol)binder.BindType(attribute.Name, instance, out var _);
		BoundAttribute result = new ExecutableCodeBinder(attribute, binder.ContainingMemberOrLambda, binder).BindAttribute(attribute, attributeType, instance);
		instance.Free();
		return result;
	}

	private int CheckAndAdjustPositionForSpeculativeAttribute(int position)
	{
		position = CheckAndAdjustPosition(position);
		SyntaxToken syntaxToken = Root.FindToken(position);
		if (position == 0 && position != syntaxToken.SpanStart)
		{
			return position;
		}
		CSharpSyntaxNode cSharpSyntaxNode = (CSharpSyntaxNode)syntaxToken.Parent;
		if (position == cSharpSyntaxNode.SpanStart)
		{
			if (cSharpSyntaxNode is BaseTypeDeclarationSyntax { OpenBraceToken: var openBraceToken })
			{
				position = openBraceToken.SpanStart;
			}
			MethodDeclarationSyntax methodDeclarationSyntax = cSharpSyntaxNode.FirstAncestorOrSelf<MethodDeclarationSyntax>();
			if (methodDeclarationSyntax != null && methodDeclarationSyntax.SpanStart == position)
			{
				position = methodDeclarationSyntax.Identifier.SpanStart;
			}
		}
		return position;
	}

	protected override IOperation GetOperationCore(SyntaxNode node, CancellationToken cancellationToken)
	{
		CSharpSyntaxNode cSharpSyntaxNode = (CSharpSyntaxNode)node;
		CheckSyntaxNode(cSharpSyntaxNode);
		return GetOperationWorker(cSharpSyntaxNode, cancellationToken);
	}

	internal virtual IOperation GetOperationWorker(CSharpSyntaxNode node, CancellationToken cancellationToken)
	{
		return null;
	}

	public abstract SymbolInfo GetSymbolInfo(OrderingSyntax node, CancellationToken cancellationToken = default(CancellationToken));

	public abstract SymbolInfo GetSymbolInfo(SelectOrGroupClauseSyntax node, CancellationToken cancellationToken = default(CancellationToken));

	public SymbolInfo GetSymbolInfo(ExpressionSyntax expression, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(expression);
		if (!CanGetSemanticInfo(expression, allowNamedArgumentName: true))
		{
			return SymbolInfo.None;
		}
		if (SyntaxFacts.IsNamedArgumentName(expression))
		{
			return GetNamedArgumentSymbolInfo((IdentifierNameSyntax)expression, cancellationToken);
		}
		if (SyntaxFacts.IsDeclarationExpressionType(expression, out var parent))
		{
			switch (parent.Designation.Kind())
			{
			case SyntaxKind.SingleVariableDesignation:
				return GetSymbolInfoFromSymbolOrNone(TypeFromVariable((SingleVariableDesignationSyntax)parent.Designation, cancellationToken));
			case SyntaxKind.DiscardDesignation:
				return GetSymbolInfoFromSymbolOrNone(GetTypeInfoWorker(parent, cancellationToken).Type);
			case SyntaxKind.ParenthesizedVariableDesignation:
				if (((TypeSyntax)expression).IsVar)
				{
					return SymbolInfo.None;
				}
				break;
			}
		}
		else if (expression is DeclarationExpressionSyntax declarationExpressionSyntax)
		{
			if (declarationExpressionSyntax.Designation.Kind() != SyntaxKind.SingleVariableDesignation)
			{
				return SymbolInfo.None;
			}
			ISymbol declaredSymbol = GetDeclaredSymbol((SingleVariableDesignationSyntax)declarationExpressionSyntax.Designation, cancellationToken);
			if (declaredSymbol == null)
			{
				return SymbolInfo.None;
			}
			return new SymbolInfo(declaredSymbol);
		}
		return GetSymbolInfoWorker(expression, SymbolInfoOptions.DefaultOptions, cancellationToken);
	}

	private static SymbolInfo GetSymbolInfoFromSymbolOrNone(ITypeSymbol type)
	{
		if (type == null || type.Kind != SymbolKind.ErrorType)
		{
			return new SymbolInfo(type);
		}
		return SymbolInfo.None;
	}

	private ITypeSymbol TypeFromVariable(SingleVariableDesignationSyntax variableDesignation, CancellationToken cancellationToken)
	{
		ISymbol declaredSymbol = GetDeclaredSymbol(variableDesignation, cancellationToken);
		if (declaredSymbol != null)
		{
			switch (declaredSymbol.Kind)
			{
			case SymbolKind.Local:
				return ((ILocalSymbol)declaredSymbol).Type;
			case SymbolKind.Field:
				return ((IFieldSymbol)declaredSymbol).Type;
			}
		}
		return null;
	}

	public SymbolInfo GetCollectionInitializerSymbolInfo(ExpressionSyntax expression, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(expression);
		if (expression.Parent != null && expression.Parent.Kind() == SyntaxKind.CollectionInitializerExpression)
		{
			InitializerExpressionSyntax initializerExpressionSyntax = (InitializerExpressionSyntax)expression.Parent;
			while (initializerExpressionSyntax.Parent != null && initializerExpressionSyntax.Parent.Kind() == SyntaxKind.SimpleAssignmentExpression && ((AssignmentExpressionSyntax)initializerExpressionSyntax.Parent).Right == initializerExpressionSyntax && initializerExpressionSyntax.Parent.Parent != null && initializerExpressionSyntax.Parent.Parent.Kind() == SyntaxKind.ObjectInitializerExpression)
			{
				initializerExpressionSyntax = (InitializerExpressionSyntax)initializerExpressionSyntax.Parent.Parent;
			}
			if (initializerExpressionSyntax.Parent != null && initializerExpressionSyntax.Parent.Kind() == SyntaxKind.ObjectCreationExpression && ((ObjectCreationExpressionSyntax)initializerExpressionSyntax.Parent).Initializer == initializerExpressionSyntax && CanGetSemanticInfo(initializerExpressionSyntax.Parent))
			{
				return GetCollectionInitializerSymbolInfoWorker((InitializerExpressionSyntax)expression.Parent, expression, cancellationToken);
			}
		}
		return SymbolInfo.None;
	}

	public SymbolInfo GetSymbolInfo(ConstructorInitializerSyntax constructorInitializer, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(constructorInitializer);
		if (!CanGetSemanticInfo(constructorInitializer))
		{
			return SymbolInfo.None;
		}
		return GetSymbolInfoWorker(constructorInitializer, SymbolInfoOptions.DefaultOptions, cancellationToken);
	}

	public SymbolInfo GetSymbolInfo(AttributeSyntax attributeSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(attributeSyntax);
		if (!CanGetSemanticInfo(attributeSyntax))
		{
			return SymbolInfo.None;
		}
		return GetSymbolInfoWorker(attributeSyntax, SymbolInfoOptions.DefaultOptions, cancellationToken);
	}

	public SymbolInfo GetSymbolInfo(CrefSyntax crefSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(crefSyntax);
		if (!CanGetSemanticInfo(crefSyntax))
		{
			return SymbolInfo.None;
		}
		return GetSymbolInfoWorker(crefSyntax, SymbolInfoOptions.DefaultOptions, cancellationToken);
	}

	public SymbolInfo GetSpeculativeSymbolInfo(int position, ExpressionSyntax expression, SpeculativeBindingOption bindingOption)
	{
		if (!CanGetSemanticInfo(expression, allowNamedArgumentName: false, isSpeculative: true))
		{
			return SymbolInfo.None;
		}
		BoundNode speculativelyBoundExpression = GetSpeculativelyBoundExpression(position, expression, bindingOption, out var binder, out var crefSymbols);
		if (speculativelyBoundExpression == null)
		{
			if (!crefSymbols.IsDefault)
			{
				return GetCrefSymbolInfo(crefSymbols, SymbolInfoOptions.DefaultOptions, hasParameterList: false);
			}
			return SymbolInfo.None;
		}
		return GetSymbolInfoForNode(SymbolInfoOptions.DefaultOptions, speculativelyBoundExpression, speculativelyBoundExpression, null, binder);
	}

	public SymbolInfo GetSpeculativeSymbolInfo(int position, AttributeSyntax attribute)
	{
		BoundNode speculativelyBoundAttribute = GetSpeculativelyBoundAttribute(position, attribute, out var binder);
		if (speculativelyBoundAttribute == null)
		{
			return SymbolInfo.None;
		}
		return GetSymbolInfoForNode(SymbolInfoOptions.DefaultOptions, speculativelyBoundAttribute, speculativelyBoundAttribute, null, binder);
	}

	public SymbolInfo GetSpeculativeSymbolInfo(int position, ConstructorInitializerSyntax constructorInitializer)
	{
		position = CheckAndAdjustPosition(position);
		if (constructorInitializer == null)
		{
			throw new ArgumentNullException("constructorInitializer");
		}
		ConstructorInitializerSyntax constructorInitializerSyntax = Root.FindToken(position).Parent.AncestorsAndSelf().OfType<ConstructorInitializerSyntax>().FirstOrDefault();
		if (constructorInitializerSyntax == null)
		{
			return SymbolInfo.None;
		}
		MemberSemanticModel memberModel = GetMemberModel(constructorInitializerSyntax);
		if (memberModel == null)
		{
			return SymbolInfo.None;
		}
		Binder enclosingBinder = memberModel.GetEnclosingBinder(position);
		if (enclosingBinder != null)
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			enclosingBinder = new ExecutableCodeBinder(constructorInitializer, enclosingBinder.ContainingMemberOrLambda, enclosingBinder);
			BoundExpressionStatement boundExpressionStatement = enclosingBinder.BindConstructorInitializer(constructorInitializer, instance);
			SymbolInfo symbolInfoForNode = memberModel.GetSymbolInfoForNode(SymbolInfoOptions.DefaultOptions, boundExpressionStatement.Expression, boundExpressionStatement.Expression, null, enclosingBinder);
			instance.Free();
			return symbolInfoForNode;
		}
		return SymbolInfo.None;
	}

	public SymbolInfo GetSpeculativeSymbolInfo(int position, CrefSyntax cref, SymbolInfoOptions options = SymbolInfoOptions.DefaultOptions)
	{
		position = CheckAndAdjustPosition(position);
		return GetCrefSymbolInfo(position, cref, options, HasParameterList(cref));
	}

	public TypeInfo GetTypeInfo(ConstructorInitializerSyntax constructorInitializer, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(constructorInitializer);
		return CanGetSemanticInfo(constructorInitializer) ? GetTypeInfoWorker(constructorInitializer, cancellationToken) : CSharpTypeInfo.None;
	}

	public abstract TypeInfo GetTypeInfo(SelectOrGroupClauseSyntax node, CancellationToken cancellationToken = default(CancellationToken));

	public TypeInfo GetTypeInfo(ExpressionSyntax expression, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(expression);
		if (!CanGetSemanticInfo(expression))
		{
			return CSharpTypeInfo.None;
		}
		if (SyntaxFacts.IsDeclarationExpressionType(expression, out var parent))
		{
			switch (parent.Designation.Kind())
			{
			case SyntaxKind.SingleVariableDesignation:
			{
				TypeSymbol typeSymbol = (TypeSymbol)TypeFromVariable((SingleVariableDesignationSyntax)parent.Designation, cancellationToken);
				return new CSharpTypeInfo(typeSymbol, typeSymbol, Conversion.Identity);
			}
			case SyntaxKind.DiscardDesignation:
			{
				TypeSymbol type = GetTypeInfoWorker(parent, cancellationToken).Type;
				return new CSharpTypeInfo(type, type, Conversion.Identity);
			}
			case SyntaxKind.ParenthesizedVariableDesignation:
				if (((TypeSyntax)expression).IsVar)
				{
					return CSharpTypeInfo.None;
				}
				break;
			}
		}
		return GetTypeInfoWorker(expression, cancellationToken);
	}

	public TypeInfo GetTypeInfo(AttributeSyntax attributeSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(attributeSyntax);
		return CanGetSemanticInfo(attributeSyntax) ? GetTypeInfoWorker(attributeSyntax, cancellationToken) : CSharpTypeInfo.None;
	}

	public Conversion GetConversion(SyntaxNode expression, CancellationToken cancellationToken = default(CancellationToken))
	{
		CSharpSyntaxNode cSharpSyntaxNode = (CSharpSyntaxNode)expression;
		CheckSyntaxNode(cSharpSyntaxNode);
		return (CanGetSemanticInfo(cSharpSyntaxNode) ? GetTypeInfoWorker(cSharpSyntaxNode, cancellationToken) : CSharpTypeInfo.None).ImplicitConversion;
	}

	public TypeInfo GetSpeculativeTypeInfo(int position, ExpressionSyntax expression, SpeculativeBindingOption bindingOption)
	{
		return GetSpeculativeTypeInfoWorker(position, expression, bindingOption);
	}

	internal CSharpTypeInfo GetSpeculativeTypeInfoWorker(int position, ExpressionSyntax expression, SpeculativeBindingOption bindingOption)
	{
		if (!CanGetSemanticInfo(expression, allowNamedArgumentName: false, isSpeculative: true))
		{
			return CSharpTypeInfo.None;
		}
		BoundNode speculativelyBoundExpression = GetSpeculativelyBoundExpression(position, expression, bindingOption, out var _, out var crefSymbols);
		if (speculativelyBoundExpression == null)
		{
			if (crefSymbols.IsDefault || crefSymbols.Length != 1)
			{
				return CSharpTypeInfo.None;
			}
			return GetTypeInfoForSymbol(crefSymbols[0]);
		}
		return GetTypeInfoForNode(speculativelyBoundExpression, speculativelyBoundExpression, null);
	}

	public Conversion GetSpeculativeConversion(int position, ExpressionSyntax expression, SpeculativeBindingOption bindingOption)
	{
		return GetSpeculativeTypeInfoWorker(position, expression, bindingOption).ImplicitConversion;
	}

	public ImmutableArray<ISymbol> GetMemberGroup(ExpressionSyntax expression, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(expression);
		if (!CanGetSemanticInfo(expression))
		{
			return ImmutableArray<ISymbol>.Empty;
		}
		return StaticCast<ISymbol>.From(GetMemberGroupWorker(expression, SymbolInfoOptions.DefaultOptions, cancellationToken));
	}

	public ImmutableArray<ISymbol> GetMemberGroup(AttributeSyntax attribute, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(attribute);
		if (!CanGetSemanticInfo(attribute))
		{
			return ImmutableArray<ISymbol>.Empty;
		}
		return StaticCast<ISymbol>.From(GetMemberGroupWorker(attribute, SymbolInfoOptions.DefaultOptions, cancellationToken));
	}

	public ImmutableArray<ISymbol> GetMemberGroup(ConstructorInitializerSyntax initializer, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(initializer);
		if (!CanGetSemanticInfo(initializer))
		{
			return ImmutableArray<ISymbol>.Empty;
		}
		return StaticCast<ISymbol>.From(GetMemberGroupWorker(initializer, SymbolInfoOptions.DefaultOptions, cancellationToken));
	}

	public ImmutableArray<IPropertySymbol> GetIndexerGroup(ExpressionSyntax expression, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(expression);
		if (!CanGetSemanticInfo(expression))
		{
			return ImmutableArray<IPropertySymbol>.Empty;
		}
		return StaticCast<IPropertySymbol>.From(GetIndexerGroupWorker(expression, SymbolInfoOptions.DefaultOptions, cancellationToken));
	}

	public Optional<object> GetConstantValue(ExpressionSyntax expression, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(expression);
		if (!CanGetSemanticInfo(expression))
		{
			return default;
		}
		return GetConstantValueWorker(expression, cancellationToken);
	}

	public abstract QueryClauseInfo GetQueryClauseInfo(QueryClauseSyntax node, CancellationToken cancellationToken = default(CancellationToken));

	public IAliasSymbol GetAliasInfo(IdentifierNameSyntax nameSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(nameSyntax);
		if (!CanGetSemanticInfo(nameSyntax))
		{
			return null;
		}
		return GetSymbolInfoWorker(nameSyntax, SymbolInfoOptions.PreferTypeToConstructors | SymbolInfoOptions.PreserveAliases, cancellationToken).Symbol as AliasSymbol;
	}

	public IAliasSymbol GetSpeculativeAliasInfo(int position, IdentifierNameSyntax nameSyntax, SpeculativeBindingOption bindingOption)
	{
		BoundNode speculativelyBoundExpression = GetSpeculativelyBoundExpression(position, nameSyntax, bindingOption, out var binder, out var crefSymbols);
		if (speculativelyBoundExpression == null)
		{
			if (crefSymbols.IsDefault || crefSymbols.Length != 1)
			{
				return null;
			}
			return crefSymbols[0] as AliasSymbol;
		}
		return GetSymbolInfoForNode(SymbolInfoOptions.PreferTypeToConstructors | SymbolInfoOptions.PreserveAliases, speculativelyBoundExpression, speculativelyBoundExpression, null, binder).Symbol as AliasSymbol;
	}

	internal Binder GetEnclosingBinder(int position)
	{
		return GetEnclosingBinderInternal(position);
	}

	internal abstract Binder GetEnclosingBinderInternal(int position);

	internal abstract MemberSemanticModel GetMemberModel(SyntaxNode node);

	internal bool IsInTree(SyntaxNode node)
	{
		return node.SyntaxTree == SyntaxTree;
	}

	private static bool IsInStructuredTriviaOtherThanCrefOrNameAttribute(CSharpSyntaxNode node)
	{
		while (node != null)
		{
			if (node.Kind() == SyntaxKind.XmlCrefAttribute || node.Kind() == SyntaxKind.XmlNameAttribute)
			{
				return false;
			}
			if (node.IsStructuredTrivia)
			{
				return true;
			}
			node = node.ParentOrStructuredTriviaParent;
		}
		return false;
	}

	protected int CheckAndAdjustPosition(int position)
	{
		SyntaxToken token;
		return CheckAndAdjustPosition(position, out token);
	}

	protected int CheckAndAdjustPosition(int position, out SyntaxToken token)
	{
		int position2 = Root.Position;
		int end = Root.FullSpan.End;
		bool flag = position == end && position == SyntaxTree.GetRoot().FullSpan.End;
		if ((position2 <= position && position < end) | flag)
		{
			token = (flag ? ((CSharpSyntaxNode)SyntaxTree.GetRoot()) : Root).FindTokenIncludingCrefAndNameAttributes(position);
			if (position < token.SpanStart)
			{
				token = token.GetPreviousToken();
			}
			return Math.Max(token.SpanStart, position2);
		}
		if (position2 == end && position == end)
		{
			token = default;
			return position2;
		}
		throw new ArgumentOutOfRangeException("position", position, string.Format(CSharpResources.PositionIsNotWithinSyntax, Root.FullSpan));
	}

	protected int GetAdjustedNodePosition(SyntaxNode node)
	{
		TextSpan fullSpan = Root.FullSpan;
		int num = node.SpanStart;
		if (node is StatementSyntax)
		{
			int spanStart = node.GetFirstToken().SpanStart;
			if (spanStart < node.Span.End)
			{
				num = spanStart;
			}
		}
		if (fullSpan.IsEmpty)
		{
			return num;
		}
		if (num == fullSpan.End)
		{
			return CheckAndAdjustPosition(num - 1);
		}
		if (node.IsMissing || node.HasErrors || node.Width == 0 || node.IsPartOfStructuredTrivia())
		{
			return CheckAndAdjustPosition(num);
		}
		return num;
	}

	[Conditional("DEBUG")]
	protected void AssertPositionAdjusted(int position)
	{
	}

	protected void CheckSyntaxNode(CSharpSyntaxNode syntax)
	{
		if (syntax == null)
		{
			throw new ArgumentNullException("syntax");
		}
		if (!IsInTree(syntax))
		{
			throw new ArgumentException(CSharpResources.SyntaxNodeIsNotWithinSynt);
		}
	}

	private void CheckModelAndSyntaxNodeToSpeculate(CSharpSyntaxNode syntax)
	{
		if (syntax == null)
		{
			throw new ArgumentNullException("syntax");
		}
		if (IsSpeculativeSemanticModel)
		{
			throw new InvalidOperationException(CSharpResources.ChainingSpeculativeModelIsNotSupported);
		}
		if (Compilation.ContainsSyntaxTree(syntax.SyntaxTree))
		{
			throw new ArgumentException(CSharpResources.SpeculatedSyntaxNodeCannotBelongToCurrentCompilation);
		}
	}

	public new ImmutableArray<ISymbol> LookupSymbols(int position, INamespaceOrTypeSymbol container = null, string name = null, bool includeReducedExtensionMethods = false)
	{
		LookupOptions options = (includeReducedExtensionMethods ? LookupOptions.IncludeExtensionMethods : LookupOptions.Default);
		return StaticCast<ISymbol>.From(LookupSymbolsInternal(position, ToLanguageSpecific(container), name, options, useBaseReferenceAccessibility: false));
	}

	public new ImmutableArray<ISymbol> LookupBaseMembers(int position, string name = null)
	{
		return StaticCast<ISymbol>.From(LookupSymbolsInternal(position, null, name, LookupOptions.Default, useBaseReferenceAccessibility: true));
	}

	public new ImmutableArray<ISymbol> LookupStaticMembers(int position, INamespaceOrTypeSymbol container = null, string name = null)
	{
		return StaticCast<ISymbol>.From(LookupSymbolsInternal(position, ToLanguageSpecific(container), name, LookupOptions.MustNotBeInstance, useBaseReferenceAccessibility: false));
	}

	public new ImmutableArray<ISymbol> LookupNamespacesAndTypes(int position, INamespaceOrTypeSymbol container = null, string name = null)
	{
		return StaticCast<ISymbol>.From(LookupSymbolsInternal(position, ToLanguageSpecific(container), name, LookupOptions.NamespacesOrTypesOnly, useBaseReferenceAccessibility: false));
	}

	public new ImmutableArray<ISymbol> LookupLabels(int position, string name = null)
	{
		return StaticCast<ISymbol>.From(LookupSymbolsInternal(position, null, name, LookupOptions.LabelsOnly, useBaseReferenceAccessibility: false));
	}

	private ImmutableArray<Symbol> LookupSymbolsInternal(int position, NamespaceOrTypeSymbol container, string name, LookupOptions options, bool useBaseReferenceAccessibility)
	{
		if (useBaseReferenceAccessibility)
		{
			options |= LookupOptions.UseBaseReferenceAccessibility;
		}
		options.ThrowIfInvalid();
		position = CheckAndAdjustPosition(position, out var token);
		if ((object)container == null || container.Kind == SymbolKind.Namespace)
		{
			options &= ~LookupOptions.IncludeExtensionMethods;
		}
		Binder enclosingBinder = GetEnclosingBinder(position);
		if (enclosingBinder == null)
		{
			return ImmutableArray<Symbol>.Empty;
		}
		if (useBaseReferenceAccessibility)
		{
			TypeSymbol containingType = enclosingBinder.ContainingType;
			if ((object)containingType != null && containingType.Kind == SymbolKind.NamedType && ((NamedTypeSymbol)containingType).IsScriptClass)
			{
				return ImmutableArray<Symbol>.Empty;
			}
			TypeSymbol baseTypeNoUseSiteDiagnostics;
			if ((object)containingType == null || (object)(baseTypeNoUseSiteDiagnostics = containingType.BaseTypeNoUseSiteDiagnostics) == null)
			{
				throw new ArgumentException("Not a valid position for a call to LookupBaseMembers (must be in a type with a base type)", "position");
			}
			container = baseTypeNoUseSiteDiagnostics;
		}
		if (!enclosingBinder.IsInMethodBody && (options & (LookupOptions.NamespaceAliasesOnly | LookupOptions.NamespacesOrTypesOnly | LookupOptions.LabelsOnly)) == 0 && token.Parent is ExpressionSyntax expressionSyntax && !(expressionSyntax.Parent is XmlNameAttributeSyntax) && !SyntaxFacts.IsInTypeOnlyContext(expressionSyntax))
		{
			options |= LookupOptions.MustNotBeMethodTypeParameter;
		}
		LookupSymbolsInfo instance = LookupSymbolsInfo.GetInstance();
		instance.FilterName = name;
		if ((object)container == null)
		{
			enclosingBinder.AddLookupSymbolsInfo(instance, options);
		}
		else
		{
			enclosingBinder.AddMemberLookupSymbolsInfo(instance, container, options, enclosingBinder);
		}
		ArrayBuilder<Symbol> instance2 = ArrayBuilder<Symbol>.GetInstance(instance.Count);
		if (name == null)
		{
			foreach (string name2 in instance.Names)
			{
				AppendSymbolsWithName(instance2, name2, enclosingBinder, container, options, instance);
			}
		}
		else
		{
			AppendSymbolsWithName(instance2, name, enclosingBinder, container, options, instance);
		}
		instance.Free();
		if ((options & LookupOptions.IncludeExtensionMethods) != 0)
		{
			LookupResult instance3 = LookupResult.GetInstance();
			options |= LookupOptions.AllMethodsOnArityZero;
			options &= ~LookupOptions.MustBeInstance;
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			enclosingBinder.LookupExtensionMethods(instance3, name, 0, options, ref useSiteDiagnostics);
			if (instance3.IsMultiViable)
			{
				TypeSymbol receiverType = (TypeSymbol)container;
				foreach (MethodSymbol symbol in instance3.Symbols)
				{
					MethodSymbol methodSymbol = symbol.ReduceExtensionMethod(receiverType);
					if ((object)methodSymbol != null)
					{
						instance2.Add(methodSymbol);
					}
				}
			}
			instance3.Free();
		}
		ImmutableArray<Symbol> immutableArray = instance2.ToImmutableAndFree();
		if (name != null)
		{
			return immutableArray;
		}
		return FilterNotReferencable(immutableArray);
	}

	private void AppendSymbolsWithName(ArrayBuilder<Symbol> results, string name, Binder binder, NamespaceOrTypeSymbol container, LookupOptions options, LookupSymbolsInfo info)
	{
		if (!info.TryGetAritiesAndUniqueSymbol(name, out var arities, out var uniqueSymbol))
		{
			return;
		}
		if ((object)uniqueSymbol != null)
		{
			results.Add(uniqueSymbol);
			return;
		}
		if (arities != null)
		{
			foreach (int item in arities)
			{
				AppendSymbolsWithNameAndArity(results, name, item, binder, container, options);
			}
			return;
		}
		AppendSymbolsWithNameAndArity(results, name, 0, binder, container, options);
	}

	private void AppendSymbolsWithNameAndArity(ArrayBuilder<Symbol> results, string name, int arity, Binder binder, NamespaceOrTypeSymbol container, LookupOptions options)
	{
		LookupResult instance = LookupResult.GetInstance();
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		binder.LookupSymbolsSimpleName(instance, container, name, arity, null, options & ~LookupOptions.IncludeExtensionMethods, diagnose: false, ref useSiteDiagnostics);
		if (instance.IsMultiViable)
		{
			if (instance.Symbols.Any((Symbol t) => t.Kind == SymbolKind.NamedType || t.Kind == SymbolKind.Namespace || t.Kind == SymbolKind.ErrorType))
			{
				DiagnosticBag instance2 = DiagnosticBag.GetInstance();
				Symbol item = binder.ResultSymbol(instance, name, arity, Root, instance2, suppressUseSiteDiagnostics: true, out var wasError, container, options);
				instance2.Free();
				if (!wasError)
				{
					results.Add(item);
				}
				else
				{
					results.AddRange(instance.Symbols);
				}
			}
			else
			{
				results.AddRange(instance.Symbols);
			}
		}
		instance.Free();
	}

	private static ImmutableArray<Symbol> FilterNotReferencable(ImmutableArray<Symbol> sealedResults)
	{
		ArrayBuilder<Symbol> arrayBuilder = null;
		int num = 0;
		foreach (Symbol item in sealedResults)
		{
			if (item.CanBeReferencedByName)
			{
				arrayBuilder?.Add(item);
			}
			else if (arrayBuilder == null)
			{
				arrayBuilder = ArrayBuilder<Symbol>.GetInstance();
				arrayBuilder.AddRange(sealedResults, num);
			}
			num++;
		}
		return arrayBuilder?.ToImmutableAndFree() ?? sealedResults;
	}

	public new bool IsAccessible(int position, ISymbol symbol)
	{
		position = CheckAndAdjustPosition(position);
		if (symbol == null)
		{
			throw new ArgumentNullException("symbol");
		}
		Symbol symbol2 = symbol.EnsureCSharpSymbolOrNull<ISymbol, Symbol>("symbol");
		Binder enclosingBinder = GetEnclosingBinder(position);
		if (enclosingBinder != null)
		{
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			return enclosingBinder.IsAccessible(symbol2, ref useSiteDiagnostics);
		}
		return false;
	}

	public new bool IsEventUsableAsField(int position, IEventSymbol eventSymbol)
	{
		EventSymbol eventSymbol2 = (EventSymbol)eventSymbol;
		if (eventSymbol != null && eventSymbol2.HasAssociatedField)
		{
			return IsAccessible(position, eventSymbol2.AssociatedField);
		}
		return false;
	}

	private bool IsInTypeofExpression(int position)
	{
		for (SyntaxNode syntaxNode = Root.FindToken(position).Parent; syntaxNode != Root; syntaxNode = syntaxNode.ParentOrStructuredTriviaParent)
		{
			if (syntaxNode.IsKind(SyntaxKind.TypeOfExpression))
			{
				return true;
			}
		}
		return false;
	}

	internal SymbolInfo GetSymbolInfoForNode(SymbolInfoOptions options, BoundNode lowestBoundNode, BoundNode highestBoundNode, BoundNode boundNodeForSyntacticParent, Binder binderOpt)
	{
		if (!(lowestBoundNode is BoundExpression boundNode))
		{
			return SymbolInfo.None;
		}
		ImmutableArray<Symbol> immutableArray = GetSemanticSymbols(boundNode, boundNodeForSyntacticParent, binderOpt, options, out var isDynamic, out var resultKind, out var _);
		if (highestBoundNode is BoundExpression boundExpression)
		{
			ImmutableArray<Symbol> semanticSymbols = GetSemanticSymbols(boundExpression, boundNodeForSyntacticParent, binderOpt, options, out var isDynamic2, out var resultKind2, out var _);
			if ((immutableArray.Length != 1 || resultKind == LookupResultKind.OverloadResolutionFailure) && semanticSymbols.Length > 0)
			{
				immutableArray = semanticSymbols;
				resultKind = resultKind2;
				isDynamic = isDynamic2;
			}
			else if (resultKind2 != LookupResultKind.Empty && (int)resultKind2 < (int)resultKind)
			{
				resultKind = resultKind2;
				isDynamic = isDynamic2;
			}
			else if (boundExpression.Kind == BoundKind.TypeOrValueExpression)
			{
				immutableArray = semanticSymbols;
				resultKind = resultKind2;
				isDynamic = isDynamic2;
			}
			else if (boundExpression.Kind == BoundKind.UnaryOperator && IsUserDefinedTrueOrFalse((BoundUnaryOperator)boundExpression))
			{
				immutableArray = semanticSymbols;
				resultKind = resultKind2;
				isDynamic = isDynamic2;
			}
		}
		if (resultKind == LookupResultKind.Empty)
		{
			return SymbolInfoFactory.Create(ImmutableArray<Symbol>.Empty, LookupResultKind.Empty, isDynamic);
		}
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(immutableArray.Length);
		foreach (Symbol item in immutableArray)
		{
			AddUnwrappingErrorTypes(instance, item);
		}
		immutableArray = instance.ToImmutableAndFree();
		if ((options & SymbolInfoOptions.ResolveAliases) != 0)
		{
			immutableArray = UnwrapAliases(immutableArray);
		}
		if (resultKind == LookupResultKind.Viable && immutableArray.Length > 1)
		{
			resultKind = LookupResultKind.OverloadResolutionFailure;
		}
		return SymbolInfoFactory.Create(immutableArray, resultKind, isDynamic);
	}

	private static void AddUnwrappingErrorTypes(ArrayBuilder<Symbol> builder, Symbol s)
	{
		if (s.OriginalDefinition is ErrorTypeSymbol errorTypeSymbol)
		{
			builder.AddRange(errorTypeSymbol.CandidateSymbols);
		}
		else
		{
			builder.Add(s);
		}
	}

	private static bool IsUserDefinedTrueOrFalse(BoundUnaryOperator @operator)
	{
		UnaryOperatorKind operatorKind = @operator.OperatorKind;
		if (operatorKind != UnaryOperatorKind.UserDefinedTrue)
		{
			return operatorKind == UnaryOperatorKind.UserDefinedFalse;
		}
		return true;
	}

	internal CSharpTypeInfo GetTypeInfoForNode(BoundNode lowestBoundNode, BoundNode highestBoundNode, BoundNode boundNodeForSyntacticParent)
	{
		BoundExpression boundExpression = lowestBoundNode as BoundExpression;
		BoundExpression boundExpression2 = highestBoundNode as BoundExpression;
		if (boundExpression != null && (boundNodeForSyntacticParent == null || boundNodeForSyntacticParent.Syntax.Kind() != SyntaxKind.ObjectCreationExpression || ((ObjectCreationExpressionSyntax)boundNodeForSyntacticParent.Syntax).Type != boundExpression.Syntax))
		{
			TypeSymbol typeSymbol = null;
			if (boundExpression.HasExpressionType())
			{
				typeSymbol = boundExpression.Type;
				switch (boundExpression.Kind)
				{
				case BoundKind.Local:
					if (typeSymbol is ExtendedErrorTypeSymbol { VariableUsedBeforeDeclaration: not false })
					{
						typeSymbol = ((BoundLocal)boundExpression).LocalSymbol.Type;
					}
					break;
				case BoundKind.ConvertedTupleLiteral:
					typeSymbol = ((BoundConvertedTupleLiteral)boundExpression).NaturalTypeOpt;
					break;
				}
			}
			BoundKind boundKind = boundExpression2?.Kind ?? BoundKind.NoOpStatement;
			TypeSymbol typeSymbol2;
			Conversion implicitConversion;
			if (boundKind == BoundKind.Lambda)
			{
				BoundLambda boundLambda = (BoundLambda)boundExpression2;
				typeSymbol2 = boundLambda.Type;
				typeSymbol = null;
				implicitConversion = new Conversion(ConversionKind.AnonymousFunction, boundLambda.Symbol, isExtensionMethod: false);
			}
			else
			{
				BoundConversion obj = boundExpression2 as BoundConversion;
				if (obj != null && obj.Conversion.IsTupleLiteralConversion)
				{
					BoundConversion boundConversion = (BoundConversion)boundExpression2;
					typeSymbol = ((boundConversion.Operand.Kind != BoundKind.ConvertedTupleLiteral) ? boundConversion.Operand.Type : ((BoundConvertedTupleLiteral)boundConversion.Operand).NaturalTypeOpt);
					typeSymbol2 = boundConversion.Type;
					implicitConversion = boundConversion.Conversion;
				}
				else if (boundKind == BoundKind.FixedLocalCollectionInitializer)
				{
					BoundFixedLocalCollectionInitializer boundFixedLocalCollectionInitializer = (BoundFixedLocalCollectionInitializer)boundExpression2;
					typeSymbol2 = boundFixedLocalCollectionInitializer.Type;
					typeSymbol = boundFixedLocalCollectionInitializer.Expression.Type;
					implicitConversion = boundFixedLocalCollectionInitializer.ElementPointerTypeConversion;
				}
				else if (boundExpression2 != null && boundExpression2 != boundExpression && boundExpression2.HasExpressionType())
				{
					typeSymbol2 = boundExpression2.Type;
					if (boundKind != BoundKind.Conversion)
					{
						implicitConversion = Conversion.Identity;
					}
					else if (((BoundConversion)boundExpression2).Operand.Kind != BoundKind.Conversion)
					{
						implicitConversion = boundExpression2.GetConversion();
						if (implicitConversion.Kind == ConversionKind.AnonymousFunction)
						{
							typeSymbol = null;
						}
					}
					else
					{
						Binder enclosingBinder = GetEnclosingBinder(boundExpression.Syntax.Span.Start);
						HashSet<DiagnosticInfo> useSiteDiagnostics = null;
						implicitConversion = enclosingBinder.Conversions.ClassifyConversionFromExpression(boundExpression, typeSymbol2, ref useSiteDiagnostics);
					}
				}
				else if (boundNodeForSyntacticParent != null && boundNodeForSyntacticParent.Kind == BoundKind.DelegateCreationExpression)
				{
					BoundDelegateCreationExpression boundDelegateCreationExpression = (BoundDelegateCreationExpression)boundNodeForSyntacticParent;
					typeSymbol2 = boundDelegateCreationExpression.Type;
					switch (boundExpression.Kind)
					{
					case BoundKind.MethodGroup:
						implicitConversion = new Conversion(ConversionKind.MethodGroup, boundDelegateCreationExpression.MethodOpt, boundDelegateCreationExpression.IsExtensionMethod);
						break;
					case BoundKind.Lambda:
					{
						BoundLambda boundLambda3 = (BoundLambda)boundExpression;
						implicitConversion = new Conversion(ConversionKind.AnonymousFunction, boundLambda3.Symbol, boundDelegateCreationExpression.IsExtensionMethod);
						break;
					}
					case BoundKind.UnboundLambda:
					{
						BoundLambda boundLambda2 = ((UnboundLambda)boundExpression).BindForErrorRecovery();
						implicitConversion = new Conversion(ConversionKind.AnonymousFunction, boundLambda2.Symbol, boundDelegateCreationExpression.IsExtensionMethod);
						break;
					}
					default:
						implicitConversion = Conversion.Identity;
						break;
					}
				}
				else
				{
					typeSymbol2 = typeSymbol;
					implicitConversion = Conversion.Identity;
				}
			}
			return new CSharpTypeInfo(typeSymbol, typeSymbol2, implicitConversion);
		}
		return CSharpTypeInfo.None;
	}

	internal ImmutableArray<Symbol> GetMemberGroupForNode(SymbolInfoOptions options, BoundNode lowestBoundNode, BoundNode boundNodeForSyntacticParent, Binder binderOpt)
	{
		if (lowestBoundNode is BoundExpression boundNode)
		{
			GetSemanticSymbols(boundNode, boundNodeForSyntacticParent, binderOpt, options, out var _, out var _, out var memberGroup);
			return memberGroup;
		}
		return ImmutableArray<Symbol>.Empty;
	}

	internal ImmutableArray<PropertySymbol> GetIndexerGroupForNode(BoundNode lowestBoundNode, Binder binderOpt)
	{
		if (lowestBoundNode is BoundExpression { Kind: not BoundKind.TypeExpression } boundExpression)
		{
			return GetIndexerGroupSemanticSymbols(boundExpression, binderOpt);
		}
		return ImmutableArray<PropertySymbol>.Empty;
	}

	internal static SymbolInfo GetSymbolInfoForSymbol(Symbol symbol, SymbolInfoOptions options)
	{
		Symbol symbol2 = UnwrapAlias(symbol);
		ErrorTypeSymbol errorTypeSymbol = ((symbol2 is TypeSymbol typeSymbol) ? (typeSymbol.OriginalDefinition as ErrorTypeSymbol) : null);
		if ((object)errorTypeSymbol != null)
		{
			ImmutableArray<Symbol> symbols = ImmutableArray<Symbol>.Empty;
			LookupResultKind resultKind = errorTypeSymbol.ResultKind;
			if (resultKind != LookupResultKind.Empty)
			{
				symbols = errorTypeSymbol.CandidateSymbols;
			}
			if ((options & SymbolInfoOptions.ResolveAliases) != 0)
			{
				symbols = UnwrapAliases(symbols);
			}
			return SymbolInfoFactory.Create(symbols, resultKind, isDynamic: false);
		}
		return new SymbolInfo(((options & SymbolInfoOptions.ResolveAliases) != 0) ? symbol2 : symbol, ImmutableArray<ISymbol>.Empty, CandidateReason.None);
	}

	internal static CSharpTypeInfo GetTypeInfoForSymbol(Symbol symbol)
	{
		TypeSymbol obj = UnwrapAlias(symbol) as TypeSymbol;
		return new CSharpTypeInfo(obj, obj, Conversion.Identity);
	}

	protected static Symbol UnwrapAlias(Symbol symbol)
	{
		if (!(symbol is AliasSymbol aliasSymbol))
		{
			return symbol;
		}
		return aliasSymbol.Target;
	}

	protected static ImmutableArray<Symbol> UnwrapAliases(ImmutableArray<Symbol> symbols)
	{
		bool flag = false;
		foreach (Symbol item in symbols)
		{
			if (item.Kind == SymbolKind.Alias)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return symbols;
		}
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		foreach (Symbol item2 in symbols)
		{
			AddUnwrappingErrorTypes(instance, UnwrapAlias(item2));
		}
		return instance.ToImmutableAndFree();
	}

	internal virtual BoundNode Bind(Binder binder, CSharpSyntaxNode node, DiagnosticBag diagnostics)
	{
		if (node != null)
		{
			if (node is ExpressionSyntax expressionSyntax)
			{
				ExpressionSyntax expressionSyntax2 = expressionSyntax;
				if (!expressionSyntax2.Parent.IsKind(SyntaxKind.GotoStatement))
				{
					return binder.BindNamespaceOrTypeOrExpression(expressionSyntax2, diagnostics);
				}
				return binder.BindLabel(expressionSyntax2, diagnostics);
			}
			if (node is StatementSyntax statementSyntax)
			{
				StatementSyntax node2 = statementSyntax;
				return binder.BindStatement(node2, diagnostics);
			}
			if (node is GlobalStatementSyntax globalStatementSyntax)
			{
				GlobalStatementSyntax globalStatementSyntax2 = globalStatementSyntax;
				BoundStatement statement = binder.BindStatement(globalStatementSyntax2.Statement, diagnostics);
				return new BoundGlobalStatementInitializer(node, statement);
			}
		}
		return null;
	}

	public virtual ControlFlowAnalysis AnalyzeControlFlow(StatementSyntax firstStatement, StatementSyntax lastStatement)
	{
		throw new NotSupportedException();
	}

	public virtual ControlFlowAnalysis AnalyzeControlFlow(StatementSyntax statement)
	{
		return AnalyzeControlFlow(statement, statement);
	}

	public virtual DataFlowAnalysis AnalyzeDataFlow(ExpressionSyntax expression)
	{
		throw new NotSupportedException();
	}

	public virtual DataFlowAnalysis AnalyzeDataFlow(StatementSyntax firstStatement, StatementSyntax lastStatement)
	{
		throw new NotSupportedException();
	}

	public virtual DataFlowAnalysis AnalyzeDataFlow(StatementSyntax statement)
	{
		return AnalyzeDataFlow(statement, statement);
	}

	public bool TryGetSpeculativeSemanticModelForMethodBody(int position, BaseMethodDeclarationSyntax method, out SemanticModel speculativeModel)
	{
		CheckModelAndSyntaxNodeToSpeculate(method);
		return TryGetSpeculativeSemanticModelForMethodBodyCore((SyntaxTreeSemanticModel)this, position, method, out speculativeModel);
	}

	internal abstract bool TryGetSpeculativeSemanticModelForMethodBodyCore(SyntaxTreeSemanticModel parentModel, int position, BaseMethodDeclarationSyntax method, out SemanticModel speculativeModel);

	public bool TryGetSpeculativeSemanticModelForMethodBody(int position, AccessorDeclarationSyntax accessor, out SemanticModel speculativeModel)
	{
		CheckModelAndSyntaxNodeToSpeculate(accessor);
		return TryGetSpeculativeSemanticModelForMethodBodyCore((SyntaxTreeSemanticModel)this, position, accessor, out speculativeModel);
	}

	internal abstract bool TryGetSpeculativeSemanticModelForMethodBodyCore(SyntaxTreeSemanticModel parentModel, int position, AccessorDeclarationSyntax accessor, out SemanticModel speculativeModel);

	public bool TryGetSpeculativeSemanticModel(int position, TypeSyntax type, out SemanticModel speculativeModel, SpeculativeBindingOption bindingOption = SpeculativeBindingOption.BindAsExpression)
	{
		CheckModelAndSyntaxNodeToSpeculate(type);
		return TryGetSpeculativeSemanticModelCore((SyntaxTreeSemanticModel)this, position, type, bindingOption, out speculativeModel);
	}

	internal abstract bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, TypeSyntax type, SpeculativeBindingOption bindingOption, out SemanticModel speculativeModel);

	public bool TryGetSpeculativeSemanticModel(int position, StatementSyntax statement, out SemanticModel speculativeModel)
	{
		CheckModelAndSyntaxNodeToSpeculate(statement);
		return TryGetSpeculativeSemanticModelCore((SyntaxTreeSemanticModel)this, position, statement, out speculativeModel);
	}

	internal abstract bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, StatementSyntax statement, out SemanticModel speculativeModel);

	public bool TryGetSpeculativeSemanticModel(int position, EqualsValueClauseSyntax initializer, out SemanticModel speculativeModel)
	{
		CheckModelAndSyntaxNodeToSpeculate(initializer);
		return TryGetSpeculativeSemanticModelCore((SyntaxTreeSemanticModel)this, position, initializer, out speculativeModel);
	}

	internal abstract bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, EqualsValueClauseSyntax initializer, out SemanticModel speculativeModel);

	public bool TryGetSpeculativeSemanticModel(int position, ArrowExpressionClauseSyntax expressionBody, out SemanticModel speculativeModel)
	{
		CheckModelAndSyntaxNodeToSpeculate(expressionBody);
		return TryGetSpeculativeSemanticModelCore((SyntaxTreeSemanticModel)this, position, expressionBody, out speculativeModel);
	}

	internal abstract bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, ArrowExpressionClauseSyntax expressionBody, out SemanticModel speculativeModel);

	public bool TryGetSpeculativeSemanticModel(int position, ConstructorInitializerSyntax constructorInitializer, out SemanticModel speculativeModel)
	{
		CheckModelAndSyntaxNodeToSpeculate(constructorInitializer);
		return TryGetSpeculativeSemanticModelCore((SyntaxTreeSemanticModel)this, position, constructorInitializer, out speculativeModel);
	}

	internal abstract bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, ConstructorInitializerSyntax constructorInitializer, out SemanticModel speculativeModel);

	public bool TryGetSpeculativeSemanticModel(int position, CrefSyntax crefSyntax, out SemanticModel speculativeModel)
	{
		CheckModelAndSyntaxNodeToSpeculate(crefSyntax);
		return TryGetSpeculativeSemanticModelCore((SyntaxTreeSemanticModel)this, position, crefSyntax, out speculativeModel);
	}

	internal abstract bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, CrefSyntax crefSyntax, out SemanticModel speculativeModel);

	public bool TryGetSpeculativeSemanticModel(int position, AttributeSyntax attribute, out SemanticModel speculativeModel)
	{
		CheckModelAndSyntaxNodeToSpeculate(attribute);
		Binder speculativeBinderForAttribute = GetSpeculativeBinderForAttribute(position, attribute);
		if (speculativeBinderForAttribute == null)
		{
			speculativeModel = null;
			return false;
		}
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		NamedTypeSymbol attributeType = (NamedTypeSymbol)speculativeBinderForAttribute.BindType(attribute.Name, instance, out var alias);
		instance.Free();
		speculativeModel = AttributeSemanticModel.CreateSpeculative((SyntaxTreeSemanticModel)this, attribute, attributeType, alias, speculativeBinderForAttribute, position);
		return true;
	}

	public abstract Conversion ClassifyConversion(ExpressionSyntax expression, ITypeSymbol destination, bool isExplicitInSource = false);

	public Conversion ClassifyConversion(int position, ExpressionSyntax expression, ITypeSymbol destination, bool isExplicitInSource = false)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		TypeSymbol typeSymbol = destination.EnsureCSharpSymbolOrNull<ITypeSymbol, TypeSymbol>("destination");
		if (expression.Kind() == SyntaxKind.DeclarationExpression)
		{
			return Conversion.NoConversion;
		}
		if (isExplicitInSource)
		{
			return ClassifyConversionForCast(position, expression, typeSymbol);
		}
		position = CheckAndAdjustPosition(position);
		Binder enclosingBinder = GetEnclosingBinder(position);
		if (enclosingBinder != null)
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			BoundExpression boundExpression = enclosingBinder.BindExpression(expression, instance);
			instance.Free();
			if (boundExpression != null && !typeSymbol.IsErrorType())
			{
				HashSet<DiagnosticInfo> useSiteDiagnostics = null;
				return enclosingBinder.Conversions.ClassifyConversionFromExpression(boundExpression, typeSymbol, ref useSiteDiagnostics);
			}
		}
		return Conversion.NoConversion;
	}

	internal abstract Conversion ClassifyConversionForCast(ExpressionSyntax expression, TypeSymbol destination);

	internal Conversion ClassifyConversionForCast(int position, ExpressionSyntax expression, TypeSymbol destination)
	{
		if ((object)destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		position = CheckAndAdjustPosition(position);
		Binder enclosingBinder = GetEnclosingBinder(position);
		if (enclosingBinder != null)
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			BoundExpression boundExpression = enclosingBinder.BindExpression(expression, instance);
			instance.Free();
			if (boundExpression != null && !destination.IsErrorType())
			{
				HashSet<DiagnosticInfo> useSiteDiagnostics = null;
				return enclosingBinder.Conversions.ClassifyConversionFromExpression(boundExpression, destination, ref useSiteDiagnostics, forCast: true);
			}
		}
		return Conversion.NoConversion;
	}

	public abstract ISymbol GetDeclaredSymbol(MemberDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract ISymbol GetDeclaredSymbol(LocalFunctionStatementSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract INamespaceSymbol GetDeclaredSymbol(NamespaceDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract INamedTypeSymbol GetDeclaredSymbol(BaseTypeDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract INamedTypeSymbol GetDeclaredSymbol(DelegateDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IFieldSymbol GetDeclaredSymbol(EnumMemberDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IMethodSymbol GetDeclaredSymbol(BaseMethodDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract ISymbol GetDeclaredSymbol(BasePropertyDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IPropertySymbol GetDeclaredSymbol(PropertyDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IPropertySymbol GetDeclaredSymbol(IndexerDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IEventSymbol GetDeclaredSymbol(EventDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IPropertySymbol GetDeclaredSymbol(AnonymousObjectMemberDeclaratorSyntax declaratorSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract INamedTypeSymbol GetDeclaredSymbol(AnonymousObjectCreationExpressionSyntax declaratorSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract INamedTypeSymbol GetDeclaredSymbol(TupleExpressionSyntax declaratorSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract ISymbol GetDeclaredSymbol(ArgumentSyntax declaratorSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IMethodSymbol GetDeclaredSymbol(AccessorDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IMethodSymbol GetDeclaredSymbol(ArrowExpressionClauseSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract ISymbol GetDeclaredSymbol(VariableDeclaratorSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract ISymbol GetDeclaredSymbol(SingleVariableDesignationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract ILabelSymbol GetDeclaredSymbol(LabeledStatementSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract ILabelSymbol GetDeclaredSymbol(SwitchLabelSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IAliasSymbol GetDeclaredSymbol(UsingDirectiveSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IAliasSymbol GetDeclaredSymbol(ExternAliasDirectiveSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IParameterSymbol GetDeclaredSymbol(ParameterSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	internal abstract ImmutableArray<ISymbol> GetDeclaredSymbols(BaseFieldDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	protected ParameterSymbol GetParameterSymbol(ImmutableArray<ParameterSymbol> parameters, ParameterSyntax parameter, CancellationToken cancellationToken = default(CancellationToken))
	{
		foreach (ParameterSymbol item in parameters)
		{
			cancellationToken.ThrowIfCancellationRequested();
			foreach (Location location in item.Locations)
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (location.SourceTree == SyntaxTree && parameter.Span.Contains(location.SourceSpan))
				{
					return item;
				}
			}
		}
		return null;
	}

	public abstract ITypeParameterSymbol GetDeclaredSymbol(TypeParameterSyntax typeParameter, CancellationToken cancellationToken = default(CancellationToken));

	internal BinderFlags GetSemanticModelBinderFlags()
	{
		if (!IgnoresAccessibility)
		{
			return BinderFlags.SemanticModel;
		}
		return BinderFlags.SemanticModel | BinderFlags.IgnoreAccessibility;
	}

	public ILocalSymbol GetDeclaredSymbol(ForEachStatementSyntax forEachStatement, CancellationToken cancellationToken = default(CancellationToken))
	{
		Binder enclosingBinder = GetEnclosingBinder(GetAdjustedNodePosition(forEachStatement));
		if (enclosingBinder == null)
		{
			return null;
		}
		Binder binder = enclosingBinder.GetBinder(forEachStatement);
		if (binder == null)
		{
			return null;
		}
		LocalSymbol localSymbol = binder.GetDeclaredLocalsForScope(forEachStatement).FirstOrDefault();
		if ((object)localSymbol == null || localSymbol.DeclarationKind != LocalDeclarationKind.ForEachIterationVariable)
		{
			return null;
		}
		return localSymbol;
	}

	public ILocalSymbol GetDeclaredSymbol(CatchDeclarationSyntax catchDeclaration, CancellationToken cancellationToken = default(CancellationToken))
	{
		CSharpSyntaxNode parent = catchDeclaration.Parent;
		Binder enclosingBinder = GetEnclosingBinder(GetAdjustedNodePosition(parent));
		if (enclosingBinder == null)
		{
			return null;
		}
		if (enclosingBinder.GetBinder(parent) == null)
		{
			return null;
		}
		LocalSymbol localSymbol = enclosingBinder.GetBinder(parent).GetDeclaredLocalsForScope(parent).FirstOrDefault();
		if ((object)localSymbol == null || localSymbol.DeclarationKind != LocalDeclarationKind.CatchVariable)
		{
			return null;
		}
		return localSymbol;
	}

	public abstract IRangeVariableSymbol GetDeclaredSymbol(QueryClauseSyntax queryClause, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IRangeVariableSymbol GetDeclaredSymbol(JoinIntoClauseSyntax node, CancellationToken cancellationToken = default(CancellationToken));

	public abstract IRangeVariableSymbol GetDeclaredSymbol(QueryContinuationSyntax node, CancellationToken cancellationToken = default(CancellationToken));

	private ImmutableArray<Symbol> GetSemanticSymbols(BoundExpression boundNode, BoundNode boundNodeForSyntacticParent, Binder binderOpt, SymbolInfoOptions options, out bool isDynamic, out LookupResultKind resultKind, out ImmutableArray<Symbol> memberGroup)
	{
		memberGroup = ImmutableArray<Symbol>.Empty;
		ImmutableArray<Symbol> symbols = ImmutableArray<Symbol>.Empty;
		resultKind = LookupResultKind.Viable;
		isDynamic = false;
		switch (boundNode.Kind)
		{
		case BoundKind.MethodGroup:
			symbols = GetMethodGroupSemanticSymbols((BoundMethodGroup)boundNode, boundNodeForSyntacticParent, binderOpt, out resultKind, out isDynamic, out memberGroup);
			break;
		case BoundKind.PropertyGroup:
			symbols = GetPropertyGroupSemanticSymbols((BoundPropertyGroup)boundNode, boundNodeForSyntacticParent, binderOpt, out resultKind, out memberGroup);
			break;
		case BoundKind.BadExpression:
		{
			BoundBadExpression boundBadExpression = (BoundBadExpression)boundNode;
			resultKind = boundBadExpression.ResultKind;
			if (boundBadExpression.Syntax.Kind() == SyntaxKind.ObjectCreationExpression)
			{
				if (resultKind == LookupResultKind.NotCreatable)
				{
					return boundBadExpression.Symbols;
				}
				if (boundBadExpression.Type.IsDelegateType())
				{
					resultKind = LookupResultKind.Empty;
					return symbols;
				}
				memberGroup = boundBadExpression.Symbols;
			}
			return boundBadExpression.Symbols;
		}
		case BoundKind.TypeExpression:
		{
			BoundTypeExpression boundTypeExpression = (BoundTypeExpression)boundNode;
			if (boundNodeForSyntacticParent != null && boundNodeForSyntacticParent.Syntax.Kind() == SyntaxKind.ObjectCreationExpression && ((ObjectCreationExpressionSyntax)boundNodeForSyntacticParent.Syntax).Type == boundTypeExpression.Syntax && boundNodeForSyntacticParent.Kind == BoundKind.BadExpression && ((BoundBadExpression)boundNodeForSyntacticParent).ResultKind == LookupResultKind.NotCreatable)
			{
				resultKind = LookupResultKind.NotCreatable;
			}
			Symbol symbol = (Symbol)(((object)boundTypeExpression.AliasOpt) ?? ((object)boundTypeExpression.Type));
			if (symbol.OriginalDefinition is ErrorTypeSymbol errorTypeSymbol)
			{
				resultKind = errorTypeSymbol.ResultKind;
				symbols = errorTypeSymbol.CandidateSymbols;
			}
			else
			{
				symbols = ImmutableArray.Create(symbol);
			}
			break;
		}
		case BoundKind.TypeOrValueExpression:
		{
			BoundExpression valueExpression = ((BoundTypeOrValueExpression)boundNode).Data.ValueExpression;
			return GetSemanticSymbols(valueExpression, boundNodeForSyntacticParent, binderOpt, options, out isDynamic, out resultKind, out memberGroup);
		}
		case BoundKind.Call:
		{
			BoundCall boundCall = (BoundCall)boundNode;
			if (boundCall.OriginalMethodsOpt.IsDefault)
			{
				if ((object)boundCall.Method != null)
				{
					symbols = CreateReducedExtensionMethodIfPossible(boundCall);
					resultKind = boundCall.ResultKind;
				}
			}
			else
			{
				symbols = StaticCast<Symbol>.From(CreateReducedExtensionMethodsFromOriginalsIfNecessary(boundCall));
				resultKind = boundCall.ResultKind;
			}
			break;
		}
		case BoundKind.IndexerAccess:
		{
			BoundIndexerAccess boundIndexerAccess = (BoundIndexerAccess)boundNode;
			resultKind = boundIndexerAccess.ResultKind;
			ImmutableArray<PropertySymbol> originalIndexersOpt = boundIndexerAccess.OriginalIndexersOpt;
			symbols = (originalIndexersOpt.IsDefault ? ImmutableArray.Create((Symbol)boundIndexerAccess.Indexer) : StaticCast<Symbol>.From(originalIndexersOpt));
			break;
		}
		case BoundKind.EventAssignmentOperator:
		{
			BoundEventAssignmentOperator boundEventAssignmentOperator = (BoundEventAssignmentOperator)boundNode;
			isDynamic = boundEventAssignmentOperator.IsDynamic;
			EventSymbol eventSymbol = boundEventAssignmentOperator.Event;
			MethodSymbol methodSymbol = (boundEventAssignmentOperator.IsAddition ? eventSymbol.AddMethod : eventSymbol.RemoveMethod);
			if ((object)methodSymbol == null)
			{
				symbols = ImmutableArray<Symbol>.Empty;
				resultKind = LookupResultKind.Empty;
			}
			else
			{
				symbols = ImmutableArray.Create((Symbol)methodSymbol);
				resultKind = boundEventAssignmentOperator.ResultKind;
			}
			break;
		}
		case BoundKind.Conversion:
		{
			BoundConversion boundConversion = (BoundConversion)boundNode;
			isDynamic = boundConversion.ConversionKind.IsDynamic();
			if (isDynamic)
			{
				break;
			}
			if (boundConversion.ConversionKind == ConversionKind.MethodGroup && boundConversion.IsExtensionMethod)
			{
				symbols = ImmutableArray.Create((Symbol)ReducedExtensionMethodSymbol.Create(boundConversion.SymbolOpt));
				resultKind = boundConversion.ResultKind;
				break;
			}
			if (boundConversion.ConversionKind.IsUserDefinedConversion())
			{
				GetSymbolsAndResultKind(boundConversion, boundConversion.SymbolOpt, boundConversion.OriginalUserDefinedConversionsOpt, out symbols, out resultKind);
				break;
			}
			goto default;
		}
		case BoundKind.BinaryOperator:
			GetSymbolsAndResultKind((BoundBinaryOperator)boundNode, out isDynamic, ref resultKind, ref symbols);
			break;
		case BoundKind.UnaryOperator:
			GetSymbolsAndResultKind((BoundUnaryOperator)boundNode, out isDynamic, ref resultKind, ref symbols);
			break;
		case BoundKind.UserDefinedConditionalLogicalOperator:
		{
			BoundUserDefinedConditionalLogicalOperator boundUserDefinedConditionalLogicalOperator = (BoundUserDefinedConditionalLogicalOperator)boundNode;
			isDynamic = false;
			GetSymbolsAndResultKind(boundUserDefinedConditionalLogicalOperator, boundUserDefinedConditionalLogicalOperator.LogicalOperator, boundUserDefinedConditionalLogicalOperator.OriginalUserDefinedOperatorsOpt, out symbols, out resultKind);
			break;
		}
		case BoundKind.CompoundAssignmentOperator:
			GetSymbolsAndResultKind((BoundCompoundAssignmentOperator)boundNode, out isDynamic, ref resultKind, ref symbols);
			break;
		case BoundKind.IncrementOperator:
			GetSymbolsAndResultKind((BoundIncrementOperator)boundNode, out isDynamic, ref resultKind, ref symbols);
			break;
		case BoundKind.AwaitExpression:
		{
			BoundAwaitExpression boundAwaitExpression = (BoundAwaitExpression)boundNode;
			isDynamic = boundAwaitExpression.IsDynamic;
			goto default;
		}
		case BoundKind.ConditionalOperator:
		{
			BoundConditionalOperator boundConditionalOperator = (BoundConditionalOperator)boundNode;
			isDynamic = boundConditionalOperator.IsDynamic;
			goto default;
		}
		case BoundKind.Attribute:
		{
			BoundAttribute boundAttribute = (BoundAttribute)boundNode;
			resultKind = boundAttribute.ResultKind;
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)boundAttribute.Type;
			if (namedTypeSymbol.IsErrorType())
			{
				ImmutableArray<Symbol> candidateSymbols = ((ErrorTypeSymbol)namedTypeSymbol).CandidateSymbols;
				if (candidateSymbols.Length != 1 || !(candidateSymbols[0] is NamedTypeSymbol))
				{
					symbols = candidateSymbols;
					break;
				}
				namedTypeSymbol = (NamedTypeSymbol)candidateSymbols[0];
			}
			AdjustSymbolsForObjectCreation(boundAttribute, namedTypeSymbol, boundAttribute.Constructor, binderOpt, ref resultKind, ref symbols, ref memberGroup);
			break;
		}
		case BoundKind.QueryClause:
		{
			BoundQueryClause boundQueryClause = (BoundQueryClause)boundNode;
			ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
			if (boundQueryClause.Operation != null && (object)boundQueryClause.Operation.ExpressionSymbol != null)
			{
				instance.Add(boundQueryClause.Operation.ExpressionSymbol);
			}
			if ((object)boundQueryClause.DefinedSymbol != null)
			{
				instance.Add(boundQueryClause.DefinedSymbol);
			}
			if (boundQueryClause.Cast != null && (object)boundQueryClause.Cast.ExpressionSymbol != null)
			{
				instance.Add(boundQueryClause.Cast.ExpressionSymbol);
			}
			symbols = instance.ToImmutableAndFree();
			break;
		}
		case BoundKind.DynamicInvocation:
		{
			BoundDynamicInvocation boundDynamicInvocation = (BoundDynamicInvocation)boundNode;
			symbols = (memberGroup = boundDynamicInvocation.ApplicableMethods.Cast<MethodSymbol, Symbol>());
			isDynamic = true;
			break;
		}
		case BoundKind.DynamicCollectionElementInitializer:
		{
			BoundDynamicCollectionElementInitializer boundDynamicCollectionElementInitializer = (BoundDynamicCollectionElementInitializer)boundNode;
			symbols = (memberGroup = boundDynamicCollectionElementInitializer.ApplicableMethods.Cast<MethodSymbol, Symbol>());
			isDynamic = true;
			break;
		}
		case BoundKind.DynamicIndexerAccess:
		{
			BoundDynamicIndexerAccess boundDynamicIndexerAccess = (BoundDynamicIndexerAccess)boundNode;
			symbols = (memberGroup = boundDynamicIndexerAccess.ApplicableIndexers.Cast<PropertySymbol, Symbol>());
			isDynamic = true;
			break;
		}
		case BoundKind.DynamicMemberAccess:
			isDynamic = true;
			break;
		case BoundKind.DynamicObjectCreationExpression:
		{
			BoundDynamicObjectCreationExpression boundDynamicObjectCreationExpression = (BoundDynamicObjectCreationExpression)boundNode;
			symbols = (memberGroup = boundDynamicObjectCreationExpression.ApplicableMethods.Cast<MethodSymbol, Symbol>());
			isDynamic = true;
			break;
		}
		case BoundKind.ObjectCreationExpression:
		{
			BoundObjectCreationExpression boundObjectCreationExpression = (BoundObjectCreationExpression)boundNode;
			if ((object)boundObjectCreationExpression.Constructor != null)
			{
				symbols = ImmutableArray.Create((Symbol)boundObjectCreationExpression.Constructor);
			}
			else if (boundObjectCreationExpression.ConstructorsGroup.Length > 0)
			{
				symbols = StaticCast<Symbol>.From(boundObjectCreationExpression.ConstructorsGroup);
				resultKind = resultKind.WorseResultKind(LookupResultKind.OverloadResolutionFailure);
			}
			memberGroup = boundObjectCreationExpression.ConstructorsGroup.Cast<MethodSymbol, Symbol>();
			break;
		}
		case BoundKind.ThisReference:
		case BoundKind.BaseReference:
		{
			Binder binder = binderOpt ?? GetEnclosingBinder(GetAdjustedNodePosition(boundNode.Syntax));
			NamedTypeSymbol containingType = binder.ContainingType;
			Symbol containingMember = binder.ContainingMember();
			ParameterSymbol thisParameter = GetThisParameter(boundNode.Type, containingType, containingMember, out resultKind);
			symbols = ((thisParameter != null) ? ImmutableArray.Create((Symbol)thisParameter) : ImmutableArray<Symbol>.Empty);
			break;
		}
		default:
		{
			Symbol expressionSymbol = boundNode.ExpressionSymbol;
			if ((object)expressionSymbol != null)
			{
				symbols = ImmutableArray.Create(expressionSymbol);
				resultKind = boundNode.ResultKind;
			}
			break;
		}
		case BoundKind.DelegateCreationExpression:
			break;
		}
		if (boundNodeForSyntacticParent != null && (options & SymbolInfoOptions.PreferConstructorsToType) != 0)
		{
			AdjustSymbolsForObjectCreation(boundNode, boundNodeForSyntacticParent, binderOpt, ref resultKind, ref symbols, ref memberGroup);
		}
		return symbols;
	}

	private static ParameterSymbol GetThisParameter(TypeSymbol typeOfThis, NamedTypeSymbol containingType, Symbol containingMember, out LookupResultKind resultKind)
	{
		if ((object)containingMember == null || (object)containingType == null)
		{
			resultKind = LookupResultKind.NotReferencable;
			return new ThisParameterSymbol(containingMember as MethodSymbol, typeOfThis);
		}
		SymbolKind kind = containingMember.Kind;
		ParameterSymbol result;
		if (kind == SymbolKind.Field || kind == SymbolKind.Method || kind == SymbolKind.Property)
		{
			if (containingMember.IsStatic)
			{
				resultKind = LookupResultKind.StaticInstanceMismatch;
				result = new ThisParameterSymbol(containingMember as MethodSymbol, containingType);
			}
			else if ((object)typeOfThis == ErrorTypeSymbol.UnknownResultType)
			{
				result = new ThisParameterSymbol(containingMember as MethodSymbol, containingType);
				resultKind = LookupResultKind.NotReferencable;
			}
			else
			{
				switch (containingMember.Kind)
				{
				case SymbolKind.Method:
					resultKind = LookupResultKind.Viable;
					result = containingMember.EnclosingThisSymbol();
					break;
				case SymbolKind.Field:
				case SymbolKind.Property:
					resultKind = LookupResultKind.NotReferencable;
					result = containingMember.EnclosingThisSymbol() ?? new ThisParameterSymbol(null, containingType);
					break;
				default:
					throw ExceptionUtilities.UnexpectedValue(containingMember.Kind);
				}
			}
		}
		else
		{
			result = new ThisParameterSymbol(containingMember as MethodSymbol, typeOfThis);
			resultKind = LookupResultKind.NotReferencable;
		}
		return result;
	}

	private static void GetSymbolsAndResultKind(BoundUnaryOperator unaryOperator, out bool isDynamic, ref LookupResultKind resultKind, ref ImmutableArray<Symbol> symbols)
	{
		UnaryOperatorKind unaryOperatorKind = unaryOperator.OperatorKind.OperandTypes();
		isDynamic = unaryOperator.OperatorKind.IsDynamic();
		if (unaryOperatorKind == UnaryOperatorKind.Error || unaryOperatorKind == UnaryOperatorKind.UserDefined || unaryOperator.ResultKind != LookupResultKind.Viable)
		{
			if (!isDynamic)
			{
				GetSymbolsAndResultKind(unaryOperator, unaryOperator.MethodOpt, unaryOperator.OriginalUserDefinedOperatorsOpt, out symbols, out resultKind);
			}
		}
		else
		{
			UnaryOperatorKind kind = unaryOperator.OperatorKind.Operator();
			symbols = ImmutableArray.Create((Symbol)new SynthesizedIntrinsicOperatorSymbol(unaryOperator.Operand.Type.StrippedType(), OperatorFacts.UnaryOperatorNameFromOperatorKind(kind), unaryOperator.Type.StrippedType(), unaryOperator.OperatorKind.IsChecked()));
			resultKind = unaryOperator.ResultKind;
		}
	}

	private static void GetSymbolsAndResultKind(BoundIncrementOperator increment, out bool isDynamic, ref LookupResultKind resultKind, ref ImmutableArray<Symbol> symbols)
	{
		UnaryOperatorKind unaryOperatorKind = increment.OperatorKind.OperandTypes();
		isDynamic = increment.OperatorKind.IsDynamic();
		if (unaryOperatorKind == UnaryOperatorKind.Error || unaryOperatorKind == UnaryOperatorKind.UserDefined || increment.ResultKind != LookupResultKind.Viable)
		{
			if (!isDynamic)
			{
				GetSymbolsAndResultKind(increment, increment.MethodOpt, increment.OriginalUserDefinedOperatorsOpt, out symbols, out resultKind);
			}
		}
		else
		{
			UnaryOperatorKind kind = increment.OperatorKind.Operator();
			symbols = ImmutableArray.Create((Symbol)new SynthesizedIntrinsicOperatorSymbol(increment.Operand.Type.StrippedType(), OperatorFacts.UnaryOperatorNameFromOperatorKind(kind), increment.Type.StrippedType(), increment.OperatorKind.IsChecked()));
			resultKind = increment.ResultKind;
		}
	}

	private static void GetSymbolsAndResultKind(BoundBinaryOperator binaryOperator, out bool isDynamic, ref LookupResultKind resultKind, ref ImmutableArray<Symbol> symbols)
	{
		BinaryOperatorKind binaryOperatorKind = binaryOperator.OperatorKind.OperandTypes();
		BinaryOperatorKind binaryOperatorKind2 = binaryOperator.OperatorKind.Operator();
		isDynamic = binaryOperator.OperatorKind.IsDynamic();
		if (binaryOperatorKind == BinaryOperatorKind.Error || binaryOperatorKind == BinaryOperatorKind.UserDefined || binaryOperator.ResultKind != LookupResultKind.Viable || binaryOperator.OperatorKind.IsLogical())
		{
			if (!isDynamic)
			{
				GetSymbolsAndResultKind(binaryOperator, binaryOperator.MethodOpt, binaryOperator.OriginalUserDefinedOperatorsOpt, out symbols, out resultKind);
			}
			return;
		}
		if (!isDynamic && (binaryOperatorKind2 == BinaryOperatorKind.Equal || binaryOperatorKind2 == BinaryOperatorKind.NotEqual) && ((binaryOperator.Left.IsLiteralNull() && binaryOperator.Right.Type.IsNullableType()) || (binaryOperator.Right.IsLiteralNull() && binaryOperator.Left.Type.IsNullableType())) && binaryOperator.Type.SpecialType == SpecialType.System_Boolean)
		{
			NamedTypeSymbol specialType = binaryOperator.Type.ContainingAssembly.GetSpecialType(SpecialType.System_Object);
			symbols = ImmutableArray.Create((Symbol)new SynthesizedIntrinsicOperatorSymbol(specialType, OperatorFacts.BinaryOperatorNameFromOperatorKind(binaryOperatorKind2), specialType, binaryOperator.Type, binaryOperator.OperatorKind.IsChecked()));
		}
		else
		{
			symbols = ImmutableArray.Create(GetIntrinsicOperatorSymbol(binaryOperatorKind2, isDynamic, binaryOperator.Left.Type, binaryOperator.Right.Type, binaryOperator.Type, binaryOperator.OperatorKind.IsChecked()));
		}
		resultKind = binaryOperator.ResultKind;
	}

	private static Symbol GetIntrinsicOperatorSymbol(BinaryOperatorKind op, bool isDynamic, TypeSymbol leftType, TypeSymbol rightType, TypeSymbol returnType, bool isChecked)
	{
		if (!isDynamic)
		{
			leftType = leftType.StrippedType();
			rightType = rightType.StrippedType();
			returnType = returnType.StrippedType();
		}
		else if ((object)leftType == null)
		{
			leftType = rightType;
		}
		else if ((object)rightType == null)
		{
			rightType = leftType;
		}
		return new SynthesizedIntrinsicOperatorSymbol(leftType, OperatorFacts.BinaryOperatorNameFromOperatorKind(op), rightType, returnType, isChecked);
	}

	private static void GetSymbolsAndResultKind(BoundCompoundAssignmentOperator compoundAssignment, out bool isDynamic, ref LookupResultKind resultKind, ref ImmutableArray<Symbol> symbols)
	{
		BinaryOperatorKind binaryOperatorKind = compoundAssignment.Operator.Kind.OperandTypes();
		BinaryOperatorKind op = compoundAssignment.Operator.Kind.Operator();
		isDynamic = compoundAssignment.Operator.Kind.IsDynamic();
		if (binaryOperatorKind == BinaryOperatorKind.Error || binaryOperatorKind == BinaryOperatorKind.UserDefined || compoundAssignment.ResultKind != LookupResultKind.Viable)
		{
			if (!isDynamic)
			{
				GetSymbolsAndResultKind(compoundAssignment, compoundAssignment.Operator.Method, compoundAssignment.OriginalUserDefinedOperatorsOpt, out symbols, out resultKind);
			}
		}
		else
		{
			symbols = ImmutableArray.Create(GetIntrinsicOperatorSymbol(op, isDynamic, compoundAssignment.Operator.LeftType, compoundAssignment.Operator.RightType, compoundAssignment.Operator.ReturnType, compoundAssignment.Operator.Kind.IsChecked()));
			resultKind = compoundAssignment.ResultKind;
		}
	}

	private static void GetSymbolsAndResultKind(BoundExpression node, Symbol symbolOpt, ImmutableArray<MethodSymbol> originalCandidates, out ImmutableArray<Symbol> symbols, out LookupResultKind resultKind)
	{
		if ((object)symbolOpt != null)
		{
			symbols = ImmutableArray.Create(symbolOpt);
			resultKind = node.ResultKind;
		}
		else if (!originalCandidates.IsDefault)
		{
			symbols = StaticCast<Symbol>.From(originalCandidates);
			resultKind = node.ResultKind;
		}
		else
		{
			symbols = ImmutableArray<Symbol>.Empty;
			resultKind = LookupResultKind.Empty;
		}
	}

	private void AdjustSymbolsForObjectCreation(BoundExpression boundNode, BoundNode boundNodeForSyntacticParent, Binder binderOpt, ref LookupResultKind resultKind, ref ImmutableArray<Symbol> symbols, ref ImmutableArray<Symbol> memberGroup)
	{
		NamedTypeSymbol typeSymbolOpt = null;
		MethodSymbol constructorOpt = null;
		SyntaxNode syntax = boundNodeForSyntacticParent.Syntax;
		if (syntax == null || syntax != boundNode.Syntax.Parent || syntax.Kind() != SyntaxKind.Attribute || ((AttributeSyntax)syntax).Name != boundNode.Syntax)
		{
			return;
		}
		ImmutableArray<Symbol> immutableArray = UnwrapAliases(symbols);
		switch (boundNodeForSyntacticParent.Kind)
		{
		case BoundKind.Attribute:
		{
			BoundAttribute boundAttribute = (BoundAttribute)boundNodeForSyntacticParent;
			if (immutableArray.Length == 1 && immutableArray[0].Kind == SymbolKind.NamedType)
			{
				typeSymbolOpt = (NamedTypeSymbol)immutableArray[0];
				constructorOpt = boundAttribute.Constructor;
				resultKind = resultKind.WorseResultKind(boundAttribute.ResultKind);
			}
			break;
		}
		case BoundKind.BadExpression:
		{
			BoundBadExpression boundBadExpression = (BoundBadExpression)boundNodeForSyntacticParent;
			if (immutableArray.Length == 1)
			{
				resultKind = resultKind.WorseResultKind(boundBadExpression.ResultKind);
				typeSymbolOpt = immutableArray[0] as NamedTypeSymbol;
			}
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(boundNodeForSyntacticParent.Kind);
		}
		AdjustSymbolsForObjectCreation(boundNode, typeSymbolOpt, constructorOpt, binderOpt, ref resultKind, ref symbols, ref memberGroup);
	}

	private void AdjustSymbolsForObjectCreation(BoundNode lowestBoundNode, NamedTypeSymbol typeSymbolOpt, MethodSymbol constructorOpt, Binder binderOpt, ref LookupResultKind resultKind, ref ImmutableArray<Symbol> symbols, ref ImmutableArray<Symbol> memberGroup)
	{
		if ((object)typeSymbolOpt == null)
		{
			return;
		}
		Binder binder = binderOpt ?? GetEnclosingBinder(GetAdjustedNodePosition(lowestBoundNode.Syntax));
		ImmutableArray<MethodSymbol> immutableArray2;
		if (binder != null)
		{
			ImmutableArray<MethodSymbol> immutableArray = ((typeSymbolOpt.IsInterfaceType() && (object)typeSymbolOpt.ComImportCoClass != null) ? typeSymbolOpt.ComImportCoClass.InstanceConstructors : typeSymbolOpt.InstanceConstructors);
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			immutableArray2 = binder.FilterInaccessibleConstructors(immutableArray, allowProtectedConstructorsOfBaseType: false, ref useSiteDiagnostics);
			if (((object)constructorOpt == null) ? (!immutableArray2.Any()) : (!immutableArray2.Contains(constructorOpt)))
			{
				immutableArray2 = immutableArray;
			}
		}
		else
		{
			immutableArray2 = ImmutableArray<MethodSymbol>.Empty;
		}
		if ((object)constructorOpt != null)
		{
			symbols = ImmutableArray.Create((Symbol)constructorOpt);
		}
		else if (immutableArray2.Length > 0)
		{
			symbols = StaticCast<Symbol>.From(immutableArray2);
			resultKind = resultKind.WorseResultKind(LookupResultKind.OverloadResolutionFailure);
		}
		memberGroup = immutableArray2.Cast<MethodSymbol, Symbol>();
	}

	private ImmutableArray<PropertySymbol> GetIndexerGroupSemanticSymbols(BoundExpression boundNode, Binder binderOpt)
	{
		TypeSymbol type = boundNode.Type;
		if ((object)type == null || type.IsStatic)
		{
			return ImmutableArray<PropertySymbol>.Empty;
		}
		Binder binder = binderOpt ?? GetEnclosingBinder(GetAdjustedNodePosition(boundNode.Syntax));
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		AppendSymbolsWithNameAndArity(instance, "this[]", 0, binder, type, LookupOptions.MustBeInstance);
		if (instance.Count == 0)
		{
			instance.Free();
			return ImmutableArray<PropertySymbol>.Empty;
		}
		return FilterOverriddenOrHiddenIndexers(instance.ToImmutableAndFree());
	}

	private static ImmutableArray<PropertySymbol> FilterOverriddenOrHiddenIndexers(ImmutableArray<Symbol> symbols)
	{
		PooledHashSet<Symbol> pooledHashSet = null;
		foreach (PropertySymbol item2 in symbols)
		{
			OverriddenOrHiddenMembersResult overriddenOrHiddenMembers = item2.OverriddenOrHiddenMembers;
			foreach (Symbol overriddenMember in overriddenOrHiddenMembers.OverriddenMembers)
			{
				if (pooledHashSet == null)
				{
					pooledHashSet = PooledHashSet<Symbol>.GetInstance();
				}
				pooledHashSet.Add(overriddenMember);
			}
			foreach (Symbol hiddenMember in overriddenOrHiddenMembers.HiddenMembers)
			{
				if (pooledHashSet == null)
				{
					pooledHashSet = PooledHashSet<Symbol>.GetInstance();
				}
				pooledHashSet.Add(hiddenMember);
			}
		}
		ArrayBuilder<PropertySymbol> instance = ArrayBuilder<PropertySymbol>.GetInstance();
		foreach (PropertySymbol item3 in symbols)
		{
			if (pooledHashSet == null || !pooledHashSet.Contains(item3))
			{
				instance.Add(item3);
			}
		}
		pooledHashSet?.Free();
		return instance.ToImmutableAndFree();
	}

	private static ImmutableArray<MethodSymbol> FilterOverriddenOrHiddenMethods(ImmutableArray<MethodSymbol> methods)
	{
		if (methods.Length <= 1)
		{
			return methods;
		}
		HashSet<Symbol> hiddenSymbols = new HashSet<Symbol>();
		foreach (MethodSymbol item in methods)
		{
			OverriddenOrHiddenMembersResult overriddenOrHiddenMembers = item.OverriddenOrHiddenMembers;
			foreach (Symbol overriddenMember in overriddenOrHiddenMembers.OverriddenMembers)
			{
				hiddenSymbols.Add(overriddenMember);
			}
			foreach (Symbol hiddenMember in overriddenOrHiddenMembers.HiddenMembers)
			{
				hiddenSymbols.Add(hiddenMember);
			}
		}
		return methods.WhereAsArray((MethodSymbol m) => !hiddenSymbols.Contains(m));
	}

	private ImmutableArray<Symbol> GetMethodGroupSemanticSymbols(BoundMethodGroup boundNode, BoundNode boundNodeForSyntacticParent, Binder binderOpt, out LookupResultKind resultKind, out bool isDynamic, out ImmutableArray<Symbol> methodGroup)
	{
		ImmutableArray<Symbol> immutableArray = ImmutableArray<Symbol>.Empty;
		resultKind = boundNode.ResultKind;
		if (resultKind == LookupResultKind.Empty)
		{
			resultKind = LookupResultKind.Viable;
		}
		isDynamic = false;
		Binder binder = binderOpt ?? GetEnclosingBinder(GetAdjustedNodePosition(boundNode.Syntax));
		methodGroup = GetReducedAndFilteredMethodGroupSymbols(binder, boundNode).Cast<MethodSymbol, Symbol>();
		if (boundNodeForSyntacticParent != null)
		{
			BoundKind kind = boundNodeForSyntacticParent.Kind;
			if (kind <= BoundKind.DynamicInvocation)
			{
				if (kind != BoundKind.BadExpression)
				{
					if (kind != BoundKind.Conversion)
					{
						if (kind != BoundKind.DynamicInvocation)
						{
							goto IL_020d;
						}
						immutableArray = ((BoundDynamicInvocation)boundNodeForSyntacticParent).ApplicableMethods.Cast<MethodSymbol, Symbol>();
						isDynamic = true;
					}
					else
					{
						BoundConversion boundConversion = (BoundConversion)boundNodeForSyntacticParent;
						MethodSymbol methodSymbol = boundConversion.SymbolOpt;
						if ((object)methodSymbol == null)
						{
							goto IL_020d;
						}
						if (boundConversion.IsExtensionMethod)
						{
							methodSymbol = ReducedExtensionMethodSymbol.Create(methodSymbol);
						}
						immutableArray = ImmutableArray.Create((Symbol)methodSymbol);
						resultKind = boundConversion.ResultKind;
					}
				}
				else
				{
					ImmutableArray<Symbol> myMethodGroup = methodGroup;
					immutableArray = ((BoundBadExpression)boundNodeForSyntacticParent).Symbols.WhereAsArray((Symbol sym) => myMethodGroup.Contains(sym));
					if (immutableArray.Any())
					{
						resultKind = ((BoundBadExpression)boundNodeForSyntacticParent).ResultKind;
					}
				}
			}
			else if (kind != BoundKind.Call)
			{
				if (kind != BoundKind.DelegateCreationExpression)
				{
					if (kind != BoundKind.NameOfOperator)
					{
						goto IL_020d;
					}
					immutableArray = methodGroup;
					resultKind = resultKind.WorseResultKind(LookupResultKind.MemberGroup);
				}
				else
				{
					BoundDelegateCreationExpression boundDelegateCreationExpression = (BoundDelegateCreationExpression)boundNodeForSyntacticParent;
					if (boundDelegateCreationExpression.Argument == boundNode && (object)boundDelegateCreationExpression.MethodOpt != null)
					{
						immutableArray = CreateReducedExtensionMethodIfPossible(boundDelegateCreationExpression, boundNode.ReceiverOpt);
					}
				}
			}
			else
			{
				BoundCall boundCall = (BoundCall)boundNodeForSyntacticParent;
				if (boundCall.Syntax is InvocationExpressionSyntax invocationExpressionSyntax && invocationExpressionSyntax.Expression.SkipParens() == boundNode.Syntax.SkipParens() && (object)boundCall.Method != null)
				{
					if (boundCall.OriginalMethodsOpt.IsDefault)
					{
						immutableArray = CreateReducedExtensionMethodIfPossible(boundCall);
						resultKind = LookupResultKind.Viable;
					}
					else
					{
						resultKind = boundCall.ResultKind.WorseResultKind(LookupResultKind.OverloadResolutionFailure);
						immutableArray = StaticCast<Symbol>.From(CreateReducedExtensionMethodsFromOriginalsIfNecessary(boundCall));
					}
				}
			}
		}
		else if (methodGroup.Length == 1 && !boundNode.HasAnyErrors)
		{
			immutableArray = methodGroup;
			if (immutableArray.Length > 0)
			{
				resultKind = resultKind.WorseResultKind(LookupResultKind.OverloadResolutionFailure);
			}
		}
		goto IL_025f;
		IL_025f:
		if (!immutableArray.Any())
		{
			immutableArray = methodGroup;
			if (!isDynamic && (int)resultKind > 12)
			{
				resultKind = LookupResultKind.OverloadResolutionFailure;
			}
		}
		return immutableArray;
		IL_020d:
		immutableArray = methodGroup;
		if (immutableArray.Length > 0)
		{
			resultKind = resultKind.WorseResultKind(LookupResultKind.OverloadResolutionFailure);
		}
		goto IL_025f;
	}

	private ImmutableArray<Symbol> GetPropertyGroupSemanticSymbols(BoundPropertyGroup boundNode, BoundNode boundNodeForSyntacticParent, Binder binderOpt, out LookupResultKind resultKind, out ImmutableArray<Symbol> propertyGroup)
	{
		ImmutableArray<Symbol> immutableArray = ImmutableArray<Symbol>.Empty;
		resultKind = boundNode.ResultKind;
		if (resultKind == LookupResultKind.Empty)
		{
			resultKind = LookupResultKind.Viable;
		}
		propertyGroup = boundNode.Properties.Cast<PropertySymbol, Symbol>();
		if (boundNodeForSyntacticParent != null)
		{
			switch (boundNodeForSyntacticParent.Kind)
			{
			case BoundKind.IndexerAccess:
			{
				BoundIndexerAccess boundIndexerAccess = (BoundIndexerAccess)boundNodeForSyntacticParent;
				if (boundIndexerAccess.Syntax is ElementAccessExpressionSyntax elementAccessExpressionSyntax && elementAccessExpressionSyntax.Expression == boundNode.Syntax && (object)boundIndexerAccess.Indexer != null)
				{
					if (boundIndexerAccess.OriginalIndexersOpt.IsDefault)
					{
						immutableArray = ImmutableArray.Create((Symbol)boundIndexerAccess.Indexer);
						resultKind = LookupResultKind.Viable;
					}
					else
					{
						resultKind = boundIndexerAccess.ResultKind.WorseResultKind(LookupResultKind.OverloadResolutionFailure);
						immutableArray = StaticCast<Symbol>.From(boundIndexerAccess.OriginalIndexersOpt);
					}
				}
				break;
			}
			case BoundKind.BadExpression:
			{
				ImmutableArray<Symbol> myPropertyGroup = propertyGroup;
				immutableArray = ((BoundBadExpression)boundNodeForSyntacticParent).Symbols.WhereAsArray((Symbol sym) => myPropertyGroup.Contains(sym));
				if (immutableArray.Any())
				{
					resultKind = ((BoundBadExpression)boundNodeForSyntacticParent).ResultKind;
				}
				break;
			}
			}
		}
		else if (propertyGroup.Length == 1 && !boundNode.HasAnyErrors)
		{
			immutableArray = propertyGroup;
		}
		if (!immutableArray.Any())
		{
			immutableArray = propertyGroup;
			if ((int)resultKind > 12)
			{
				resultKind = LookupResultKind.OverloadResolutionFailure;
			}
		}
		return immutableArray;
	}

	private SymbolInfo GetNamedArgumentSymbolInfo(IdentifierNameSyntax identifierNameSyntax, CancellationToken cancellationToken)
	{
		string valueText = identifierNameSyntax.Identifier.ValueText;
		if (valueText.Length == 0)
		{
			return SymbolInfo.None;
		}
		CSharpSyntaxNode parent = identifierNameSyntax.Parent.Parent.Parent;
		if (parent.IsKind(SyntaxKind.TupleExpression))
		{
			ArgumentSyntax declaratorSyntax = (ArgumentSyntax)identifierNameSyntax.Parent.Parent;
			ISymbol declaredSymbol = GetDeclaredSymbol(declaratorSyntax, cancellationToken);
			if (declaredSymbol != null)
			{
				return new SymbolInfo(declaredSymbol, ImmutableArray<ISymbol>.Empty, CandidateReason.None);
			}
			return SymbolInfo.None;
		}
		CSharpSyntaxNode parent2 = parent.Parent;
		SymbolInfo symbolInfoWorker = GetSymbolInfoWorker(parent2, SymbolInfoOptions.DefaultOptions, cancellationToken);
		if (symbolInfoWorker.Symbol != null)
		{
			ParameterSymbol parameterSymbol = FindNamedParameter(((Symbol)symbolInfoWorker.Symbol).GetParameters(), valueText);
			if ((object)parameterSymbol != null)
			{
				return new SymbolInfo(parameterSymbol, ImmutableArray<ISymbol>.Empty, CandidateReason.None);
			}
			return SymbolInfo.None;
		}
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		foreach (Symbol candidateSymbol in symbolInfoWorker.CandidateSymbols)
		{
			SymbolKind kind = candidateSymbol.Kind;
			if (kind == SymbolKind.Method || kind == SymbolKind.Property)
			{
				ParameterSymbol parameterSymbol2 = FindNamedParameter(candidateSymbol.GetParameters(), valueText);
				if ((object)parameterSymbol2 != null)
				{
					instance.Add(parameterSymbol2);
				}
			}
		}
		if (instance.Count == 0)
		{
			instance.Free();
			return SymbolInfo.None;
		}
		return new SymbolInfo(null, StaticCast<ISymbol>.From(instance.ToImmutableAndFree()), symbolInfoWorker.CandidateReason);
	}

	private static ParameterSymbol FindNamedParameter(ImmutableArray<ParameterSymbol> parameters, string argumentName)
	{
		foreach (ParameterSymbol item in parameters)
		{
			if (item.Name == argumentName)
			{
				return item;
			}
		}
		return null;
	}

	internal static ImmutableArray<MethodSymbol> GetReducedAndFilteredMethodGroupSymbols(Binder binder, BoundMethodGroup node)
	{
		ArrayBuilder<MethodSymbol> instance = ArrayBuilder<MethodSymbol>.GetInstance();
		ArrayBuilder<MethodSymbol> instance2 = ArrayBuilder<MethodSymbol>.GetInstance();
		LookupResultKind resultKind = LookupResultKind.Empty;
		ImmutableArray<TypeSymbol> typeArgumentsOpt = node.TypeArgumentsOpt;
		if (node.Methods.Any())
		{
			foreach (MethodSymbol item in FilterOverriddenOrHiddenMethods(node.Methods))
			{
				MergeReducedAndFilteredMethodGroupSymbol(instance, instance2, new SingleLookupResult(node.ResultKind, item, node.LookupError), typeArgumentsOpt, null, ref resultKind);
			}
		}
		else
		{
			Symbol lookupSymbolOpt = node.LookupSymbolOpt;
			if ((object)lookupSymbolOpt != null && lookupSymbolOpt.Kind == SymbolKind.Method)
			{
				MergeReducedAndFilteredMethodGroupSymbol(instance, instance2, new SingleLookupResult(node.ResultKind, lookupSymbolOpt, node.LookupError), typeArgumentsOpt, null, ref resultKind);
			}
		}
		BoundExpression receiverOpt = node.ReceiverOpt;
		string name = node.Name;
		if (node.SearchExtensionMethods)
		{
			int arity;
			LookupOptions options;
			if (typeArgumentsOpt.IsDefault)
			{
				arity = 0;
				options = LookupOptions.AllMethodsOnArityZero;
			}
			else
			{
				arity = typeArgumentsOpt.Length;
				options = LookupOptions.Default;
			}
			binder = binder.WithAdditionalFlags(BinderFlags.SemanticModel);
			foreach (ExtensionMethodScope item2 in new ExtensionMethodScopes(binder))
			{
				ArrayBuilder<MethodSymbol> instance3 = ArrayBuilder<MethodSymbol>.GetInstance();
				item2.Binder.GetCandidateExtensionMethods(item2.SearchUsingsNotNamespace, instance3, name, arity, options, binder);
				foreach (MethodSymbol item3 in instance3)
				{
					HashSet<DiagnosticInfo> useSiteDiagnostics = null;
					MergeReducedAndFilteredMethodGroupSymbol(instance, instance2, binder.CheckViability(item3, arity, options, null, diagnose: false, ref useSiteDiagnostics), typeArgumentsOpt, receiverOpt.Type, ref resultKind);
				}
				instance3.Free();
			}
		}
		instance.Free();
		return instance2.ToImmutableAndFree();
	}

	private static bool AddReducedAndFilteredMethodGroupSymbol(ArrayBuilder<MethodSymbol> methods, ArrayBuilder<MethodSymbol> filteredMethods, MethodSymbol method, ImmutableArray<TypeSymbol> typeArguments, TypeSymbol receiverType)
	{
		MethodSymbol methodSymbol = ((typeArguments.IsDefaultOrEmpty || method.Arity != typeArguments.Length) ? method : method.Construct(typeArguments));
		if ((object)receiverType != null)
		{
			methodSymbol = methodSymbol.ReduceExtensionMethod(receiverType);
			if ((object)methodSymbol == null)
			{
				return false;
			}
		}
		if (filteredMethods.Contains(methodSymbol))
		{
			return false;
		}
		methods.Add(method);
		filteredMethods.Add(methodSymbol);
		return true;
	}

	private static void MergeReducedAndFilteredMethodGroupSymbol(ArrayBuilder<MethodSymbol> methods, ArrayBuilder<MethodSymbol> filteredMethods, SingleLookupResult singleResult, ImmutableArray<TypeSymbol> typeArguments, TypeSymbol receiverType, ref LookupResultKind resultKind)
	{
		LookupResultKind kind = singleResult.Kind;
		if ((int)resultKind <= (int)kind)
		{
			if ((int)resultKind < (int)kind)
			{
				methods.Clear();
				filteredMethods.Clear();
				resultKind = LookupResultKind.Empty;
			}
			MethodSymbol method = (MethodSymbol)singleResult.Symbol;
			if (AddReducedAndFilteredMethodGroupSymbol(methods, filteredMethods, method, typeArguments, receiverType) && (int)resultKind < (int)kind)
			{
				resultKind = kind;
			}
		}
	}

	private static ImmutableArray<MethodSymbol> CreateReducedExtensionMethodsFromOriginalsIfNecessary(BoundCall call)
	{
		ImmutableArray<MethodSymbol> originalMethodsOpt = call.OriginalMethodsOpt;
		TypeSymbol receiverType = null;
		if (call.InvokedAsExtensionMethod)
		{
			receiverType = ((call.ReceiverOpt == null) ? call.Arguments[0].Type : call.ReceiverOpt.Type);
		}
		ArrayBuilder<MethodSymbol> instance = ArrayBuilder<MethodSymbol>.GetInstance();
		ArrayBuilder<MethodSymbol> instance2 = ArrayBuilder<MethodSymbol>.GetInstance();
		foreach (MethodSymbol item in FilterOverriddenOrHiddenMethods(originalMethodsOpt))
		{
			AddReducedAndFilteredMethodGroupSymbol(instance, instance2, item, default, receiverType);
		}
		instance.Free();
		return instance2.ToImmutableAndFree();
	}

	private static ImmutableArray<Symbol> CreateReducedExtensionMethodIfPossible(BoundCall call)
	{
		MethodSymbol methodSymbol = call.Method;
		if (call.InvokedAsExtensionMethod && methodSymbol.IsExtensionMethod && methodSymbol.MethodKind != MethodKind.ReducedExtension)
		{
			BoundExpression boundExpression = call.Arguments[0];
			methodSymbol = methodSymbol.ReduceExtensionMethod(boundExpression.Type) ?? methodSymbol;
		}
		return ImmutableArray.Create((Symbol)methodSymbol);
	}

	private static ImmutableArray<Symbol> CreateReducedExtensionMethodIfPossible(BoundDelegateCreationExpression delegateCreation, BoundExpression receiverOpt)
	{
		MethodSymbol methodSymbol = delegateCreation.MethodOpt;
		if (delegateCreation.IsExtensionMethod && methodSymbol.IsExtensionMethod && receiverOpt != null)
		{
			methodSymbol = methodSymbol.ReduceExtensionMethod(receiverOpt.Type) ?? methodSymbol;
		}
		return ImmutableArray.Create((Symbol)methodSymbol);
	}

	public abstract ForEachStatementInfo GetForEachStatementInfo(ForEachStatementSyntax node);

	public abstract ForEachStatementInfo GetForEachStatementInfo(CommonForEachStatementSyntax node);

	public abstract DeconstructionInfo GetDeconstructionInfo(AssignmentExpressionSyntax node);

	public abstract DeconstructionInfo GetDeconstructionInfo(ForEachVariableStatementSyntax node);

	public abstract AwaitExpressionInfo GetAwaitExpressionInfo(AwaitExpressionSyntax node);

	public PreprocessingSymbolInfo GetPreprocessingSymbolInfo(IdentifierNameSyntax node)
	{
		CheckSyntaxNode(node);
		if (node.Ancestors().Any((SyntaxNode n) => SyntaxFacts.IsPreprocessorDirective(n.Kind())))
		{
			bool isDefined = SyntaxTree.IsPreprocessorSymbolDefined(node.Identifier.ValueText, node.Identifier.SpanStart);
			return new PreprocessingSymbolInfo(new PreprocessingSymbol(node.Identifier.ValueText), isDefined);
		}
		return PreprocessingSymbolInfo.None;
	}

	internal static void ValidateSymbolInfoOptions(SymbolInfoOptions options)
	{
	}

	public new ISymbol GetEnclosingSymbol(int position, CancellationToken cancellationToken = default(CancellationToken))
	{
		position = CheckAndAdjustPosition(position);
		return GetEnclosingBinder(position)?.ContainingMemberOrLambda;
	}

	private SymbolInfo GetSymbolInfoFromNode(SyntaxNode node, CancellationToken cancellationToken)
	{
		if (node != null)
		{
			if (!(node is ExpressionSyntax expressionSyntax))
			{
				if (!(node is ConstructorInitializerSyntax constructorInitializerSyntax))
				{
					if (!(node is AttributeSyntax attributeSyntax))
					{
						if (!(node is CrefSyntax crefSyntax))
						{
							if (!(node is SelectOrGroupClauseSyntax selectOrGroupClauseSyntax))
							{
								if (node is OrderingSyntax orderingSyntax)
								{
									OrderingSyntax node2 = orderingSyntax;
									return GetSymbolInfo(node2, cancellationToken);
								}
								return SymbolInfo.None;
							}
							SelectOrGroupClauseSyntax node3 = selectOrGroupClauseSyntax;
							return GetSymbolInfo(node3, cancellationToken);
						}
						CrefSyntax crefSyntax2 = crefSyntax;
						return GetSymbolInfo(crefSyntax2, cancellationToken);
					}
					AttributeSyntax attributeSyntax2 = attributeSyntax;
					return GetSymbolInfo(attributeSyntax2, cancellationToken);
				}
				ConstructorInitializerSyntax constructorInitializer = constructorInitializerSyntax;
				return GetSymbolInfo(constructorInitializer, cancellationToken);
			}
			ExpressionSyntax expression = expressionSyntax;
			return GetSymbolInfo(expression, cancellationToken);
		}
		throw new ArgumentNullException("node");
	}

	private TypeInfo GetTypeInfoFromNode(SyntaxNode node, CancellationToken cancellationToken)
	{
		if (node != null)
		{
			if (!(node is ExpressionSyntax expressionSyntax))
			{
				if (!(node is ConstructorInitializerSyntax constructorInitializerSyntax))
				{
					if (!(node is AttributeSyntax attributeSyntax))
					{
						if (node is SelectOrGroupClauseSyntax selectOrGroupClauseSyntax)
						{
							SelectOrGroupClauseSyntax node2 = selectOrGroupClauseSyntax;
							return GetTypeInfo(node2, cancellationToken);
						}
						return CSharpTypeInfo.None;
					}
					AttributeSyntax attributeSyntax2 = attributeSyntax;
					return GetTypeInfo(attributeSyntax2, cancellationToken);
				}
				ConstructorInitializerSyntax constructorInitializer = constructorInitializerSyntax;
				return GetTypeInfo(constructorInitializer, cancellationToken);
			}
			ExpressionSyntax expression = expressionSyntax;
			return GetTypeInfo(expression, cancellationToken);
		}
		throw new ArgumentNullException("node");
	}

	private ImmutableArray<ISymbol> GetMemberGroupFromNode(SyntaxNode node, CancellationToken cancellationToken)
	{
		if (node != null)
		{
			if (!(node is ExpressionSyntax expressionSyntax))
			{
				if (!(node is ConstructorInitializerSyntax constructorInitializerSyntax))
				{
					if (node is AttributeSyntax attributeSyntax)
					{
						AttributeSyntax attribute = attributeSyntax;
						return GetMemberGroup(attribute, cancellationToken);
					}
					return ImmutableArray<ISymbol>.Empty;
				}
				ConstructorInitializerSyntax initializer = constructorInitializerSyntax;
				return GetMemberGroup(initializer, cancellationToken);
			}
			ExpressionSyntax expression = expressionSyntax;
			return GetMemberGroup(expression, cancellationToken);
		}
		throw new ArgumentNullException("node");
	}

	protected sealed override ImmutableArray<ISymbol> GetMemberGroupCore(SyntaxNode node, CancellationToken cancellationToken)
	{
		return StaticCast<ISymbol>.From(GetMemberGroupFromNode(node, cancellationToken));
	}

	protected sealed override SymbolInfo GetSpeculativeSymbolInfoCore(int position, SyntaxNode node, SpeculativeBindingOption bindingOption)
	{
		if (node != null)
		{
			if (node is ExpressionSyntax expressionSyntax)
			{
				ExpressionSyntax expression = expressionSyntax;
				return GetSpeculativeSymbolInfo(position, expression, bindingOption);
			}
			if (node is ConstructorInitializerSyntax constructorInitializerSyntax)
			{
				ConstructorInitializerSyntax constructorInitializer = constructorInitializerSyntax;
				return GetSpeculativeSymbolInfo(position, constructorInitializer);
			}
			if (node is AttributeSyntax attributeSyntax)
			{
				AttributeSyntax attribute = attributeSyntax;
				return GetSpeculativeSymbolInfo(position, attribute);
			}
			if (node is CrefSyntax crefSyntax)
			{
				CrefSyntax cref = crefSyntax;
				return GetSpeculativeSymbolInfo(position, cref);
			}
		}
		return SymbolInfo.None;
	}

	protected sealed override TypeInfo GetSpeculativeTypeInfoCore(int position, SyntaxNode node, SpeculativeBindingOption bindingOption)
	{
		if (!(node is ExpressionSyntax expression))
		{
			return CSharpTypeInfo.None;
		}
		return GetSpeculativeTypeInfo(position, expression, bindingOption);
	}

	protected sealed override IAliasSymbol GetSpeculativeAliasInfoCore(int position, SyntaxNode nameSyntax, SpeculativeBindingOption bindingOption)
	{
		if (!(nameSyntax is IdentifierNameSyntax nameSyntax2))
		{
			return null;
		}
		return GetSpeculativeAliasInfo(position, nameSyntax2, bindingOption);
	}

	protected sealed override SymbolInfo GetSymbolInfoCore(SyntaxNode node, CancellationToken cancellationToken)
	{
		return GetSymbolInfoFromNode(node, cancellationToken);
	}

	protected sealed override TypeInfo GetTypeInfoCore(SyntaxNode node, CancellationToken cancellationToken)
	{
		return GetTypeInfoFromNode(node, cancellationToken);
	}

	protected sealed override IAliasSymbol GetAliasInfoCore(SyntaxNode node, CancellationToken cancellationToken)
	{
		if (!(node is IdentifierNameSyntax nameSyntax))
		{
			return null;
		}
		return GetAliasInfo(nameSyntax, cancellationToken);
	}

	protected sealed override PreprocessingSymbolInfo GetPreprocessingSymbolInfoCore(SyntaxNode node)
	{
		if (!(node is IdentifierNameSyntax node2))
		{
			return PreprocessingSymbolInfo.None;
		}
		return GetPreprocessingSymbolInfo(node2);
	}

	protected sealed override ISymbol GetDeclaredSymbolCore(SyntaxNode node, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (node != null)
		{
			if (node is AccessorDeclarationSyntax accessorDeclarationSyntax)
			{
				AccessorDeclarationSyntax declarationSyntax = accessorDeclarationSyntax;
				return GetDeclaredSymbol(declarationSyntax, cancellationToken);
			}
			if (node is BaseTypeDeclarationSyntax baseTypeDeclarationSyntax)
			{
				BaseTypeDeclarationSyntax declarationSyntax2 = baseTypeDeclarationSyntax;
				return GetDeclaredSymbol(declarationSyntax2, cancellationToken);
			}
			if (node is QueryClauseSyntax queryClauseSyntax)
			{
				QueryClauseSyntax queryClause = queryClauseSyntax;
				return GetDeclaredSymbol(queryClause, cancellationToken);
			}
			if (node is MemberDeclarationSyntax memberDeclarationSyntax)
			{
				MemberDeclarationSyntax declarationSyntax3 = memberDeclarationSyntax;
				return GetDeclaredSymbol(declarationSyntax3, cancellationToken);
			}
		}
		switch (node.Kind())
		{
		case SyntaxKind.LocalFunctionStatement:
			return GetDeclaredSymbol((LocalFunctionStatementSyntax)node, cancellationToken);
		case SyntaxKind.LabeledStatement:
			return GetDeclaredSymbol((LabeledStatementSyntax)node, cancellationToken);
		case SyntaxKind.CaseSwitchLabel:
		case SyntaxKind.DefaultSwitchLabel:
			return GetDeclaredSymbol((SwitchLabelSyntax)node, cancellationToken);
		case SyntaxKind.AnonymousObjectCreationExpression:
			return GetDeclaredSymbol((AnonymousObjectCreationExpressionSyntax)node, cancellationToken);
		case SyntaxKind.AnonymousObjectMemberDeclarator:
			return GetDeclaredSymbol((AnonymousObjectMemberDeclaratorSyntax)node, cancellationToken);
		case SyntaxKind.TupleExpression:
			return GetDeclaredSymbol((TupleExpressionSyntax)node, cancellationToken);
		case SyntaxKind.Argument:
			return GetDeclaredSymbol((ArgumentSyntax)node, cancellationToken);
		case SyntaxKind.VariableDeclarator:
			return GetDeclaredSymbol((VariableDeclaratorSyntax)node, cancellationToken);
		case SyntaxKind.SingleVariableDesignation:
			return GetDeclaredSymbol((SingleVariableDesignationSyntax)node, cancellationToken);
		case SyntaxKind.TupleElement:
			return GetDeclaredSymbol((TupleElementSyntax)node, cancellationToken);
		case SyntaxKind.NamespaceDeclaration:
			return GetDeclaredSymbol((NamespaceDeclarationSyntax)node, cancellationToken);
		case SyntaxKind.Parameter:
			return GetDeclaredSymbol((ParameterSyntax)node, cancellationToken);
		case SyntaxKind.TypeParameter:
			return GetDeclaredSymbol((TypeParameterSyntax)node, cancellationToken);
		case SyntaxKind.UsingDirective:
		{
			UsingDirectiveSyntax usingDirectiveSyntax = (UsingDirectiveSyntax)node;
			if (usingDirectiveSyntax.Alias != null)
			{
				return GetDeclaredSymbol(usingDirectiveSyntax, cancellationToken);
			}
			break;
		}
		case SyntaxKind.ForEachStatement:
			return GetDeclaredSymbol((ForEachStatementSyntax)node, cancellationToken);
		case SyntaxKind.CatchDeclaration:
			return GetDeclaredSymbol((CatchDeclarationSyntax)node, cancellationToken);
		case SyntaxKind.JoinIntoClause:
			return GetDeclaredSymbol((JoinIntoClauseSyntax)node, cancellationToken);
		case SyntaxKind.QueryContinuation:
			return GetDeclaredSymbol((QueryContinuationSyntax)node, cancellationToken);
		}
		return null;
	}

	public ISymbol GetDeclaredSymbol(TupleElementSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declarationSyntax);
		if (declarationSyntax.Parent is TupleTypeSyntax tupleTypeSyntax)
		{
			return (GetSymbolInfo(tupleTypeSyntax).Symbol as TupleTypeSymbol)?.TupleElements.ElementAtOrDefault(tupleTypeSyntax.Elements.IndexOf(declarationSyntax));
		}
		return null;
	}

	protected sealed override ImmutableArray<ISymbol> GetDeclaredSymbolsCore(SyntaxNode declaration, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (declaration is BaseFieldDeclarationSyntax declarationSyntax)
		{
			return GetDeclaredSymbols(declarationSyntax, cancellationToken);
		}
		ISymbol declaredSymbolCore = GetDeclaredSymbolCore(declaration, cancellationToken);
		if (declaredSymbolCore != null)
		{
			return ImmutableArray.Create(declaredSymbolCore);
		}
		return ImmutableArray.Create<ISymbol>();
	}

	internal override void ComputeDeclarationsInSpan(TextSpan span, bool getSymbol, ArrayBuilder<DeclarationInfo> builder, CancellationToken cancellationToken)
	{
		CSharpDeclarationComputer.ComputeDeclarationsInSpan(this, span, getSymbol, builder, cancellationToken);
	}

	internal override void ComputeDeclarationsInNode(SyntaxNode node, bool getSymbol, ArrayBuilder<DeclarationInfo> builder, CancellationToken cancellationToken, int? levelsToCompute = null)
	{
		CSharpDeclarationComputer.ComputeDeclarationsInNode(this, node, getSymbol, builder, cancellationToken, levelsToCompute);
	}

	protected internal override SyntaxNode GetTopmostNodeForDiagnosticAnalysis(ISymbol symbol, SyntaxNode declaringSyntax)
	{
		SymbolKind kind = symbol.Kind;
		if ((uint)(kind - 5) <= 1u)
		{
			BaseFieldDeclarationSyntax baseFieldDeclarationSyntax = declaringSyntax.FirstAncestorOrSelf<BaseFieldDeclarationSyntax>();
			if (baseFieldDeclarationSyntax != null)
			{
				return baseFieldDeclarationSyntax;
			}
		}
		return declaringSyntax;
	}

	protected sealed override ImmutableArray<ISymbol> LookupSymbolsCore(int position, INamespaceOrTypeSymbol container, string name, bool includeReducedExtensionMethods)
	{
		return LookupSymbols(position, ToLanguageSpecific(container), name, includeReducedExtensionMethods);
	}

	protected sealed override ImmutableArray<ISymbol> LookupBaseMembersCore(int position, string name)
	{
		return LookupBaseMembers(position, name);
	}

	protected sealed override ImmutableArray<ISymbol> LookupStaticMembersCore(int position, INamespaceOrTypeSymbol container, string name)
	{
		return LookupStaticMembers(position, ToLanguageSpecific(container), name);
	}

	protected sealed override ImmutableArray<ISymbol> LookupNamespacesAndTypesCore(int position, INamespaceOrTypeSymbol container, string name)
	{
		return LookupNamespacesAndTypes(position, ToLanguageSpecific(container), name);
	}

	protected sealed override ImmutableArray<ISymbol> LookupLabelsCore(int position, string name)
	{
		return LookupLabels(position, name);
	}

	private static NamespaceOrTypeSymbol ToLanguageSpecific(INamespaceOrTypeSymbol container)
	{
		if (container == null)
		{
			return null;
		}
		return (container as NamespaceOrTypeSymbol) ?? throw new ArgumentException(CSharpResources.NotACSharpSymbol, "container");
	}

	protected sealed override ControlFlowAnalysis AnalyzeControlFlowCore(SyntaxNode firstStatement, SyntaxNode lastStatement)
	{
		if (firstStatement == null)
		{
			throw new ArgumentNullException("firstStatement");
		}
		if (lastStatement == null)
		{
			throw new ArgumentNullException("lastStatement");
		}
		if (!(firstStatement is StatementSyntax firstStatement2))
		{
			throw new ArgumentException("firstStatement is not a StatementSyntax.");
		}
		if (!(lastStatement is StatementSyntax lastStatement2))
		{
			throw new ArgumentException("firstStatement is a StatementSyntax but lastStatement isn't.");
		}
		return AnalyzeControlFlow(firstStatement2, lastStatement2);
	}

	protected sealed override ControlFlowAnalysis AnalyzeControlFlowCore(SyntaxNode statement)
	{
		if (statement == null)
		{
			throw new ArgumentNullException("statement");
		}
		if (!(statement is StatementSyntax statement2))
		{
			throw new ArgumentException("statement is not a StatementSyntax.");
		}
		return AnalyzeControlFlow(statement2);
	}

	protected sealed override DataFlowAnalysis AnalyzeDataFlowCore(SyntaxNode firstStatement, SyntaxNode lastStatement)
	{
		if (firstStatement == null)
		{
			throw new ArgumentNullException("firstStatement");
		}
		if (lastStatement == null)
		{
			throw new ArgumentNullException("lastStatement");
		}
		if (!(firstStatement is StatementSyntax firstStatement2))
		{
			throw new ArgumentException("firstStatement is not a StatementSyntax.");
		}
		if (!(lastStatement is StatementSyntax lastStatement2))
		{
			throw new ArgumentException("lastStatement is not a StatementSyntax.");
		}
		return AnalyzeDataFlow(firstStatement2, lastStatement2);
	}

	protected sealed override DataFlowAnalysis AnalyzeDataFlowCore(SyntaxNode statementOrExpression)
	{
		if (statementOrExpression != null)
		{
			if (!(statementOrExpression is StatementSyntax statementSyntax))
			{
				if (statementOrExpression is ExpressionSyntax expressionSyntax)
				{
					ExpressionSyntax expression = expressionSyntax;
					return AnalyzeDataFlow(expression);
				}
				throw new ArgumentException("statementOrExpression is not a StatementSyntax or an ExpressionSyntax.");
			}
			StatementSyntax statement = statementSyntax;
			return AnalyzeDataFlow(statement);
		}
		throw new ArgumentNullException("statementOrExpression");
	}

	protected sealed override Optional<object> GetConstantValueCore(SyntaxNode node, CancellationToken cancellationToken)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		if (!(node is ExpressionSyntax expression))
		{
			return default;
		}
		return GetConstantValue(expression, cancellationToken);
	}

	protected sealed override ISymbol GetEnclosingSymbolCore(int position, CancellationToken cancellationToken)
	{
		return GetEnclosingSymbol(position, cancellationToken);
	}

	protected sealed override bool IsAccessibleCore(int position, ISymbol symbol)
	{
		return IsAccessible(position, symbol.EnsureCSharpSymbolOrNull<ISymbol, Symbol>("symbol"));
	}

	protected sealed override bool IsEventUsableAsFieldCore(int position, IEventSymbol symbol)
	{
		return IsEventUsableAsField(position, symbol.EnsureCSharpSymbolOrNull<IEventSymbol, EventSymbol>("symbol"));
	}
}
