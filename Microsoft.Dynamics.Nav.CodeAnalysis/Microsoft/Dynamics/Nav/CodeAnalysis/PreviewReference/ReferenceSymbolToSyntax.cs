using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.PreviewReference;

[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal abstract class ReferenceSymbolToSyntax
{
	internal const string NotAvailableText = "'Not available'";

	private static readonly Lazy<IEnumerable<ReferenceSymbolToSyntax>> Converters = new Lazy<IEnumerable<ReferenceSymbolToSyntax>>(() => new List<ReferenceSymbolToSyntax>
	{
		new ReferenceSymbolToSyntaxCodeunit(),
		new ReferenceSymbolToSyntaxPageExtension(),
		new ReferenceSymbolToSyntaxTableExtension(),
		new ReferenceSymbolToSyntaxPage(),
		new ReferenceSymbolToSyntaxTable(),
		new ReferenceSymbolToSyntaxQuery(),
		new ReferenceSymbolToSyntaxReport(),
		new ReferenceSymbolToSyntaxXmlport()
	});

	internal ConvertOptions ConvertOption { get; set; }

	public static ApplicationObjectSyntax Convert(ISymbol symbol, ConvertOptions option = ConvertOptions.All)
	{
		foreach (ReferenceSymbolToSyntax item in Converters.Value)
		{
			item.ConvertOption = option;
			ApplicationObjectSyntax applicationObjectSyntax = item.ConvertApplicationSymbol(symbol);
			if (applicationObjectSyntax != null)
			{
				return applicationObjectSyntax;
			}
		}
		return null;
	}

	protected abstract ApplicationObjectSyntax ConvertApplicationSymbol(ISymbol symbol);

	protected static ApplicationObjectSyntax ConvertApplicationSymbol<T>(ISymbol symbol, Func<T, ApplicationObjectSyntax> converter) where T : class
	{
		if (symbol is T arg)
		{
			return converter(arg);
		}
		return null;
	}

	protected SyntaxList<MemberSyntax> ConvertMembers(ImmutableArray<Symbol> members)
	{
		SyntaxListBuilder<MemberSyntax> syntaxListBuilder = SyntaxListBuilder<MemberSyntax>.Create();
		SyntaxList<VariableDeclarationSyntax> variables = ConvertVariables(members.Where((Symbol m) => m.Kind == SymbolKind.GlobalVariable).OfType<VariableSymbol>());
		if (variables.Any())
		{
			VarSectionSyntax node = SyntaxFactory.VarSection(variables);
			syntaxListBuilder.Add(node);
		}
		if ((ConvertOption & ConvertOptions.NoMethodsOrTriggers) == 0)
		{
			MethodOrTriggerDeclarationSyntax[] array = ConvertProceduresAndTriggers(members.Where((Symbol m) => m.Kind == SymbolKind.Method).OfType<MethodSymbol>());
			if (array.Any())
			{
				MethodOrTriggerDeclarationSyntax[] array2 = array;
				foreach (MethodOrTriggerDeclarationSyntax node2 in array2)
				{
					syntaxListBuilder.Add(node2);
				}
			}
		}
		return syntaxListBuilder.ToList();
	}

	private static SyntaxList<VariableDeclarationSyntax> ConvertVariables(IEnumerable<VariableSymbol> variables)
	{
		SyntaxListBuilder<VariableDeclarationSyntax> syntaxListBuilder = SyntaxListBuilder<VariableDeclarationSyntax>.Create();
		foreach (VariableSymbol variable in variables)
		{
			if (variable.VariableKind != VariableKind.Synthesized)
			{
				VariableDeclarationSyntax node = SyntaxFactory.VariableDeclaration(variable.Name, ConvertTypeReference(variable.Type));
				syntaxListBuilder.Add(node);
			}
		}
		return syntaxListBuilder.ToList();
	}

	protected static PropertySyntax[] ConvertProperties(IEnumerable<PropertySymbol> properties, Symbol parentSymbol)
	{
		return ReferenceSymbolToSyntaxHelper.ConvertProperties(properties, parentSymbol);
	}

	private static MethodOrTriggerDeclarationSyntax[] ConvertProceduresAndTriggers(IEnumerable<MethodSymbol> procedures)
	{
		PooledList<MethodOrTriggerDeclarationSyntax> instance = PooledList<MethodOrTriggerDeclarationSyntax>.GetInstance();
		try
		{
			foreach (MethodSymbol procedure in procedures)
			{
				if (!procedure.IsTrigger())
				{
					if (procedure.MethodKind != MethodKind.Method)
					{
						continue;
					}
					MethodDeclarationSyntax methodDeclarationSyntax = SyntaxFactory.MethodDeclaration(procedure.Name);
					methodDeclarationSyntax = methodDeclarationSyntax.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
					methodDeclarationSyntax = methodDeclarationSyntax.WithBody(SyntaxFactory.Block().WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
					if (procedure.IsLocal)
					{
						methodDeclarationSyntax = methodDeclarationSyntax.WithLocalKeyword(SyntaxFactory.Token(SyntaxKind.LocalKeyword));
					}
					if (procedure.Attributes != null && !procedure.Attributes.IsEmpty)
					{
						methodDeclarationSyntax = methodDeclarationSyntax.WithAttributes(ConvertAttributes(procedure.Attributes));
					}
					if (procedure.ParameterCount > 0)
					{
						methodDeclarationSyntax = methodDeclarationSyntax.WithParameterList(ConvertParameters(procedure.Parameters));
					}
					if (procedure.HasReturnValue())
					{
						ReturnValueSyntax returnValueSyntax = SyntaxFactory.ReturnValue(GetDataTypeSyntax(procedure.ReturnType));
						if (procedure.ReturnValue.IsNamed)
						{
							returnValueSyntax = returnValueSyntax.WithName(SyntaxFactory.IdentifierName(procedure.ReturnValue.Name));
						}
						methodDeclarationSyntax = methodDeclarationSyntax.WithReturnValue(returnValueSyntax);
					}
					instance.Add(methodDeclarationSyntax);
				}
				else
				{
					if (procedure.MethodKind != MethodKind.Trigger)
					{
						continue;
					}
					TriggerDeclarationSyntax triggerDeclarationSyntax = SyntaxFactory.TriggerDeclaration(procedure.Name);
					triggerDeclarationSyntax = triggerDeclarationSyntax.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
					triggerDeclarationSyntax = triggerDeclarationSyntax.WithBody(SyntaxFactory.Block().WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
					if (procedure.Attributes != null && !procedure.Attributes.IsEmpty)
					{
						triggerDeclarationSyntax = triggerDeclarationSyntax.WithAttributes(ConvertAttributes(procedure.Attributes));
					}
					if (procedure.ParameterCount > 0)
					{
						triggerDeclarationSyntax = triggerDeclarationSyntax.WithParameterList(ConvertParameters(procedure.Parameters));
					}
					if (procedure.HasReturnValue())
					{
						ReturnValueSyntax returnValueSyntax2 = SyntaxFactory.ReturnValue(GetDataTypeSyntax(procedure.ReturnType));
						if (procedure.ReturnValue.IsNamed)
						{
							returnValueSyntax2 = returnValueSyntax2.WithName(SyntaxFactory.IdentifierName(procedure.ReturnValue.Name));
						}
						triggerDeclarationSyntax = triggerDeclarationSyntax.WithReturnValue(returnValueSyntax2);
					}
					instance.Add(triggerDeclarationSyntax);
				}
			}
			return instance.ToArray();
		}
		finally
		{
			instance.Free();
		}
	}

	protected static SeparatedSyntaxList<IdentifierNameSyntax> ConvertPageControlIdentifiers(IEnumerable<ControlSymbol> controls)
	{
		SeparatedSyntaxListBuilder<IdentifierNameSyntax> separatedSyntaxListBuilder = SeparatedSyntaxListBuilder<IdentifierNameSyntax>.Create();
		foreach (ControlSymbol control in controls)
		{
			if (separatedSyntaxListBuilder.Count > 0)
			{
				separatedSyntaxListBuilder.AddSeparator(SyntaxFactory.Token(SyntaxKind.CommaToken));
			}
			separatedSyntaxListBuilder.Add(SyntaxFactory.IdentifierName(control.Name));
		}
		return separatedSyntaxListBuilder.ToList();
	}

	protected static SyntaxList<ControlBaseSyntax> ConvertPageControls(IEnumerable<ControlSymbol> controls)
	{
		SyntaxListBuilder<ControlBaseSyntax> syntaxListBuilder = SyntaxListBuilder<ControlBaseSyntax>.Create();
		foreach (ControlSymbol control in controls)
		{
			switch (control.ControlKind)
			{
			case ControlKind.Field:
			{
				string expression = control.Properties.FirstOrDefault((PropertySymbol p) => string.Equals(p.Name, "SourceExpression", StringComparison.OrdinalIgnoreCase))?.ValueText ?? "'Not available'";
				PageFieldSyntax pageFieldSyntax = SyntaxFactory.PageField(control.Name, expression);
				PageFieldSyntax pageFieldSyntax2 = pageFieldSyntax;
				PropertySyntaxOrEmpty[] items = ConvertProperties(control.Properties, control);
				pageFieldSyntax = pageFieldSyntax2.AddPropertyListProperties(items);
				syntaxListBuilder.Add(pageFieldSyntax);
				break;
			}
			case ControlKind.Label:
			{
				PageLabelSyntax pageLabelSyntax = SyntaxFactory.PageLabel(control.Name);
				PageLabelSyntax pageLabelSyntax2 = pageLabelSyntax;
				PropertySyntaxOrEmpty[] items = ConvertProperties(control.Properties, control);
				pageLabelSyntax = pageLabelSyntax2.AddPropertyListProperties(items);
				syntaxListBuilder.Add(pageLabelSyntax);
				break;
			}
			case ControlKind.SystemPart:
			{
				string systemPartType = control.Properties.FirstOrDefault((PropertySymbol p) => string.Equals(p.Name, "SystemPartId", StringComparison.OrdinalIgnoreCase))?.ValueText ?? "'Not available'";
				PageSystemPartSyntax pageSystemPartSyntax = SyntaxFactory.PageSystemPart(control.Name, systemPartType);
				PageSystemPartSyntax pageSystemPartSyntax2 = pageSystemPartSyntax;
				PropertySyntaxOrEmpty[] items = ConvertProperties(control.Properties, control);
				pageSystemPartSyntax = pageSystemPartSyntax2.AddPropertyListProperties(items);
				syntaxListBuilder.Add(pageSystemPartSyntax);
				break;
			}
			case ControlKind.Part:
			{
				IdentifierNameSyntax name = SyntaxFactory.IdentifierName(control.Name);
				ObjectNameOrIdSyntax partName = SyntaxFactory.ObjectNameOrId(SyntaxFactory.IdentifierName(control.RelatedPartSymbol.Name));
				PagePartSyntax pagePartSyntax = SyntaxFactory.PagePart(name, partName);
				PagePartSyntax pagePartSyntax2 = pagePartSyntax;
				PropertySyntaxOrEmpty[] items = ConvertProperties(control.Properties, control);
				pagePartSyntax = pagePartSyntax2.AddPropertyListProperties(items);
				syntaxListBuilder.Add(pagePartSyntax);
				break;
			}
			case ControlKind.Group:
			case ControlKind.Repeater:
			case ControlKind.Fixed:
			case ControlKind.Grid:
			{
				PageGroupSyntax pageGroupSyntax3 = SyntaxFactory.PageGroup(ControlKindToKeyword(control.ControlKind), control.Name);
				SyntaxList<ControlBaseSyntax> syntaxList3 = ConvertPageControls(control.Controls);
				if (!syntaxList3.IsEmpty())
				{
					pageGroupSyntax3 = pageGroupSyntax3.WithControls(syntaxList3);
				}
				PageGroupSyntax pageGroupSyntax4 = pageGroupSyntax3;
				PropertySyntaxOrEmpty[] items = ConvertProperties(control.Properties, control);
				pageGroupSyntax3 = pageGroupSyntax4.AddPropertyListProperties(items);
				syntaxListBuilder.Add(pageGroupSyntax3);
				break;
			}
			case ControlKind.CueGroup:
			{
				PageGroupSyntax pageGroupSyntax = SyntaxFactory.PageGroup(ControlKindToKeyword(ControlKind.CueGroup), control.Name);
				SyntaxList<ControlBaseSyntax> syntaxList = ConvertPageControls(control.Controls);
				if (!syntaxList.IsEmpty())
				{
					pageGroupSyntax = pageGroupSyntax.WithControls(syntaxList);
				}
				SyntaxList<ActionBaseSyntax> syntaxList2 = ConvertPageActions(control.Actions);
				if (!syntaxList2.IsEmpty())
				{
					pageGroupSyntax = pageGroupSyntax.WithActionList(SyntaxFactory.GroupActionList(syntaxList2));
				}
				PageGroupSyntax pageGroupSyntax2 = pageGroupSyntax;
				PropertySyntaxOrEmpty[] items = ConvertProperties(control.Properties, control);
				pageGroupSyntax2.AddPropertyListProperties(items);
				syntaxListBuilder.Add(pageGroupSyntax);
				break;
			}
			}
		}
		return syntaxListBuilder.ToList();
	}

	private static SyntaxToken ControlKindToKeyword(ControlKind controlKind)
	{
		return controlKind switch
		{
			ControlKind.Group => SyntaxFactory.Token(SyntaxKind.PageGroupKeyword), 
			ControlKind.CueGroup => SyntaxFactory.Token(SyntaxKind.PageCueGroupKeyword), 
			ControlKind.Repeater => SyntaxFactory.Token(SyntaxKind.PageRepeaterKeyword), 
			ControlKind.Fixed => SyntaxFactory.Token(SyntaxKind.PageFixedKeyword), 
			ControlKind.Grid => SyntaxFactory.Token(SyntaxKind.PageGridKeyword), 
			_ => default, 
		};
	}

	protected static SyntaxList<ActionBaseSyntax> ConvertPageActions(IEnumerable<ActionSymbol> actions)
	{
		SyntaxListBuilder<ActionBaseSyntax> syntaxListBuilder = SyntaxListBuilder<ActionBaseSyntax>.Create();
		foreach (ActionSymbol action in actions)
		{
			switch (action.ActionKind)
			{
			case ActionKind.Action:
			{
				PageActionSyntax pageActionSyntax = SyntaxFactory.PageAction(action.Name);
				PageActionSyntax pageActionSyntax2 = pageActionSyntax;
				PropertySyntaxOrEmpty[] items = ConvertProperties(action.Properties, action);
				pageActionSyntax = pageActionSyntax2.AddPropertyListProperties(items);
				syntaxListBuilder.Add(pageActionSyntax);
				break;
			}
			case ActionKind.Separator:
			{
				PageActionSeparatorSyntax pageActionSeparatorSyntax = SyntaxFactory.PageActionSeparator(action.Name);
				PageActionSeparatorSyntax pageActionSeparatorSyntax2 = pageActionSeparatorSyntax;
				PropertySyntaxOrEmpty[] items = ConvertProperties(action.Properties, action);
				pageActionSeparatorSyntax = pageActionSeparatorSyntax2.AddPropertyListProperties(items);
				syntaxListBuilder.Add(pageActionSeparatorSyntax);
				break;
			}
			case ActionKind.Group:
			{
				PageActionGroupSyntax pageActionGroupSyntax = SyntaxFactory.PageActionGroup(action.Name);
				SyntaxList<ActionBaseSyntax> syntaxList = ConvertPageActions(action.Actions);
				if (!syntaxList.IsEmpty())
				{
					pageActionGroupSyntax = pageActionGroupSyntax.WithActions(syntaxList);
				}
				PageActionGroupSyntax pageActionGroupSyntax2 = pageActionGroupSyntax;
				PropertySyntaxOrEmpty[] items = ConvertProperties(action.Properties, action);
				pageActionGroupSyntax = pageActionGroupSyntax2.AddPropertyListProperties(items);
				syntaxListBuilder.Add(pageActionGroupSyntax);
				break;
			}
			}
		}
		return syntaxListBuilder.ToList();
	}

	protected static SyntaxList<PageViewSyntax> ConvertPageViews(IEnumerable<ViewSymbol> views)
	{
		SyntaxListBuilder<PageViewSyntax> syntaxListBuilder = SyntaxListBuilder<PageViewSyntax>.Create();
		foreach (ViewSymbol view in views)
		{
			syntaxListBuilder.Add(ConvertPageView(view));
		}
		return syntaxListBuilder.ToList();
	}

	protected internal static PageViewSyntax ConvertPageView(ViewSymbol view)
	{
		PageViewSyntax pageViewSyntax = SyntaxFactory.PageView(view.Name);
		PageViewSyntax pageViewSyntax2 = pageViewSyntax;
		PropertySyntaxOrEmpty[] items = ConvertProperties(view.Properties, view);
		pageViewSyntax = pageViewSyntax2.AddPropertyListProperties(items);
		SyntaxList<ControlChangeBaseSyntax> changes = ReferenceSymbolToSyntaxPageExtension.ConvertPageExtensionControls(view.GetMembers().OfType<ChangeSymbol>());
		if (changes.Any())
		{
			pageViewSyntax = pageViewSyntax.WithLayout(SyntaxFactory.PageExtensionLayout(changes));
		}
		return pageViewSyntax;
	}

	private static SyntaxList<MemberAttributeSyntax> ConvertAttributes(IEnumerable<AttributeSymbol> attributes)
	{
		SyntaxListBuilder<MemberAttributeSyntax> syntaxListBuilder = SyntaxListBuilder<MemberAttributeSyntax>.Create();
		foreach (AttributeSymbol attribute in attributes)
		{
			MemberAttributeSyntax memberAttributeSyntax = SyntaxFactory.MemberAttribute(attribute.Name);
			AttributeArgumentSyntax[] array = ConvertAttributeArgument(attribute);
			if (array.Any())
			{
				memberAttributeSyntax = memberAttributeSyntax.AddArgumentListArguments(array);
			}
			syntaxListBuilder.Add(memberAttributeSyntax);
		}
		return syntaxListBuilder.ToList();
	}

	private static ParameterListSyntax ConvertParameters(ImmutableArray<ParameterSymbol> procedureParameters)
	{
		SeparatedSyntaxListBuilder<ParameterSyntax> separatedSyntaxListBuilder = SeparatedSyntaxListBuilder<ParameterSyntax>.Create();
		foreach (ParameterSymbol item in procedureParameters)
		{
			if (separatedSyntaxListBuilder.Count > 0)
			{
				separatedSyntaxListBuilder.AddSeparator(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
			}
			ParameterSyntax parameterSyntax = SyntaxFactory.Parameter(item.Name, ConvertTypeReference(item.Type));
			if (item.IsVar)
			{
				parameterSyntax = parameterSyntax.WithVarKeyword(SyntaxFactory.Token(SyntaxKind.VarKeyword));
			}
			separatedSyntaxListBuilder.Add(parameterSyntax);
		}
		return SyntaxFactory.ParameterList(separatedSyntaxListBuilder.ToList<ParameterSyntax>());
	}

	private static TypeReferenceBaseSyntax ConvertTypeReference(TypeSymbol type)
	{
		if (type.IsRecordType())
		{
			return SyntaxFactory.RecordTypeReference(SyntaxFactory.SubtypedDataType(SyntaxFactory.Identifier(SymbolKind.Record.ToString()), SyntaxFactory.ObjectNameOrId(SyntaxFactory.IdentifierName(type.Name))));
		}
		if (type.IsArray())
		{
			ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)type;
			SeparatedSyntaxListBuilder<DimensionSyntax> separatedSyntaxListBuilder = SeparatedSyntaxListBuilder<DimensionSyntax>.Create();
			bool flag = true;
			foreach (int dimension in arrayTypeSymbol.Dimensions)
			{
				if (!flag)
				{
					separatedSyntaxListBuilder.AddSeparator(SyntaxFactory.Token(SyntaxKind.CommaToken));
				}
				separatedSyntaxListBuilder.Add(SyntaxFactory.Dimension(SyntaxFactory.Literal(dimension)));
				flag = false;
			}
			ArraySyntax array = SyntaxFactory.Array(SyntaxFactory.BracketedDimensionList(separatedSyntaxListBuilder.ToList()));
			DataTypeSyntax dataTypeSyntax = GetDataTypeSyntax(arrayTypeSymbol.ElementType);
			return SyntaxFactory.SimpleTypeReference(array, dataTypeSyntax);
		}
		return SyntaxFactory.SimpleTypeReference(GetDataTypeSyntax(type));
	}

	protected static DataTypeSyntax GetDataTypeSyntax(TypeSymbol type)
	{
		if (type.HasLength)
		{
			return SyntaxFactory.LengthDataType(SyntaxFactory.Identifier(type.Name), SyntaxFactory.Token(SyntaxKind.OpenBracketToken), SyntaxFactory.Literal(type.Length), SyntaxFactory.Token(SyntaxKind.CloseBracketToken));
		}
		if (IsApplicationObject(type.NavTypeKind))
		{
			return SyntaxFactory.SubtypedDataType(SyntaxFactory.Token(SyntaxFacts.GetObjectKeywordKind(type.NavTypeKind.ToString().ToUpperInvariant())), SyntaxFactory.ObjectNameOrId(SyntaxFactory.IdentifierName(type.Name)));
		}
		if (type.IsValidOptionType())
		{
			return ConvertOptionType(type);
		}
		if (type.NavTypeKind == NavTypeKind.TextConst)
		{
			return ConvertTextConst(type);
		}
		if (type.NavTypeKind == NavTypeKind.TestPage || type.NavTypeKind == NavTypeKind.TestRequestPage)
		{
			return SyntaxFactory.SubtypedDataType(SyntaxFactory.Identifier(type.NavTypeKind.ToString()), SyntaxFactory.ObjectNameOrId(SyntaxFactory.IdentifierName(type.Name)));
		}
		return SyntaxFactory.SimpleNamedDataType(SyntaxFactory.Identifier(type.Name));
	}

	private static DataTypeSyntax ConvertTextConst(TypeSymbol type)
	{
		return SyntaxFactory.TextConstDataType(SyntaxFactory.Identifier(type.Name));
	}

	private static bool IsApplicationObject(NavTypeKind kind)
	{
		if (kind == NavTypeKind.Table || (uint)(kind - 393317) <= 4u)
		{
			return true;
		}
		return false;
	}

	private static DataTypeSyntax ConvertOptionType(TypeSymbol type)
	{
		SeparatedSyntaxListBuilder<IdentifierNameOrEmptySyntax> separatedSyntaxListBuilder = SeparatedSyntaxListBuilder<IdentifierNameOrEmptySyntax>.Create();
		ImmutableArray<Symbol> members = type.GetMembers();
		int i = 0;
		foreach (OptionSymbol item in members.OfType<OptionSymbol>())
		{
			if (i > 0)
			{
				separatedSyntaxListBuilder.AddSeparator(SyntaxFactory.Token(SyntaxKind.CommaToken));
			}
			if (item.Ordinal > i)
			{
				for (; i < item.Ordinal; i++)
				{
					separatedSyntaxListBuilder.Add(SyntaxFactory.IdentifierNameOrEmpty());
					separatedSyntaxListBuilder.AddSeparator(SyntaxFactory.Token(SyntaxKind.CommaToken));
				}
			}
			separatedSyntaxListBuilder.Add(SyntaxFactory.IdentifierNameOrEmpty(SyntaxFactory.IdentifierName(item.Name)));
			i++;
		}
		OptionValuesSyntax optionValues = SyntaxFactory.OptionValues(separatedSyntaxListBuilder.ToList());
		return SyntaxFactory.OptionDataType(SyntaxFactory.Identifier(type.Name), optionValues);
	}

	private static AttributeArgumentSyntax[] ConvertAttributeArgument(AttributeSymbol attribute)
	{
		PooledList<AttributeArgumentSyntax> instance = PooledList<AttributeArgumentSyntax>.GetInstance();
		try
		{
			ImmutableArray<AttributeParameter> parameters = attribute.AttributeInfo.Parameters;
			ImmutableArray<AttributeArgumentSymbol> arguments = attribute.Arguments;
			int num = ((parameters.Length < arguments.Length) ? parameters.Length : arguments.Length);
			for (int i = 0; i < num; i++)
			{
				AttributeArgumentSymbol attributeArgumentSymbol = arguments[i];
				switch (parameters[i].Type.NavTypeKind)
				{
				case NavTypeKind.Boolean:
					instance.Add(SyntaxFactory.LiteralAttributeArgument(SyntaxFactory.LiteralExpression(SyntaxFactory.BooleanLiteralValue(SyntaxFactory.Token((attributeArgumentSymbol.ValueAsBoolean == true) ? SyntaxKind.TrueKeyword : SyntaxKind.FalseKeyword)))));
					break;
				case NavTypeKind.Integer:
					instance.Add(SyntaxFactory.LiteralAttributeArgument(SyntaxFactory.LiteralExpression(SyntaxFactory.Int32SignedLiteralValue(SyntaxFactory.Literal(attributeArgumentSymbol.ValueAsInteger ?? (-1))))));
					break;
				case NavTypeKind.Text:
					instance.Add(SyntaxFactory.LiteralAttributeArgument(SyntaxFactory.LiteralExpression(SyntaxFactory.StringLiteralValue(SyntaxFactory.Literal(attributeArgumentSymbol.ValueText)))));
					break;
				case NavTypeKind.ObjectType:
					instance.Add(SyntaxFactory.OptionAccessAttributeArgument(SyntaxFactory.OptionAccessExpression(SyntaxFactory.IdentifierName("ObjectType"), attributeArgumentSymbol.ValueText)));
					break;
				}
			}
			return instance.ToArray();
		}
		finally
		{
			instance.Free();
		}
	}

	protected static SyntaxList<KeySyntax> ConvertTableKeys(IEnumerable<FieldNameListSymbol> keys)
	{
		SyntaxListBuilder<KeySyntax> syntaxListBuilder = SyntaxListBuilder<KeySyntax>.Create();
		foreach (FieldNameListSymbol key in keys)
		{
			KeySyntax keySyntax = SyntaxFactory.Key(SyntaxFactory.IdentifierName(key.Name));
			SeparatedSyntaxListBuilder<IdentifierNameSyntax> separatedSyntaxListBuilder = SeparatedSyntaxListBuilder<IdentifierNameSyntax>.Create();
			foreach (FieldSymbol field in key.Fields)
			{
				if (separatedSyntaxListBuilder.Count > 0)
				{
					separatedSyntaxListBuilder.AddSeparator(SyntaxFactory.Token(SyntaxKind.CommaToken));
				}
				separatedSyntaxListBuilder.Add(SyntaxFactory.IdentifierName(field.Name));
			}
			keySyntax = keySyntax.WithFields(separatedSyntaxListBuilder.ToList());
			KeySyntax keySyntax2 = keySyntax;
			PropertySyntaxOrEmpty[] items = ConvertProperties(key.Properties, key);
			keySyntax = keySyntax2.AddPropertyListProperties(items);
			syntaxListBuilder.Add(keySyntax);
		}
		return syntaxListBuilder.ToList();
	}

	protected static SyntaxList<FieldGroupSyntax> ConvertTableFieldGroups(IEnumerable<FieldGroupSymbol> fieldGroups)
	{
		SyntaxListBuilder<FieldGroupSyntax> syntaxListBuilder = SyntaxListBuilder<FieldGroupSyntax>.Create();
		foreach (FieldGroupSymbol fieldGroup in fieldGroups)
		{
			FieldGroupSyntax fieldGroupSyntax = SyntaxFactory.FieldGroup(SyntaxFactory.IdentifierName(fieldGroup.Name));
			SeparatedSyntaxListBuilder<IdentifierNameSyntax> separatedSyntaxListBuilder = SeparatedSyntaxListBuilder<IdentifierNameSyntax>.Create();
			foreach (FieldSymbol field in fieldGroup.Fields)
			{
				if (separatedSyntaxListBuilder.Count > 0)
				{
					separatedSyntaxListBuilder.AddSeparator(SyntaxFactory.Token(SyntaxKind.CommaToken));
				}
				separatedSyntaxListBuilder.Add(SyntaxFactory.IdentifierName(field.Name));
			}
			fieldGroupSyntax = fieldGroupSyntax.WithFields(separatedSyntaxListBuilder.ToList());
			FieldGroupSyntax fieldGroupSyntax2 = fieldGroupSyntax;
			PropertySyntaxOrEmpty[] items = ConvertProperties(fieldGroup.Properties, fieldGroup);
			fieldGroupSyntax = fieldGroupSyntax2.AddPropertyListProperties(items);
			syntaxListBuilder.Add(fieldGroupSyntax);
		}
		return syntaxListBuilder.ToList();
	}
}
