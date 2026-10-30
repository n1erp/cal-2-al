using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class SyntaxTreeSemanticModel : CSharpSemanticModel
{
	private readonly CSharpCompilation _compilation;

	private readonly SyntaxTree _syntaxTree;

	private ImmutableDictionary<CSharpSyntaxNode, MemberSemanticModel> _memberModels = ImmutableDictionary<CSharpSyntaxNode, MemberSemanticModel>.Empty;

	private readonly BinderFactory _binderFactory;

	private Func<CSharpSyntaxNode, MemberSemanticModel> _createMemberModelFunction;

	private readonly bool _ignoresAccessibility;

	private ScriptLocalScopeBinder.Labels _globalStatementLabels;

	private static readonly Func<CSharpSyntaxNode, bool> s_isMemberDeclarationFunction = IsMemberDeclaration;

	public override CSharpCompilation Compilation => _compilation;

	internal override CSharpSyntaxNode Root => (CSharpSyntaxNode)_syntaxTree.GetRoot();

	public override SyntaxTree SyntaxTree => _syntaxTree;

	public override bool IgnoresAccessibility => _ignoresAccessibility;

	public override bool IsSpeculativeSemanticModel => false;

	public override int OriginalPositionForSpeculation => 0;

	public override CSharpSemanticModel ParentModel => null;

	internal override SemanticModel ContainingModelOrSelf => this;

	internal ImmutableDictionary<CSharpSyntaxNode, MemberSemanticModel> TestOnlyMemberModels => _memberModels;

	private bool IsRegularCSharp => SyntaxTree.Options.Kind == SourceCodeKind.Regular;

	internal SyntaxTreeSemanticModel(CSharpCompilation compilation, SyntaxTree syntaxTree, bool ignoreAccessibility = false)
	{
		_compilation = compilation;
		_syntaxTree = syntaxTree;
		_ignoresAccessibility = ignoreAccessibility;
		if (!Compilation.SyntaxTrees.Contains(syntaxTree))
		{
			throw new ArgumentOutOfRangeException("syntaxTree", CSharpResources.TreeNotPartOfCompilation);
		}
		_binderFactory = compilation.GetBinderFactory(SyntaxTree);
	}

	internal SyntaxTreeSemanticModel(CSharpCompilation parentCompilation, SyntaxTree parentSyntaxTree, SyntaxTree speculatedSyntaxTree)
	{
		_compilation = parentCompilation;
		_syntaxTree = speculatedSyntaxTree;
		_binderFactory = _compilation.GetBinderFactory(parentSyntaxTree);
	}

	private void VerifySpanForGetDiagnostics(TextSpan? span)
	{
		if (span.HasValue && !Root.FullSpan.Contains(span.Value))
		{
			throw new ArgumentException("span");
		}
	}

	public override ImmutableArray<Diagnostic> GetSyntaxDiagnostics(TextSpan? span = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		VerifySpanForGetDiagnostics(span);
		return Compilation.GetDiagnosticsForSyntaxTree(CompilationStage.Parse, SyntaxTree, span, includeEarlierStages: false, cancellationToken);
	}

	public override ImmutableArray<Diagnostic> GetDeclarationDiagnostics(TextSpan? span = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		VerifySpanForGetDiagnostics(span);
		return Compilation.GetDiagnosticsForSyntaxTree(CompilationStage.Declare, SyntaxTree, span, includeEarlierStages: false, cancellationToken);
	}

	public override ImmutableArray<Diagnostic> GetMethodBodyDiagnostics(TextSpan? span = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		VerifySpanForGetDiagnostics(span);
		return Compilation.GetDiagnosticsForSyntaxTree(CompilationStage.Compile, SyntaxTree, span, includeEarlierStages: false, cancellationToken);
	}

	public override ImmutableArray<Diagnostic> GetDiagnostics(TextSpan? span = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		VerifySpanForGetDiagnostics(span);
		return Compilation.GetDiagnosticsForSyntaxTree(CompilationStage.Compile, SyntaxTree, span, includeEarlierStages: true, cancellationToken);
	}

	internal override Binder GetEnclosingBinderInternal(int position)
	{
		SyntaxToken syntaxToken = Root.FindTokenIncludingCrefAndNameAttributes(position);
		if (position == 0 && position != syntaxToken.SpanStart)
		{
			return _binderFactory.GetBinder(Root, position).WithAdditionalFlags(GetSemanticModelBinderFlags());
		}
		MemberSemanticModel memberModel = GetMemberModel(position);
		if (memberModel != null)
		{
			return memberModel.GetEnclosingBinder(position);
		}
		return _binderFactory.GetBinder((CSharpSyntaxNode)syntaxToken.Parent, position).WithAdditionalFlags(GetSemanticModelBinderFlags());
	}

	internal override IOperation GetOperationWorker(CSharpSyntaxNode node, CancellationToken cancellationToken)
	{
		if (node == null)
		{
			goto IL_0080;
		}
		MemberSemanticModel memberSemanticModel;
		if (!(node is ConstructorDeclarationSyntax constructorDeclarationSyntax))
		{
			if (!(node is BaseMethodDeclarationSyntax declaration))
			{
				if (!(node is AccessorDeclarationSyntax accessorDeclarationSyntax))
				{
					goto IL_0080;
				}
				AccessorDeclarationSyntax accessorDeclarationSyntax2 = accessorDeclarationSyntax;
				memberSemanticModel = ((accessorDeclarationSyntax2.Body != null || accessorDeclarationSyntax2.ExpressionBody != null) ? GetOrAddModel(node) : null);
			}
			else
			{
				memberSemanticModel = (declaration.HasAnyBody() ? GetOrAddModel(node) : null);
			}
		}
		else
		{
			ConstructorDeclarationSyntax constructorDeclarationSyntax2 = constructorDeclarationSyntax;
			memberSemanticModel = ((constructorDeclarationSyntax2.HasAnyBody() || constructorDeclarationSyntax2.Initializer != null) ? GetOrAddModel(node) : null);
		}
		goto IL_0088;
		IL_0080:
		memberSemanticModel = GetMemberModel(node);
		goto IL_0088;
		IL_0088:
		return memberSemanticModel?.GetOperationWorker(node, cancellationToken);
	}

	internal override SymbolInfo GetSymbolInfoWorker(CSharpSyntaxNode node, SymbolInfoOptions options, CancellationToken cancellationToken = default(CancellationToken))
	{
		CSharpSemanticModel.ValidateSymbolInfoOptions(options);
		node = SyntaxFactory.GetStandaloneNode(node);
		MemberSemanticModel memberModel = GetMemberModel(node);
		SymbolInfo result;
		XmlNameAttributeSyntax syntax;
		if (memberModel != null)
		{
			result = memberModel.GetSymbolInfoWorker(node, options, cancellationToken);
			if (result.Symbol == null && result.CandidateReason == CandidateReason.None && node is ExpressionSyntax && SyntaxFacts.IsInNamespaceOrTypeContext((ExpressionSyntax)node))
			{
				Binder enclosingBinder = GetEnclosingBinder(GetAdjustedNodePosition(node));
				if (enclosingBinder != null)
				{
					enclosingBinder = new LocalScopeBinder(enclosingBinder);
					DiagnosticBag instance = DiagnosticBag.GetInstance();
					BoundExpression boundExpression = enclosingBinder.BindExpression((ExpressionSyntax)node, instance);
					instance.Free();
					SymbolInfo symbolInfoForNode = GetSymbolInfoForNode(options, boundExpression, boundExpression, null, null);
					if (symbolInfoForNode.Symbol != null)
					{
						result = new SymbolInfo(null, ImmutableArray.Create(symbolInfoForNode.Symbol), CandidateReason.NotATypeOrNamespace);
					}
					else if (!symbolInfoForNode.CandidateSymbols.IsEmpty)
					{
						result = new SymbolInfo(null, symbolInfoForNode.CandidateSymbols, CandidateReason.NotATypeOrNamespace);
					}
				}
			}
		}
		else if (node.Parent.Kind() == SyntaxKind.XmlNameAttribute && (syntax = (XmlNameAttributeSyntax)node.Parent).Identifier == node)
		{
			result = SymbolInfo.None;
			Binder enclosingBinder2 = GetEnclosingBinder(GetAdjustedNodePosition(node));
			if (enclosingBinder2 != null)
			{
				HashSet<DiagnosticInfo> useSiteDiagnostics = null;
				ImmutableArray<Symbol> symbols = enclosingBinder2.BindXmlNameAttribute(syntax, ref useSiteDiagnostics);
				result = symbols.Length switch
				{
					0 => SymbolInfo.None, 
					1 => SymbolInfoFactory.Create(symbols, LookupResultKind.Viable, isDynamic: false), 
					_ => SymbolInfoFactory.Create(symbols, LookupResultKind.Ambiguous, isDynamic: false), 
				};
			}
		}
		else if (node is CrefSyntax crefSyntax)
		{
			int adjustedNodePosition = GetAdjustedNodePosition(crefSyntax);
			result = GetCrefSymbolInfo(adjustedNodePosition, crefSyntax, options, CSharpSemanticModel.HasParameterList(crefSyntax));
		}
		else
		{
			Symbol semanticInfoSymbolInNonMemberContext = GetSemanticInfoSymbolInNonMemberContext(node, (options & SymbolInfoOptions.PreserveAliases) != 0);
			result = (((object)semanticInfoSymbolInNonMemberContext != null) ? CSharpSemanticModel.GetSymbolInfoForSymbol(semanticInfoSymbolInNonMemberContext, options) : SymbolInfo.None);
		}
		return result;
	}

	internal override SymbolInfo GetCollectionInitializerSymbolInfoWorker(InitializerExpressionSyntax collectionInitializer, ExpressionSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetMemberModel(collectionInitializer)?.GetCollectionInitializerSymbolInfoWorker(collectionInitializer, node, cancellationToken) ?? SymbolInfo.None;
	}

	internal override CSharpTypeInfo GetTypeInfoWorker(CSharpSyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		node = SyntaxFactory.GetStandaloneNode(node);
		MemberSemanticModel memberModel = GetMemberModel(node);
		if (memberModel != null)
		{
			return memberModel.GetTypeInfoWorker(node, cancellationToken);
		}
		Symbol semanticInfoSymbolInNonMemberContext = GetSemanticInfoSymbolInNonMemberContext(node, bindVarAsAliasFirst: false);
		if ((object)semanticInfoSymbolInNonMemberContext == null)
		{
			return CSharpTypeInfo.None;
		}
		return CSharpSemanticModel.GetTypeInfoForSymbol(semanticInfoSymbolInNonMemberContext);
	}

	private Symbol GetSemanticInfoSymbolInNonMemberContext(CSharpSyntaxNode node, bool bindVarAsAliasFirst)
	{
		Binder enclosingBinder = GetEnclosingBinder(GetAdjustedNodePosition(node));
		if (enclosingBinder != null && node is TypeSyntax typeSyntax)
		{
			ConsList<Symbol> basesBeingResolved = GetBasesBeingResolved(typeSyntax);
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			try
			{
				if (SyntaxFacts.IsNamespaceAliasQualifier(typeSyntax))
				{
					return enclosingBinder.BindNamespaceAliasSymbol(node as IdentifierNameSyntax, instance);
				}
				if (SyntaxFacts.IsInTypeOnlyContext(typeSyntax))
				{
					if (!typeSyntax.IsVar)
					{
						return enclosingBinder.BindTypeOrAlias(typeSyntax, instance, basesBeingResolved);
					}
					Symbol symbol = (bindVarAsAliasFirst ? enclosingBinder.BindTypeOrAlias(typeSyntax, instance, basesBeingResolved) : null);
					if (((object)symbol == null || symbol.Kind == SymbolKind.ErrorType) && typeSyntax.Parent is VariableDeclarationSyntax { Variables: var variables } variableDeclarationSyntax && variables.Any())
					{
						SourceMemberFieldSymbol declaredFieldSymbol = GetDeclaredFieldSymbol(variableDeclarationSyntax.Variables.First());
						if ((object)declaredFieldSymbol != null)
						{
							symbol = declaredFieldSymbol.Type;
						}
					}
					return symbol ?? enclosingBinder.BindTypeOrAlias(typeSyntax, instance, basesBeingResolved);
				}
				return enclosingBinder.BindNamespaceOrTypeOrAliasSymbol(typeSyntax, instance, basesBeingResolved, basesBeingResolved != null);
			}
			finally
			{
				instance.Free();
			}
		}
		return null;
	}

	internal override ImmutableArray<Symbol> GetMemberGroupWorker(CSharpSyntaxNode node, SymbolInfoOptions options, CancellationToken cancellationToken = default(CancellationToken))
	{
		node = SyntaxFactory.GetStandaloneNode(node);
		return GetMemberModel(node)?.GetMemberGroupWorker(node, options, cancellationToken) ?? ImmutableArray<Symbol>.Empty;
	}

	internal override ImmutableArray<PropertySymbol> GetIndexerGroupWorker(CSharpSyntaxNode node, SymbolInfoOptions options, CancellationToken cancellationToken = default(CancellationToken))
	{
		node = SyntaxFactory.GetStandaloneNode(node);
		return GetMemberModel(node)?.GetIndexerGroupWorker(node, options, cancellationToken) ?? ImmutableArray<PropertySymbol>.Empty;
	}

	internal override Optional<object> GetConstantValueWorker(CSharpSyntaxNode node, CancellationToken cancellationToken)
	{
		node = SyntaxFactory.GetStandaloneNode(node);
		return GetMemberModel(node)?.GetConstantValueWorker(node, cancellationToken) ?? default(Optional<object>);
	}

	public override QueryClauseInfo GetQueryClauseInfo(QueryClauseSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(node);
		return GetMemberModel(node)?.GetQueryClauseInfo(node, cancellationToken) ?? default(QueryClauseInfo);
	}

	public override SymbolInfo GetSymbolInfo(SelectOrGroupClauseSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(node);
		return GetMemberModel(node)?.GetSymbolInfo(node, cancellationToken) ?? SymbolInfo.None;
	}

	public override TypeInfo GetTypeInfo(SelectOrGroupClauseSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(node);
		return GetMemberModel(node)?.GetTypeInfo(node, cancellationToken) ?? ((TypeInfo)CSharpTypeInfo.None);
	}

	public override IPropertySymbol GetDeclaredSymbol(AnonymousObjectMemberDeclaratorSyntax declaratorSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declaratorSyntax);
		return GetMemberModel(declaratorSyntax)?.GetDeclaredSymbol(declaratorSyntax, cancellationToken);
	}

	public override INamedTypeSymbol GetDeclaredSymbol(AnonymousObjectCreationExpressionSyntax declaratorSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declaratorSyntax);
		return GetMemberModel(declaratorSyntax)?.GetDeclaredSymbol(declaratorSyntax, cancellationToken);
	}

	public override INamedTypeSymbol GetDeclaredSymbol(TupleExpressionSyntax declaratorSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declaratorSyntax);
		return GetMemberModel(declaratorSyntax)?.GetDeclaredSymbol(declaratorSyntax, cancellationToken);
	}

	public override ISymbol GetDeclaredSymbol(ArgumentSyntax declaratorSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declaratorSyntax);
		return GetMemberModel(declaratorSyntax)?.GetDeclaredSymbol(declaratorSyntax, cancellationToken);
	}

	public override IRangeVariableSymbol GetDeclaredSymbol(QueryClauseSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(node);
		return GetMemberModel(node)?.GetDeclaredSymbol(node, cancellationToken);
	}

	public override IRangeVariableSymbol GetDeclaredSymbol(JoinIntoClauseSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(node);
		return GetMemberModel(node)?.GetDeclaredSymbol(node, cancellationToken);
	}

	public override IRangeVariableSymbol GetDeclaredSymbol(QueryContinuationSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(node);
		return GetMemberModel(node)?.GetDeclaredSymbol(node, cancellationToken);
	}

	public override SymbolInfo GetSymbolInfo(OrderingSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(node);
		return GetMemberModel(node)?.GetSymbolInfo(node, cancellationToken) ?? SymbolInfo.None;
	}

	private ConsList<Symbol> GetBasesBeingResolved(TypeSyntax expression)
	{
		while (expression != null && expression.Parent != null)
		{
			CSharpSyntaxNode parent = expression.Parent;
			if (parent is BaseTypeSyntax && parent.Parent != null && parent.Parent.Kind() == SyntaxKind.BaseList && ((BaseTypeSyntax)parent).Type == expression)
			{
				BaseTypeDeclarationSyntax declarationSyntax = (BaseTypeDeclarationSyntax)parent.Parent.Parent;
				INamedTypeSymbol declaredSymbol = GetDeclaredSymbol(declarationSyntax);
				return ConsList<Symbol>.Empty.Prepend((Symbol)declaredSymbol.OriginalDefinition);
			}
			expression = expression.Parent as TypeSyntax;
		}
		return null;
	}

	public override Conversion ClassifyConversion(ExpressionSyntax expression, ITypeSymbol destination, bool isExplicitInSource = false)
	{
		TypeSymbol destination2 = destination.EnsureCSharpSymbolOrNull<ITypeSymbol, TypeSymbol>("destination");
		if (expression.Kind() == SyntaxKind.DeclarationExpression)
		{
			return Conversion.NoConversion;
		}
		if (isExplicitInSource)
		{
			return ClassifyConversionForCast(expression, destination2);
		}
		CheckSyntaxNode(expression);
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		return GetMemberModel(expression)?.ClassifyConversion(expression, destination) ?? Conversion.NoConversion;
	}

	internal override Conversion ClassifyConversionForCast(ExpressionSyntax expression, TypeSymbol destination)
	{
		CheckSyntaxNode(expression);
		if ((object)destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		return GetMemberModel(expression)?.ClassifyConversionForCast(expression, destination) ?? Conversion.NoConversion;
	}

	internal sealed override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, TypeSyntax type, SpeculativeBindingOption bindingOption, out SemanticModel speculativeModel)
	{
		position = CheckAndAdjustPosition(position);
		MemberSemanticModel memberModel = GetMemberModel(position);
		if (memberModel != null)
		{
			return memberModel.TryGetSpeculativeSemanticModelCore(parentModel, position, type, bindingOption, out speculativeModel);
		}
		Binder speculativeBinder = GetSpeculativeBinder(position, type, bindingOption);
		if (speculativeBinder != null)
		{
			speculativeModel = SpeculativeSyntaxTreeSemanticModel.Create(parentModel, type, speculativeBinder, position, bindingOption);
			return true;
		}
		speculativeModel = null;
		return false;
	}

	internal override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, CrefSyntax crefSyntax, out SemanticModel speculativeModel)
	{
		position = CheckAndAdjustPosition(position);
		Binder enclosingBinder = GetEnclosingBinder(position);
		if (enclosingBinder != null && enclosingBinder.InCref)
		{
			speculativeModel = SpeculativeSyntaxTreeSemanticModel.Create(parentModel, crefSyntax, enclosingBinder, position);
			return true;
		}
		speculativeModel = null;
		return false;
	}

	internal sealed override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, StatementSyntax statement, out SemanticModel speculativeModel)
	{
		position = CheckAndAdjustPosition(position);
		MemberSemanticModel memberModel = GetMemberModel(position);
		if (memberModel != null)
		{
			return memberModel.TryGetSpeculativeSemanticModelCore(parentModel, position, statement, out speculativeModel);
		}
		speculativeModel = null;
		return false;
	}

	internal sealed override bool TryGetSpeculativeSemanticModelForMethodBodyCore(SyntaxTreeSemanticModel parentModel, int position, BaseMethodDeclarationSyntax method, out SemanticModel speculativeModel)
	{
		position = CheckAndAdjustPosition(position);
		MemberSemanticModel memberModel = GetMemberModel(position);
		if (memberModel != null)
		{
			return memberModel.TryGetSpeculativeSemanticModelForMethodBodyCore(parentModel, position, method, out speculativeModel);
		}
		speculativeModel = null;
		return false;
	}

	internal sealed override bool TryGetSpeculativeSemanticModelForMethodBodyCore(SyntaxTreeSemanticModel parentModel, int position, AccessorDeclarationSyntax accessor, out SemanticModel speculativeModel)
	{
		position = CheckAndAdjustPosition(position);
		MemberSemanticModel memberModel = GetMemberModel(position);
		if (memberModel != null)
		{
			return memberModel.TryGetSpeculativeSemanticModelForMethodBodyCore(parentModel, position, accessor, out speculativeModel);
		}
		speculativeModel = null;
		return false;
	}

	internal sealed override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, EqualsValueClauseSyntax initializer, out SemanticModel speculativeModel)
	{
		position = CheckAndAdjustPosition(position);
		MemberSemanticModel memberModel = GetMemberModel(position);
		if (memberModel != null)
		{
			return memberModel.TryGetSpeculativeSemanticModelCore(parentModel, position, initializer, out speculativeModel);
		}
		speculativeModel = null;
		return false;
	}

	internal override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, ArrowExpressionClauseSyntax expressionBody, out SemanticModel speculativeModel)
	{
		position = CheckAndAdjustPosition(position);
		MemberSemanticModel memberModel = GetMemberModel(position);
		if (memberModel != null)
		{
			return memberModel.TryGetSpeculativeSemanticModelCore(parentModel, position, expressionBody, out speculativeModel);
		}
		speculativeModel = null;
		return false;
	}

	internal sealed override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, ConstructorInitializerSyntax constructorInitializer, out SemanticModel speculativeModel)
	{
		position = CheckAndAdjustPosition(position);
		MemberSemanticModel memberModel = GetMemberModel(position);
		if (memberModel != null)
		{
			return memberModel.TryGetSpeculativeSemanticModelCore(parentModel, position, constructorInitializer, out speculativeModel);
		}
		speculativeModel = null;
		return false;
	}

	private MemberSemanticModel GetMemberModel(int position)
	{
		CSharpSyntaxNode node = (CSharpSyntaxNode)Root.FindTokenIncludingCrefAndNameAttributes(position).Parent;
		CSharpSyntaxNode memberDeclaration = GetMemberDeclaration(node);
		bool flag = false;
		if (memberDeclaration != null)
		{
			switch (memberDeclaration.Kind())
			{
			case SyntaxKind.GetAccessorDeclaration:
			case SyntaxKind.SetAccessorDeclaration:
			case SyntaxKind.AddAccessorDeclaration:
			case SyntaxKind.RemoveAccessorDeclaration:
				flag = !LookupPosition.IsInBody(position, (AccessorDeclarationSyntax)memberDeclaration);
				break;
			case SyntaxKind.ConstructorDeclaration:
			{
				ConstructorDeclarationSyntax constructorDeclarationSyntax = (ConstructorDeclarationSyntax)memberDeclaration;
				flag = !LookupPosition.IsInConstructorParameterScope(position, constructorDeclarationSyntax) && !LookupPosition.IsInParameterList(position, constructorDeclarationSyntax);
				break;
			}
			case SyntaxKind.MethodDeclaration:
			case SyntaxKind.OperatorDeclaration:
			case SyntaxKind.ConversionOperatorDeclaration:
			case SyntaxKind.DestructorDeclaration:
			{
				BaseMethodDeclarationSyntax baseMethodDeclarationSyntax = (BaseMethodDeclarationSyntax)memberDeclaration;
				flag = !LookupPosition.IsInBody(position, baseMethodDeclarationSyntax) && !LookupPosition.IsInParameterList(position, baseMethodDeclarationSyntax);
				break;
			}
			}
		}
		if (!flag)
		{
			return GetMemberModel(node);
		}
		return null;
	}

	internal override MemberSemanticModel GetMemberModel(SyntaxNode node)
	{
		if (IsInDocumentationComment(node))
		{
			return null;
		}
		CSharpSyntaxNode memberDeclaration = GetMemberDeclaration(node);
		if (memberDeclaration != null)
		{
			TextSpan span = node.Span;
			switch (memberDeclaration.Kind())
			{
			case SyntaxKind.MethodDeclaration:
			case SyntaxKind.OperatorDeclaration:
			case SyntaxKind.ConversionOperatorDeclaration:
			{
				BaseMethodDeclarationSyntax baseMethodDeclarationSyntax = (BaseMethodDeclarationSyntax)memberDeclaration;
				ArrowExpressionClauseSyntax expressionBodySyntax2 = baseMethodDeclarationSyntax.GetExpressionBodySyntax();
				if (expressionBodySyntax2 == null || !expressionBodySyntax2.FullSpan.Contains(span))
				{
					BlockSyntax body3 = baseMethodDeclarationSyntax.Body;
					if (body3 == null || !body3.FullSpan.Contains(span))
					{
						return null;
					}
				}
				return GetOrAddModel(baseMethodDeclarationSyntax);
			}
			case SyntaxKind.ConstructorDeclaration:
			{
				ConstructorDeclarationSyntax constructorDeclarationSyntax = (ConstructorDeclarationSyntax)memberDeclaration;
				ArrowExpressionClauseSyntax expressionBodySyntax3 = constructorDeclarationSyntax.GetExpressionBodySyntax();
				ConstructorInitializerSyntax initializer = constructorDeclarationSyntax.Initializer;
				if ((initializer == null || !initializer.FullSpan.Contains(span)) && (expressionBodySyntax3 == null || !expressionBodySyntax3.FullSpan.Contains(span)))
				{
					BlockSyntax body4 = constructorDeclarationSyntax.Body;
					if (body4 == null || !body4.FullSpan.Contains(span))
					{
						return null;
					}
				}
				return GetOrAddModel(constructorDeclarationSyntax);
			}
			case SyntaxKind.DestructorDeclaration:
			{
				DestructorDeclarationSyntax destructorDeclarationSyntax = (DestructorDeclarationSyntax)memberDeclaration;
				ArrowExpressionClauseSyntax expressionBodySyntax = destructorDeclarationSyntax.GetExpressionBodySyntax();
				if (expressionBodySyntax == null || !expressionBodySyntax.FullSpan.Contains(span))
				{
					BlockSyntax body2 = destructorDeclarationSyntax.Body;
					if (body2 == null || !body2.FullSpan.Contains(span))
					{
						return null;
					}
				}
				return GetOrAddModel(destructorDeclarationSyntax);
			}
			case SyntaxKind.GetAccessorDeclaration:
			case SyntaxKind.SetAccessorDeclaration:
			case SyntaxKind.AddAccessorDeclaration:
			case SyntaxKind.RemoveAccessorDeclaration:
			{
				AccessorDeclarationSyntax accessorDeclarationSyntax = (AccessorDeclarationSyntax)memberDeclaration;
				ArrowExpressionClauseSyntax expressionBody = accessorDeclarationSyntax.ExpressionBody;
				if (expressionBody == null || !expressionBody.FullSpan.Contains(span))
				{
					BlockSyntax body = accessorDeclarationSyntax.Body;
					if (body == null || !body.FullSpan.Contains(span))
					{
						return null;
					}
				}
				return GetOrAddModel(accessorDeclarationSyntax);
			}
			case SyntaxKind.IndexerDeclaration:
			{
				IndexerDeclarationSyntax indexerDeclarationSyntax = (IndexerDeclarationSyntax)memberDeclaration;
				return GetOrAddModelIfContains(indexerDeclarationSyntax.ExpressionBody, span);
			}
			case SyntaxKind.FieldDeclaration:
			case SyntaxKind.EventFieldDeclaration:
				foreach (VariableDeclaratorSyntax variable in ((BaseFieldDeclarationSyntax)memberDeclaration).Declaration.Variables)
				{
					MemberSemanticModel orAddModelIfContains = GetOrAddModelIfContains(variable.Initializer, span);
					if (orAddModelIfContains != null)
					{
						return orAddModelIfContains;
					}
				}
				break;
			case SyntaxKind.EnumMemberDeclaration:
			{
				EnumMemberDeclarationSyntax enumMemberDeclarationSyntax = (EnumMemberDeclarationSyntax)memberDeclaration;
				if (enumMemberDeclarationSyntax.EqualsValue == null)
				{
					return null;
				}
				return GetOrAddModelIfContains(enumMemberDeclarationSyntax.EqualsValue, span);
			}
			case SyntaxKind.PropertyDeclaration:
			{
				PropertyDeclarationSyntax propertyDeclarationSyntax = (PropertyDeclarationSyntax)memberDeclaration;
				return GetOrAddModelIfContains(propertyDeclarationSyntax.Initializer, span) ?? GetOrAddModelIfContains(propertyDeclarationSyntax.ExpressionBody, span);
			}
			case SyntaxKind.GlobalStatement:
				return GetOrAddModel(memberDeclaration);
			case SyntaxKind.Attribute:
				return GetOrAddModelForAttribute((AttributeSyntax)memberDeclaration);
			case SyntaxKind.Parameter:
				return GetOrAddModelForParameter((ParameterSyntax)memberDeclaration, span);
			}
		}
		return null;
	}

	private MemberSemanticModel GetOrAddModelForAttribute(AttributeSyntax attribute)
	{
		MemberSemanticModel memberSemanticModel = ((attribute.Parent != null) ? GetMemberModel(attribute.Parent) : null);
		if (memberSemanticModel == null)
		{
			return GetOrAddModel(attribute);
		}
		return ImmutableInterlocked.GetOrAdd(ref _memberModels, attribute, (CSharpSyntaxNode node, Binder binder) => CreateModelForAttribute(binder, (AttributeSyntax)node), memberSemanticModel.GetEnclosingBinder(attribute.SpanStart));
	}

	private static bool IsInDocumentationComment(SyntaxNode node)
	{
		for (SyntaxNode syntaxNode = node; syntaxNode != null; syntaxNode = syntaxNode.Parent)
		{
			if (SyntaxFacts.IsDocumentationCommentTrivia(syntaxNode.Kind()))
			{
				return true;
			}
		}
		return false;
	}

	private MemberSemanticModel GetOrAddModelForParameter(ParameterSyntax paramDecl, TextSpan span)
	{
		EqualsValueClauseSyntax equalsValueClauseSyntax = paramDecl.Default;
		MemberSemanticModel memberSemanticModel = ((paramDecl.Parent != null) ? GetMemberModel(paramDecl.Parent) : null);
		if (memberSemanticModel == null)
		{
			return GetOrAddModelIfContains(equalsValueClauseSyntax, span);
		}
		if (equalsValueClauseSyntax != null && equalsValueClauseSyntax.FullSpan.Contains(span))
		{
			ParameterSymbol parameterSymbol = (ParameterSymbol)memberSemanticModel.GetDeclaredSymbol(paramDecl);
			if ((object)parameterSymbol != null)
			{
				return ImmutableInterlocked.GetOrAdd(ref _memberModels, equalsValueClauseSyntax, (CSharpSyntaxNode equalsValue, (CSharpCompilation compilation, ParameterSyntax paramDecl, ParameterSymbol parameterSymbol, MemberSemanticModel containing) tuple) => InitializerSemanticModel.Create(this, tuple.paramDecl, tuple.parameterSymbol, tuple.containing.GetEnclosingBinder(tuple.paramDecl.SpanStart).CreateBinderForParameterDefaultValue(tuple.parameterSymbol, (EqualsValueClauseSyntax)equalsValue)), (Compilation, paramDecl, parameterSymbol, memberSemanticModel));
			}
		}
		return memberSemanticModel;
	}

	private static CSharpSyntaxNode GetMemberDeclaration(SyntaxNode node)
	{
		return node.FirstAncestorOrSelf(s_isMemberDeclarationFunction);
	}

	private MemberSemanticModel GetOrAddModelIfContains(CSharpSyntaxNode node, TextSpan span)
	{
		if (node != null && node.FullSpan.Contains(span))
		{
			return GetOrAddModel(node);
		}
		return null;
	}

	private MemberSemanticModel GetOrAddModel(CSharpSyntaxNode node)
	{
		Func<CSharpSyntaxNode, MemberSemanticModel> createMemberModelFunction = CreateMemberModel;
		return GetOrAddModel(node, createMemberModelFunction);
	}

	internal MemberSemanticModel GetOrAddModel(CSharpSyntaxNode node, Func<CSharpSyntaxNode, MemberSemanticModel> createMemberModelFunction)
	{
		return ImmutableInterlocked.GetOrAdd(ref _memberModels, node, createMemberModelFunction);
	}

	private MemberSemanticModel CreateMemberModel(CSharpSyntaxNode node)
	{
		BinderFlags additionalFlags = BinderFlags.None;
		if (IgnoresAccessibility)
		{
			additionalFlags = BinderFlags.IgnoreAccessibility;
		}
		switch (node.Kind())
		{
		case SyntaxKind.MethodDeclaration:
		case SyntaxKind.OperatorDeclaration:
		case SyntaxKind.ConversionOperatorDeclaration:
		case SyntaxKind.ConstructorDeclaration:
		case SyntaxKind.DestructorDeclaration:
		{
			MemberDeclarationSyntax memberDeclarationSyntax = (MemberDeclarationSyntax)node;
			SourceMemberMethodSymbol sourceMemberMethodSymbol2 = (SourceMemberMethodSymbol)GetDeclaredSymbol(memberDeclarationSyntax);
			ExecutableCodeBinder executableCodeBinder2 = sourceMemberMethodSymbol2?.TryGetBodyBinder(_binderFactory, additionalFlags);
			if (executableCodeBinder2 == null)
			{
				return null;
			}
			return MethodBodySemanticModel.Create(this, sourceMemberMethodSymbol2, executableCodeBinder2, memberDeclarationSyntax);
		}
		case SyntaxKind.GetAccessorDeclaration:
		case SyntaxKind.SetAccessorDeclaration:
		case SyntaxKind.AddAccessorDeclaration:
		case SyntaxKind.RemoveAccessorDeclaration:
		{
			AccessorDeclarationSyntax accessorDeclarationSyntax = (AccessorDeclarationSyntax)node;
			SourceMemberMethodSymbol sourceMemberMethodSymbol3 = (SourceMemberMethodSymbol)GetDeclaredSymbol(accessorDeclarationSyntax);
			ExecutableCodeBinder executableCodeBinder3 = sourceMemberMethodSymbol3?.TryGetBodyBinder(_binderFactory, additionalFlags);
			if (executableCodeBinder3 == null)
			{
				return null;
			}
			return MethodBodySemanticModel.Create(this, sourceMemberMethodSymbol3, executableCodeBinder3, accessorDeclarationSyntax);
		}
		case SyntaxKind.Block:
			ExceptionUtilities.UnexpectedValue(node.Parent);
			break;
		case SyntaxKind.EqualsValueClause:
			switch (node.Parent.Kind())
			{
			case SyntaxKind.VariableDeclarator:
			{
				VariableDeclaratorSyntax variableDeclaratorSyntax = (VariableDeclaratorSyntax)node.Parent;
				SourceMemberFieldSymbol declaredFieldSymbol = GetDeclaredFieldSymbol(variableDeclaratorSyntax);
				return InitializerSemanticModel.Create(this, variableDeclaratorSyntax, declaredFieldSymbol, GetFieldOrPropertyInitializerBinder(declaredFieldSymbol, defaultOuter(), variableDeclaratorSyntax.Initializer));
			}
			case SyntaxKind.PropertyDeclaration:
			{
				PropertyDeclarationSyntax propertyDeclarationSyntax = (PropertyDeclarationSyntax)node.Parent;
				SourcePropertySymbol sourcePropertySymbol = (SourcePropertySymbol)GetDeclaredSymbol(propertyDeclarationSyntax);
				return InitializerSemanticModel.Create(this, propertyDeclarationSyntax, sourcePropertySymbol, GetFieldOrPropertyInitializerBinder(sourcePropertySymbol.BackingField, defaultOuter(), propertyDeclarationSyntax.Initializer));
			}
			case SyntaxKind.Parameter:
			{
				ParameterSyntax parameterSyntax = (ParameterSyntax)node.Parent;
				ParameterSymbol declaredNonLambdaParameterSymbol = GetDeclaredNonLambdaParameterSymbol(parameterSyntax);
				if ((object)declaredNonLambdaParameterSymbol == null)
				{
					return null;
				}
				return InitializerSemanticModel.Create(this, parameterSyntax, declaredNonLambdaParameterSymbol, defaultOuter().CreateBinderForParameterDefaultValue(declaredNonLambdaParameterSymbol, (EqualsValueClauseSyntax)node));
			}
			case SyntaxKind.EnumMemberDeclaration:
			{
				EnumMemberDeclarationSyntax enumMemberDeclarationSyntax = (EnumMemberDeclarationSyntax)node.Parent;
				FieldSymbol fieldSymbol = (FieldSymbol)GetDeclaredSymbol(enumMemberDeclarationSyntax);
				if ((object)fieldSymbol == null)
				{
					return null;
				}
				return InitializerSemanticModel.Create(this, enumMemberDeclarationSyntax, fieldSymbol, GetFieldOrPropertyInitializerBinder(fieldSymbol, defaultOuter(), enumMemberDeclarationSyntax.EqualsValue));
			}
			default:
				throw ExceptionUtilities.UnexpectedValue(node.Parent.Kind());
			}
		case SyntaxKind.ArrowExpressionClause:
		{
			SourceMemberMethodSymbol sourceMemberMethodSymbol = null;
			ArrowExpressionClauseSyntax arrowExpressionClauseSyntax = (ArrowExpressionClauseSyntax)node;
			if (node.Parent is BasePropertyDeclarationSyntax)
			{
				sourceMemberMethodSymbol = (SourceMemberMethodSymbol)GetDeclaredSymbol(arrowExpressionClauseSyntax);
			}
			else
			{
				ExceptionUtilities.UnexpectedValue(node.Parent);
			}
			ExecutableCodeBinder executableCodeBinder = sourceMemberMethodSymbol?.TryGetBodyBinder(_binderFactory, additionalFlags);
			if (executableCodeBinder == null)
			{
				return null;
			}
			return MethodBodySemanticModel.Create(this, sourceMemberMethodSymbol, executableCodeBinder, arrowExpressionClauseSyntax);
		}
		case SyntaxKind.GlobalStatement:
		{
			CSharpSyntaxNode parent = node.Parent;
			if (parent.Kind() == SyntaxKind.CompilationUnit && !IsRegularCSharp && (object)_compilation.ScriptClass != null)
			{
				SynthesizedInteractiveInitializerMethod scriptInitializer = _compilation.ScriptClass.GetScriptInitializer();
				if ((object)scriptInitializer == null)
				{
					return null;
				}
				if (_globalStatementLabels == null)
				{
					Interlocked.CompareExchange(ref _globalStatementLabels, new ScriptLocalScopeBinder.Labels(scriptInitializer, (CompilationUnitSyntax)parent), null);
				}
				return MethodBodySemanticModel.Create(this, scriptInitializer, new ExecutableCodeBinder(node, scriptInitializer, new ScriptLocalScopeBinder(_globalStatementLabels, defaultOuter())), node);
			}
			break;
		}
		case SyntaxKind.Attribute:
			return CreateModelForAttribute(defaultOuter(), (AttributeSyntax)node);
		}
		return null;
		Binder defaultOuter()
		{
			return _binderFactory.GetBinder(node).WithAdditionalFlags(additionalFlags);
		}
	}

	private AttributeSemanticModel CreateModelForAttribute(Binder enclosingBinder, AttributeSyntax attribute)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		NamedTypeSymbol attributeType = (NamedTypeSymbol)enclosingBinder.BindType(attribute.Name, instance, out var alias);
		instance.Free();
		return AttributeSemanticModel.Create(this, attribute, attributeType, alias, enclosingBinder.WithAdditionalFlags(BinderFlags.AttributeArgument));
	}

	private SourceMemberFieldSymbol GetDeclaredFieldSymbol(VariableDeclaratorSyntax variableDecl)
	{
		ISymbol declaredSymbol = GetDeclaredSymbol(variableDecl);
		if (declaredSymbol != null)
		{
			switch (variableDecl.Parent.Parent.Kind())
			{
			case SyntaxKind.FieldDeclaration:
				return (SourceMemberFieldSymbol)declaredSymbol;
			case SyntaxKind.EventFieldDeclaration:
				return (SourceMemberFieldSymbol)((EventSymbol)declaredSymbol).AssociatedField;
			}
		}
		return null;
	}

	private Binder GetFieldOrPropertyInitializerBinder(FieldSymbol symbol, Binder outer, EqualsValueClauseSyntax initializer)
	{
		BinderFlags binderFlags = BinderFlags.None;
		if (IsRegularCSharp || !symbol.ContainingType.IsScriptClass)
		{
			binderFlags |= BinderFlags.FieldInitializer;
		}
		outer = new LocalScopeBinder(outer).WithAdditionalFlagsAndContainingMemberOrLambda(binderFlags, symbol);
		if (initializer != null)
		{
			outer = new ExecutableCodeBinder(initializer, symbol, outer);
		}
		return outer;
	}

	private static bool IsMemberDeclaration(CSharpSyntaxNode node)
	{
		if (!(node is MemberDeclarationSyntax) && !(node is AccessorDeclarationSyntax) && node.Kind() != SyntaxKind.Attribute)
		{
			return node.Kind() == SyntaxKind.Parameter;
		}
		return true;
	}

	public override INamespaceSymbol GetDeclaredSymbol(NamespaceDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declarationSyntax);
		return GetDeclaredNamespace(declarationSyntax);
	}

	private NamespaceSymbol GetDeclaredNamespace(NamespaceDeclarationSyntax declarationSyntax)
	{
		NamespaceOrTypeSymbol container = ((declarationSyntax.Parent.Kind() != SyntaxKind.CompilationUnit) ? GetDeclaredNamespaceOrType(declarationSyntax.Parent) : _compilation.Assembly.GlobalNamespace);
		NamespaceSymbol namespaceSymbol = (NamespaceSymbol)GetDeclaredMember(container, declarationSyntax.Span, declarationSyntax.Name);
		return _compilation.GetCompilationNamespace(namespaceSymbol);
	}

	public override INamedTypeSymbol GetDeclaredSymbol(BaseTypeDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declarationSyntax);
		return GetDeclaredType(declarationSyntax);
	}

	public override INamedTypeSymbol GetDeclaredSymbol(DelegateDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declarationSyntax);
		return GetDeclaredType(declarationSyntax);
	}

	private NamedTypeSymbol GetDeclaredType(BaseTypeDeclarationSyntax declarationSyntax)
	{
		string valueText = declarationSyntax.Identifier.ValueText;
		return GetDeclaredNamedType(declarationSyntax, valueText);
	}

	private NamedTypeSymbol GetDeclaredType(DelegateDeclarationSyntax declarationSyntax)
	{
		string valueText = declarationSyntax.Identifier.ValueText;
		return GetDeclaredNamedType(declarationSyntax, valueText);
	}

	private NamedTypeSymbol GetDeclaredNamedType(CSharpSyntaxNode declarationSyntax, string name)
	{
		NamespaceOrTypeSymbol declaredTypeMemberContainer = GetDeclaredTypeMemberContainer(declarationSyntax);
		return GetDeclaredMember(declaredTypeMemberContainer, declarationSyntax.Span, name) as NamedTypeSymbol;
	}

	private NamespaceOrTypeSymbol GetDeclaredNamespaceOrType(CSharpSyntaxNode declarationSyntax)
	{
		if (declarationSyntax is NamespaceDeclarationSyntax declarationSyntax2)
		{
			return GetDeclaredNamespace(declarationSyntax2);
		}
		if (declarationSyntax is BaseTypeDeclarationSyntax declarationSyntax3)
		{
			return GetDeclaredType(declarationSyntax3);
		}
		if (declarationSyntax is DelegateDeclarationSyntax declarationSyntax4)
		{
			return GetDeclaredType(declarationSyntax4);
		}
		return null;
	}

	public override ISymbol GetDeclaredSymbol(MemberDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declarationSyntax);
		switch (declarationSyntax.Kind())
		{
		case SyntaxKind.GlobalStatement:
			return null;
		case SyntaxKind.IncompleteMember:
			return null;
		case SyntaxKind.FieldDeclaration:
		case SyntaxKind.EventFieldDeclaration:
			return null;
		default:
			return GetDeclaredNamespaceOrType(declarationSyntax) ?? GetDeclaredMemberSymbol(declarationSyntax);
		}
	}

	public override ISymbol GetDeclaredSymbol(LocalFunctionStatementSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declarationSyntax);
		return GetMemberModel(declarationSyntax)?.GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public override IFieldSymbol GetDeclaredSymbol(EnumMemberDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (IFieldSymbol)GetDeclaredMemberSymbol(declarationSyntax);
	}

	public override IMethodSymbol GetDeclaredSymbol(BaseMethodDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (IMethodSymbol)GetDeclaredMemberSymbol(declarationSyntax);
	}

	public override ISymbol GetDeclaredSymbol(BasePropertyDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDeclaredMemberSymbol(declarationSyntax);
	}

	public override IPropertySymbol GetDeclaredSymbol(PropertyDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (IPropertySymbol)GetDeclaredMemberSymbol(declarationSyntax);
	}

	public override IPropertySymbol GetDeclaredSymbol(IndexerDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (IPropertySymbol)GetDeclaredMemberSymbol(declarationSyntax);
	}

	public override IEventSymbol GetDeclaredSymbol(EventDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (IEventSymbol)GetDeclaredMemberSymbol(declarationSyntax);
	}

	public override IMethodSymbol GetDeclaredSymbol(AccessorDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declarationSyntax);
		if (declarationSyntax.Kind() == SyntaxKind.UnknownAccessorDeclaration)
		{
			return null;
		}
		CSharpSyntaxNode parent = declarationSyntax.Parent.Parent;
		SyntaxKind syntaxKind = parent.Kind();
		if (syntaxKind == SyntaxKind.EventFieldDeclaration || syntaxKind - 8892 <= (SyntaxKind)2)
		{
			NamespaceOrTypeSymbol declaredTypeMemberContainer = GetDeclaredTypeMemberContainer(parent);
			return GetDeclaredMember(declaredTypeMemberContainer, declarationSyntax.Span) as MethodSymbol;
		}
		throw ExceptionUtilities.UnexpectedValue(parent.Kind());
	}

	public override IMethodSymbol GetDeclaredSymbol(ArrowExpressionClauseSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declarationSyntax);
		CSharpSyntaxNode parent = declarationSyntax.Parent;
		SyntaxKind syntaxKind = parent.Kind();
		if (syntaxKind == SyntaxKind.PropertyDeclaration || syntaxKind == SyntaxKind.IndexerDeclaration)
		{
			NamespaceOrTypeSymbol declaredTypeMemberContainer = GetDeclaredTypeMemberContainer(parent);
			return GetDeclaredMember(declaredTypeMemberContainer, declarationSyntax.Span) as MethodSymbol;
		}
		ExceptionUtilities.UnexpectedValue(parent.Kind());
		return null;
	}

	private string GetDeclarationName(CSharpSyntaxNode declaration)
	{
		switch (declaration.Kind())
		{
		case SyntaxKind.MethodDeclaration:
		{
			MethodDeclarationSyntax methodDeclarationSyntax = (MethodDeclarationSyntax)declaration;
			return GetDeclarationName(declaration, methodDeclarationSyntax.ExplicitInterfaceSpecifier, methodDeclarationSyntax.Identifier.ValueText);
		}
		case SyntaxKind.PropertyDeclaration:
		{
			PropertyDeclarationSyntax propertyDeclarationSyntax = (PropertyDeclarationSyntax)declaration;
			return GetDeclarationName(declaration, propertyDeclarationSyntax.ExplicitInterfaceSpecifier, propertyDeclarationSyntax.Identifier.ValueText);
		}
		case SyntaxKind.IndexerDeclaration:
		{
			IndexerDeclarationSyntax indexerDeclarationSyntax = (IndexerDeclarationSyntax)declaration;
			return GetDeclarationName(declaration, indexerDeclarationSyntax.ExplicitInterfaceSpecifier, "this[]");
		}
		case SyntaxKind.EventDeclaration:
		{
			EventDeclarationSyntax eventDeclarationSyntax = (EventDeclarationSyntax)declaration;
			return GetDeclarationName(declaration, eventDeclarationSyntax.ExplicitInterfaceSpecifier, eventDeclarationSyntax.Identifier.ValueText);
		}
		case SyntaxKind.DelegateDeclaration:
			return ((DelegateDeclarationSyntax)declaration).Identifier.ValueText;
		case SyntaxKind.ClassDeclaration:
		case SyntaxKind.StructDeclaration:
		case SyntaxKind.InterfaceDeclaration:
		case SyntaxKind.EnumDeclaration:
			return ((BaseTypeDeclarationSyntax)declaration).Identifier.ValueText;
		case SyntaxKind.VariableDeclarator:
			return ((VariableDeclaratorSyntax)declaration).Identifier.ValueText;
		case SyntaxKind.EnumMemberDeclaration:
			return ((EnumMemberDeclarationSyntax)declaration).Identifier.ValueText;
		case SyntaxKind.DestructorDeclaration:
			return "Finalize";
		case SyntaxKind.ConstructorDeclaration:
			if (((ConstructorDeclarationSyntax)declaration).Modifiers.Any(SyntaxKind.StaticKeyword))
			{
				return ".cctor";
			}
			return ".ctor";
		case SyntaxKind.OperatorDeclaration:
			return OperatorFacts.OperatorNameFromDeclaration((OperatorDeclarationSyntax)declaration);
		case SyntaxKind.ConversionOperatorDeclaration:
			if (((ConversionOperatorDeclarationSyntax)declaration).ImplicitOrExplicitKeyword.Kind() == SyntaxKind.ExplicitKeyword)
			{
				return "op_Explicit";
			}
			return "op_Implicit";
		case SyntaxKind.FieldDeclaration:
		case SyntaxKind.EventFieldDeclaration:
			throw new ArgumentException(CSharpResources.InvalidGetDeclarationNameMultipleDeclarators);
		case SyntaxKind.IncompleteMember:
			return null;
		default:
			throw ExceptionUtilities.UnexpectedValue(declaration.Kind());
		}
	}

	private string GetDeclarationName(CSharpSyntaxNode declaration, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifierOpt, string memberName)
	{
		if (explicitInterfaceSpecifierOpt == null)
		{
			return memberName;
		}
		return ExplicitInterfaceHelpers.GetMemberName(_binderFactory.GetBinder(declaration), explicitInterfaceSpecifierOpt, memberName);
	}

	private Symbol GetDeclaredMember(NamespaceOrTypeSymbol container, TextSpan declarationSpan, NameSyntax name)
	{
		switch (name.Kind())
		{
		case SyntaxKind.IdentifierName:
		case SyntaxKind.GenericName:
			return GetDeclaredMember(container, declarationSpan, ((SimpleNameSyntax)name).Identifier.ValueText);
		case SyntaxKind.QualifiedName:
		{
			QualifiedNameSyntax qualifiedNameSyntax = (QualifiedNameSyntax)name;
			NamespaceOrTypeSymbol container2 = GetDeclaredMember(container, declarationSpan, qualifiedNameSyntax.Left) as NamespaceOrTypeSymbol;
			return GetDeclaredMember(container2, declarationSpan, qualifiedNameSyntax.Right);
		}
		case SyntaxKind.AliasQualifiedName:
		{
			AliasQualifiedNameSyntax aliasQualifiedNameSyntax = (AliasQualifiedNameSyntax)name;
			return GetDeclaredMember(container, declarationSpan, aliasQualifiedNameSyntax.Name);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(name.Kind());
		}
	}

	private Symbol GetDeclaredMember(NamespaceOrTypeSymbol container, TextSpan declarationSpan, string name = null)
	{
		if ((object)container == null)
		{
			return null;
		}
		ImmutableArray<Symbol> immutableArray = ((name != null) ? container.GetMembers(name) : container.GetMembersUnordered());
		Symbol symbol = null;
		foreach (Symbol item in immutableArray)
		{
			if (item is ImplicitNamedTypeSymbol { IsImplicitClass: not false } implicitNamedTypeSymbol)
			{
				Symbol declaredMember = GetDeclaredMember(implicitNamedTypeSymbol, declarationSpan, name);
				if ((object)declaredMember != null)
				{
					return declaredMember;
				}
			}
			foreach (Location location2 in item.Locations)
			{
				if (location2.IsInSource && location2.SourceTree == SyntaxTree && declarationSpan.Contains(location2.SourceSpan))
				{
					if (!location2.SourceSpan.IsEmpty || location2.SourceSpan.End != declarationSpan.Start)
					{
						return item;
					}
					symbol = item;
				}
			}
			MethodSymbol methodSymbol = ((item.Kind == SymbolKind.Method) ? ((MethodSymbol)item).PartialImplementationPart : null);
			if ((object)methodSymbol != null)
			{
				Location location = methodSymbol.Locations[0];
				if (location.IsInSource && location.SourceTree == SyntaxTree && declarationSpan.Contains(location.SourceSpan))
				{
					return methodSymbol;
				}
			}
		}
		Symbol symbol2 = symbol;
		if ((object)symbol2 == null)
		{
			if (name == null)
			{
				return null;
			}
			symbol2 = GetDeclaredMember(container, declarationSpan);
		}
		return symbol2;
	}

	public override ISymbol GetDeclaredSymbol(VariableDeclaratorSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declarationSyntax);
		BaseFieldDeclarationSyntax baseFieldDeclarationSyntax = ((declarationSyntax.Parent == null) ? null : (declarationSyntax.Parent.Parent as BaseFieldDeclarationSyntax));
		if (baseFieldDeclarationSyntax != null)
		{
			NamespaceOrTypeSymbol declaredTypeMemberContainer = GetDeclaredTypeMemberContainer(baseFieldDeclarationSyntax);
			return GetDeclaredMember(declaredTypeMemberContainer, declarationSyntax.Span, declarationSyntax.Identifier.ValueText);
		}
		return GetMemberModel(declarationSyntax)?.GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public override ISymbol GetDeclaredSymbol(SingleVariableDesignationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		ISymbol symbol = GetMemberModel(declarationSyntax)?.GetDeclaredSymbol(declarationSyntax, cancellationToken);
		if (symbol != null)
		{
			return symbol;
		}
		return GetEnclosingBinder(declarationSyntax.Position)?.LookupDeclaredField(declarationSyntax);
	}

	public override ILabelSymbol GetDeclaredSymbol(LabeledStatementSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declarationSyntax);
		return GetMemberModel(declarationSyntax)?.GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public override ILabelSymbol GetDeclaredSymbol(SwitchLabelSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declarationSyntax);
		return GetMemberModel(declarationSyntax)?.GetDeclaredSymbol(declarationSyntax, cancellationToken);
	}

	public override IAliasSymbol GetDeclaredSymbol(UsingDirectiveSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declarationSyntax);
		if (declarationSyntax.Alias == null)
		{
			return null;
		}
		InContainerBinder importsBinder = _binderFactory.GetImportsBinder(declarationSyntax.Parent);
		AliasAndUsingDirective aliasAndUsingDirective = importsBinder.GetImports(null).UsingAliases[declarationSyntax.Alias.Name.Identifier.ValueText];
		if ((object)aliasAndUsingDirective.Alias == null)
		{
			return null;
		}
		if (aliasAndUsingDirective.Alias.Locations[0].SourceSpan == declarationSyntax.Alias.Name.Span)
		{
			return aliasAndUsingDirective.Alias;
		}
		return new AliasSymbol(importsBinder, declarationSyntax);
	}

	public override IAliasSymbol GetDeclaredSymbol(ExternAliasDirectiveSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declarationSyntax);
		InContainerBinder importsBinder = _binderFactory.GetImportsBinder(declarationSyntax.Parent);
		foreach (AliasAndExternAliasDirective externAlias in importsBinder.GetImports(null).ExternAliases)
		{
			if (externAlias.Alias.Locations[0].SourceSpan == declarationSyntax.Identifier.Span)
			{
				return externAlias.Alias;
			}
		}
		return new AliasSymbol(importsBinder, declarationSyntax);
	}

	internal override ImmutableArray<ISymbol> GetDeclaredSymbols(BaseFieldDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declarationSyntax);
		ArrayBuilder<ISymbol> arrayBuilder = new ArrayBuilder<ISymbol>();
		foreach (VariableDeclaratorSyntax variable in declarationSyntax.Declaration.Variables)
		{
			ISymbol declaredSymbol = GetDeclaredSymbol(variable, cancellationToken);
			if (declaredSymbol != null)
			{
				arrayBuilder.Add(declaredSymbol);
			}
		}
		return arrayBuilder.ToImmutableAndFree();
	}

	private ParameterSymbol GetMethodParameterSymbol(ParameterSyntax parameter, CancellationToken cancellationToken)
	{
		if (!(parameter.Parent is ParameterListSyntax parameterListSyntax))
		{
			return null;
		}
		if (!(parameterListSyntax.Parent is MemberDeclarationSyntax declarationSyntax))
		{
			return null;
		}
		if (!(GetDeclaredSymbol(declarationSyntax, cancellationToken) is MethodSymbol methodSymbol))
		{
			return null;
		}
		object obj = GetParameterSymbol(methodSymbol.Parameters, parameter, cancellationToken);
		if (obj == null)
		{
			if ((object)methodSymbol.PartialDefinitionPart != null)
			{
				return GetParameterSymbol(methodSymbol.PartialDefinitionPart.Parameters, parameter, cancellationToken);
			}
			obj = null;
		}
		return (ParameterSymbol)obj;
	}

	private ParameterSymbol GetIndexerParameterSymbol(ParameterSyntax parameter, CancellationToken cancellationToken)
	{
		if (!(parameter.Parent is BracketedParameterListSyntax bracketedParameterListSyntax))
		{
			return null;
		}
		if (!(bracketedParameterListSyntax.Parent is MemberDeclarationSyntax declarationSyntax))
		{
			return null;
		}
		if (!(GetDeclaredSymbol(declarationSyntax, cancellationToken) is PropertySymbol propertySymbol))
		{
			return null;
		}
		return GetParameterSymbol(propertySymbol.Parameters, parameter, cancellationToken);
	}

	private ParameterSymbol GetDelegateParameterSymbol(ParameterSyntax parameter, CancellationToken cancellationToken)
	{
		if (!(parameter.Parent is ParameterListSyntax parameterListSyntax))
		{
			return null;
		}
		if (!(parameterListSyntax.Parent is DelegateDeclarationSyntax declarationSyntax))
		{
			return null;
		}
		if (!(GetDeclaredSymbol(declarationSyntax, cancellationToken) is NamedTypeSymbol { DelegateInvokeMethod: var delegateInvokeMethod }))
		{
			return null;
		}
		if ((object)delegateInvokeMethod == null || delegateInvokeMethod.HasUseSiteError)
		{
			return null;
		}
		return GetParameterSymbol(delegateInvokeMethod.Parameters, parameter, cancellationToken);
	}

	public override IParameterSymbol GetDeclaredSymbol(ParameterSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declarationSyntax);
		MemberSemanticModel memberModel = GetMemberModel(declarationSyntax);
		if (memberModel != null)
		{
			return memberModel.GetDeclaredSymbol(declarationSyntax, cancellationToken);
		}
		return GetDeclaredNonLambdaParameterSymbol(declarationSyntax, cancellationToken);
	}

	private ParameterSymbol GetDeclaredNonLambdaParameterSymbol(ParameterSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetMethodParameterSymbol(declarationSyntax, cancellationToken) ?? GetIndexerParameterSymbol(declarationSyntax, cancellationToken) ?? GetDelegateParameterSymbol(declarationSyntax, cancellationToken);
	}

	public override ITypeParameterSymbol GetDeclaredSymbol(TypeParameterSyntax typeParameter, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (typeParameter == null)
		{
			throw new ArgumentNullException("typeParameter");
		}
		if (!IsInTree(typeParameter))
		{
			throw new ArgumentException("typeParameter not within tree");
		}
		if (typeParameter.Parent is TypeParameterListSyntax { Parent: var parent })
		{
			if (parent == null)
			{
				goto IL_0076;
			}
			ISymbol declaredSymbol;
			if (!(parent is MemberDeclarationSyntax memberDeclarationSyntax))
			{
				if (!(parent is LocalFunctionStatementSyntax localFunctionStatementSyntax))
				{
					goto IL_0076;
				}
				LocalFunctionStatementSyntax declarationSyntax = localFunctionStatementSyntax;
				declaredSymbol = GetDeclaredSymbol(declarationSyntax, cancellationToken);
			}
			else
			{
				MemberDeclarationSyntax declarationSyntax2 = memberDeclarationSyntax;
				declaredSymbol = GetDeclaredSymbol(declarationSyntax2, cancellationToken);
			}
			ISymbol symbol = declaredSymbol;
			if (symbol != null)
			{
				if (symbol is NamedTypeSymbol namedTypeSymbol)
				{
					NamedTypeSymbol namedTypeSymbol2 = namedTypeSymbol;
					return GetTypeParameterSymbol(namedTypeSymbol2.TypeParameters, typeParameter);
				}
				if (symbol is MethodSymbol methodSymbol)
				{
					MethodSymbol methodSymbol2 = methodSymbol;
					object obj = GetTypeParameterSymbol(methodSymbol2.TypeParameters, typeParameter);
					if (obj == null)
					{
						if ((object)methodSymbol2.PartialDefinitionPart != null)
						{
							return GetTypeParameterSymbol(methodSymbol2.PartialDefinitionPart.TypeParameters, typeParameter);
						}
						obj = null;
					}
					return (ITypeParameterSymbol)obj;
				}
			}
		}
		return null;
		IL_0076:
		throw ExceptionUtilities.UnexpectedValue(typeParameter.Parent.Kind());
	}

	private TypeParameterSymbol GetTypeParameterSymbol(ImmutableArray<TypeParameterSymbol> parameters, TypeParameterSyntax parameter)
	{
		foreach (TypeParameterSymbol item in parameters)
		{
			foreach (Location location in item.Locations)
			{
				if (location.SourceTree == SyntaxTree && parameter.Span.Contains(location.SourceSpan))
				{
					return item;
				}
			}
		}
		return null;
	}

	public override ControlFlowAnalysis AnalyzeControlFlow(StatementSyntax firstStatement, StatementSyntax lastStatement)
	{
		ValidateStatementRange(firstStatement, lastStatement);
		return new CSharpControlFlowAnalysis(RegionAnalysisContext(firstStatement, lastStatement));
	}

	private void ValidateStatementRange(StatementSyntax firstStatement, StatementSyntax lastStatement)
	{
		if (firstStatement == null)
		{
			throw new ArgumentNullException("firstStatement");
		}
		if (lastStatement == null)
		{
			throw new ArgumentNullException("lastStatement");
		}
		if (!IsInTree(firstStatement))
		{
			throw new ArgumentException("statements not within tree");
		}
		if (firstStatement.Parent == null || firstStatement.Parent != lastStatement.Parent)
		{
			throw new ArgumentException("statements not within the same statement list");
		}
		if (firstStatement.SpanStart > lastStatement.SpanStart)
		{
			throw new ArgumentException("first statement does not precede last statement");
		}
	}

	public override DataFlowAnalysis AnalyzeDataFlow(ExpressionSyntax expression)
	{
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		if (!IsInTree(expression))
		{
			throw new ArgumentException("expression not within tree");
		}
		return new CSharpDataFlowAnalysis(RegionAnalysisContext(expression));
	}

	public override DataFlowAnalysis AnalyzeDataFlow(StatementSyntax firstStatement, StatementSyntax lastStatement)
	{
		ValidateStatementRange(firstStatement, lastStatement);
		return new CSharpDataFlowAnalysis(RegionAnalysisContext(firstStatement, lastStatement));
	}

	private static BoundNode GetBoundRoot(MemberSemanticModel memberModel, out Symbol member)
	{
		member = memberModel.MemberSymbol;
		return memberModel.GetBoundRoot();
	}

	private NamespaceOrTypeSymbol GetDeclaredTypeMemberContainer(CSharpSyntaxNode memberDeclaration)
	{
		if (memberDeclaration.Parent.Kind() == SyntaxKind.CompilationUnit)
		{
			if (memberDeclaration.Kind() == SyntaxKind.NamespaceDeclaration)
			{
				return _compilation.Assembly.GlobalNamespace;
			}
			if (SyntaxTree.Options.Kind != SourceCodeKind.Regular)
			{
				return Compilation.ScriptClass;
			}
			if (SyntaxFacts.IsTypeDeclaration(memberDeclaration.Kind()))
			{
				return _compilation.Assembly.GlobalNamespace;
			}
			return _compilation.Assembly.GlobalNamespace.ImplicitType;
		}
		NamespaceOrTypeSymbol declaredNamespaceOrType = GetDeclaredNamespaceOrType(memberDeclaration.Parent);
		if (!declaredNamespaceOrType.IsNamespace)
		{
			return declaredNamespaceOrType;
		}
		if (memberDeclaration.Kind() == SyntaxKind.NamespaceDeclaration || SyntaxFacts.IsTypeDeclaration(memberDeclaration.Kind()))
		{
			return declaredNamespaceOrType;
		}
		return ((NamespaceSymbol)declaredNamespaceOrType).ImplicitType;
	}

	private Symbol GetDeclaredMemberSymbol(CSharpSyntaxNode declarationSyntax)
	{
		CheckSyntaxNode(declarationSyntax);
		NamespaceOrTypeSymbol declaredTypeMemberContainer = GetDeclaredTypeMemberContainer(declarationSyntax);
		string declarationName = GetDeclarationName(declarationSyntax);
		return GetDeclaredMember(declaredTypeMemberContainer, declarationSyntax.Span, declarationName);
	}

	public override AwaitExpressionInfo GetAwaitExpressionInfo(AwaitExpressionSyntax node)
	{
		return GetMemberModel(node)?.GetAwaitExpressionInfo(node) ?? default(AwaitExpressionInfo);
	}

	public override ForEachStatementInfo GetForEachStatementInfo(ForEachStatementSyntax node)
	{
		return GetMemberModel(node)?.GetForEachStatementInfo(node) ?? default(ForEachStatementInfo);
	}

	public override ForEachStatementInfo GetForEachStatementInfo(CommonForEachStatementSyntax node)
	{
		return GetMemberModel(node)?.GetForEachStatementInfo(node) ?? default(ForEachStatementInfo);
	}

	public override DeconstructionInfo GetDeconstructionInfo(AssignmentExpressionSyntax node)
	{
		return GetMemberModel(node)?.GetDeconstructionInfo(node) ?? default(DeconstructionInfo);
	}

	public override DeconstructionInfo GetDeconstructionInfo(ForEachVariableStatementSyntax node)
	{
		return GetMemberModel(node)?.GetDeconstructionInfo(node) ?? default(DeconstructionInfo);
	}

	private RegionAnalysisContext RegionAnalysisContext(ExpressionSyntax expression)
	{
		while (expression.Kind() == SyntaxKind.ParenthesizedExpression)
		{
			expression = ((ParenthesizedExpressionSyntax)expression).Expression;
		}
		MemberSemanticModel memberModel = GetMemberModel(expression);
		if (memberModel == null)
		{
			BoundBadStatement boundBadStatement = new BoundBadStatement(expression, ImmutableArray<BoundNode>.Empty, hasErrors: true);
			return new RegionAnalysisContext(Compilation, null, boundBadStatement, boundBadStatement, boundBadStatement);
		}
		BoundNode boundRoot = GetBoundRoot(memberModel, out var member);
		BoundNode upperBoundNode = memberModel.GetUpperBoundNode(expression, promoteToBindable: true);
		BoundNode lastInRegion = upperBoundNode;
		return new RegionAnalysisContext(Compilation, member, boundRoot, upperBoundNode, lastInRegion);
	}

	private RegionAnalysisContext RegionAnalysisContext(StatementSyntax firstStatement, StatementSyntax lastStatement)
	{
		MemberSemanticModel memberModel = GetMemberModel(firstStatement);
		if (memberModel == null)
		{
			BoundBadStatement boundBadStatement = new BoundBadStatement(firstStatement, ImmutableArray<BoundNode>.Empty, hasErrors: true);
			return new RegionAnalysisContext(Compilation, null, boundBadStatement, boundBadStatement, boundBadStatement);
		}
		BoundNode boundRoot = GetBoundRoot(memberModel, out var member);
		BoundNode upperBoundNode = memberModel.GetUpperBoundNode(firstStatement, promoteToBindable: true);
		BoundNode upperBoundNode2 = memberModel.GetUpperBoundNode(lastStatement, promoteToBindable: true);
		return new RegionAnalysisContext(Compilation, member, boundRoot, upperBoundNode, upperBoundNode2);
	}
}
