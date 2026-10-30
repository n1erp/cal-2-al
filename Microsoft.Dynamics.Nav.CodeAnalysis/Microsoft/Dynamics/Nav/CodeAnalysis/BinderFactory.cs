using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class BinderFactory
{
	internal enum NodeUsage : byte
	{
		Normal = 0,
		MethodBody = 2
	}

	private struct BinderCacheKey(SyntaxNode syntaxNode, NodeUsage usage) : IEquatable<BinderCacheKey>
	{
		private readonly SyntaxNode syntaxNode = syntaxNode;

		private readonly NodeUsage usage = usage;

		bool IEquatable<BinderCacheKey>.Equals(BinderCacheKey other)
		{
			if (syntaxNode == other.syntaxNode)
			{
				return usage == other.usage;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(syntaxNode.GetHashCode(), (int)usage);
		}

		public override bool Equals(object obj)
		{
			throw new NotSupportedException();
		}
	}

	private sealed class BinderFactoryVisitor : SyntaxVisitor<Binder>
	{
		private readonly BinderFactory factory;

		private int position;

		internal int Position
		{
			set
			{
				position = value;
			}
		}

		private Compilation Compilation => factory.compilation;

		private SyntaxTree SyntaxTree => factory.SyntaxTree;

		private BuckStopsHereBinder BuckStopsHereBinder => factory.buckStopsHereBinder;

		private ConcurrentCache<BinderCacheKey, Binder> BinderCache => factory.binderCache;

		internal BinderFactoryVisitor(BinderFactory factory)
		{
			this.factory = factory;
		}

		public override Binder DefaultVisit(SyntaxNode parent)
		{
			return VisitCore(parent.Parent);
		}

		public override Binder Visit(SyntaxNode node)
		{
			return VisitCore(node);
		}

		public override Binder VisitMethodDeclaration(MethodDeclarationSyntax methodDecl)
		{
			return VisitMethodOrTriggerDeclaration(methodDecl, methodDecl.Name.Unquoted());
		}

		public override Binder VisitTriggerDeclaration(TriggerDeclarationSyntax triggerDecl)
		{
			return VisitMethodOrTriggerDeclaration(triggerDecl, triggerDecl.Name.Unquoted());
		}

		public override Binder VisitEventTriggerDeclaration(EventTriggerDeclarationSyntax eventTriggerDeclaration)
		{
			return VisitMethodOrTriggerDeclaration(eventTriggerDeclaration, eventTriggerDeclaration.GetName());
		}

		public override Binder VisitCodeunit(CodeunitSyntax node)
		{
			return VisitApplicationObject(SymbolKind.Codeunit, node);
		}

		public override Binder VisitTable(TableSyntax node)
		{
			return VisitApplicationObject(SymbolKind.Table, node);
		}

		public override Binder VisitField(FieldSyntax node)
		{
			return VisitSyntaxNode<FieldSymbol>(node, node.Name.Unquoted(), SymbolKind.Field);
		}

		public override Binder VisitPage(PageSyntax node)
		{
			return VisitApplicationObject(SymbolKind.Page, node);
		}

		public override Binder VisitXmlPort(XmlPortSyntax node)
		{
			return VisitApplicationObject(SymbolKind.XmlPort, node);
		}

		public override Binder VisitPageArea(PageAreaSyntax node)
		{
			return VisitPageControlBase(node);
		}

		public override Binder VisitPageField(PageFieldSyntax node)
		{
			return VisitPageControlBase(node);
		}

		public override Binder VisitPageGroup(PageGroupSyntax node)
		{
			return VisitPageControlBase(node);
		}

		public override Binder VisitPagePart(PagePartSyntax node)
		{
			return VisitPageControlBase(node);
		}

		public override Binder VisitPageUserControl(PageUserControlSyntax node)
		{
			return VisitPageControlBase(node);
		}

		public override Binder VisitRequestPage(RequestPageSyntax node)
		{
			return VisitObject(SymbolKind.RequestPage, node);
		}

		public override Binder VisitReport(ReportSyntax node)
		{
			return VisitApplicationObject(SymbolKind.Report, node);
		}

		public override Binder VisitQuery(QuerySyntax node)
		{
			return VisitApplicationObject(SymbolKind.Query, node);
		}

		public override Binder VisitQueryColumn(QueryColumnSyntax node)
		{
			return VisitQueryElement(node, node.Name.Unquoted(), SymbolKind.QueryColumn);
		}

		public override Binder VisitQueryFilter(QueryFilterSyntax node)
		{
			return VisitQueryElement(node, node.Name.Unquoted(), SymbolKind.QueryFilter);
		}

		public override Binder VisitReportDataItem(ReportDataItemSyntax node)
		{
			IsolatedScopeBinder.ReceiverFactory receiverFactory = (ContainerSymbol owningSymbol) =>
			{
				SourceReportDataItemSymbol sourceReportDataItemSymbol = (SourceReportDataItemSymbol)owningSymbol;
				return new BoundReportDataItemAccess(sourceReportDataItemSymbol.Syntax, null, sourceReportDataItemSymbol, LookupResultKind.Viable, sourceReportDataItemSymbol.Type);
			};
			return CreateIsolatedScope(node, node.Name.Unquoted(), receiverFactory, SymbolKind.ReportDataItem);
		}

		public override Binder VisitQueryDataItem(QueryDataItemSyntax node)
		{
			IsolatedScopeBinder.ReceiverFactory receiverFactory = (ContainerSymbol owningSymbol) =>
			{
				SourceQueryDataItemSymbol sourceQueryDataItemSymbol = (SourceQueryDataItemSymbol)owningSymbol;
				return new BoundQueryDataItemAccess(sourceQueryDataItemSymbol.Syntax, null, sourceQueryDataItemSymbol, LookupResultKind.Viable, sourceQueryDataItemSymbol.Type);
			};
			return CreateIsolatedScope(node, node.Name.Unquoted(), receiverFactory, SymbolKind.QueryDataItem);
		}

		public override Binder VisitXmlPortTableElement(XmlPortTableElementSyntax node)
		{
			return VisitSyntaxNode<XmlPortNodeSymbol>(node, node.Name.Unquoted(), SymbolKind.XmlPortNode);
		}

		public override Binder VisitXmlPortFieldElement(XmlPortFieldElementSyntax node)
		{
			return VisitSyntaxNode<SourceXmlPortNodeSymbol>(node, node.Name.Unquoted(), SymbolKind.XmlPortNode);
		}

		public override Binder VisitDotNetAssembly(DotNetAssemblySyntax node)
		{
			return VisitSyntaxNode<SourceDotNetAssemblySymbol>(node, node.AssemblyName.Unquoted(), SymbolKind.DotNetAssembly);
		}

		public override Binder VisitDotNetTypeDeclaration(DotNetTypeDeclarationSyntax node)
		{
			return VisitSyntaxNode<SourceDotNetTypeDeclarationSymbol>(node, node.TypeName.Unquoted(), SymbolKind.DotNetTypeDeclaration);
		}

		public override Binder VisitXmlPortTextElement(XmlPortTextElementSyntax node)
		{
			return VisitSyntaxNode<SourceXmlPortNodeSymbol>(node, node.Name.Unquoted(), SymbolKind.XmlPortNode);
		}

		public override Binder VisitXmlPortFieldAttribute(XmlPortFieldAttributeSyntax node)
		{
			return VisitSyntaxNode<SourceXmlPortNodeSymbol>(node, node.Name.Unquoted(), SymbolKind.XmlPortNode);
		}

		public override Binder VisitXmlPortTextAttribute(XmlPortTextAttributeSyntax node)
		{
			return VisitSyntaxNode<XmlPortNodeSymbol>(node, node.Name.Unquoted(), SymbolKind.XmlPortNode);
		}

		public override Binder VisitReportColumn(ReportColumnSyntax node)
		{
			return VisitSyntaxNode<SourceReportColumnSymbol>(node, node.Name.Unquoted(), SymbolKind.XmlPortNode);
		}

		public override Binder VisitPageExtension(PageExtensionSyntax node)
		{
			return VisitApplicationObjectExtension(SymbolKind.PageExtension, node);
		}

		public override Binder VisitPageCustomization(PageCustomizationSyntax node)
		{
			return VisitApplicationObjectExtension(SymbolKind.PageCustomization, node);
		}

		public override Binder VisitTableExtension(TableExtensionSyntax node)
		{
			return VisitApplicationObjectExtension(SymbolKind.TableExtension, node);
		}

		public override Binder VisitFieldModification(FieldModificationSyntax node)
		{
			return VisitSyntaxNode<ChangeModifySymbol>(node, node.Name.Unquoted(), SymbolKind.Change);
		}

		public override Binder VisitActionAddChange(ActionAddChangeSyntax node)
		{
			return VisitSyntaxNode<ChangeAddSymbol>(node, node.GetName(), SymbolKind.Change);
		}

		public override Binder VisitActionModifyChange(ActionModifyChangeSyntax node)
		{
			return VisitSyntaxNode<ChangeModifySymbol>(node, node.Name.Unquoted(), SymbolKind.Change);
		}

		public override Binder VisitControlAddChange(ControlAddChangeSyntax node)
		{
			return VisitSyntaxNode<ChangeAddSymbol>(node, node.GetName(), SymbolKind.Change);
		}

		public override Binder VisitControlModifyChange(ControlModifyChangeSyntax node)
		{
			return VisitSyntaxNode<ChangeModifySymbol>(node, node.Name.Unquoted(), SymbolKind.Change);
		}

		public override Binder VisitProfile(ProfileSyntax node)
		{
			return VisitObject(SymbolKind.Profile, node);
		}

		public override Binder VisitDotNetPackage(DotNetPackageSyntax node)
		{
			return VisitObject(SymbolKind.DotNetPackage, node);
		}

		public override Binder VisitPageActionGroup(PageActionGroupSyntax node)
		{
			return VisitPageActionBase(node);
		}

		public override Binder VisitPageActionArea(PageActionAreaSyntax node)
		{
			return VisitPageActionBase(node);
		}

		public override Binder VisitPageAction(PageActionSyntax node)
		{
			return VisitPageActionBase(node);
		}

		public override Binder VisitObjectCompilationUnit(ObjectCompilationUnitSyntax parent)
		{
			return VisitCompilationUnitInternal(parent);
		}

		private Binder VisitCore(SyntaxNode node)
		{
			return node.Accept(this);
		}

		private Binder VisitMethodOrTriggerDeclaration(MethodOrTriggerDeclarationSyntax methodDecl, string methodName)
		{
			NodeUsage nodeUsage = (LookupPosition.IsInBody(position, methodDecl) ? NodeUsage.MethodBody : NodeUsage.Normal);
			BinderCacheKey key = CreateBinderCacheKey(methodDecl, nodeUsage);
			if (!BinderCache.TryGetValue(key, out var value))
			{
				value = VisitCore(methodDecl.Parent);
				if (nodeUsage == NodeUsage.MethodBody)
				{
					MethodSymbol symbolForNodeFromParentContainer = GetSymbolForNodeFromParentContainer<MethodSymbol>(GetContainer(value), methodDecl, methodName, SymbolKind.Method);
					if (symbolForNodeFromParentContainer != null)
					{
						value = new InMethodBinder(symbolForNodeFromParentContainer, value);
					}
				}
				BinderCache.TryAdd(key, value);
			}
			return value;
		}

		private Binder VisitSyntaxNode<T>(SyntaxNode node, string name, Func<T, Binder, SyntaxNode, Binder> binderCreator, SymbolKind kind) where T : Symbol
		{
			BinderCacheKey key = CreateBinderCacheKey(node, NodeUsage.Normal);
			if (!BinderCache.TryGetValue(key, out var value))
			{
				value = VisitCore(node.Parent);
				T symbolForNodeFromParentContainer = GetSymbolForNodeFromParentContainer<T>(value.ContainingMember as ContainerSymbol, node, name, kind);
				if (symbolForNodeFromParentContainer != null)
				{
					value = binderCreator(symbolForNodeFromParentContainer, value, node);
				}
				BinderCache.TryAdd(key, value);
			}
			return value;
		}

		private Binder VisitObject(SymbolKind kind, ObjectSyntax node)
		{
			Func<ObjectTypeSymbol, Binder, SyntaxNode, Binder> binderCreator = (ObjectTypeSymbol symbol, Binder binder, SyntaxNode sameNode) => new ObjectBinder(symbol, binder, sameNode);
			string name = node.Name?.Unquoted();
			return VisitSyntaxNode(node, name, binderCreator, kind);
		}

		private Binder VisitApplicationObject(SymbolKind kind, ApplicationObjectSyntax node)
		{
			Func<ApplicationObjectTypeSymbol, Binder, SyntaxNode, Binder> binderCreator = (ApplicationObjectTypeSymbol symbol, Binder binder, SyntaxNode sameNode) => new ObjectBinder(symbol, binder, sameNode);
			string name = node.Name.Unquoted();
			return VisitSyntaxNode(node, name, binderCreator, kind);
		}

		private Binder VisitApplicationObjectExtension(SymbolKind kind, ApplicationObjectSyntax node)
		{
			Func<ApplicationObjectExtensionTypeSymbol, Binder, SyntaxNode, Binder> binderCreator = (ApplicationObjectExtensionTypeSymbol symbol, Binder binder, SyntaxNode sameNode) => new ApplicationObjectExtensionBinder(symbol, binder, sameNode);
			string name = node.Name.Unquoted();
			return VisitSyntaxNode(node, name, binderCreator, kind);
		}

		private Binder VisitSyntaxNode<T>(SyntaxNode node, string name, SymbolKind kind) where T : Symbol
		{
			Func<T, Binder, SyntaxNode, Binder> binderCreator = (T symbol, Binder binder, SyntaxNode sameNode) => new SimpleBinder<T>(symbol, binder, sameNode);
			return VisitSyntaxNode(node, name, binderCreator, kind);
		}

		private Binder CreateIsolatedScope(SyntaxNode node, string name, IsolatedScopeBinder.ReceiverFactory receiverFactory, SymbolKind kind)
		{
			Func<ContainerSymbol, Binder, SyntaxNode, Binder> binderCreator = (ContainerSymbol symbol, Binder binder, SyntaxNode sameNode) => CreateIsolatedScopeBinder(sameNode, symbol, symbol.ContainingSymbol, binder, receiverFactory);
			return VisitSyntaxNode(node, name, binderCreator, kind);
		}

		private static T GetSymbolForNodeFromParentContainer<T>(ContainerSymbol parentContainer, SyntaxNode node, string name, SymbolKind kind) where T : Symbol
		{
			if (parentContainer == null)
			{
				return null;
			}
			Func<ImmutableArray<Symbol>> func = ((name != null) ? ((Func<ImmutableArray<Symbol>>)(() => parentContainer.GetMembers(name))) : new Func<ImmutableArray<Symbol>>(parentContainer.GetMembers));
			foreach (Symbol item in from s in func()
				where s.IsKind(kind)
				select s)
			{
				foreach (SyntaxReference declaringSyntaxReference in item.DeclaringSyntaxReferences)
				{
					if (declaringSyntaxReference.GetSyntax() == node)
					{
						return item as T;
					}
				}
			}
			return null;
		}

		private Binder CreateIsolatedScopeBinder(SyntaxNode node, ContainerSymbol isolatedScopeContainer, Symbol containerSymbol, Binder parentBinder, IsolatedScopeBinder.ReceiverFactory receiverFactory)
		{
			Binder result = parentBinder;
			if (isolatedScopeContainer != null)
			{
				SyntaxNode containingApplicationObjectSyntax = node.GetContainingApplicationObjectSyntax();
				SyntaxNode firstSyntax = containerSymbol.GetFirstSyntax();
				if (containingApplicationObjectSyntax != firstSyntax)
				{
					parentBinder = VisitCore(containingApplicationObjectSyntax);
				}
				result = new IsolatedScopeBinder(isolatedScopeContainer, receiverFactory, parentBinder, node);
			}
			return result;
		}

		private Binder VisitPageControlBase(ControlBaseSyntax node)
		{
			return VisitSyntaxNode<ControlSymbol>(node, node.Name.Unquoted(), SymbolKind.Control);
		}

		private Binder VisitPageActionBase(ActionBaseSyntax node)
		{
			return VisitSyntaxNode<ActionSymbol>(node, node.Name.Unquoted(), SymbolKind.Action);
		}

		private static ContainerSymbol GetContainer(Binder binder)
		{
			ContainerSymbol obj = binder.ContainingMember as ContainerSymbol;
			if ((object)obj == null)
			{
				DebugAssertHelper.Assert(binder.ContainingMember is ModuleSymbol);
			}
			return obj;
		}

		private InContainerBinder VisitCompilationUnitInternal(CompilationUnitSyntax compilationUnit)
		{
			if (compilationUnit != SyntaxTree.GetRoot())
			{
				throw new ArgumentOutOfRangeException("compilationUnit", "node not part of tree");
			}
			BinderCacheKey key = CreateBinderCacheKey(compilationUnit, NodeUsage.Normal);
			if (!BinderCache.TryGetValue(key, out var value))
			{
				value = BuckStopsHereBinder;
				value = new InContainerBinder(Compilation.CompiledModule, value);
				BinderCache.TryAdd(key, value);
			}
			return (InContainerBinder)value;
		}

		private static BinderCacheKey CreateBinderCacheKey(SyntaxNode node, NodeUsage usage)
		{
			return new BinderCacheKey(node, usage);
		}

		private Binder VisitQueryElement(SyntaxNode node, string elementName, SymbolKind kind)
		{
			return CreateBinderWithSelfInScope(node, elementName, kind, (QueryElementSymbol elementSymbol) => SynthesizeQueryElementContainer.Create(elementSymbol), (TypeSymbol synthesizedContainer, Binder _) => factory.GetLookupBinder(synthesizedContainer));
		}

		private Binder CreateBinderWithSelfInScope<T>(SyntaxNode node, string elementName, SymbolKind kind, Func<T, TypeSymbol> synthesizedContainerFactory, Func<TypeSymbol, Binder, Binder> binderFactory) where T : Symbol
		{
			Binder binder = DefaultVisit(node);
			T symbolForNodeFromParentContainer = GetSymbolForNodeFromParentContainer<T>((ContainerSymbol)binder.ContainingMember, node, elementName, kind);
			DebugAssertHelper.Assert(symbolForNodeFromParentContainer != null);
			TypeSymbol arg = synthesizedContainerFactory(symbolForNodeFromParentContainer);
			return binderFactory(arg, binder);
		}
	}

	private readonly ConcurrentCache<BinderCacheKey, Binder> binderCache;

	private readonly ObjectPool<BinderFactoryVisitor> binderFactoryVisitorPool;

	private readonly BuckStopsHereBinder buckStopsHereBinder;

	private readonly Compilation compilation;

	internal SyntaxTree SyntaxTree { get; }

	internal BinderFactory(Compilation compilation, SyntaxTree syntaxTree)
	{
		this.compilation = compilation;
		SyntaxTree = syntaxTree;
		binderFactoryVisitorPool = new ObjectPool<BinderFactoryVisitor>(() => new BinderFactoryVisitor(this), 64);
		binderCache = new ConcurrentCache<BinderCacheKey, Binder>(50);
		buckStopsHereBinder = new BuckStopsHereBinder(compilation);
	}

	internal Binder GetBinder(SyntaxNode node)
	{
		int spanStart = node.SpanStart;
		return GetBinder(node.Parent, spanStart);
	}

	internal Binder GetBinder(SyntaxNode node, int position)
	{
		DebugAssertHelper.Assert(node != null);
		BinderFactoryVisitor binderFactoryVisitor = binderFactoryVisitorPool.Allocate();
		try
		{
			binderFactoryVisitor.Position = position;
			return node.Accept(binderFactoryVisitor);
		}
		finally
		{
			binderFactoryVisitorPool.Free(binderFactoryVisitor);
		}
	}

	internal Binder GetLookupBinder(ContainerSymbol symbol)
	{
		return new SimpleSymbolBinder<ContainerSymbol>(symbol, buckStopsHereBinder);
	}
}
