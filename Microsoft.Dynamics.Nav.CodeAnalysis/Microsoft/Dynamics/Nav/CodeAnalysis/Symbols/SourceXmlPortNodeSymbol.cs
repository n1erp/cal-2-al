using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class SourceXmlPortNodeSymbol : XmlPortNodeSymbol
{
	internal ImmutableArray<XmlPortNodeSymbol> nodes;

	private ImmutableArray<Symbol> lazyMembers;

	private TypeSymbol lazyTypeSymbol;

	private ImmutableArray<PropertySymbol> properties;

	private SymbolCompletionState state;

	private ImmutableArray<MethodSymbol> triggers;

	private ImmutableArray<Symbol> lazyCodeMembers;

	private Dictionary<string, ImmutableArray<Symbol>> lazyCodeMembersDictionary;

	public override XmlPortSourceTypeKind SourceTypeKind { get; }

	public override XmlPortNodeXmlKind NodeXmlKind { get; }

	internal XmlPortNodeSyntax Syntax { get; }

	private SyntaxToken IdentifierToken { get; }

	public override string MetadataName { get; }

	public override string Name { get; }

	public override string MemberName { get; }

	public override TypeSymbol Type
	{
		get
		{
			LazyGetType();
			return lazyTypeSymbol;
		}
	}

	public override ImmutableArray<Location> Locations { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray.Create(IdentifierToken.Parent.Parent.GetReference());

	public override Symbol ContainingSymbol { get; }

	public override int Id { get; }

	public ImmutableArray<XmlPortNodeSymbol> Nodes
	{
		get
		{
			LazyGetMembers();
			return nodes;
		}
	}

	public override ImmutableArray<PropertySymbol> Properties
	{
		get
		{
			LazyGetProperties();
			return properties;
		}
	}

	public override ImmutableArray<MethodSymbol> Triggers
	{
		get
		{
			LazyGetTriggers();
			return triggers;
		}
	}

	private SourceXmlPortNodeSymbol(Symbol owner, string name, XmlPortNodeSyntax syntax, SyntaxToken nameIdentifier, ImmutableArray<Location> locations)
	{
		SourceXmlPortTypeSymbol containingSymbolOfType = owner.GetContainingSymbolOfType<SourceXmlPortTypeSymbol>();
		Name = name;
		Id = containingSymbolOfType.Members.GetNextMemberId();
		Locations = locations;
		ContainingSymbol = owner;
		IdentifierToken = nameIdentifier;
		Syntax = syntax;
		MetadataName = GetNodeMetadataName(syntax.Kind, name, Id);
		MemberName = name.MangleUnquotedIdentifierName();
		SourceTypeKind = GetNodeSourceTypeKind(syntax);
		NodeXmlKind = GetNodeXmlKind(syntax);
	}

	public static SourceXmlPortNodeSymbol Create(Symbol owner, XmlPortNodeSyntax syntax)
	{
		string name = syntax.Name.Unquoted();
		ImmutableArray<Location> locations = ImmutableArray.Create((Location)new SourceLocation(syntax.Name.Identifier));
		return new SourceXmlPortNodeSymbol(owner, name, syntax, syntax.Name.Identifier, locations);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		instance.AddRange(Nodes);
		instance.AddRange(Triggers);
		instance.AddRange(Properties);
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, instance.ToImmutableAndFree(), default);
		return lazyMembers;
	}

	internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			CompletionPart nextIncompletePart = state.NextIncompletePart;
			switch (nextIncompletePart)
			{
			case CompletionPart.Variables:
				ForceCompleteProperties(locationOpt, cancellationToken);
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.Variables));
				break;
			case CompletionPart.Members:
				LazyGetMembers();
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.Members));
				break;
			case CompletionPart.MembersCompleted:
				ForceCompleteMembers(locationOpt, cancellationToken);
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.MembersCompleted));
				break;
			case CompletionPart.FieldNameListSymbol:
				ForceCompleteTriggers(locationOpt, cancellationToken);
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.FieldNameListSymbol));
				break;
			case CompletionPart.ReturnType:
				LazyGetType();
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.ReturnType));
				break;
			case CompletionPart.StartMethodChecks:
			case CompletionPart.FinishMethodChecks:
				CheckXmlPortNode();
				break;
			case CompletionPart.SymbolCompleted:
				NotifyCompilationAboutSymbolCompletion(ref state);
				break;
			case CompletionPart.None:
				return;
			default:
				state.NotePartComplete((CompletionPart)72487);
				break;
			}
			state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	internal static string GetNodeMetadataName(SyntaxKind syntaxKind, string name, int id)
	{
		switch (syntaxKind)
		{
		case SyntaxKind.XmlPortTableElement:
			return name.MangleUnquotedIdentifierName();
		case SyntaxKind.XmlPortFieldElement:
		case SyntaxKind.XmlPortTextElement:
		case SyntaxKind.XmlPortFieldAttribute:
		case SyntaxKind.XmlPortTextAttribute:
			return string.Format(CultureInfo.InvariantCulture, "{0}_{1}_Node", name.MangleUnquotedIdentifierName(), id).MangleUnquotedIdentifierName();
		default:
			throw ExceptionUtilities.UnexpectedValue(syntaxKind);
		}
	}

	private void LazyGetProperties()
	{
		if (!(properties != null))
		{
			ImmutableArray<PropertySymbol> value = Syntax.PropertyList?.Properties.CreatePropertySymbols(this) ?? ImmutableArray<PropertySymbol>.Empty;
			ImmutableInterlocked.InterlockedCompareExchange(ref properties, value, default);
		}
	}

	private void LazyGetTriggers()
	{
		if (!(triggers != null))
		{
			ImmutableArray<MethodSymbol> value = ((IEnumerable<MethodSymbol>)Syntax.Triggers.Select((TriggerDeclarationSyntax trigger) => SourceTriggerSymbol.Create(this, trigger))).ToImmutableArray();
			ImmutableInterlocked.InterlockedCompareExchange(ref triggers, value, default);
		}
	}

	private void LazyGetMembers()
	{
		if (state.HasComplete(CompletionPart.Members))
		{
			return;
		}
		lock (IdentifierToken.Parent)
		{
			LazySetSymbol((DiagnosticBag d) =>
			{
				nodes = ImmutableArray<XmlPortNodeSymbol>.Empty;
				if (NodeXmlKind == XmlPortNodeXmlKind.Element)
				{
					SyntaxList<XmlPortNodeSyntax> schema = Syntax.Schema;
					ArrayBuilder<XmlPortNodeSymbol> instance = ArrayBuilder<XmlPortNodeSymbol>.GetInstance();
					try
					{
						foreach (XmlPortNodeSyntax item in schema)
						{
							instance.Add(Create(this, item));
						}
					}
					finally
					{
						nodes = instance.ToImmutableAndFree();
					}
				}
			}, CompletionPart.Members, ref state);
		}
	}

	private void CheckXmlPortNode()
	{
		if (state.HasComplete(CompletionPart.FinishMethodChecks))
		{
			return;
		}
		lock (Syntax.Parent)
		{
			if (state.NotePartComplete(CompletionPart.StartMethodChecks))
			{
				LazySetSymbol((DiagnosticBag diagnostics) =>
				{
					CheckIfAttributesNotNestedInElements(Nodes, diagnostics);
					CheckIfAttributesContainOtherElement(Nodes, diagnostics);
					CheckIfXmlNameValid(this, diagnostics);
					TriggerValidationHelper.CheckTriggers(this, Triggers, GetTriggerTypeInfo, diagnostics);
				}, CompletionPart.FinishMethodChecks, ref state);
			}
		}
	}

	private static void CheckIfXmlNameValid(SourceXmlPortNodeSymbol node, DiagnosticBag diagnostics)
	{
		PropertySymbol property = node.GetProperty(PropertyKind.XmlName);
		string text;
		Location location;
		if (property != null)
		{
			text = property.ValueText;
			location = property.GetLocation();
		}
		else
		{
			text = node.Name;
			location = node.Syntax.Name.Location;
		}
		if (string.IsNullOrEmpty(text))
		{
			diagnostics.Add(ErrorCode.ERR_XmlNameMustBeSpecified, location);
			return;
		}
		try
		{
			XmlConvert.VerifyName(text);
		}
		catch (XmlException ex)
		{
			diagnostics.Add(ErrorCode.ERR_InvalidXmlName, location, ex.Message);
		}
	}

	private static void CheckIfAttributesNotNestedInElements(ImmutableArray<XmlPortNodeSymbol> xmlPortNodeSymbols, DiagnosticBag diagnostics)
	{
		bool flag = false;
		foreach (XmlPortNodeSymbol item in xmlPortNodeSymbols)
		{
			if (item.NodeXmlKind == XmlPortNodeXmlKind.Element)
			{
				flag = true;
			}
			else if (flag)
			{
				diagnostics.Add(ErrorCode.ERR_AttributesMustBeNestedInsideElements, GetAttributeKeywordToken(((SourceXmlPortNodeSymbol)item).Syntax).GetLocation());
			}
		}
	}

	private static void CheckIfAttributesContainOtherElement(ImmutableArray<XmlPortNodeSymbol> xmlPortNodeSymbols, DiagnosticBag diagnostics)
	{
		foreach (XmlPortNodeSymbol item in xmlPortNodeSymbols)
		{
			if (item.NodeXmlKind == XmlPortNodeXmlKind.Attribute)
			{
				XmlPortNodeSyntax syntax = ((SourceXmlPortNodeSymbol)item).Syntax;
				if (syntax?.Schema.Node != null)
				{
					diagnostics.Add(ErrorCode.ERR_AttributesCannotHaveNestedElements, GetAttributeKeywordToken(syntax).GetLocation());
				}
			}
		}
	}

	private static SyntaxToken GetAttributeKeywordToken(XmlPortNodeSyntax syntax)
	{
		return (syntax as XmlPortFieldAttributeSyntax)?.XmlPortFieldAttributeKeyword ?? ((XmlPortTextAttributeSyntax)syntax).XmlPortTextAttributeKeyword;
	}

	private void ForceCompleteProperties(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		LazyGetProperties();
		foreach (PropertySymbol property in properties)
		{
			cancellationToken.ThrowIfCancellationRequested();
			property.ForceComplete(locationOpt, cancellationToken);
		}
		state.NotePartComplete(CompletionPart.Variables);
	}

	private void ForceCompleteMembers(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		LazyGetMembers();
		if (state.HasComplete(CompletionPart.MembersCompleted))
		{
			return;
		}
		if (NodeXmlKind == XmlPortNodeXmlKind.Element)
		{
			foreach (XmlPortNodeSymbol node in nodes)
			{
				cancellationToken.ThrowIfCancellationRequested();
				node.ForceComplete(locationOpt, cancellationToken);
			}
		}
		state.NotePartComplete(CompletionPart.MembersCompleted);
	}

	private void ForceCompleteTriggers(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		LazyGetTriggers();
		foreach (MethodSymbol trigger in triggers)
		{
			cancellationToken.ThrowIfCancellationRequested();
			trigger.ForceComplete(locationOpt, cancellationToken);
		}
		state.NotePartComplete(CompletionPart.FieldNameListSymbol);
	}

	public static RecordTypeSymbol GetRelatedTable(XmlPortTableElementSyntax tableSyntax, Compilation compilation)
	{
		string identifierOrLiteralValue = tableSyntax.SourceTable.Identifier.GetIdentifierOrLiteralValue();
		ISymbol symbol = ((!int.TryParse(identifierOrLiteralValue, out var result)) ? compilation.GetObjectSymbolsByNameAcrossModules(SymbolKind.Table, identifierOrLiteralValue).FirstOrDefault() : compilation.GetObjectSymbolsByIdAcrossModules(SymbolKind.Table, result).FirstOrDefault());
		if (symbol != null)
		{
			return compilation.CompiledModule.GetRecord((TableTypeSymbol)symbol, isTemporary: false);
		}
		return null;
	}

	private FieldSymbol GetRelatedField(DiagnosticBag diagnostics)
	{
		CodeExpressionSyntax sourceField = ((XmlPortFieldNodeSyntax)Syntax).SourceField;
		if (!(sourceField is MemberAccessExpressionSyntax memberAccessExpressionSyntax))
		{
			diagnostics.Add(ErrorCode.ERR_InvalidFieldAccessSyntax, sourceField.Location);
			return null;
		}
		string identifierOrLiteralValue = memberAccessExpressionSyntax.Expression.GetIdentifierOrLiteralValue();
		if (GetParentTableElementWithName(identifierOrLiteralValue, ContainingSymbol) == null)
		{
			diagnostics.Add(ErrorCode.ERR_MissingTableElement, memberAccessExpressionSyntax.Expression.Location, identifierOrLiteralValue);
			return null;
		}
		return ContainingSymbol.DeclaringCompilation.GetBinderFactory(Syntax.SyntaxTree).GetBinder(memberAccessExpressionSyntax).BindExpression(memberAccessExpressionSyntax, diagnostics)
			.ExpressionSymbol as FieldSymbol;
	}

	private static SourceXmlPortNodeSymbol GetParentTableElementWithName(string tableName, Symbol currentParent)
	{
		SourceXmlPortNodeSymbol result = null;
		while (currentParent.Kind == SymbolKind.XmlPortNode)
		{
			SourceXmlPortNodeSymbol sourceXmlPortNodeSymbol = (SourceXmlPortNodeSymbol)currentParent;
			if (sourceXmlPortNodeSymbol.SourceTypeKind == XmlPortSourceTypeKind.Table && string.Equals(sourceXmlPortNodeSymbol.Name, tableName, StringComparison.OrdinalIgnoreCase))
			{
				result = sourceXmlPortNodeSymbol;
				break;
			}
			currentParent = currentParent.ContainingSymbol;
		}
		return result;
	}

	private void LazyGetType()
	{
		if (state.HasComplete(CompletionPart.ReturnType))
		{
			return;
		}
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		try
		{
			switch (SourceTypeKind)
			{
			case XmlPortSourceTypeKind.Text:
				if (this.GetEnumPropertyValue<TextTypeKind>(PropertyKind.TextType) == TextTypeKind.BigText)
				{
					lazyTypeSymbol = NavCorLib.BigTextType;
				}
				else
				{
					lazyTypeSymbol = NavCorLib.TextType;
				}
				return;
			case XmlPortSourceTypeKind.Field:
				lazyTypeSymbol = GetRelatedField(instance) ?? NavCorLib.NoneType;
				return;
			case XmlPortSourceTypeKind.Table:
				lazyTypeSymbol = GetRelatedTable(instance) ?? NavCorLib.NoneType;
				return;
			}
		}
		finally
		{
			if (state.NotePartComplete(CompletionPart.ReturnType))
			{
				AddDeclarationDiagnostics(instance);
			}
			instance.Free();
		}
		state.SpinWaitComplete(CompletionPart.ReturnType, CancellationToken.None);
	}

	private TypeSymbol GetRelatedTable(DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(SourceTypeKind == XmlPortSourceTypeKind.Table);
		XmlPortTableElementSyntax xmlPortTableElementSyntax = (XmlPortTableElementSyntax)Syntax;
		RecordTypeSymbol relatedTable = GetRelatedTable(xmlPortTableElementSyntax, DeclaringCompilation);
		if (relatedTable == null)
		{
			diagnostics.Add(ErrorCode.ERR_MissingApplicationObject, xmlPortTableElementSyntax.SourceTable.Location, SymbolKind.Table.ToString(), xmlPortTableElementSyntax.SourceTable.Identifier.GetText());
		}
		else
		{
			Binder.ReportDiagnosticsIfObsoleteDeprecatedOrIncompatible(diagnostics, relatedTable, xmlPortTableElementSyntax.SourceTable, null);
		}
		return relatedTable;
	}

	private static XmlPortNodeXmlKind GetNodeXmlKind(XmlPortNodeSyntax syntax)
	{
		switch (syntax.Kind)
		{
		case SyntaxKind.XmlPortTableElement:
		case SyntaxKind.XmlPortFieldElement:
		case SyntaxKind.XmlPortTextElement:
			return XmlPortNodeXmlKind.Element;
		case SyntaxKind.XmlPortFieldAttribute:
		case SyntaxKind.XmlPortTextAttribute:
			return XmlPortNodeXmlKind.Attribute;
		default:
			throw ExceptionUtilities.UnexpectedValue(syntax.Kind);
		}
	}

	private static XmlPortSourceTypeKind GetNodeSourceTypeKind(XmlPortNodeSyntax syntax)
	{
		switch (syntax.Kind)
		{
		case SyntaxKind.XmlPortTableElement:
			return XmlPortSourceTypeKind.Table;
		case SyntaxKind.XmlPortFieldElement:
		case SyntaxKind.XmlPortFieldAttribute:
			return XmlPortSourceTypeKind.Field;
		case SyntaxKind.XmlPortTextElement:
		case SyntaxKind.XmlPortTextAttribute:
			return XmlPortSourceTypeKind.Text;
		default:
			throw ExceptionUtilities.UnexpectedValue(syntax.Kind);
		}
	}

	internal override ImmutableArray<Symbol> GetCodeMembers()
	{
		if (lazyCodeMembers != null)
		{
			return lazyCodeMembers;
		}
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		instance.AddRange((Type as RecordTypeSymbol)?.BaseType.GetMembers() ?? ImmutableArray<Symbol>.Empty);
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyCodeMembers, instance.ToImmutableAndFree(), default);
		return lazyCodeMembers;
	}

	internal override ImmutableArray<Symbol> GetCodeMembers(string name)
	{
		return GetCodeMembers(ref lazyCodeMembersDictionary, name);
	}
}
