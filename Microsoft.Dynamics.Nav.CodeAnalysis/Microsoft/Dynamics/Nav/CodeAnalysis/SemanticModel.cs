using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public abstract class SemanticModel
{
	internal enum GetOperationOptions
	{
		Highest,
		Lowest,
		Parent
	}

	public abstract bool IsSpeculativeSemanticModel { get; }

	public abstract int OriginalPositionForSpeculation { get; }

	public Compilation Compilation { get; }

	public SyntaxTree SyntaxTree => SyntaxTreeCore;

	protected abstract SyntaxTree SyntaxTreeCore { get; }

	public virtual bool IgnoresAccessibility => false;

	internal abstract SyntaxNode Root { get; }

	internal SemanticModel(Compilation compilation)
	{
		Compilation = compilation;
	}

	public SymbolInfo GetSymbolInfo(SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetSymbolInfoCore(node, cancellationToken);
	}

	internal virtual ImmutableArray<ISymbol> LookupSymbols(SyntaxNode node, LookupOptions lookupOptions = LookupOptions.Default, IContainerSymbol container = null, string name = null, CancellationToken token = default(CancellationToken))
	{
		if ((lookupOptions & LookupOptions.OnlyPropertyDeclarations) != 0)
		{
			SyntaxNode syntaxNode = ((node.Kind == SyntaxKind.Property) ? node.Parent.Parent : node);
			if (syntaxNode != null)
			{
				LookupSymbolsInfo instance = LookupSymbolsInfo.GetInstance();
				try
				{
					ArrayBuilder<ISymbol> instance2 = ArrayBuilder<ISymbol>.GetInstance(instance.Count);
					foreach (IPropertySymbol declaredProperty in GetDeclaredProperties(syntaxNode))
					{
						instance2.Add(declaredProperty);
					}
					return instance2.ToImmutableAndFree();
				}
				finally
				{
					instance.Free();
				}
			}
		}
		return LookupSymbols(node.Position, lookupOptions, container, name, token);
	}

	internal virtual ImmutableArray<ISymbol> LookupSymbols(int position, LookupOptions lookupOptions = LookupOptions.Default, IContainerSymbol container = null, string name = null, CancellationToken token = default(CancellationToken))
	{
		return LookupSymbolsInternal(position, container, name, lookupOptions, token);
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "token")]
	internal ImmutableArray<ISymbol> LookupSymbolsInternal(Binder binder, IContainerSymbol container = null, string name = null, LookupOptions lookupOptions = LookupOptions.Default, CancellationToken token = default(CancellationToken))
	{
		if (binder == null)
		{
			return ImmutableArray<ISymbol>.Empty;
		}
		LookupSymbolsInfo instance = LookupSymbolsInfo.GetInstance();
		try
		{
			ArrayBuilder<ISymbol> instance2 = ArrayBuilder<ISymbol>.GetInstance(instance.Count);
			TypeSymbol typeSymbol = null;
			if (container == null)
			{
				binder.AddLookupSymbolsInfo(instance, lookupOptions);
			}
			else
			{
				typeSymbol = container as TypeSymbol;
				if (typeSymbol != null)
				{
					Binder.AddMemberLookupSymbolsInfo(instance, typeSymbol, lookupOptions, binder);
				}
			}
			if (name == null)
			{
				foreach (string name2 in instance.Names)
				{
					AppendSymbolsWithName(instance2, name2, binder, typeSymbol, lookupOptions);
				}
			}
			else
			{
				AppendSymbolsWithName(instance2, name, binder, typeSymbol, lookupOptions);
			}
			return instance2.ToImmutableAndFree();
		}
		finally
		{
			instance.Free();
		}
	}

	protected internal virtual ImmutableArray<IPropertySymbol> GetDeclaredProperties(SyntaxNode propertyParent)
	{
		return ImmutableArray<IPropertySymbol>.Empty;
	}

	private ImmutableArray<ISymbol> LookupSymbolsInternal(int position, IContainerSymbol container, string name, LookupOptions options, CancellationToken cancellationToken = default(CancellationToken))
	{
		options.ThrowIfInvalid();
		position = CheckAndAdjustPosition(position);
		Binder enclosingBinder = GetEnclosingBinder(position);
		return LookupSymbolsInternal(enclosingBinder, container, name, options, cancellationToken);
	}

	private void AppendSymbolsWithName(ArrayBuilder<ISymbol> results, string name, Binder binder, TypeSymbol container, LookupOptions options)
	{
		DebugAssertHelper.Assert(results != null);
		LookupResult instance = LookupResult.GetInstance();
		try
		{
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			binder.LookupSymbolsSimpleName(instance, container, name, options, diagnose: false, ref useSiteDiagnostics);
			if (!instance.IsMultiViable)
			{
				return;
			}
			if (instance.Symbols.Any((Symbol t) => t.Kind == SymbolKind.NamedType || t.Kind == SymbolKind.ErrorType))
			{
				DiagnosticBag instance2 = DiagnosticBag.GetInstance();
				try
				{
					Symbol item = binder.ResultSymbol(instance, name, Root, instance2, suppressUseSiteDiagnostics: true, out var wasError, options);
					if (!wasError)
					{
						results.Add(item);
					}
					else
					{
						results.AddRange(instance.Symbols);
					}
					return;
				}
				finally
				{
					instance2.Free();
				}
			}
			results.AddRange(instance.Symbols);
		}
		finally
		{
			instance.Free();
		}
	}

	internal static bool CanGetSemanticInfo(SyntaxNode node)
	{
		DebugAssertHelper.Assert(node != null);
		switch (node.Kind)
		{
		case SyntaxKind.TriggerDeclaration:
		case SyntaxKind.MethodDeclaration:
		case SyntaxKind.EventDeclaration:
		case SyntaxKind.VariableDeclaration:
		case SyntaxKind.Property:
			return true;
		default:
			if (node.IsMissing)
			{
				return false;
			}
			return node is ExpressionSyntax;
		}
	}

	private SymbolInfo GetSymbolInfoCore(SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetSymbolInfoFromNode(node, cancellationToken);
	}

	private SymbolInfo GetSymbolInfoFromNode(SyntaxNode node, CancellationToken cancellationToken)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		if (node is ExpressionSyntax expression)
		{
			return GetSymbolInfo(expression, cancellationToken);
		}
		if (node is PropertyNameSyntax propertyNameSyntax)
		{
			PropertySymbol declaredSymbol = GetDeclaredSymbol(propertyNameSyntax.Parent as PropertySyntax, cancellationToken);
			if (declaredSymbol != null)
			{
				return new SymbolInfo(declaredSymbol);
			}
		}
		return SymbolInfo.None;
	}

	internal SymbolInfo GetSymbolInfoForNode(BoundNode lowestBoundNode, BoundNode highestBoundNode, BoundNode boundNodeForSyntacticParent, Binder binderOpt)
	{
		BoundExpression boundExpression = lowestBoundNode as BoundExpression;
		BoundExpression boundExpression2 = highestBoundNode as BoundExpression;
		if (boundExpression != null)
		{
			ImmutableArray<Symbol> immutableArray = GetSemanticSymbols(boundExpression, boundNodeForSyntacticParent, binderOpt, out var resultKind, out var _);
			if (boundExpression2 != null)
			{
				ImmutableArray<Symbol> semanticSymbols = GetSemanticSymbols(boundExpression2, boundNodeForSyntacticParent, binderOpt, out var resultKind2, out var _);
				if ((immutableArray.Length != 1 || resultKind == LookupResultKind.OverloadResolutionFailure) && semanticSymbols.Length > 0)
				{
					immutableArray = semanticSymbols;
					resultKind = resultKind2;
				}
				else if (resultKind2 != LookupResultKind.Empty && (int)resultKind2 < (int)resultKind)
				{
					resultKind = resultKind2;
				}
			}
			if (resultKind == LookupResultKind.Empty)
			{
				return SymbolInfoFactory.Create(ImmutableArray<Symbol>.Empty, LookupResultKind.Empty);
			}
			ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
			foreach (Symbol item in immutableArray)
			{
				AddUnwrappingErrorTypes(instance, item);
			}
			immutableArray = instance.ToImmutableAndFree();
			if (resultKind == LookupResultKind.Viable && immutableArray.Length > 1)
			{
				resultKind = LookupResultKind.OverloadResolutionFailure;
			}
			return SymbolInfoFactory.Create(immutableArray, resultKind);
		}
		return SymbolInfo.None;
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

	internal static TypeInfo GetTypeInfoForSymbol(Symbol symbol)
	{
		DebugAssertHelper.Assert((object)symbol != null);
		TypeSymbol typeSymbol = symbol as TypeSymbol;
		return new TypeInfo(typeSymbol, typeSymbol);
	}

	internal static SymbolInfo GetSymbolInfoForSymbol(Symbol symbol)
	{
		ErrorTypeSymbol errorTypeSymbol = ((symbol is TypeSymbol typeSymbol) ? (typeSymbol.OriginalDefinition as ErrorTypeSymbol) : null);
		if ((object)errorTypeSymbol != null)
		{
			ImmutableArray<Symbol> symbols = ImmutableArray<Symbol>.Empty;
			LookupResultKind resultKind = errorTypeSymbol.ResultKind;
			if (resultKind != LookupResultKind.Empty)
			{
				symbols = errorTypeSymbol.CandidateSymbols;
			}
			return SymbolInfoFactory.Create(symbols, resultKind);
		}
		return new SymbolInfo(symbol, ImmutableArray<ISymbol>.Empty, CandidateReason.None);
	}

	[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
	public SymbolInfo GetSymbolInfo(ExpressionSyntax expression, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(expression);
		if (!CanGetSemanticInfo(expression))
		{
			return SymbolInfo.None;
		}
		SyntaxNode potentialDeclarationParentNode = GetPotentialDeclarationParentNode(expression);
		if (potentialDeclarationParentNode != null)
		{
			ISymbol declaredSymbol = GetDeclaredSymbol(potentialDeclarationParentNode, cancellationToken);
			if (declaredSymbol != null)
			{
				return new SymbolInfo(declaredSymbol);
			}
		}
		return GetSymbolInfoWorker(expression, cancellationToken);
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	private static SyntaxNode GetPotentialDeclarationParentNode(ExpressionSyntax expression)
	{
		SyntaxNode syntaxNode = expression?.Parent;
		if (syntaxNode == null)
		{
			return null;
		}
		switch (syntaxNode.Kind)
		{
		case SyntaxKind.PageField:
			if ((expression.Parent as PageFieldSyntax)?.Expression == expression)
			{
				return null;
			}
			break;
		case SyntaxKind.ReportColumn:
			if ((expression.Parent as ReportColumnSyntax)?.SourceExpression == expression)
			{
				return null;
			}
			break;
		case SyntaxKind.QueryColumn:
		case SyntaxKind.QueryFilter:
			if ((expression.Parent as QueryElementSyntax)?.RelatedField == expression)
			{
				return null;
			}
			break;
		case SyntaxKind.XmlPortFieldElement:
		case SyntaxKind.XmlPortFieldAttribute:
			if ((expression.Parent as XmlPortFieldNodeSyntax)?.SourceField == expression)
			{
				return null;
			}
			break;
		case SyntaxKind.Key:
		case SyntaxKind.FieldGroup:
			if ((expression.Parent as KeyFieldGroupBaseSyntax)?.Name != expression)
			{
				return null;
			}
			break;
		case SyntaxKind.EventTriggerDeclaration:
			if ((expression.Parent as EventTriggerDeclarationSyntax)?.Publisher == expression)
			{
				return null;
			}
			break;
		case SyntaxKind.QualifiedName:
		{
			SyntaxNode syntaxNode2 = syntaxNode;
			while (syntaxNode2.Kind == SyntaxKind.QualifiedName)
			{
				syntaxNode2 = syntaxNode2.Parent;
			}
			return syntaxNode2;
		}
		}
		if (IsChangeKind(syntaxNode.Kind))
		{
			return null;
		}
		return syntaxNode;
	}

	private static bool IsChangeKind(SyntaxKind kind)
	{
		if (!kind.IsFieldModificationChange() && !kind.IsControlChange() && !kind.IsActionChange())
		{
			return kind.IsViewChange();
		}
		return true;
	}

	internal BinderFlags GetSemanticModelBinderFlags()
	{
		if (!IgnoresAccessibility)
		{
			return BinderFlags.SemanticModel;
		}
		return BinderFlags.SemanticModel | BinderFlags.IgnoreAccessibility;
	}

	internal Binder GetEnclosingBinder(int position)
	{
		Binder enclosingBinderInternal = GetEnclosingBinderInternal(position);
		DebugAssertHelper.Assert(enclosingBinderInternal?.IsSemanticModelBinder ?? true);
		return enclosingBinderInternal;
	}

	internal abstract Binder GetEnclosingBinderInternal(int position);

	internal TypeInfo GetTypeInfo(SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetTypeInfoCore(node, cancellationToken);
	}

	protected TypeInfo GetTypeInfoCore(SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetTypeInfoFromNode(node, cancellationToken);
	}

	private TypeInfo GetTypeInfoFromNode(SyntaxNode node, CancellationToken cancellationToken)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		if (node is ExpressionSyntax expression)
		{
			return GetTypeInfo(expression, cancellationToken);
		}
		return TypeInfo.None;
	}

	private TypeInfo GetTypeInfo(ExpressionSyntax expression, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(expression);
		if (!CanGetSemanticInfo(expression))
		{
			return TypeInfo.None;
		}
		return GetTypeInfoWorker(expression, cancellationToken);
	}

	public abstract ImmutableArray<Diagnostic> GetSyntaxDiagnostics(TextSpan? span = null, CancellationToken cancellationToken = default(CancellationToken));

	public abstract ImmutableArray<Diagnostic> GetDeclarationDiagnostics(TextSpan? span = null, CancellationToken cancellationToken = default(CancellationToken));

	public abstract ImmutableArray<Diagnostic> GetMethodBodyDiagnostics(TextSpan? span = null, CancellationToken cancellationToken = default(CancellationToken));

	public abstract ImmutableArray<Diagnostic> GetDiagnostics(TextSpan? span = null, CancellationToken cancellationToken = default(CancellationToken));

	internal ISymbol GetDeclaredSymbolForNode(SyntaxNode declaration, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDeclaredSymbolCore(declaration, cancellationToken);
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	protected internal ISymbol GetDeclaredSymbolCore(SyntaxNode declaration, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (declaration is MethodOrTriggerDeclarationSyntax declarationSyntax)
		{
			return GetDeclaredSymbol(declarationSyntax, cancellationToken);
		}
		if (declaration is EventDeclarationSyntax declarationSyntax2)
		{
			return GetDeclaredSymbol(declarationSyntax2, cancellationToken);
		}
		if (declaration.Kind.IsObject())
		{
			return GetDeclaredSymbol((ObjectSyntax)declaration, cancellationToken);
		}
		switch (declaration.Kind)
		{
		default:
			return null;
		case SyntaxKind.Property:
			return GetDeclaredSymbol((PropertySyntax)declaration, cancellationToken);
		case SyntaxKind.VariableDeclaration:
			return GetDeclaredSymbol((VariableDeclarationSyntax)declaration, cancellationToken);
		case SyntaxKind.Field:
			return GetDeclaredSymbol((FieldSyntax)declaration, cancellationToken);
		case SyntaxKind.PageArea:
		case SyntaxKind.PageGroup:
		case SyntaxKind.PageField:
		case SyntaxKind.PageLabel:
		case SyntaxKind.PagePart:
		case SyntaxKind.PageSystemPart:
		case SyntaxKind.PageChartPart:
		case SyntaxKind.PageUserControl:
			return GetDeclaredSymbol((ControlBaseSyntax)declaration, cancellationToken);
		case SyntaxKind.PageView:
			return GetDeclaredSymbol((PageViewSyntax)declaration, cancellationToken);
		case SyntaxKind.PageAction:
		case SyntaxKind.PageActionGroup:
		case SyntaxKind.PageActionArea:
			return GetDeclaredSymbol((ActionBaseSyntax)declaration, cancellationToken);
		case SyntaxKind.Parameter:
			return GetDeclaredSymbol((ParameterSyntax)declaration, cancellationToken);
		case SyntaxKind.ReturnValue:
			return GetDeclaredSymbol((ReturnValueSyntax)declaration, cancellationToken);
		case SyntaxKind.Key:
			return GetDeclaredSymbol((KeySyntax)declaration, cancellationToken);
		case SyntaxKind.FieldGroup:
			return GetDeclaredSymbol((FieldGroupSyntax)declaration, cancellationToken);
		case SyntaxKind.MemberAttribute:
			return GetDeclaredSymbol((MemberAttributeSyntax)declaration, cancellationToken);
		case SyntaxKind.ControlAddChange:
		case SyntaxKind.ControlMoveChange:
		case SyntaxKind.ControlModifyChange:
			return GetDeclaredSymbol((ControlChangeBaseSyntax)declaration, cancellationToken);
		case SyntaxKind.ActionAddChange:
		case SyntaxKind.ActionMoveChange:
		case SyntaxKind.ActionModifyChange:
			return GetDeclaredSymbol((ActionChangeBaseSyntax)declaration, cancellationToken);
		case SyntaxKind.ViewAddChange:
		case SyntaxKind.ViewMoveChange:
		case SyntaxKind.ViewModifyChange:
			return GetDeclaredSymbol((ViewChangeBaseSyntax)declaration, cancellationToken);
		case SyntaxKind.FieldModification:
			return GetDeclaredSymbol((FieldModificationSyntax)declaration, cancellationToken);
		case SyntaxKind.ObjectReference:
			return GetDeclaredSymbol((ObjectNameOrIdSyntax)declaration, cancellationToken);
		case SyntaxKind.ObjectNameReference:
			return GetDeclaredSymbol((ObjectNameReferenceSyntax)declaration, cancellationToken);
		case SyntaxKind.OptionDataType:
			return GetDeclaredSymbol((OptionDataTypeSyntax)declaration, cancellationToken);
		case SyntaxKind.EnumDataType:
			return GetDeclaredSymbol((EnumDataTypeSyntax)declaration, cancellationToken);
		case SyntaxKind.EnumValue:
			return GetDeclaredSymbol((EnumValueSyntax)declaration, cancellationToken);
		case SyntaxKind.ReportLabel:
		case SyntaxKind.ReportLabelMultilanguage:
			return GetDeclaredSymbol((ReportLabelBaseSyntax)declaration, cancellationToken);
		case SyntaxKind.ReportDataItem:
			return GetDeclaredSymbol((ReportDataItemSyntax)declaration, cancellationToken);
		case SyntaxKind.ReportColumn:
			return GetDeclaredSymbol((ReportColumnSyntax)declaration, cancellationToken);
		case SyntaxKind.XmlPortTableElement:
		case SyntaxKind.XmlPortFieldElement:
		case SyntaxKind.XmlPortTextElement:
		case SyntaxKind.XmlPortFieldAttribute:
		case SyntaxKind.XmlPortTextAttribute:
			return GetDeclaredSymbol((XmlPortNodeSyntax)declaration, cancellationToken);
		case SyntaxKind.DotNetAssembly:
			return GetDeclaredSymbol((DotNetAssemblySyntax)declaration, cancellationToken);
		case SyntaxKind.DotNetTypeDeclaration:
			return GetDeclaredSymbol((DotNetTypeDeclarationSyntax)declaration, cancellationToken);
		case SyntaxKind.QueryFilter:
			return GetDeclaredSymbol((QueryFilterSyntax)declaration, cancellationToken);
		case SyntaxKind.QueryColumn:
			return GetDeclaredSymbol((QueryColumnSyntax)declaration, cancellationToken);
		case SyntaxKind.QueryDataItem:
			return GetDeclaredSymbol((QueryDataItemSyntax)declaration, cancellationToken);
		}
	}

	internal ParameterSymbol GetParameterSymbol(ImmutableArray<ParameterSymbol> parameters, ParameterSyntax parameter, CancellationToken cancellationToken = default(CancellationToken))
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

	internal abstract IObjectTypeSymbol GetDeclaredSymbol(ObjectSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	internal abstract IMethodSymbol GetDeclaredSymbol(MethodOrTriggerDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken));

	internal virtual IEventSymbol GetDeclaredSymbol(EventDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual IAttributeSymbol GetDeclaredSymbol(MemberAttributeSyntax attributeSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual PropertySymbol GetDeclaredSymbol(PropertySyntax propertySyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual OptionSymbol GetDeclaredSymbol(EnumValueSyntax enumValueSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual VariableSymbol GetDeclaredSymbol(VariableDeclarationSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual FieldSymbol GetDeclaredSymbol(FieldSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual TypeSymbol GetDeclaredSymbol(ReportDataItemSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual TypeSymbol GetDeclaredSymbol(ReportColumnSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual QueryFilterSymbol GetDeclaredSymbol(QueryFilterSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual DotNetAssemblySymbol GetDeclaredSymbol(DotNetAssemblySyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual DotNetTypeDeclarationSymbol GetDeclaredSymbol(DotNetTypeDeclarationSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual QueryColumnSymbol GetDeclaredSymbol(QueryColumnSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual QueryDataItemSymbol GetDeclaredSymbol(QueryDataItemSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual ControlSymbol GetDeclaredSymbol(ControlBaseSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual ViewSymbol GetDeclaredSymbol(PageViewSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual ActionSymbol GetDeclaredSymbol(ActionBaseSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual ParameterSymbol GetDeclaredSymbol(ParameterSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual ReturnValueSymbol GetDeclaredSymbol(ReturnValueSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual FieldNameListSymbol GetDeclaredSymbol(KeySyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual FieldNameListSymbol GetDeclaredSymbol(FieldGroupSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual ChangeSymbol GetDeclaredSymbol(ControlChangeBaseSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual ChangeSymbol GetDeclaredSymbol(ActionChangeBaseSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual ChangeSymbol GetDeclaredSymbol(ViewChangeBaseSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual ChangeSymbol GetDeclaredSymbol(FieldModificationSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual Symbol GetDeclaredSymbol(ObjectNameOrIdSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual Symbol GetDeclaredSymbol(ObjectNameReferenceSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual Symbol GetDeclaredSymbol(OptionDataTypeSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual Symbol GetDeclaredSymbol(EnumDataTypeSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	public ISymbol GetEnclosingSymbol(int position, CancellationToken cancellationToken = default(CancellationToken))
	{
		position = CheckAndAdjustPosition(position);
		return GetEnclosingBinder(position)?.ContainingMember;
	}

	internal virtual ReportLabelSymbol GetDeclaredSymbol(ReportLabelBaseSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal virtual XmlPortNodeSymbol GetDeclaredSymbol(XmlPortNodeSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	protected internal virtual SyntaxNode GetTopmostNodeForDiagnosticAnalysis(ISymbol symbol, SyntaxNode declaringSyntax)
	{
		return declaringSyntax;
	}

	internal abstract MemberSemanticModel GetMemberModel(SyntaxNode node);

	internal abstract SymbolInfo GetSymbolInfoWorker(SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken));

	internal abstract TypeInfo GetTypeInfoWorker(SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken));

	private ImmutableArray<Symbol> GetSemanticSymbols(BoundExpression boundNode, BoundNode boundNodeForSyntacticParent, Binder binderOpt, out LookupResultKind resultKind, out ImmutableArray<Symbol> memberGroup)
	{
		memberGroup = ImmutableArray<Symbol>.Empty;
		ImmutableArray<Symbol> symbols = ImmutableArray<Symbol>.Empty;
		resultKind = LookupResultKind.Viable;
		BoundKind kind = boundNode.Kind;
		if (kind <= BoundKind.TestFilterAccess)
		{
			if (kind != BoundKind.BadExpression)
			{
				if (kind != BoundKind.Conversion)
				{
					if (kind == BoundKind.TestFilterAccess)
					{
						symbols = ImmutableArray.Create((Symbol)((BoundTestFilterAccess)boundNode).TestFilter);
						goto IL_013b;
					}
				}
				else
				{
					boundNode = ((BoundConversion)boundNode).Operand;
				}
				goto IL_011e;
			}
			BoundBadExpression boundBadExpression = (BoundBadExpression)boundNode;
			resultKind = boundBadExpression.ResultKind;
			return boundBadExpression.Symbols;
		}
		if (kind <= BoundKind.BinaryOperator)
		{
			if (kind != BoundKind.Call)
			{
				if (kind != BoundKind.BinaryOperator)
				{
					goto IL_011e;
				}
				GetSymbolsAndResultKind((BoundBinaryOperator)boundNode, out resultKind, out symbols);
			}
			else
			{
				BoundCall boundCall = (BoundCall)boundNode;
				if (boundCall.OriginalMethodsOpt.IsDefault)
				{
					if ((object)boundCall.Method != null)
					{
						symbols = ImmutableArray.Create((Symbol)boundCall.Method);
						resultKind = boundCall.ResultKind;
					}
				}
				else
				{
					symbols = StaticCast<Symbol>.From(boundCall.OriginalMethodsOpt);
					resultKind = boundCall.ResultKind;
				}
			}
		}
		else
		{
			if (kind != BoundKind.MethodGroup)
			{
				if (kind == BoundKind.CodeExpressionPropertyValue)
				{
					boundNode = ((BoundCodeExpressionPropertyValue)boundNode).Expression;
				}
				goto IL_011e;
			}
			symbols = GetMethodGroupSemanticSymbols(boundNode, boundNodeForSyntacticParent, binderOpt, out resultKind, out memberGroup);
		}
		goto IL_013b;
		IL_013b:
		return symbols;
		IL_011e:
		Symbol expressionSymbol = boundNode.ExpressionSymbol;
		if ((object)expressionSymbol != null)
		{
			symbols = ImmutableArray.Create(expressionSymbol);
			resultKind = boundNode.ResultKind;
		}
		goto IL_013b;
	}

	private static void GetSymbolsAndResultKind(BoundBinaryOperator binaryOperator, out LookupResultKind resultKind, out ImmutableArray<Symbol> symbols)
	{
		BinaryOperatorKind op = binaryOperator.OperatorKind.Operator();
		symbols = ImmutableArray.Create(GetIntrinsicOperatorSymbol(op, binaryOperator.Left.Type, binaryOperator.Right.Type, binaryOperator.Type));
		resultKind = binaryOperator.ResultKind;
	}

	private static Symbol GetIntrinsicOperatorSymbol(BinaryOperatorKind op, TypeSymbol leftType, TypeSymbol rightType, TypeSymbol returnType)
	{
		leftType = (TypeSymbol)leftType.StrippedType();
		rightType = (TypeSymbol)rightType.StrippedType();
		returnType = (TypeSymbol)returnType.StrippedType();
		return new SynthesizedIntrinsicOperatorSymbol(leftType, OperatorFacts.BinaryOperatorNameFromOperatorKind(op), rightType, returnType);
	}

	private ImmutableArray<Symbol> GetMethodGroupSemanticSymbols(BoundExpression boundNode, BoundNode boundNodeForSyntacticParent, Binder binderOpt, out LookupResultKind resultKind, out ImmutableArray<Symbol> methodGroup)
	{
		DebugAssertHelper.Assert(binderOpt != null || IsInTree(boundNode.Syntax));
		ImmutableArray<Symbol> immutableArray = ImmutableArray<Symbol>.Empty;
		BoundMethodGroup boundMethodGroup = (BoundMethodGroup)boundNode;
		resultKind = boundMethodGroup.ResultKind;
		if (resultKind == LookupResultKind.Empty)
		{
			resultKind = LookupResultKind.Viable;
		}
		methodGroup = boundMethodGroup.Methods.Cast<MethodSymbol, Symbol>();
		if (boundNodeForSyntacticParent != null)
		{
			switch (boundNodeForSyntacticParent.Kind)
			{
			case BoundKind.Call:
			{
				BoundCall boundCall = (BoundCall)boundNodeForSyntacticParent;
				if (boundCall.Syntax is InvocationExpressionSyntax invocationExpressionSyntax && invocationExpressionSyntax.Expression.SkipParens() == boundNode.Syntax.SkipParens() && (object)boundCall.Method != null)
				{
					if (!boundCall.OriginalMethodsOpt.IsDefault)
					{
						throw ExceptionUtilities.SkippedForNow;
					}
					immutableArray = ImmutableArray.Create((Symbol)boundCall.Method);
					resultKind = LookupResultKind.Viable;
				}
				break;
			}
			case BoundKind.BadExpression:
			{
				ImmutableArray<Symbol> myMethodGroup = methodGroup;
				BoundBadExpression boundBadExpression = (BoundBadExpression)boundNodeForSyntacticParent;
				immutableArray = boundBadExpression.Symbols.WhereAsArray((Symbol sym) => myMethodGroup.Contains(sym));
				if (immutableArray.Any())
				{
					resultKind = boundBadExpression.ResultKind;
				}
				break;
			}
			default:
				immutableArray = methodGroup;
				if (immutableArray.Length > 0)
				{
					resultKind = resultKind.WorseResultKind(LookupResultKind.OverloadResolutionFailure);
				}
				break;
			}
		}
		else if (methodGroup.Length == 1 && !boundMethodGroup.HasAnyErrors)
		{
			immutableArray = methodGroup;
			if (immutableArray.Length > 0)
			{
				resultKind = resultKind.WorseResultKind(LookupResultKind.OverloadResolutionFailure);
			}
		}
		if (!immutableArray.Any())
		{
			immutableArray = methodGroup;
			if ((int)resultKind > 12)
			{
				resultKind = LookupResultKind.OverloadResolutionFailure;
			}
		}
		return immutableArray;
	}

	protected void CheckSyntaxNode(SyntaxNode syntax)
	{
		if (syntax == null)
		{
			throw new ArgumentNullException("syntax");
		}
		if (!IsInTree(syntax))
		{
			throw new ArgumentException("CSharpResources.SyntaxNodeIsNotWithinSynt");
		}
	}

	internal bool IsInTree(SyntaxNode node)
	{
		return node.SyntaxTree == SyntaxTree;
	}

	internal BoundNode Bind(SyntaxNode node, DiagnosticBag diagnostics)
	{
		Binder binder = Compilation.GetBinder(node);
		if (binder == null)
		{
			return null;
		}
		return Bind(binder, node, diagnostics);
	}

	internal virtual BoundNode Bind(Binder binder, SyntaxNode node, DiagnosticBag diagnostics)
	{
		if (node is ExpressionSyntax node2)
		{
			return binder.BindExpression(node2, diagnostics);
		}
		if (node is StatementSyntax node3)
		{
			return binder.BindStatement(node3, diagnostics);
		}
		return null;
	}

	protected int GetAdjustedNodePosition(SyntaxNode node)
	{
		DebugAssertHelper.Assert(IsInTree(node));
		TextSpan fullSpan = Root.FullSpan;
		int spanStart = node.SpanStart;
		if (fullSpan.IsEmpty)
		{
			DebugAssertHelper.Assert(spanStart == fullSpan.Start);
			return spanStart;
		}
		if (spanStart == fullSpan.End)
		{
			DebugAssertHelper.Assert(node.Width == 0);
			return CheckAndAdjustPosition(spanStart - 1);
		}
		if (node.IsMissing || node.HasErrors || node.Width == 0 || node.IsPartOfStructuredTrivia())
		{
			return CheckAndAdjustPosition(spanStart);
		}
		return spanStart;
	}

	[Conditional("DEBUG")]
	protected void AssertPositionAdjusted(int position)
	{
		DebugAssertHelper.Assert(position == CheckAndAdjustPosition(position), "Expected adjusted position");
	}

	internal int CheckAndAdjustPosition(int position)
	{
		SyntaxToken token;
		return CheckAndAdjustPosition(position, out token);
	}

	[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
	internal int CheckAndAdjustPosition(int position, out SyntaxToken token)
	{
		int position2 = Root.Position;
		int end = Root.FullSpan.End;
		bool flag = position == end && position == SyntaxTree.GetRoot().FullSpan.End;
		if ((position2 <= position && position < end) | flag)
		{
			token = (flag ? SyntaxTree.GetRoot() : Root).FindToken(position);
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
		throw new ArgumentOutOfRangeException("position", position, string.Format(CultureInfo.InvariantCulture, CodeAnalysisResources.PositionIsNotWithinSyntax, Root.FullSpan));
	}

	public ISymbol GetDeclaredSymbol(SyntaxNode declaration, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDeclaredSymbolForNode(declaration, cancellationToken);
	}

	public bool TryGetSpeculativeSemanticModelForMethodBody(int position, MemberSyntax method, out SemanticModel speculativeModel)
	{
		CheckModelAndSyntaxNodeToSpeculate(method);
		return TryGetSpeculativeSemanticModelForMethodBodyCore((SyntaxTreeSemanticModel)this, position, method, out speculativeModel);
	}

	internal abstract bool TryGetSpeculativeSemanticModelForMethodBodyCore(SyntaxTreeSemanticModel parentModel, int position, MemberSyntax method, out SemanticModel speculativeModel);

	private void CheckModelAndSyntaxNodeToSpeculate(SyntaxNode syntax)
	{
		if (syntax == null)
		{
			throw new ArgumentNullException("syntax");
		}
		if (IsSpeculativeSemanticModel)
		{
			throw new InvalidOperationException("Chaining speculative semantic model is not supported.You should create a speculative model from the non-speculative ParentModel.");
		}
		if (Compilation.ContainsSyntaxTree(syntax.SyntaxTree))
		{
			throw new ArgumentException("Syntax node to be speculated cannot belong to a syntax tree from the current compilation.");
		}
	}

	internal void ComputeDeclarationsInSpan(TextSpan span, bool getSymbol, List<DeclarationInfo> builder, CancellationToken cancellationToken)
	{
		DeclarationComputer.ComputeDeclarationsInSpan(this, span, getSymbol, builder, cancellationToken);
	}

	internal void ComputeDeclarationsInNode(SyntaxNode node, bool getSymbol, List<DeclarationInfo> builder, CancellationToken cancellationToken, int? levelsToCompute = null)
	{
		DeclarationComputer.ComputeDeclarationsInNode(this, node, getSymbol, builder, cancellationToken, levelsToCompute);
	}

	public IOperation GetOperation(SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetOperationInternal(node, cancellationToken);
	}

	internal IOperation GetOperationInternal(SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		try
		{
			return GetOperationCore(node, cancellationToken);
		}
		catch (Exception exception) when (FatalError.ReportWithoutCrashUnlessCanceled(exception))
		{
			DebugAssertHelper.Assert(condition: false);
		}
		return null;
	}

	private IOperation GetOperationCore(SyntaxNode node, CancellationToken cancellationToken)
	{
		CheckSyntaxNode(node);
		return GetOperationWorker(node, GetOperationOptions.Lowest, cancellationToken);
	}

	internal virtual IOperation GetOperationWorker(SyntaxNode node, GetOperationOptions options, CancellationToken cancellationToken)
	{
		return null;
	}
}
