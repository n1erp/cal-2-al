using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class SyntaxTreeSemanticModel : SemanticModel
{
	private readonly SyntaxTree syntaxTree;

	private ImmutableDictionary<SyntaxNode, MemberSemanticModel> memberModels = ImmutableDictionary<SyntaxNode, MemberSemanticModel>.Empty;

	private readonly BinderFactory binderFactory;

	private Func<SyntaxNode, MemberSemanticModel> createMemberModelFunction;

	private static readonly Func<SyntaxNode, bool> isMemberDeclarationFunction = IsMemberDeclaration;

	private readonly MemberSemanticModelFactory memberSemanticModelFactory;

	public override bool IsSpeculativeSemanticModel => false;

	public override int OriginalPositionForSpeculation => 0;

	protected override SyntaxTree SyntaxTreeCore => syntaxTree;

	internal override SyntaxNode Root => syntaxTree.GetRoot();

	internal SyntaxTreeSemanticModel(Compilation compilation, SyntaxTree syntaxTree)
		: base(compilation)
	{
		this.syntaxTree = syntaxTree;
		if (!Compilation.ContainsSyntaxTree(syntaxTree))
		{
			throw new ArgumentOutOfRangeException("syntaxTree", CodeAnalysisResources.TreeNotPartOfCompilation);
		}
		binderFactory = compilation.GetBinderFactory(SyntaxTree);
		memberSemanticModelFactory = new MemberSemanticModelFactory(this, binderFactory);
	}

	internal override Binder GetEnclosingBinderInternal(int position)
	{
		SyntaxToken syntaxToken = Root.FindToken(position);
		if (position == 0 && position != syntaxToken.SpanStart)
		{
			return binderFactory.GetBinder(Root, position).WithAdditionalFlags(GetSemanticModelBinderFlags());
		}
		MemberSemanticModel memberModel = GetMemberModel(position);
		if (memberModel != null)
		{
			return memberModel.GetEnclosingBinder(position);
		}
		return binderFactory.GetBinder(syntaxToken.Parent, position).WithAdditionalFlags(GetSemanticModelBinderFlags());
	}

	internal override SymbolInfo GetSymbolInfoWorker(SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		node = SyntaxFactory.GetStandaloneNode(node);
		MemberSemanticModel memberModel = GetMemberModel(node);
		if (memberModel != null)
		{
			return memberModel.GetSymbolInfoWorker(node, cancellationToken);
		}
		Symbol semanticInfoSymbolInNonMemberContext = GetSemanticInfoSymbolInNonMemberContext(node);
		return ((object)semanticInfoSymbolInNonMemberContext != null) ? SemanticModel.GetSymbolInfoForSymbol(semanticInfoSymbolInNonMemberContext) : SymbolInfo.None;
	}

	internal override TypeInfo GetTypeInfoWorker(SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		node = SyntaxFactory.GetStandaloneNode(node);
		MemberSemanticModel memberModel = GetMemberModel(node);
		if (memberModel != null)
		{
			return memberModel.GetTypeInfoWorker(node, cancellationToken);
		}
		Symbol semanticInfoSymbolInNonMemberContext = GetSemanticInfoSymbolInNonMemberContext(node);
		if ((object)semanticInfoSymbolInNonMemberContext == null)
		{
			return TypeInfo.None;
		}
		return SemanticModel.GetTypeInfoForSymbol(semanticInfoSymbolInNonMemberContext);
	}

	internal override bool TryGetSpeculativeSemanticModelForMethodBodyCore(SyntaxTreeSemanticModel parentModel, int position, MemberSyntax method, out SemanticModel speculativeModel)
	{
		speculativeModel = null;
		return false;
	}

	private Symbol GetSemanticInfoSymbolInNonMemberContext(SyntaxNode node)
	{
		DebugAssertHelper.Assert(GetMemberModel(node) == null);
		if (node.ParentIsKind(SyntaxKind.ObjectReference))
		{
			ObjectNameOrIdSyntax objectNameOrIdSyntax = node.Parent as ObjectNameOrIdSyntax;
			if (objectNameOrIdSyntax.ParentIsKind(SyntaxKind.SubtypedDataType))
			{
				SymbolKind kind = NavTypeExtensions.GetNavTypeKind(((SubtypedDataTypeSyntax)objectNameOrIdSyntax.Parent).TypeName.ValueText).ToSubTypeSymbolKind();
				return GetSymbolFromObjectReference(objectNameOrIdSyntax, kind);
			}
			ApplicationObjectExtensionSyntax applicationObjectExtensionSyntax = objectNameOrIdSyntax.Parent as ApplicationObjectExtensionSyntax;
			if (applicationObjectExtensionSyntax?.BaseObject == objectNameOrIdSyntax)
			{
				switch (applicationObjectExtensionSyntax.Kind)
				{
				case SyntaxKind.TableExtensionObject:
					return GetSymbolFromObjectReference(objectNameOrIdSyntax, SymbolKind.Table);
				case SyntaxKind.PageExtensionObject:
					return GetSymbolFromObjectReference(objectNameOrIdSyntax, SymbolKind.Page);
				case SyntaxKind.PageCustomizationObject:
					return GetSymbolFromObjectReference(objectNameOrIdSyntax, SymbolKind.Page);
				case SyntaxKind.EnumExtensionType:
					return GetSymbolFromObjectReference(objectNameOrIdSyntax, SymbolKind.Enum);
				}
			}
		}
		else if (node.Parent != null)
		{
			if (node.Parent.Kind == SyntaxKind.EventTriggerDeclaration && ((EventTriggerDeclarationSyntax)node.Parent).Publisher == node)
			{
				ContainerSymbol declaredTypeMemberContainer = GetDeclaredTypeMemberContainer(node);
				string name = ((EventTriggerDeclarationSyntax)node.Parent).Publisher.Unquoted();
				ImmutableArray<Symbol> codeMembers = declaredTypeMemberContainer.GetCodeMembers(name);
				if (codeMembers.Length != 1)
				{
					return null;
				}
				return codeMembers[0];
			}
			if (node.Parent.Kind == SyntaxKind.Key || node.Parent.Kind == SyntaxKind.FieldGroup)
			{
				SyntaxNode firstParent = node.GetFirstParent(SyntaxKind.TableObject);
				if (firstParent != null)
				{
					return (GetDeclaredSymbol((TableSyntax)firstParent) as TableTypeSymbol)?.GetMembers(node.GetIdentifierOrLiteralValue().UnquoteIdentifier()).FirstOrDefault();
				}
				SyntaxNode firstParent2 = node.GetFirstParent(SyntaxKind.TableExtensionObject);
				if (firstParent2 != null)
				{
					return (GetDeclaredSymbol((TableExtensionSyntax)firstParent2) as TableExtensionTypeSymbol)?.GetMembers(node.GetIdentifierOrLiteralValue().UnquoteIdentifier()).FirstOrDefault();
				}
			}
			if (node.Parent.ParentIsKind(SyntaxKind.OptionValues))
			{
				return (GetDeclaredSymbol(((OptionValuesSyntax)node.Parent.Parent)?.Parent as OptionDataTypeSyntax) as OptionTypeSymbol)?.GetMembers(node.GetIdentifierOrLiteralValue().UnquoteIdentifier()).FirstOrDefault();
			}
			Symbol symbol = GetSemanticInfoSymbolForControlChanges(node) ?? GetSemanticInfoSymbolForActionChanges(node) ?? GetSemanticInfoSymbolForFieldChanges(node) ?? GetSemanticInfoSymbolForViewChanges(node);
			if (symbol != null)
			{
				return symbol;
			}
		}
		return null;
	}

	private Symbol GetSemanticInfoSymbolForControlChanges(SyntaxNode node)
	{
		switch (node.Parent.Kind)
		{
		case SyntaxKind.ControlAddChange:
		{
			ControlAddChangeSyntax controlAddChangeSyntax = (ControlAddChangeSyntax)node.Parent;
			if (controlAddChangeSyntax != null && controlAddChangeSyntax.Anchor == node)
			{
				return (GetDeclaredSymbol(controlAddChangeSyntax) as ChangeAddSymbol)?.Anchor;
			}
			break;
		}
		case SyntaxKind.ControlModifyChange:
		{
			ControlModifyChangeSyntax controlModifyChangeSyntax = (ControlModifyChangeSyntax)node.Parent;
			if (controlModifyChangeSyntax != null && controlModifyChangeSyntax.Name == node)
			{
				return (GetDeclaredSymbol(controlModifyChangeSyntax) as ChangeModifySymbol)?.Target as Symbol;
			}
			break;
		}
		case SyntaxKind.ControlMoveChange:
		{
			ControlMoveChangeSyntax controlMoveChangeSyntax = (ControlMoveChangeSyntax)node.Parent;
			if (controlMoveChangeSyntax.Anchor == node)
			{
				return (GetDeclaredSymbol(controlMoveChangeSyntax) as ChangeMoveSymbol)?.Anchor;
			}
			ChangeMoveSymbol changeMoveSymbol = GetDeclaredSymbol(controlMoveChangeSyntax) as ChangeMoveSymbol;
			string identifierValue = (node as IdentifierNameSyntax)?.GetIdentifierOrLiteralValue();
			if (identifierValue != null && changeMoveSymbol != null)
			{
				return changeMoveSymbol.MovedSymbols.FirstOrDefault((Symbol s) => string.Equals(s.Name, identifierValue, StringComparison.OrdinalIgnoreCase));
			}
			break;
		}
		}
		return null;
	}

	private Symbol GetSemanticInfoSymbolForActionChanges(SyntaxNode node)
	{
		switch (node.Parent.Kind)
		{
		case SyntaxKind.ActionAddChange:
		{
			ActionAddChangeSyntax actionAddChangeSyntax = (ActionAddChangeSyntax)node.Parent;
			if (actionAddChangeSyntax.Anchor == node)
			{
				return (GetDeclaredSymbol(actionAddChangeSyntax) as ChangeAddSymbol)?.Anchor;
			}
			break;
		}
		case SyntaxKind.ActionModifyChange:
		{
			ActionModifyChangeSyntax actionModifyChangeSyntax = (ActionModifyChangeSyntax)node.Parent;
			if (actionModifyChangeSyntax.Name == node)
			{
				return (GetDeclaredSymbol(actionModifyChangeSyntax) as ChangeModifySymbol)?.Target as Symbol;
			}
			break;
		}
		case SyntaxKind.ActionMoveChange:
		{
			ActionMoveChangeSyntax actionMoveChangeSyntax = (ActionMoveChangeSyntax)node.Parent;
			if (actionMoveChangeSyntax.Anchor == node)
			{
				return (GetDeclaredSymbol(actionMoveChangeSyntax) as ChangeMoveSymbol)?.Anchor;
			}
			ChangeMoveSymbol changeMoveSymbol = GetDeclaredSymbol(actionMoveChangeSyntax) as ChangeMoveSymbol;
			string identifierValue = (node as IdentifierNameSyntax)?.GetIdentifierOrLiteralValue();
			if (identifierValue != null && changeMoveSymbol != null)
			{
				return changeMoveSymbol.MovedSymbols.FirstOrDefault((Symbol s) => string.Equals(s.Name, identifierValue, StringComparison.OrdinalIgnoreCase));
			}
			break;
		}
		}
		return null;
	}

	private Symbol GetSemanticInfoSymbolForViewChanges(SyntaxNode node)
	{
		switch (node.Parent.Kind)
		{
		case SyntaxKind.ViewAddChange:
		{
			ViewAddChangeSyntax viewAddChangeSyntax = (ViewAddChangeSyntax)node.Parent;
			if (viewAddChangeSyntax.Anchor == node)
			{
				return (GetDeclaredSymbol(viewAddChangeSyntax) as ChangeAddSymbol)?.Anchor;
			}
			break;
		}
		case SyntaxKind.ViewModifyChange:
		{
			ViewModifyChangeSyntax viewModifyChangeSyntax = (ViewModifyChangeSyntax)node.Parent;
			if (viewModifyChangeSyntax.Name == node)
			{
				return (GetDeclaredSymbol(viewModifyChangeSyntax) as ChangeModifySymbol)?.Target as Symbol;
			}
			break;
		}
		case SyntaxKind.ViewMoveChange:
		{
			ViewMoveChangeSyntax viewMoveChangeSyntax = (ViewMoveChangeSyntax)node.Parent;
			if (viewMoveChangeSyntax.Anchor == node)
			{
				return (GetDeclaredSymbol(viewMoveChangeSyntax) as ChangeMoveSymbol)?.Anchor;
			}
			ChangeMoveSymbol changeMoveSymbol = GetDeclaredSymbol(viewMoveChangeSyntax) as ChangeMoveSymbol;
			string identifierValue = (node as IdentifierNameSyntax)?.GetIdentifierOrLiteralValue();
			if (identifierValue != null && changeMoveSymbol != null)
			{
				return changeMoveSymbol.MovedSymbols.FirstOrDefault((Symbol s) => string.Equals(s.Name, identifierValue, StringComparison.OrdinalIgnoreCase));
			}
			break;
		}
		}
		return null;
	}

	private Symbol GetSemanticInfoSymbolForFieldChanges(SyntaxNode node)
	{
		if (node.Parent is FieldModificationSyntax fieldModificationSyntax && fieldModificationSyntax.Name == node)
		{
			return (GetDeclaredSymbol(fieldModificationSyntax) as ChangeModifySymbol)?.Target as Symbol;
		}
		return null;
	}

	private Symbol GetSymbolFromObjectReference(ObjectNameOrIdSyntax objectReference, SymbolKind kind)
	{
		IReadOnlyList<ISymbol> readOnlyList;
		switch (objectReference.Identifier.Kind)
		{
		case SyntaxKind.IdentifierName:
			readOnlyList = Compilation.GetObjectTypeSymbolsByNameAcrossModules(kind, objectReference.Identifier.GetIdentifierOrLiteralValue());
			break;
		case SyntaxKind.ObjectId:
		{
			int id = ((int?)((ObjectIdSyntax)objectReference.Identifier).Value.Value) ?? 0;
			readOnlyList = Compilation.GetApplicationObjectTypeSymbolsByIdAcrossModules(kind, id);
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(objectReference.Identifier.Kind);
		}
		if (readOnlyList.Count != 1)
		{
			return null;
		}
		return (Symbol)readOnlyList[0];
	}

	private MemberSemanticModel GetMemberModel(int position)
	{
		SyntaxNode parent = Root.FindToken(position).Parent;
		SyntaxNode memberDeclaration = GetMemberDeclaration(parent);
		bool flag = false;
		if (memberDeclaration != null)
		{
			switch (memberDeclaration.Kind)
			{
			case SyntaxKind.MethodDeclaration:
			{
				MethodDeclarationSyntax methodDeclarationSyntax = (MethodDeclarationSyntax)memberDeclaration;
				flag = !LookupPosition.IsInBlock(position, methodDeclarationSyntax.Body) && !LookupPosition.IsInParameterList(position, methodDeclarationSyntax);
				break;
			}
			case SyntaxKind.TriggerDeclaration:
			{
				TriggerDeclarationSyntax triggerDeclarationSyntax = (TriggerDeclarationSyntax)memberDeclaration;
				flag = !LookupPosition.IsInBlock(position, triggerDeclarationSyntax.Body);
				break;
			}
			case SyntaxKind.Property:
			{
				PropertySyntax property = (PropertySyntax)memberDeclaration;
				flag = !LookupPosition.IsInProperty(position, property);
				break;
			}
			}
		}
		if (!flag)
		{
			return GetMemberModel(parent);
		}
		return null;
	}

	internal override MemberSemanticModel GetMemberModel(SyntaxNode node)
	{
		SyntaxNode memberDeclaration = GetMemberDeclaration(node);
		if (memberDeclaration != null)
		{
			TextSpan span = node.Span;
			switch (memberDeclaration.Kind)
			{
			case SyntaxKind.TriggerDeclaration:
			case SyntaxKind.EventTriggerDeclaration:
			case SyntaxKind.MethodDeclaration:
			case SyntaxKind.EventDeclaration:
			{
				MethodOrTriggerDeclarationSyntax methodOrTriggerDeclarationSyntax = (MethodOrTriggerDeclarationSyntax)memberDeclaration;
				MemberSemanticModel orAddModelIfContains = GetOrAddModelIfContains(methodOrTriggerDeclarationSyntax.Body, span);
				if (orAddModelIfContains == null)
				{
					foreach (MemberAttributeSyntax attribute in methodOrTriggerDeclarationSyntax.Attributes)
					{
						orAddModelIfContains = GetOrAddModelIfContains(attribute, span);
						if (orAddModelIfContains != null)
						{
							break;
						}
					}
				}
				return orAddModelIfContains;
			}
			case SyntaxKind.PageField:
			case SyntaxKind.ReportDataItem:
			case SyntaxKind.ReportColumn:
			case SyntaxKind.XmlPortTableElement:
			case SyntaxKind.XmlPortFieldElement:
			case SyntaxKind.XmlPortTextElement:
			case SyntaxKind.XmlPortFieldAttribute:
			case SyntaxKind.XmlPortTextAttribute:
			case SyntaxKind.QueryDataItem:
			case SyntaxKind.QueryColumn:
			case SyntaxKind.QueryFilter:
			case SyntaxKind.Property:
				return GetOrAddModelIfContains(memberDeclaration, span);
			}
		}
		return null;
	}

	private MemberSemanticModel GetOrAddModelIfContains(SyntaxNode node, TextSpan span)
	{
		if (node != null && node.FullSpan.Contains(span))
		{
			return GetOrAddModel(node);
		}
		return null;
	}

	private MemberSemanticModel GetOrAddModel(SyntaxNode node)
	{
		Func<SyntaxNode, MemberSemanticModel> valueFactory = CreateMemberModel;
		return ImmutableInterlocked.GetOrAdd(ref memberModels, node, valueFactory);
	}

	private MemberSemanticModel CreateMemberModel(SyntaxNode node)
	{
		return memberSemanticModelFactory.CreateMemberModel(node);
	}

	private Symbol GetDeclaredMemberSymbol(SyntaxNode declarationSyntax)
	{
		CheckSyntaxNode(declarationSyntax);
		ContainerSymbol declaredTypeMemberContainer = GetDeclaredTypeMemberContainer(declarationSyntax);
		string declarationName = GetDeclarationName(declarationSyntax);
		return GetDeclaredMember(declaredTypeMemberContainer, declarationSyntax.Span, declarationName);
	}

	private ContainerSymbol GetDeclaredTypeMemberContainer(SyntaxNode memberDeclaration)
	{
		if (memberDeclaration.Parent.Kind == SyntaxKind.CompilationUnit)
		{
			return Compilation.CompiledModule;
		}
		ContainerSymbol declaredParentType = GetDeclaredParentType(memberDeclaration);
		if (declaredParentType != null)
		{
			return declaredParentType;
		}
		ContainerSymbol declaredParentType2 = GetDeclaredParentType(memberDeclaration.Parent);
		DebugAssertHelper.Assert((object)declaredParentType2 != null);
		return declaredParentType2;
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	private ContainerSymbol GetDeclaredParentType(SyntaxNode memberDeclaration)
	{
		SyntaxNode parent = memberDeclaration.Parent;
		if (parent != null)
		{
			if (parent is ApplicationObjectSyntax applicationObjectSyntax)
			{
				ApplicationObjectSyntax declarationSyntax = applicationObjectSyntax;
				return GetDeclaredContainer(declarationSyntax);
			}
			if (parent is RequestPageSyntax requestPageSyntax)
			{
				RequestPageSyntax declarationSyntax2 = requestPageSyntax;
				return GetDeclaredContainer(declarationSyntax2);
			}
			if (parent is MethodDeclarationSyntax methodDeclarationSyntax)
			{
				MethodDeclarationSyntax declarationSyntax3 = methodDeclarationSyntax;
				return GetDeclaredContainer(declarationSyntax3);
			}
			if (parent is FieldSyntax fieldSyntax)
			{
				FieldSyntax declarationSyntax4 = fieldSyntax;
				return GetDeclaredContainer(declarationSyntax4);
			}
			if (parent is ControlBaseSyntax controlBaseSyntax)
			{
				ControlBaseSyntax declarationSyntax5 = controlBaseSyntax;
				return GetDeclaredContainer(declarationSyntax5);
			}
			if (parent is PageViewSyntax pageViewSyntax)
			{
				PageViewSyntax declarationSyntax6 = pageViewSyntax;
				return GetDeclaredContainer(declarationSyntax6);
			}
			if (parent is PageViewListSyntax pageViewListSyntax)
			{
				PageViewListSyntax pageViewListSyntax2 = pageViewListSyntax;
				return GetDeclaredContainer(pageViewListSyntax2.Parent as ApplicationObjectSyntax);
			}
			if (parent is PageExtensionViewListSyntax pageExtensionViewListSyntax)
			{
				PageExtensionViewListSyntax pageExtensionViewListSyntax2 = pageExtensionViewListSyntax;
				return GetDeclaredContainer(pageExtensionViewListSyntax2.Parent as ApplicationObjectSyntax);
			}
			if (parent is ActionGroupBaseSyntax actionGroupBaseSyntax)
			{
				ActionGroupBaseSyntax declarationSyntax7 = actionGroupBaseSyntax;
				return GetDeclaredContainer(declarationSyntax7);
			}
			if (parent is FieldModificationSyntax fieldModificationSyntax)
			{
				FieldModificationSyntax declarationSyntax8 = fieldModificationSyntax;
				return GetDeclaredContainer(declarationSyntax8);
			}
			if (parent is FieldListSyntax fieldListSyntax)
			{
				FieldListSyntax fieldListSyntax2 = fieldListSyntax;
				return GetDeclaredContainer(fieldListSyntax2.Parent as ApplicationObjectSyntax);
			}
			if (parent is KeyListSyntax keyListSyntax)
			{
				KeyListSyntax keyListSyntax2 = keyListSyntax;
				return keyListSyntax2.Parent.Kind switch
				{
					SyntaxKind.TableObject => GetDeclaredContainer(keyListSyntax2.Parent as TableSyntax), 
					SyntaxKind.TableExtensionObject => GetDeclaredContainer(keyListSyntax2.Parent as TableExtensionSyntax), 
					_ => null, 
				};
			}
			if (parent is FieldGroupListSyntax fieldGroupListSyntax)
			{
				FieldGroupListSyntax fieldGroupListSyntax2 = fieldGroupListSyntax;
				return GetDeclaredContainer(fieldGroupListSyntax2.Parent as TableSyntax);
			}
			if (parent is ActionChangeBaseSyntax actionChangeBaseSyntax)
			{
				ActionChangeBaseSyntax declarationSyntax9 = actionChangeBaseSyntax;
				return GetDeclaredContainer(declarationSyntax9);
			}
			if (parent is ControlChangeBaseSyntax controlChangeBaseSyntax)
			{
				ControlChangeBaseSyntax declarationSyntax10 = controlChangeBaseSyntax;
				return GetDeclaredContainer(declarationSyntax10);
			}
			if (parent is ViewChangeBaseSyntax viewChangeBaseSyntax)
			{
				ViewChangeBaseSyntax declarationSyntax11 = viewChangeBaseSyntax;
				return GetDeclaredContainer(declarationSyntax11);
			}
			if (parent is ActionBaseSyntax actionBaseSyntax)
			{
				ActionBaseSyntax declarationSyntax12 = actionBaseSyntax;
				return GetDeclaredContainer(declarationSyntax12);
			}
			if (parent is ReportDataItemSyntax reportDataItemSyntax)
			{
				ReportDataItemSyntax declarationSyntax13 = reportDataItemSyntax;
				return GetDeclaredContainer(declarationSyntax13);
			}
			if (parent is ReportDataSetSectionSyntax reportDataSetSectionSyntax)
			{
				ReportDataSetSectionSyntax reportDataSetSectionSyntax2 = reportDataSetSectionSyntax;
				return GetDeclaredContainer(reportDataSetSectionSyntax2.Parent as ReportSyntax);
			}
			if (parent is ReportLabelsSectionSyntax reportLabelsSectionSyntax)
			{
				ReportLabelsSectionSyntax reportLabelsSectionSyntax2 = reportLabelsSectionSyntax;
				return GetDeclaredContainer(reportLabelsSectionSyntax2.Parent as ReportSyntax);
			}
			if (parent is XmlPortNodeSyntax xmlPortNodeSyntax)
			{
				XmlPortNodeSyntax declarationSyntax14 = xmlPortNodeSyntax;
				return GetDeclaredContainer(declarationSyntax14);
			}
			if (parent is XmlPortSchemaSyntax xmlPortSchemaSyntax)
			{
				XmlPortSchemaSyntax xmlPortSchemaSyntax2 = xmlPortSchemaSyntax;
				return GetDeclaredContainer(xmlPortSchemaSyntax2.Parent as XmlPortSyntax);
			}
			if (parent is QueryDataItemSyntax queryDataItemSyntax)
			{
				QueryDataItemSyntax declarationSyntax15 = queryDataItemSyntax;
				return GetDeclaredContainer(declarationSyntax15);
			}
			if (parent is QueryElementsSyntax queryElementsSyntax)
			{
				QueryElementsSyntax queryElementsSyntax2 = queryElementsSyntax;
				return GetDeclaredContainer(queryElementsSyntax2.Parent as QuerySyntax);
			}
			if (parent is ControlAddInSyntax controlAddInSyntax)
			{
				ControlAddInSyntax declarationSyntax16 = controlAddInSyntax;
				return GetDeclaredContainer(declarationSyntax16);
			}
			if (parent is DotNetPackageSyntax dotNetPackageSyntax)
			{
				DotNetPackageSyntax declarationSyntax17 = dotNetPackageSyntax;
				return GetDeclaredDotNetPackageContainer(declarationSyntax17);
			}
			if (parent is DotNetAssemblySyntax dotNetAssemblySyntax)
			{
				DotNetAssemblySyntax declarationSyntax18 = dotNetAssemblySyntax;
				return GetDeclaredContainer(declarationSyntax18);
			}
		}
		return null;
	}

	private static string GetDeclarationName(SyntaxNode declaration)
	{
		switch (declaration.Kind)
		{
		case SyntaxKind.Property:
			return ((PropertySyntax)declaration).Name.Identifier.Text;
		case SyntaxKind.TriggerDeclaration:
		case SyntaxKind.MethodDeclaration:
			return ((MethodOrTriggerDeclarationSyntax)declaration).Name.Identifier.ValueText;
		case SyntaxKind.EventDeclaration:
			return ((EventDeclarationSyntax)declaration).Name.Identifier.ValueText;
		case SyntaxKind.Field:
			return ((FieldSyntax)declaration).Name.Identifier.ValueText;
		case SyntaxKind.ReportDataItem:
			return ((ReportDataItemSyntax)declaration).Name.Identifier.ValueText;
		case SyntaxKind.ReportLabel:
		case SyntaxKind.ReportLabelMultilanguage:
			return ((ReportLabelBaseSyntax)declaration).Name.Identifier.ValueText;
		case SyntaxKind.XmlPortTableElement:
		case SyntaxKind.XmlPortFieldElement:
		case SyntaxKind.XmlPortTextElement:
		case SyntaxKind.XmlPortFieldAttribute:
		case SyntaxKind.XmlPortTextAttribute:
			return ((XmlPortNodeSyntax)declaration).Name.Identifier.ValueText;
		case SyntaxKind.DotNetAssembly:
			return ((DotNetAssemblySyntax)declaration).AssemblyName.Unquoted();
		default:
			if (declaration is ActionBaseSyntax actionBaseSyntax)
			{
				return actionBaseSyntax.Name.Identifier.ValueText;
			}
			if (declaration is ApplicationObjectSyntax applicationObjectSyntax)
			{
				return applicationObjectSyntax.Name.Identifier.ValueText;
			}
			return null;
		}
	}

	private TypeSymbol GetDeclaredNamedType(SyntaxNode declarationSyntax, string name)
	{
		return GetDeclaredContainer(declarationSyntax, name) as TypeSymbol;
	}

	private ContainerSymbol GetDeclaredContainer(SyntaxNode declarationSyntax, string name)
	{
		DebugAssertHelper.Assert(declarationSyntax != null);
		ContainerSymbol declaredTypeMemberContainer = GetDeclaredTypeMemberContainer(declarationSyntax);
		DebugAssertHelper.Assert((object)declaredTypeMemberContainer != null);
		return GetDeclaredMember(declaredTypeMemberContainer, declarationSyntax.Span, name) as ContainerSymbol;
	}

	private ContainerSymbol GetDeclaredContainer<U>(U declarationSyntax) where U : SyntaxNode
	{
		DebugAssertHelper.Assert(declarationSyntax != null);
		if (declarationSyntax == null)
		{
			return null;
		}
		string declarationName = GetDeclarationName(declarationSyntax);
		return GetDeclaredContainer(declarationSyntax, declarationName);
	}

	private ContainerSymbol GetDeclaredDotNetPackageContainer(DotNetPackageSyntax declarationSyntax)
	{
		DebugAssertHelper.Assert(declarationSyntax != null);
		if (declarationSyntax == null)
		{
			return null;
		}
		ContainerSymbol declaredTypeMemberContainer = GetDeclaredTypeMemberContainer(declarationSyntax);
		DebugAssertHelper.Assert((object)declaredTypeMemberContainer != null);
		ImmutableArray<Symbol> immutableArray = declaredTypeMemberContainer.GetMembersUnordered().WhereAsArray((Symbol x) => x.Kind == SymbolKind.DotNetPackage);
		GetClosestSymbol(immutableArray, declarationSyntax.Span, out var found, out var _);
		return found as ContainerSymbol;
	}

	internal override IObjectTypeSymbol GetDeclaredSymbol(ObjectSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (declarationSyntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		return GetDeclaredContainer(declarationSyntax) as IObjectTypeSymbol;
	}

	internal override IMethodSymbol GetDeclaredSymbol(MethodOrTriggerDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (declarationSyntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		return (IMethodSymbol)GetDeclaredMemberSymbol(declarationSyntax);
	}

	internal override IEventSymbol GetDeclaredSymbol(EventDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (declarationSyntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		return (IEventSymbol)GetDeclaredMemberSymbol(declarationSyntax);
	}

	internal override IAttributeSymbol GetDeclaredSymbol(MemberAttributeSyntax attributeSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (attributeSyntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		SyntaxNode parent = attributeSyntax.Parent;
		if (parent.Kind == SyntaxKind.MethodDeclaration)
		{
			return GetDeclaredSymbol((MethodOrTriggerDeclarationSyntax)parent, cancellationToken).Attributes.FirstOrDefault((IAttributeSymbol attribute) => attribute.Name.Equals(attributeSyntax.Name.Identifier.ValueText, StringComparison.OrdinalIgnoreCase));
		}
		if (parent.Kind == SyntaxKind.VariableDeclaration)
		{
			return GetDeclaredSymbol((VariableDeclarationSyntax)parent, cancellationToken).Attributes.FirstOrDefault((AttributeSymbol attribute) => attribute.Name.Equals(attributeSyntax.Name.Identifier.ValueText, StringComparison.OrdinalIgnoreCase));
		}
		return null;
	}

	internal override PropertySymbol GetDeclaredSymbol(PropertySyntax propertySyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (propertySyntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		return GetDeclaredProperty(propertySyntax);
	}

	internal override OptionSymbol GetDeclaredSymbol(EnumValueSyntax enumValueSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (enumValueSyntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		return GetDeclaredEnumValue(enumValueSyntax);
	}

	internal override Symbol GetDeclaredSymbol(ObjectNameOrIdSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		SyntaxNode syntaxNode = syntax?.Parent;
		if (syntaxNode == null)
		{
			return null;
		}
		SyntaxKind kind = syntaxNode.Kind;
		if (kind == SyntaxKind.PagePart)
		{
			return GetSymbolFromObjectReference(syntax, SymbolKind.Page);
		}
		return null;
	}

	internal override Symbol GetDeclaredSymbol(ObjectNameReferenceSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (syntax == null)
		{
			return null;
		}
		return Compilation.GetControlAddInTypeSymbol(syntax, DiagnosticBag.GetNullInstance());
	}

	internal override VariableSymbol GetDeclaredSymbol(VariableDeclarationSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (syntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		SyntaxNode firstParent = syntax.GetFirstParent((SyntaxNode n) => n.Kind == SyntaxKind.MethodDeclaration || n.Kind == SyntaxKind.TriggerDeclaration || n.Kind == SyntaxKind.EventDeclaration || n.Kind == SyntaxKind.RequestPage);
		if (firstParent == null || firstParent.Kind == SyntaxKind.RequestPage)
		{
			return GetGlobalVariable(syntax, firstParent, cancellationToken);
		}
		MethodSymbol obj = GetDeclaredSymbol(firstParent as MethodOrTriggerDeclarationSyntax) as MethodSymbol;
		string variableName = syntax.Name.GetIdentifierOrLiteralValue();
		return obj?.LocalVariables.FirstOrDefault((VariableSymbol s) => string.Compare(variableName, s.Name, StringComparison.OrdinalIgnoreCase) == 0);
	}

	private VariableSymbol GetGlobalVariable(VariableDeclarationSyntax syntax, SyntaxNode requestPage, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (requestPage != null)
		{
			if (requestPage.Parent.Kind == SyntaxKind.ReportObject)
			{
				ReportTypeSymbol reportTypeSymbol = (ReportTypeSymbol)GetDeclaredParentType((RequestPageSyntax)requestPage);
				return GetDeclaredMember(reportTypeSymbol.RequestPage, syntax.Span, syntax.Name.Identifier.ValueText) as VariableSymbol;
			}
			if (requestPage.Parent.Kind == SyntaxKind.XmlPortObject)
			{
				SourceXmlPortTypeSymbol sourceXmlPortTypeSymbol = (SourceXmlPortTypeSymbol)GetDeclaredParentType((RequestPageSyntax)requestPage);
				return GetDeclaredMember(sourceXmlPortTypeSymbol.RequestPage, syntax.Span, syntax.Name.Identifier.ValueText) as VariableSymbol;
			}
		}
		return GetSymbol<VariableSymbol, VariableDeclarationSyntax>(syntax, syntax.Name, GetDeclaringObject);
	}

	internal override Symbol GetDeclaredSymbol(OptionDataTypeSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDeclaredDataTypeSymbol(syntax, cancellationToken);
	}

	internal override Symbol GetDeclaredSymbol(EnumDataTypeSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDeclaredDataTypeSymbol(syntax, cancellationToken);
	}

	private Symbol GetDeclaredDataTypeSymbol(SyntaxNode syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (syntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		SyntaxNode firstParent = syntax.GetFirstParent((SyntaxNode n) => n.Kind == SyntaxKind.VariableDeclaration || n.Kind == SyntaxKind.Field || n.Kind == SyntaxKind.Parameter);
		if (firstParent == null)
		{
			return null;
		}
		return GetDeclaredSymbol(firstParent, cancellationToken).GetTypeSymbol() as Symbol;
	}

	internal override FieldSymbol GetDeclaredSymbol(FieldSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (syntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		return GetSymbol<FieldSymbol, FieldSyntax>(syntax, syntax.Name, GetDeclaringTable);
	}

	internal override ControlSymbol GetDeclaredSymbol(ControlBaseSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (syntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		return GetSymbol<ControlSymbol, ControlBaseSyntax>(syntax, syntax.Name, GetDeclaringPageControlContainer);
	}

	internal override ViewSymbol GetDeclaredSymbol(PageViewSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (syntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		return GetSymbol<ViewSymbol, PageViewSyntax>(syntax, syntax.Name, GetDeclaringPageViewContainer);
	}

	internal override TypeSymbol GetDeclaredSymbol(ReportDataItemSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (syntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		return GetSymbol<TypeSymbol, ReportDataItemSyntax>(syntax, syntax.Name, GetDeclaringReportDataItemContainer);
	}

	internal override XmlPortNodeSymbol GetDeclaredSymbol(XmlPortNodeSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (syntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		return GetSymbol<XmlPortNodeSymbol, XmlPortNodeSyntax>(syntax, syntax.Name, GetDeclaringXmlPortNodeContainer);
	}

	internal override TypeSymbol GetDeclaredSymbol(ReportColumnSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (syntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		return GetSymbol<TypeSymbol, ReportColumnSyntax>(syntax, syntax.Name, GetDeclaringReportDataItemContainer);
	}

	internal override ActionSymbol GetDeclaredSymbol(ActionBaseSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (syntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		return GetSymbol<ActionSymbol, ActionBaseSyntax>(syntax, syntax.Name, GetDeclaringPageActionContainer);
	}

	internal override ParameterSymbol GetDeclaredSymbol(ParameterSyntax parameter, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (parameter == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		if (!(parameter.Parent is ParameterListSyntax { Parent: var parent } parameterListSyntax))
		{
			return null;
		}
		if (parameterListSyntax.ParentIsKind(SyntaxKind.MethodDeclaration))
		{
			MethodSymbol methodSymbol = GetDeclaredSymbol(parent, cancellationToken) as MethodSymbol;
			if (methodSymbol != null)
			{
				return GetParameterSymbol(methodSymbol.Parameters, parameter, cancellationToken);
			}
		}
		return null;
	}

	internal override ReturnValueSymbol GetDeclaredSymbol(ReturnValueSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (syntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		SyntaxNode firstParent = syntax.GetFirstParent((SyntaxNode n) => n.Kind == SyntaxKind.MethodDeclaration);
		return (GetDeclaredSymbol(firstParent as MethodOrTriggerDeclarationSyntax) as MethodSymbol)?.ReturnValue;
	}

	internal override FieldNameListSymbol GetDeclaredSymbol(KeySyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (syntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		return GetFieldNameListSymbol(syntax, SymbolKind.Key);
	}

	internal override ReportLabelSymbol GetDeclaredSymbol(ReportLabelBaseSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (syntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		ContainerSymbol declaredParentType = GetDeclaredParentType(syntax);
		if (declaredParentType == null)
		{
			return null;
		}
		string text = syntax.Name.Identifier.Text;
		DebugAssertHelper.Assert(text != null);
		text = text.UnquoteIdentifier();
		return declaredParentType.GetMembers(text).FirstOrDefault((Symbol s) => s.Kind == SymbolKind.ReportLabel) as ReportLabelSymbol;
	}

	internal override QueryFilterSymbol GetDeclaredSymbol(QueryFilterSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (syntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		string text = syntax.Name.Identifier.Text;
		DebugAssertHelper.Assert(text != null);
		text = text.UnquoteIdentifier();
		return GetDeclaredParentType(syntax)?.GetMembers(text).FirstOrDefault((Symbol s) => s.Kind == SymbolKind.QueryFilter) as QueryFilterSymbol;
	}

	internal override DotNetAssemblySymbol GetDeclaredSymbol(DotNetAssemblySyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (syntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		string name = syntax.AssemblyName.Unquoted();
		return GetDeclaredParentType(syntax)?.GetMembers(name).FirstOrDefault((Symbol s) => s.Kind == SymbolKind.DotNetAssembly) as DotNetAssemblySymbol;
	}

	internal override DotNetTypeDeclarationSymbol GetDeclaredSymbol(DotNetTypeDeclarationSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (syntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		string typeAlias = syntax.GetTypeAlias();
		return GetDeclaredParentType(syntax)?.GetMembers(typeAlias).FirstOrDefault((Symbol s) => s.Kind == SymbolKind.DotNetTypeDeclaration) as DotNetTypeDeclarationSymbol;
	}

	internal override QueryColumnSymbol GetDeclaredSymbol(QueryColumnSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (syntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		string text = syntax.Name.Identifier.Text;
		DebugAssertHelper.Assert(text != null);
		text = text.UnquoteIdentifier();
		return GetDeclaredParentType(syntax)?.GetMembers(text).FirstOrDefault((Symbol s) => s.Kind == SymbolKind.QueryColumn) as QueryColumnSymbol;
	}

	internal override QueryDataItemSymbol GetDeclaredSymbol(QueryDataItemSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (syntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		string text = syntax.Name.Identifier.Text;
		DebugAssertHelper.Assert(text != null);
		text = text.UnquoteIdentifier();
		return (this?.GetDeclaredParentType(syntax)).GetMembers(text).FirstOrDefault((Symbol s) => s.Kind == SymbolKind.QueryDataItem) as QueryDataItemSymbol;
	}

	internal override FieldNameListSymbol GetDeclaredSymbol(FieldGroupSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (syntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		return GetFieldNameListSymbol(syntax, SymbolKind.FieldGroup);
	}

	internal override ChangeSymbol GetDeclaredSymbol(ControlChangeBaseSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (syntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		switch (syntax.Kind)
		{
		default:
			throw ExceptionUtilities.UnexpectedValue(syntax.Kind);
		case SyntaxKind.ControlAddChange:
		case SyntaxKind.ControlMoveChange:
			return GetControlChangeSymbol(syntax);
		case SyntaxKind.ControlModifyChange:
		{
			ControlModifyChangeSyntax controlModifyChangeSyntax = (ControlModifyChangeSyntax)syntax;
			return GetSymbol<ChangeSymbol, ControlModifyChangeSyntax>(controlModifyChangeSyntax, controlModifyChangeSyntax.Name, GetDeclaringPageControlContainer);
		}
		}
	}

	private ChangeSymbol GetControlChangeSymbol(ControlChangeBaseSyntax syntax)
	{
		SyntaxNode syntaxNode = syntax.GetFirstParent(SyntaxKind.PageView) ?? syntax.GetFirstParent(SyntaxKind.PageExtensionObject) ?? syntax.GetFirstParent(SyntaxKind.PageCustomizationObject);
		if (syntaxNode != null)
		{
			ISymbol declaredSymbol = GetDeclaredSymbol(syntaxNode);
			if (declaredSymbol is PageExtensionBaseTypeSymbol pageExtensionBaseTypeSymbol)
			{
				return pageExtensionBaseTypeSymbol.GetMembers().FirstOrDefault((Symbol s) => s.GetFirstSyntax() == syntax) as ChangeSymbol;
			}
			if (declaredSymbol is ViewSymbol viewSymbol)
			{
				return viewSymbol.GetMembers().FirstOrDefault((Symbol s) => s.GetFirstSyntax() == syntax) as ChangeSymbol;
			}
		}
		return null;
	}

	internal override ChangeSymbol GetDeclaredSymbol(ActionChangeBaseSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (syntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		switch (syntax.Kind)
		{
		default:
			throw ExceptionUtilities.UnexpectedValue(syntax.Kind);
		case SyntaxKind.ActionAddChange:
		{
			ActionAddChangeSyntax actionAddChangeSyntax = (ActionAddChangeSyntax)syntax;
			return GetSymbol<ChangeSymbol, ActionAddChangeSyntax>(actionAddChangeSyntax, actionAddChangeSyntax.GetName(), GetDeclaringPageActionContainer);
		}
		case SyntaxKind.ActionMoveChange:
		{
			ActionMoveChangeSyntax syntaxNode = (ActionMoveChangeSyntax)syntax;
			return GetSymbol<ChangeSymbol, ActionMoveChangeSyntax>(syntaxNode, syntaxNode.GetNameStringValue(), GetDeclaringPageActionContainer);
		}
		case SyntaxKind.ActionModifyChange:
		{
			ActionModifyChangeSyntax actionModifyChangeSyntax = (ActionModifyChangeSyntax)syntax;
			return GetSymbol<ChangeSymbol, ActionModifyChangeSyntax>(actionModifyChangeSyntax, actionModifyChangeSyntax.Name, GetDeclaringPageActionContainer);
		}
		}
	}

	internal override ChangeSymbol GetDeclaredSymbol(ViewChangeBaseSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (syntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		switch (syntax.Kind)
		{
		default:
			throw ExceptionUtilities.UnexpectedValue(syntax.Kind);
		case SyntaxKind.ViewAddChange:
		{
			ViewAddChangeSyntax viewAddChangeSyntax = (ViewAddChangeSyntax)syntax;
			return GetSymbol<ChangeSymbol, ViewAddChangeSyntax>(viewAddChangeSyntax, viewAddChangeSyntax.GetName(), GetDeclaringPageViewContainer);
		}
		case SyntaxKind.ViewMoveChange:
		{
			ViewMoveChangeSyntax syntaxNode = (ViewMoveChangeSyntax)syntax;
			return GetSymbol<ChangeSymbol, ViewMoveChangeSyntax>(syntaxNode, syntaxNode.GetNameStringValue(), GetDeclaringPageViewContainer);
		}
		case SyntaxKind.ViewModifyChange:
		{
			ViewModifyChangeSyntax viewModifyChangeSyntax = (ViewModifyChangeSyntax)syntax;
			return GetSymbol<ChangeSymbol, ViewModifyChangeSyntax>(viewModifyChangeSyntax, viewModifyChangeSyntax.Name, GetDeclaringPageViewContainer);
		}
		}
	}

	internal override ChangeSymbol GetDeclaredSymbol(FieldModificationSyntax syntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (syntax == null)
		{
			return null;
		}
		cancellationToken.ThrowIfCancellationRequested();
		return GetSymbol<ChangeSymbol, FieldModificationSyntax>(syntax, syntax.Name, GetDeclaringTable);
	}

	private FieldNameListSymbol GetFieldNameListSymbol(SyntaxNode syntax, SymbolKind kind)
	{
		DebugAssertHelper.Assert(kind == SymbolKind.Key || kind == SymbolKind.FieldGroup);
		ContainerSymbol containerSymbol = this?.GetDeclaredParentType(syntax);
		string text = null;
		switch (kind)
		{
		case SymbolKind.Key:
			text = ((KeySyntax)syntax).Name.Identifier.Text;
			break;
		case SymbolKind.FieldGroup:
			text = ((FieldGroupSyntax)syntax).Name.Identifier.Text;
			break;
		}
		DebugAssertHelper.Assert(text != null);
		text = text.UnquoteIdentifier();
		return containerSymbol.GetMembers(text).FirstOrDefault((Symbol s) => s.Kind == kind) as FieldNameListSymbol;
	}

	protected internal override ImmutableArray<IPropertySymbol> GetDeclaredProperties(SyntaxNode propertyParent)
	{
		IEnumerable<PropertySymbol> items = ImmutableArray<PropertySymbol>.Empty;
		SyntaxKind kind = propertyParent.Kind;
		switch ((int)kind)
		{
		case 260:
			items = GetFieldDeclaredProperties((FieldSyntax)propertyParent);
			break;
		case 280:
			items = GetPageActionDeclaredProperties((ActionBaseSyntax)propertyParent);
			break;
		case 320:
			items = GetPageViewDeclaredProperties((PageViewSyntax)propertyParent);
			break;
		case 299:
			items = GetReportColumnDeclaredProperties((ReportColumnSyntax)propertyParent);
			break;
		case 298:
			items = GetReportDataItemDeclaredProperties((ReportDataItemSyntax)propertyParent);
			break;
		case 311:
			items = GetQueryColumnDeclaredProperties((QueryColumnSyntax)propertyParent);
			break;
		case 312:
			items = GetQueryFilterDeclaredProperties((QueryFilterSyntax)propertyParent);
			break;
		case 310:
			items = GetQueryDataItemDeclaredProperties((QueryDataItemSyntax)propertyParent);
			break;
		case 303:
			items = GetXmlPortTableElementDeclaredProperties((XmlPortTableElementSyntax)propertyParent);
			break;
		case 287:
			items = GetActionModifyDeclaredProperties((ActionModifyChangeSyntax)propertyParent);
			break;
		case 291:
			items = GetControlModifyDeclaredProperties((ControlModifyChangeSyntax)propertyParent);
			break;
		case 295:
			items = GetViewModifyDeclaredProperties((ViewModifyChangeSyntax)propertyParent);
			break;
		case 264:
			items = GetFieldModificationDeclaredProperties((FieldModificationSyntax)propertyParent);
			break;
		case 261:
			items = GetDotNetAssemblyDeclaredProperties((DotNetAssemblySyntax)propertyParent);
			break;
		default:
			if (kind.IsPageControl())
			{
				items = GetPageControlDeclaredProperties((ControlBaseSyntax)propertyParent);
			}
			else if (kind.IsFieldGroup())
			{
				items = GetFieldNameListDeclaredProperties((KeyFieldGroupBaseSyntax)propertyParent);
			}
			else if (kind.IsObject())
			{
				items = ((ObjectTypeSymbol)GetDeclaredContainer(propertyParent as ObjectSyntax)).Properties;
			}
			break;
		}
		return ((IEnumerable<IPropertySymbol>)items).ToImmutableArray();
	}

	private IEnumerable<PropertySymbol> GetActionModifyDeclaredProperties(ActionModifyChangeSyntax propertyParent)
	{
		return GetSymbol<ChangeModifySymbol, ActionModifyChangeSyntax>(propertyParent, propertyParent.Name, GetDeclaringObject)?.Properties ?? ImmutableArray<PropertySymbol>.Empty;
	}

	private IEnumerable<PropertySymbol> GetControlModifyDeclaredProperties(ControlModifyChangeSyntax propertyParent)
	{
		return GetSymbol<ChangeModifySymbol, ControlModifyChangeSyntax>(propertyParent, propertyParent.Name, GetDeclaringObject)?.Properties ?? ImmutableArray<PropertySymbol>.Empty;
	}

	private IEnumerable<PropertySymbol> GetViewModifyDeclaredProperties(ViewModifyChangeSyntax propertyParent)
	{
		return GetSymbol<ChangeModifySymbol, ViewModifyChangeSyntax>(propertyParent, propertyParent.Name, GetDeclaringObject)?.Properties ?? ImmutableArray<PropertySymbol>.Empty;
	}

	private IEnumerable<PropertySymbol> GetFieldModificationDeclaredProperties(FieldModificationSyntax propertyParent)
	{
		return GetSymbol<ChangeModifySymbol, FieldModificationSyntax>(propertyParent, propertyParent.Name, GetDeclaringObject)?.Properties ?? ImmutableArray<PropertySymbol>.Empty;
	}

	private IEnumerable<PropertySymbol> GetXmlPortTableElementDeclaredProperties(XmlPortTableElementSyntax propertyParent)
	{
		return GetSymbol<XmlPortNodeSymbol, XmlPortTableElementSyntax>(propertyParent, propertyParent.Name, GetDeclaringXmlPortNodeContainer)?.Properties ?? ImmutableArray<PropertySymbol>.Empty;
	}

	private IEnumerable<PropertySymbol> GetQueryDataItemDeclaredProperties(QueryDataItemSyntax propertyParent)
	{
		return GetSymbol<SourceQueryDataItemSymbol, QueryDataItemSyntax>(propertyParent, propertyParent.Name, GetDeclaringQueryDataItemContainer)?.Properties ?? ImmutableArray<PropertySymbol>.Empty;
	}

	private IEnumerable<PropertySymbol> GetQueryColumnDeclaredProperties(QueryColumnSyntax propertyParent)
	{
		return GetSymbol<SourceQueryColumnSymbol, QueryColumnSyntax>(propertyParent, propertyParent.Name, GetDeclaringQueryDataItem)?.Properties ?? ImmutableArray<PropertySymbol>.Empty;
	}

	private IEnumerable<PropertySymbol> GetDotNetAssemblyDeclaredProperties(DotNetAssemblySyntax propertyParent)
	{
		return GetSymbol<SourceDotNetAssemblySymbol, DotNetAssemblySyntax>(propertyParent, propertyParent.AssemblyName.Unquoted(), GetDeclaringDotNetPackage)?.Properties ?? ImmutableArray<PropertySymbol>.Empty;
	}

	private TypeSymbol GetDeclaringDotNetPackage(string identifierName, SyntaxNode syntaxNode)
	{
		ObjectSyntax objectSyntax = syntaxNode.GetFirstParent((SyntaxNode n) => n.IsKind(SyntaxKind.DotNetPackage)) as ObjectSyntax;
		DebugAssertHelper.Assert(objectSyntax != null);
		return GetDeclaredNamedType(objectSyntax, identifierName);
	}

	private IEnumerable<PropertySymbol> GetQueryFilterDeclaredProperties(QueryFilterSyntax propertyParent)
	{
		return GetSymbol<SourceQueryFilterSymbol, QueryFilterSyntax>(propertyParent, propertyParent.Name, GetDeclaringQueryDataItem)?.Properties ?? ImmutableArray<PropertySymbol>.Empty;
	}

	private ContainerSymbol GetDeclaringQueryDataItem(string identifierName, SyntaxNode syntaxNode)
	{
		SyntaxNode firstParent = syntaxNode.GetFirstParent((SyntaxNode n) => n.Kind == SyntaxKind.QueryDataItem);
		DebugAssertHelper.Assert(firstParent != null);
		return GetDeclaredContainer(firstParent, identifierName);
	}

	private PropertySymbol GetDeclaredProperty(PropertySyntax propertySyntax)
	{
		CheckSyntaxNode(propertySyntax);
		SyntaxNode syntaxNode = propertySyntax.FirstAncestorOrSelf<PropertyListSyntax>()?.Parent;
		ImmutableArray<PropertySymbol> immutableArray = GetDeclaredProperties(syntaxNode).Cast<PropertySymbol>().ToImmutableArray();
		if (immutableArray.IsDefaultOrEmpty)
		{
			return null;
		}
		string propertyName = propertySyntax.Name.Identifier.Text;
		PropertySymbol propertySymbol = immutableArray.FirstOrDefault((PropertySymbol sym) => string.Compare(sym.Name, propertyName, StringComparison.OrdinalIgnoreCase) == 0);
		if (propertySymbol != null)
		{
			return propertySymbol;
		}
		GetClosestSymbol(immutableArray, syntaxNode.Span, out var found, out var isFoundThroughSyntaxRecovery);
		if (isFoundThroughSyntaxRecovery || !(found != null) || found.Kind != SymbolKind.Property)
		{
			return null;
		}
		return (PropertySymbol)found;
	}

	private OptionSymbol GetDeclaredEnumValue(EnumValueSyntax syntax)
	{
		return GetSymbol<OptionSymbol, EnumValueSyntax>(syntax, syntax.Name, GetDeclaringObject);
	}

	private U GetSymbol<U, V>(V syntaxNode, string identifierName, Func<string, V, ContainerSymbol> getMember) where U : Symbol where V : SyntaxNode
	{
		ContainerSymbol containerSymbol = getMember(identifierName, syntaxNode);
		DebugAssertHelper.Assert(containerSymbol != null);
		return GetDeclaredMember(containerSymbol, syntaxNode.Span, identifierName) as U;
	}

	private U GetSymbol<U, V>(V syntaxNode, IdentifierNameSyntax identifierSyntax, Func<string, V, ContainerSymbol> getMember) where U : Symbol where V : SyntaxNode
	{
		return GetSymbol<U, V>(syntaxNode, identifierSyntax.Identifier.ValueText, getMember);
	}

	private ImmutableArray<PropertySymbol> GetFieldNameListDeclaredProperties(KeyFieldGroupBaseSyntax syntaxNode)
	{
		return GetSymbol<FieldNameListSymbol, KeyFieldGroupBaseSyntax>(syntaxNode, syntaxNode.Name, GetDeclaringTable)?.Properties ?? ImmutableArray<PropertySymbol>.Empty;
	}

	private ImmutableArray<PropertySymbol> GetFieldDeclaredProperties(FieldSyntax fieldSyntax)
	{
		return GetSymbol<FieldSymbol, FieldSyntax>(fieldSyntax, fieldSyntax.Name, GetDeclaringTable)?.Properties ?? ImmutableArray<PropertySymbol>.Empty;
	}

	private ImmutableArray<PropertySymbol> GetReportColumnDeclaredProperties(ReportColumnSyntax columnSyntax)
	{
		return GetSymbol<SourceReportColumnSymbol, ReportColumnSyntax>(columnSyntax, columnSyntax.Name, GetDeclaringReportDataItem)?.Properties ?? ImmutableArray<PropertySymbol>.Empty;
	}

	private ImmutableArray<PropertySymbol> GetReportDataItemDeclaredProperties(ReportDataItemSyntax dataItemSyntax)
	{
		return GetSymbol<ReportDataItemSymbol, ReportDataItemSyntax>(dataItemSyntax, dataItemSyntax.Name, GetDeclaringReportDataItemContainer)?.Properties ?? ImmutableArray<PropertySymbol>.Empty;
	}

	private ImmutableArray<PropertySymbol> GetPageControlDeclaredProperties(ControlBaseSyntax controlSyntax)
	{
		return GetSymbol<ControlSymbol, ControlBaseSyntax>(controlSyntax, controlSyntax.Name, GetDeclaringPageControlContainer)?.Properties ?? ImmutableArray<PropertySymbol>.Empty;
	}

	private ImmutableArray<PropertySymbol> GetPageActionDeclaredProperties(ActionBaseSyntax actionSyntax)
	{
		return GetSymbol<ActionSymbol, ActionBaseSyntax>(actionSyntax, actionSyntax.Name, GetDeclaringPageActionContainer)?.Properties ?? ImmutableArray<PropertySymbol>.Empty;
	}

	private ImmutableArray<PropertySymbol> GetPageViewDeclaredProperties(PageViewSyntax viewSyntax)
	{
		return GetSymbol<ViewSymbol, PageViewSyntax>(viewSyntax, viewSyntax.Name, GetDeclaringPageViewContainer)?.Properties ?? ImmutableArray<PropertySymbol>.Empty;
	}

	private TypeSymbol GetDeclaringObject(string identifierName, SyntaxNode syntaxNode)
	{
		ObjectSyntax objectSyntax = syntaxNode.GetFirstParent((SyntaxNode n) => n.Kind.IsObjectWithMemberDeclarations()) as ObjectSyntax;
		DebugAssertHelper.Assert(objectSyntax != null);
		return GetDeclaredNamedType(objectSyntax, identifierName);
	}

	private TypeSymbol GetDeclaringTable(string identifierName, SyntaxNode syntaxNode)
	{
		SyntaxNode syntaxNode2 = syntaxNode.GetFirstParent(SyntaxKind.TableObject) ?? syntaxNode.GetFirstParent(SyntaxKind.TableExtensionObject);
		DebugAssertHelper.Assert(syntaxNode2 != null);
		return GetDeclaredNamedType(syntaxNode2, identifierName);
	}

	private ContainerSymbol GetDeclaringReportDataItem(string identifierName, SyntaxNode syntaxNode)
	{
		SyntaxNode firstParent = syntaxNode.GetFirstParent((SyntaxNode n) => n.Kind == SyntaxKind.ReportDataItem);
		DebugAssertHelper.Assert(firstParent != null);
		return GetDeclaredContainer(firstParent, identifierName);
	}

	private ContainerSymbol GetDeclaringReportDataItemContainer(string identifierName, SyntaxNode syntaxNode)
	{
		SyntaxNode firstParent = syntaxNode.GetFirstParent((SyntaxNode n) => n.Kind != SyntaxKind.ReportDataSetSection);
		DebugAssertHelper.Assert(firstParent != null);
		return GetDeclaredContainer(firstParent, identifierName);
	}

	private ContainerSymbol GetDeclaringXmlPortNodeContainer(string identifierName, SyntaxNode syntaxNode)
	{
		SyntaxNode firstParent = syntaxNode.GetFirstParent((SyntaxNode n) => n.Kind != SyntaxKind.XmlPortSchema);
		DebugAssertHelper.Assert(firstParent != null);
		return GetDeclaredContainer(firstParent, identifierName);
	}

	private ContainerSymbol GetDeclaringPageControlContainer(string identifierName, SyntaxNode syntaxNode)
	{
		SyntaxNode firstParent = syntaxNode.GetFirstParent(IsDeclaringPageControlContainerSymbol);
		DebugAssertHelper.Assert(firstParent != null);
		return GetDeclaredContainer(firstParent, identifierName);
	}

	private static bool IsDeclaringPageControlContainerSymbol(SyntaxNode node)
	{
		switch (node.Kind)
		{
		case SyntaxKind.ControlAddChange:
		case SyntaxKind.ControlMoveChange:
		case SyntaxKind.ControlModifyChange:
		case SyntaxKind.RequestPage:
		case SyntaxKind.PageView:
		case SyntaxKind.PageObject:
		case SyntaxKind.PageExtensionObject:
		case SyntaxKind.PageCustomizationObject:
			return true;
		case SyntaxKind.PageLayout:
		case SyntaxKind.PageExtensionLayout:
		case SyntaxKind.PageExtensionViewList:
		case SyntaxKind.PageViewList:
			return false;
		default:
			return node is ControlGroupBaseSyntax;
		}
	}

	private ContainerSymbol GetDeclaringPageViewContainer(string identifierName, SyntaxNode syntaxNode)
	{
		SyntaxNode firstParent = syntaxNode.GetFirstParent(IsDeclaringPageViewContainerSymbol);
		DebugAssertHelper.Assert(firstParent != null);
		return GetDeclaredContainer(firstParent, identifierName);
	}

	private static bool IsDeclaringPageViewContainerSymbol(SyntaxNode node)
	{
		switch (node.Kind)
		{
		case SyntaxKind.ViewAddChange:
		case SyntaxKind.ViewMoveChange:
		case SyntaxKind.ViewModifyChange:
		case SyntaxKind.PageView:
		case SyntaxKind.PageObject:
		case SyntaxKind.PageExtensionObject:
		case SyntaxKind.PageCustomizationObject:
			return true;
		case SyntaxKind.PageExtensionViewList:
		case SyntaxKind.PageViewList:
			return false;
		default:
			return false;
		}
	}

	private ContainerSymbol GetDeclaringQueryDataItemContainer(string identifierName, SyntaxNode syntaxNode)
	{
		SyntaxNode firstParent = syntaxNode.GetFirstParent((SyntaxNode n) => n.Kind == SyntaxKind.QueryDataItem || n.Kind == SyntaxKind.QueryObject);
		DebugAssertHelper.Assert(firstParent != null);
		return GetDeclaredContainer(firstParent, identifierName);
	}

	private ContainerSymbol GetDeclaringPageActionContainer(string identifierName, SyntaxNode syntaxNode)
	{
		SyntaxNode firstParent = syntaxNode.GetFirstParent(IsDeclaringPageActionContainerSymbol);
		DebugAssertHelper.Assert(firstParent != null);
		return GetDeclaredContainer(firstParent, identifierName);
	}

	private static bool IsDeclaringPageActionContainerSymbol(SyntaxNode node)
	{
		switch (node.Kind)
		{
		case SyntaxKind.PageGroup:
		case SyntaxKind.ActionAddChange:
		case SyntaxKind.ActionMoveChange:
		case SyntaxKind.ActionModifyChange:
		case SyntaxKind.RequestPage:
		case SyntaxKind.PageObject:
		case SyntaxKind.PageExtensionObject:
		case SyntaxKind.PageCustomizationObject:
			return true;
		case SyntaxKind.PageActionList:
		case SyntaxKind.PageExtensionActionList:
			return false;
		default:
			return node is ActionGroupBaseSyntax;
		}
	}

	private Symbol GetDeclaredMember(ContainerSymbol container, TextSpan declarationSpan, string name = null)
	{
		if ((object)container == null)
		{
			return null;
		}
		ImmutableArray<Symbol> immutableArray = ((name != null) ? container.GetMembers(name) : container.GetMembersUnordered());
		GetClosestSymbol(immutableArray, declarationSpan, out var found, out var isFoundThroughSyntaxRecovery);
		if (!isFoundThroughSyntaxRecovery)
		{
			return found;
		}
		Symbol symbol = found;
		if ((object)symbol == null)
		{
			if (name == null)
			{
				return null;
			}
			symbol = GetDeclaredMember(container, declarationSpan);
		}
		return symbol;
	}

	private void GetClosestSymbol(IEnumerable<Symbol> parentCollection, TextSpan declarationSpan, out Symbol found, out bool isFoundThroughSyntaxRecovery)
	{
		isFoundThroughSyntaxRecovery = false;
		found = null;
		foreach (Symbol item in parentCollection)
		{
			foreach (Location location in item.Locations)
			{
				if (location.IsInSource && location.SourceTree == SyntaxTree && declarationSpan.Contains(location.SourceSpan))
				{
					found = item;
					if (!location.SourceSpan.IsEmpty || location.SourceSpan.End != declarationSpan.Start)
					{
						return;
					}
				}
			}
		}
		isFoundThroughSyntaxRecovery = true;
	}

	private static SyntaxNode GetMemberDeclaration(SyntaxNode node)
	{
		return node.FirstAncestorOrSelf(isMemberDeclarationFunction);
	}

	private static bool IsMemberDeclaration(SyntaxNode node)
	{
		if (!(node is MethodOrTriggerDeclarationSyntax) && !(node is PropertySyntax) && !(node is VariableDeclarationSyntax) && !(node is PageFieldSyntax) && !(node is ReportDataItemElementSyntax) && !(node is QueryDataItemElementSyntax))
		{
			return node is XmlPortNodeSyntax;
		}
		return true;
	}

	public override ImmutableArray<Diagnostic> GetDiagnostics(TextSpan? span = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		VerifySpanForGetDiagnostics(span);
		return Compilation.GetDiagnosticsForSyntaxTree(CompilationStage.Compile, SyntaxTree, span, includeEarlierStages: true, cancellationToken);
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

	private void VerifySpanForGetDiagnostics(TextSpan? span)
	{
		if (span.HasValue && !Root.FullSpan.Contains(span.Value))
		{
			throw new ArgumentException(CodeAnalysisResources.TextSpanNotIncludedInSemanticModel, "span");
		}
	}

	internal override IOperation GetOperationWorker(SyntaxNode node, GetOperationOptions options, CancellationToken cancellationToken)
	{
		node = SyntaxFactory.GetStandaloneNode(node);
		return GetMemberModel(node)?.GetOperationWorker(node, options, cancellationToken);
	}
}
