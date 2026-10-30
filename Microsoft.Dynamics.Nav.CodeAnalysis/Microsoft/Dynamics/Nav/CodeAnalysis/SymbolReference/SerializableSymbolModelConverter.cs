using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal static class SerializableSymbolModelConverter
{
	private static class LegacyNames
	{
		public const string SourceExpression = "SourceExpression";

		public const string SystemPartId = "SystemPartId";

		public const string ChartPartId = "ChartPartId";

		public const string PartType = "PartType";

		public const string System = "System";

		public const string Chart = "Chart";
	}

	internal static ModuleDefinition ConvertModuleToSerializableSymbolModel(IModuleSymbol module)
	{
		List<TableDefinition> list = ConvertTables(module.GetObjectSymbols(SymbolKind.Table).Cast<ApplicationObjectTypeSymbol>());
		List<CodeunitDefinition> list2 = ConvertCodeunits(module.GetObjectSymbols(SymbolKind.Codeunit).Cast<ApplicationObjectTypeSymbol>());
		List<PageDefinition> list3 = ConvertPages(module.GetObjectSymbols(SymbolKind.Page).Cast<ApplicationObjectTypeSymbol>());
		List<XmlPortDefinition> list4 = ConvertXmlPorts(module.GetObjectSymbols(SymbolKind.XmlPort).Cast<ApplicationObjectTypeSymbol>());
		List<ReportDefinition> list5 = ConvertReports(module.GetObjectSymbols(SymbolKind.Report).Cast<ApplicationObjectTypeSymbol>());
		List<QueryDefinition> list6 = ConvertQueries(module.GetObjectSymbols(SymbolKind.Query).Cast<ApplicationObjectTypeSymbol>());
		List<PageExtensionDefinition> list7 = ConvertPageExtensions(module.GetObjectSymbols(SymbolKind.PageExtension).Cast<ApplicationObjectTypeSymbol>());
		List<PageCustomizationDefinition> list8 = ConvertPageCustomizations(module.GetObjectSymbols(SymbolKind.PageCustomization).Cast<ApplicationObjectTypeSymbol>());
		List<TableExtensionDefinition> list9 = ConvertTableExtensions(module.GetObjectSymbols(SymbolKind.TableExtension).Cast<ApplicationObjectTypeSymbol>());
		List<ProfileDefinition> list10 = ConvertProfiles(module.GetObjectSymbols(SymbolKind.Profile).Cast<ObjectTypeSymbol>());
		List<ControlAddInDefinition> list11 = ConvertControlAddIns(module.GetObjectSymbols(SymbolKind.ControlAddIn).Cast<ControlAddInTypeSymbol>());
		EnumTypeDefinition[] source = ConvertEnumTypes(module.GetObjectSymbols(SymbolKind.Enum).Cast<EnumTypeSymbol>());
		EnumExtensionTypeDefinition[] enumExtensionTypes = ConvertEnumExtensionTypes(module.GetObjectSymbols(SymbolKind.EnumExtension).Cast<EnumExtensionTypeSymbol>());
		DotNetPackageDefinition[] dotNetPackages = ConvertDotNetPackages(module.GetObjectSymbols(SymbolKind.DotNetPackage).Cast<DotNetPackageTypeSymbol>());
		return new ModuleDefinition
		{
			AppId = module.AppId,
			Name = module.Name,
			Publisher = module.PublisherName,
			Version = module.Version.ToString(),
			Tables = list.ToArray(),
			Codeunits = list2.ToArray(),
			Pages = list3.ToArray(),
			XmlPorts = list4.ToArray(),
			Reports = list5.ToArray(),
			Queries = list6.ToArray(),
			PageExtensions = list7.ToArray(),
			PageCustomizations = list8.ToArray(),
			TableExtensions = list9.ToArray(),
			Profiles = list10.ToArray(),
			ControlAddIns = list11.ToArray(),
			EnumTypes = source.ToArray(),
			EnumExtensionTypes = enumExtensionTypes,
			DotNetPackages = dotNetPackages
		};
	}

	private static DotNetPackageDefinition[] ConvertDotNetPackages(IEnumerable<DotNetPackageTypeSymbol> packages)
	{
		return packages.Select((DotNetPackageTypeSymbol p) => new DotNetPackageDefinition
		{
			AssemblyDeclarations = ConvertDotNetAssembliesInPackage(p)
		}).ToArray();
	}

	private static DotNetAssemblyDefinition[] ConvertDotNetAssembliesInPackage(DotNetPackageTypeSymbol p)
	{
		return p.Assemblies.Select((DotNetAssemblySymbol a) => new DotNetAssemblyDefinition
		{
			Name = a.Name,
			Properties = ConvertProperties(a),
			TypeDeclarations = ConvertTypeDeclarations(a)
		}).ToArray();
	}

	private static DotNetTypeDeclarationDefinition[] ConvertTypeDeclarations(DotNetAssemblySymbol a)
	{
		return (from t in a.TypeDeclarations
			where !t.IsControlAddIn
			select new DotNetTypeDeclarationDefinition
			{
				TypeName = t.TypeName,
				AliasName = t.Name
			}).ToArray();
	}

	internal static IEnumerable<ModuleDefinition> GetModelsWithProfilesAndTheirCustomizations(ModuleDefinition moduleDefinition, IEnumerable<ProfileTypeSymbol> profiles)
	{
		return moduleDefinition.Profiles.Select((ProfileDefinition profile) =>
		{
			IEnumerable<string> customizations = profiles.FirstOrDefault((ProfileTypeSymbol p) => p.Name == profile.Name)?.GetBoundPropertyValue<BoundIdentifierListPropertyValue>(PropertyKind.Customizations)?.Value.Select((BoundExpression access) => ((BoundApplicationObjectAccess)access).ApplicationObjectTypeSymbol.Name) ?? Enumerable.Empty<string>();
			return new ModuleDefinition
			{
				Profiles = new ProfileDefinition[1] { profile },
				PageCustomizations = moduleDefinition.PageCustomizations.Where((PageCustomizationDefinition pageExt) => customizations.Any((string customization) => string.Equals(pageExt.Name, customization))).ToArray()
			};
		});
	}

	internal static TypeSymbol ResolveType(ModuleSymbol module, Symbol owner, TypeDefinition typeDefinition)
	{
		TypeSymbol typeSymbol = ResolveElementType(module, owner, typeDefinition);
		if (typeSymbol == null)
		{
			return MissingTypeSymbol.Instance;
		}
		if (typeDefinition.ArrayDimensions != null)
		{
			typeSymbol = ArrayTypeSymbol.Create(typeSymbol, typeDefinition.ArrayDimensions.AsImmutable());
		}
		return typeSymbol;
	}

	private static List<ProfileDefinition> ConvertProfiles(IEnumerable<ObjectTypeSymbol> profiles)
	{
		List<ProfileDefinition> list = new List<ProfileDefinition>();
		foreach (ObjectTypeSymbol profile in profiles)
		{
			PropertyDefinition[] properties = ConvertProperties(profile);
			list.Add(new ProfileDefinition
			{
				Name = profile.Name,
				Properties = properties
			});
		}
		return list;
	}

	private static List<ControlAddInDefinition> ConvertControlAddIns(IEnumerable<ControlAddInTypeSymbol> controlAddIns)
	{
		List<ControlAddInDefinition> list = new List<ControlAddInDefinition>();
		foreach (ControlAddInTypeSymbol controlAddIn in controlAddIns)
		{
			list.Add(new ControlAddInDefinition
			{
				Name = controlAddIn.Name,
				PublicKeyToken = controlAddIn.PublicKeyToken,
				Methods = ConvertMethods(controlAddIn.GetMembers()),
				Events = ConvertEvents(controlAddIn)
			});
		}
		return list;
	}

	private static EnumTypeDefinition[] ConvertEnumTypes(IEnumerable<EnumTypeSymbol> enumTypes)
	{
		PooledList<EnumTypeDefinition> instance = PooledList<EnumTypeDefinition>.GetInstance();
		try
		{
			foreach (EnumTypeSymbol enumType in enumTypes)
			{
				instance.Add(new EnumTypeDefinition
				{
					Id = enumType.Id,
					Name = enumType.Name,
					Properties = ConvertProperties(enumType),
					Values = ConvertEnumValues(enumType.Values)
				});
			}
			return instance.ToArray();
		}
		finally
		{
			instance.Free();
		}
	}

	private static EnumExtensionTypeDefinition[] ConvertEnumExtensionTypes(IEnumerable<EnumExtensionTypeSymbol> enumExtensionTypes)
	{
		PooledList<EnumExtensionTypeDefinition> instance = PooledList<EnumExtensionTypeDefinition>.GetInstance();
		try
		{
			foreach (EnumExtensionTypeSymbol enumExtensionType in enumExtensionTypes)
			{
				instance.Add(new EnumExtensionTypeDefinition
				{
					Id = enumExtensionType.Id,
					Name = enumExtensionType.Name,
					TargetObject = enumExtensionType.Target?.Name,
					Properties = ConvertProperties(enumExtensionType),
					Values = ConvertEnumValues(enumExtensionType.Values)
				});
			}
			return instance.ToArray();
		}
		finally
		{
			instance.Free();
		}
	}

	private static EnumValueDefinition[] ConvertEnumValues(IEnumerable<OptionSymbol> values)
	{
		ArrayBuilder<EnumValueDefinition> instance = ArrayBuilder<EnumValueDefinition>.GetInstance();
		foreach (OptionSymbol value in values)
		{
			instance.Add(new EnumValueDefinition
			{
				Name = value.Name,
				Ordinal = value.Ordinal,
				Properties = ConvertProperties(value)
			});
		}
		return instance.ToArrayAndFree();
	}

	private static List<XmlPortDefinition> ConvertXmlPorts(IEnumerable<ApplicationObjectTypeSymbol> xmlPorts)
	{
		List<XmlPortDefinition> list = new List<XmlPortDefinition>();
		foreach (ApplicationObjectTypeSymbol xmlPort in xmlPorts)
		{
			VariableDefinition[] variables = ConvertVariables(xmlPort.GlobalVariables);
			MethodDefinition[] methods = ConvertMethods(xmlPort.GetMembers());
			PropertyDefinition[] properties = ConvertProperties(xmlPort);
			list.Add(new XmlPortDefinition
			{
				Id = xmlPort.Id,
				Name = xmlPort.Name,
				Variables = variables,
				Methods = methods,
				Properties = properties
			});
		}
		return list;
	}

	private static List<QueryDefinition> ConvertQueries(IEnumerable<ApplicationObjectTypeSymbol> applicationObjects)
	{
		List<QueryDefinition> list = new List<QueryDefinition>();
		foreach (ApplicationObjectTypeSymbol applicationObject in applicationObjects)
		{
			QueryTypeSymbol queryTypeSymbol = applicationObject as QueryTypeSymbol;
			VariableDefinition[] variables = ConvertVariables(queryTypeSymbol.GlobalVariables);
			MethodDefinition[] methods = ConvertMethods(queryTypeSymbol.GetMembers());
			PropertyDefinition[] properties = ConvertProperties(queryTypeSymbol);
			QueryDataItemDefinition[] elements = ConvertQueryDataItems(queryTypeSymbol);
			list.Add(new QueryDefinition
			{
				Id = queryTypeSymbol.Id,
				Name = queryTypeSymbol.Name,
				Variables = variables,
				Methods = methods,
				Properties = properties,
				Elements = elements
			});
		}
		return list;
	}

	private static QueryDataItemDefinition[] ConvertQueryDataItems(QueryTypeSymbol query)
	{
		return (from QueryDataItemSymbol dataItem in from s in query.GetMembers()
				where s.Kind == SymbolKind.QueryDataItem
				select s
			select ConvertQueryDataItem(dataItem)).ToArray();
	}

	private static QueryDataItemDefinition ConvertQueryDataItem(QueryDataItemSymbol dataItem)
	{
		QueryDataItemDefinition[] dataItems = (from QueryDataItemSymbol d in from s in dataItem.GetMembers()
				where s.Kind == SymbolKind.QueryDataItem
				select s
			select ConvertQueryDataItem(d)).ToArray();
		QueryColumnDefinition[] columns = (from QueryColumnSymbol c in from s in dataItem.GetMembers()
				where s.Kind == SymbolKind.QueryColumn
				select s
			select ConvertQueryColumn(c)).ToArray();
		QueryFilterDefinition[] filters = (from QueryFilterSymbol c in from s in dataItem.GetMembers()
				where s.Kind == SymbolKind.QueryFilter
				select s
			select ConvertQueryFilter(c)).ToArray();
		PropertyDefinition[] properties = ConvertProperties(dataItem);
		return new QueryDataItemDefinition
		{
			Id = dataItem.Id,
			Name = dataItem.Name,
			Properties = properties,
			Columns = columns,
			DataItems = dataItems,
			Filters = filters,
			RelatedTable = dataItem.RelatedTable?.Name
		};
	}

	private static QueryFilterDefinition ConvertQueryFilter(QueryFilterSymbol filter)
	{
		return new QueryFilterDefinition
		{
			Id = filter.Id,
			Name = filter.Name,
			Properties = ConvertProperties(filter),
			SourceColumn = filter.RelatedField?.Name
		};
	}

	private static QueryColumnDefinition ConvertQueryColumn(QueryColumnSymbol column)
	{
		return new QueryColumnDefinition
		{
			Id = column.Id,
			Name = column.Name,
			Properties = ConvertProperties(column),
			SourceColumn = column.RelatedField?.Name
		};
	}

	private static List<TableDefinition> ConvertTables(IEnumerable<ApplicationObjectTypeSymbol> tables)
	{
		List<TableDefinition> list = new List<TableDefinition>();
		foreach (ApplicationObjectTypeSymbol table in tables)
		{
			list.Add(ConvertTableBase(table));
		}
		return list;
	}

	private static List<TableExtensionDefinition> ConvertTableExtensions(IEnumerable<ApplicationObjectTypeSymbol> tableExtensions)
	{
		List<TableExtensionDefinition> list = new List<TableExtensionDefinition>();
		foreach (ApplicationObjectTypeSymbol tableExtension in tableExtensions)
		{
			TableDefinition tableDefinition = ConvertTableBase(tableExtension);
			TableExtensionTypeSymbol tableExtensionTypeSymbol = tableExtension as TableExtensionTypeSymbol;
			list.Add(new TableExtensionDefinition
			{
				TargetObject = tableExtensionTypeSymbol?.Target?.Name,
				Id = tableDefinition.Id,
				Name = tableDefinition.Name,
				Variables = tableDefinition.Variables,
				Fields = tableDefinition.Fields,
				Methods = tableDefinition.Methods,
				Properties = tableDefinition.Properties,
				Keys = tableDefinition.Keys
			});
		}
		return list;
	}

	private static TableDefinition ConvertTableBase(ApplicationObjectTypeSymbol tableBase)
	{
		FieldDefinition[] fields = ConvertTableFields(tableBase);
		VariableDefinition[] variables = ConvertVariables(tableBase.GlobalVariables);
		MethodDefinition[] methods = ConvertMethods(tableBase.GetMembers());
		PropertyDefinition[] properties = ConvertProperties(tableBase);
		KeyDefinition[] keys = ConvertKeys(tableBase);
		FieldGroupDefinition[] fieldGroups = ConvertFieldGroups(tableBase);
		return new TableDefinition
		{
			Id = tableBase.Id,
			Name = tableBase.Name,
			Variables = variables,
			Fields = fields,
			Methods = methods,
			Properties = properties,
			Keys = keys,
			FieldGroups = fieldGroups
		};
	}

	private static FieldDefinition[] ConvertTableFields(ApplicationObjectTypeSymbol table)
	{
		return (from FieldSymbol f in from x in table.GetMembers()
				where x.Kind == SymbolKind.Field
				select x
			select new FieldDefinition
			{
				Id = f.Id,
				Name = f.Name,
				TypeDefinition = GetType(f.Type),
				Properties = ConvertProperties(f)
			}).ToArray();
	}

	private static KeyDefinition[] ConvertKeys(ApplicationObjectTypeSymbol table)
	{
		ArrayBuilder<KeyDefinition> instance = ArrayBuilder<KeyDefinition>.GetInstance();
		try
		{
			foreach (FieldNameListSymbol item in table.GetMembersOfKind<FieldNameListSymbol>(SymbolKind.Key))
			{
				instance.Add(new KeyDefinition
				{
					Name = item.Name,
					FieldNames = item.Fields.Select((FieldSymbol f) => f.Name).ToArray(),
					Properties = ConvertProperties(item)
				});
			}
			return (instance.Count > 0) ? instance.ToArray() : null;
		}
		finally
		{
			instance.Free();
		}
	}

	private static FieldGroupDefinition[] ConvertFieldGroups(ApplicationObjectTypeSymbol table)
	{
		ArrayBuilder<FieldGroupDefinition> instance = ArrayBuilder<FieldGroupDefinition>.GetInstance();
		try
		{
			foreach (FieldNameListSymbol item in table.GetMembersOfKind<FieldNameListSymbol>(SymbolKind.FieldGroup))
			{
				instance.Add(new FieldGroupDefinition
				{
					Name = item.Name,
					FieldNames = item.Fields.Select((FieldSymbol f) => f.Name).ToArray(),
					Properties = ConvertProperties(item)
				});
			}
			return (instance.Count > 0) ? instance.ToArray() : null;
		}
		finally
		{
			instance.Free();
		}
	}

	private static List<CodeunitDefinition> ConvertCodeunits(IEnumerable<ApplicationObjectTypeSymbol> codeunits)
	{
		List<CodeunitDefinition> list = new List<CodeunitDefinition>();
		foreach (ApplicationObjectTypeSymbol codeunit in codeunits)
		{
			VariableDefinition[] variables = ConvertVariables(codeunit.GlobalVariables);
			MethodDefinition[] methods = ConvertMethods(codeunit.GetMembers());
			PropertyDefinition[] properties = ConvertProperties(codeunit);
			list.Add(new CodeunitDefinition
			{
				Id = codeunit.Id,
				Name = codeunit.Name,
				Variables = variables,
				Methods = methods,
				Properties = properties
			});
		}
		return list;
	}

	private static List<ReportDefinition> ConvertReports(IEnumerable<ApplicationObjectTypeSymbol> reports)
	{
		List<ReportDefinition> list = new List<ReportDefinition>();
		foreach (ApplicationObjectTypeSymbol report in reports)
		{
			VariableDefinition[] variables = ConvertVariables(report.GlobalVariables);
			MethodDefinition[] methods = ConvertMethods(report.GetMembers());
			PropertyDefinition[] properties = ConvertProperties(report);
			RequestPageDefinition requestPage = null;
			ReportTypeSymbol reportTypeSymbol = (ReportTypeSymbol)report;
			ReportDataItemDefinition[] dataItems = ConvertReportDataItems(reportTypeSymbol);
			if (reportTypeSymbol.RequestPage != null)
			{
				requestPage = ConvertRequestPage(reportTypeSymbol.RequestPage);
			}
			list.Add(new ReportDefinition
			{
				Id = report.Id,
				Name = report.Name,
				Variables = variables,
				Methods = methods,
				Properties = properties,
				RequestPage = requestPage,
				DataItems = dataItems
			});
		}
		return list;
	}

	private static ReportDataItemDefinition[] ConvertReportDataItems(ReportTypeSymbol reportSymbol)
	{
		return reportSymbol.FlattenedDataItems.Select((ReportDataItemSymbol dataItem) => ConvertReportDataItem(dataItem)).ToArray();
	}

	private static ReportDataItemDefinition ConvertReportDataItem(ReportDataItemSymbol dataItem)
	{
		return new ReportDataItemDefinition
		{
			Id = dataItem.Id,
			Name = dataItem.Name,
			RelatedTable = dataItem.RelatedTable?.Name,
			Properties = ConvertProperties(dataItem)
		};
	}

	private static RequestPageDefinition ConvertRequestPage(RequestPageTypeSymbol requestPage)
	{
		if (requestPage == null)
		{
			return null;
		}
		return new RequestPageDefinition
		{
			Id = 0,
			Name = requestPage.Name,
			Properties = ConvertProperties(requestPage),
			Controls = ConvertControls((from m in requestPage.GetMembers()
				where m.Kind == SymbolKind.Control && !m.IsImplicitlyDeclared
				select m).Cast<ControlSymbol>()),
			Actions = ConvertActions((from m in requestPage.GetMembers()
				where m.Kind == SymbolKind.Action && !m.IsImplicitlyDeclared
				select m).Cast<ActionSymbol>())
		};
	}

	private static List<PageDefinition> ConvertPages(IEnumerable<ApplicationObjectTypeSymbol> pages)
	{
		List<PageDefinition> list = new List<PageDefinition>();
		foreach (ApplicationObjectTypeSymbol page in pages)
		{
			VariableDefinition[] variables = ConvertVariables(page.GlobalVariables);
			MethodDefinition[] methods = ConvertMethods(page.GetMembers());
			PropertyDefinition[] properties = ConvertProperties(page);
			ActionDefinition[] actions = ConvertActions((from m in page.GetMembers()
				where m.Kind == SymbolKind.Action && !m.IsImplicitlyDeclared
				select m).Cast<ActionSymbol>());
			ControlDefinition[] controls = ConvertControls((from m in page.GetMembers()
				where m.Kind == SymbolKind.Control && !m.IsImplicitlyDeclared
				select m).Cast<ControlSymbol>());
			ViewDefinition[] views = ConvertViews((from m in page.GetMembers()
				where m.Kind == SymbolKind.View && !m.IsImplicitlyDeclared
				select m).Cast<ViewSymbol>());
			list.Add(new PageDefinition
			{
				Id = page.Id,
				Name = page.Name,
				Variables = variables,
				Methods = methods,
				Properties = properties,
				Actions = actions,
				Controls = controls,
				Views = views
			});
		}
		return list;
	}

	private static ControlDefinition[] ConvertControls(IEnumerable<ControlSymbol> controls)
	{
		ArrayBuilder<ControlDefinition> instance = ArrayBuilder<ControlDefinition>.GetInstance();
		try
		{
			foreach (ControlSymbol control in controls)
			{
				ControlDefinition controlDefinition = new ControlDefinition
				{
					Id = control.Id,
					Name = control.Name,
					Kind = control.ControlKind,
					Properties = ConvertProperties(control),
					Controls = ConvertControls(control.Controls),
					Actions = ConvertActions(control.Actions)
				};
				int? num = ((control.ControlKind == ControlKind.Part) ? control.RelatedPartSymbol?.Id : ((int?)null));
				controlDefinition.RelatedPagePartId = (num.HasValue ? ((Subtype)num.GetValueOrDefault()) : null);
				controlDefinition.TypeDefinition = GetType(control.Type);
				instance.Add(controlDefinition);
			}
			return (instance.Count > 0) ? instance.ToArray() : null;
		}
		finally
		{
			instance.Free();
		}
	}

	private static ExtensionControlDefinition[] ConvertViewControlChanges(ImmutableArray<Symbol> members)
	{
		List<ExtensionControlDefinition> list = new List<ExtensionControlDefinition>();
		foreach (Symbol item in members)
		{
			if (item is ChangeMoveSymbol changeMoveSymbol)
			{
				list.Add(new ExtensionControlDefinition
				{
					Anchor = changeMoveSymbol.Anchor.Name,
					ChangeKind = changeMoveSymbol.ChangeKind,
					Controls = ConvertControls(changeMoveSymbol.MovedSymbols.Cast<ControlSymbol>())
				});
			}
			else if (item is ChangeModifySymbol changeModifySymbol)
			{
				list.Add(new ExtensionControlDefinition
				{
					Anchor = changeModifySymbol.Target.Name,
					ChangeKind = changeModifySymbol.ChangeKind,
					Properties = ConvertProperties(changeModifySymbol)
				});
			}
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.ToArray();
	}

	private static ExtensionControlDefinition[] ConvertControlChanges(ImmutableArray<Symbol> controlChange)
	{
		List<ExtensionControlDefinition> list = new List<ExtensionControlDefinition>();
		foreach (SourceChangeAddControlSymbol item in controlChange.Where((Symbol a) => a is SourceChangeAddControlSymbol).Cast<SourceChangeAddControlSymbol>())
		{
			list.Add(new ExtensionControlDefinition
			{
				Anchor = item.Anchor.Name,
				ChangeKind = item.ChangeKind,
				Controls = ConvertControls(item.AddedSymbols.Cast<ControlSymbol>())
			});
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.ToArray();
	}

	private static ActionDefinition[] ConvertActions(IEnumerable<ActionSymbol> actions)
	{
		List<ActionDefinition> list = new List<ActionDefinition>();
		foreach (ActionSymbol action in actions)
		{
			list.Add(new ActionDefinition
			{
				Id = action.Id,
				Name = action.Name,
				Kind = action.ActionKind,
				Properties = ConvertProperties(action),
				Actions = ConvertActions(action.Actions)
			});
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.ToArray();
	}

	private static ExtensionActionDefinition[] ConvertActionChanges(ImmutableArray<Symbol> actionChange)
	{
		List<ExtensionActionDefinition> list = new List<ExtensionActionDefinition>();
		foreach (SourceChangeAddActionSymbol item in actionChange.Where((Symbol a) => a is SourceChangeAddActionSymbol).Cast<SourceChangeAddActionSymbol>())
		{
			DebugAssertHelper.Assert(item.Anchor != null);
			list.Add(new ExtensionActionDefinition
			{
				Anchor = item.Anchor?.Name,
				ChangeKind = item.ChangeKind,
				Actions = ConvertActions(item.AddedSymbols.Cast<ActionSymbol>())
			});
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.ToArray();
	}

	private static ViewDefinition[] ConvertViews(IEnumerable<ViewSymbol> views)
	{
		ArrayBuilder<ViewDefinition> instance = ArrayBuilder<ViewDefinition>.GetInstance();
		try
		{
			foreach (ViewSymbol view in views)
			{
				instance.Add(new ViewDefinition
				{
					Id = view.Id,
					Name = view.Name,
					Properties = ConvertProperties(view),
					ControlChanges = ConvertViewControlChanges(view.GetMembers())
				});
			}
			return (instance.Count > 0) ? instance.ToArray() : null;
		}
		finally
		{
			instance.Free();
		}
	}

	private static ExtensionViewDefinition[] ConvertViewChanges(ImmutableArray<Symbol> controlChange)
	{
		List<ExtensionViewDefinition> list = new List<ExtensionViewDefinition>();
		foreach (SourceChangeAddViewSymbol item in controlChange.Where((Symbol a) => a is SourceChangeAddViewSymbol).Cast<SourceChangeAddViewSymbol>())
		{
			list.Add(new ExtensionViewDefinition
			{
				Anchor = item.Anchor?.Name,
				ChangeKind = item.ChangeKind,
				Views = ConvertViews(item.AddedSymbols.Cast<ViewSymbol>())
			});
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.ToArray();
	}

	private static PropertyDefinition[] ConvertProperties(Symbol symbolWithProperties)
	{
		ArrayBuilder<PropertyDefinition> instance = ArrayBuilder<PropertyDefinition>.GetInstance();
		try
		{
			foreach (PropertySymbol property in symbolWithProperties.Properties)
			{
				if (property.PropertyKind == PropertyKind.SourceTable)
				{
					instance.Add(new PropertyDefinition
					{
						Name = property.Name,
						Value = (property.Value as TableTypeSymbol)?.Id.ToString(CultureInfo.InvariantCulture)
					});
				}
				else
				{
					instance.Add(new PropertyDefinition
					{
						Name = property.Name,
						Value = property.ValueText
					});
				}
			}
			ControlProperties(symbolWithProperties, instance);
			return (instance.Count > 0) ? instance.ToArray() : null;
		}
		finally
		{
			instance.Free();
		}
	}

	private static void ControlProperties(Symbol symbolWithProperties, ArrayBuilder<PropertyDefinition> builder)
	{
		SourceControlSymbol sourceControlSymbol = symbolWithProperties as SourceControlSymbol;
		if (sourceControlSymbol != null)
		{
			if (sourceControlSymbol.ControlBaseSyntax is PageFieldSyntax pageFieldSyntax)
			{
				builder.Add(new PropertyDefinition
				{
					Name = "SourceExpression",
					Value = pageFieldSyntax.Expression.ToFullString()
				});
			}
			if (sourceControlSymbol.ControlBaseSyntax is PageSystemPartSyntax pageSystemPartSyntax && sourceControlSymbol.ControlKind == ControlKind.SystemPart)
			{
				builder.Add(new PropertyDefinition
				{
					Name = "SystemPartId",
					Value = pageSystemPartSyntax.SystemPartType.ToFullString()
				});
				builder.Add(new PropertyDefinition
				{
					Name = "PartType",
					Value = "System"
				});
			}
			if (sourceControlSymbol.ControlBaseSyntax is PageChartPartSyntax pageChartPartSyntax && sourceControlSymbol.ControlKind == ControlKind.ChartPart)
			{
				builder.Add(new PropertyDefinition
				{
					Name = "ChartPartId",
					Value = pageChartPartSyntax.ChartPartType.ToFullString()
				});
				builder.Add(new PropertyDefinition
				{
					Name = "PartType",
					Value = "Chart"
				});
			}
		}
	}

	private static VariableDefinition[] ConvertVariables(IEnumerable<VariableSymbol> variables)
	{
		List<VariableDefinition> list = new List<VariableDefinition>();
		foreach (VariableSymbol variable in variables)
		{
			if (!(variable is SynthesizedGlobalVariableSymbol))
			{
				list.Add(new VariableDefinition
				{
					Name = variable.Name,
					TypeDefinition = GetType(variable.Type),
					Attributes = ConvertAttributes(variable)
				});
			}
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.ToArray();
	}

	private static ParameterDefinition[] ConvertParameters(IEnumerable<ParameterSymbol> parameters)
	{
		return parameters.Select((ParameterSymbol p) => new ParameterDefinition
		{
			IsVar = (p.IsVar ? new bool?(true) : ((bool?)null)),
			Name = p.Name,
			TypeDefinition = GetType(p.Type)
		}).ToArray();
	}

	private static MethodDefinition[] ConvertMethods(IEnumerable<Symbol> members)
	{
		MethodDefinition[] array = (from method in members.OfType<SourceMethodOrTriggerSymbol>().Where(FilterMethods)
			select new MethodDefinition
			{
				Id = method.Id,
				Name = method.Name,
				IsLocal = (method.IsLocal ? new bool?(true) : ((bool?)null)),
				Parameters = ConvertParameters(method.Parameters),
				ReturnTypeDefinition = GetType(method.ReturnType),
				Attributes = ConvertAttributes(method),
				MethodKind = method.MethodKind
			}).ToArray();
		if (array.Length == 0)
		{
			return null;
		}
		return array;
	}

	private static bool FilterMethods(SourceMethodOrTriggerSymbol m)
	{
		if (!m.IsEvent)
		{
			return !m.IsLocal;
		}
		return true;
	}

	private static EventDefinition[] ConvertEvents(ObjectTypeSymbol applicationObject)
	{
		EventDefinition[] array = (from e in applicationObject.GetMembers().OfType<SourceEventSymbol>()
			select new EventDefinition
			{
				Name = e.Name,
				Parameters = ConvertParameters(e.Parameters),
				Attributes = ConvertAttributes(e)
			}).ToArray();
		if (array.Length == 0)
		{
			return null;
		}
		return array;
	}

	private static AttributeDefinition[] ConvertAttributes(ISymbolWithAttributes symbolWithAttributes)
	{
		List<AttributeDefinition> list = new List<AttributeDefinition>();
		foreach (AttributeSymbol attribute in symbolWithAttributes.Attributes)
		{
			list.Add(new AttributeDefinition
			{
				Name = attribute.Name,
				Arguments = ConvertAttributeArguments(attribute)
			});
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.ToArray();
	}

	private static AttributeArgumentDefinition[] ConvertAttributeArguments(AttributeSymbol attribute)
	{
		return attribute.Arguments.Select((AttributeArgumentSymbol arg) => new AttributeArgumentDefinition
		{
			Value = arg.ValueText
		}).ToArray();
	}

	private static Subtype GetSubType(TypeSymbol type)
	{
		type = GetElementType(type);
		Symbol aliasedTypeSymbol = type.GetAliasedTypeSymbol();
		if (aliasedTypeSymbol.Kind.IsObjectWithSubtype())
		{
			return new Subtype(type.Name, (aliasedTypeSymbol as ISymbolWithId)?.Id);
		}
		if (type.NavTypeKind == NavTypeKind.DotNet)
		{
			DotNetTypeDeclarationSymbol declaringSymbol = ((DotNetTypeSymbol)type).DeclaringSymbol;
			DebugAssertHelper.Assert(declaringSymbol != null, "A .NET type symbol referenced from the application should always have a declaration because there is no other way to reference it other than throw an alias.");
			return new Subtype(declaringSymbol.Name, null);
		}
		return null;
	}

	private static string GetTypeName(TypeSymbol type)
	{
		type = GetElementType(type);
		return type.NavTypeKind.ToString() + (type.HasLength ? string.Format(CultureInfo.InvariantCulture, "[{0}]", type.Length) : string.Empty);
	}

	private static TypeDefinition GetType(TypeSymbol type)
	{
		return new TypeDefinition
		{
			Name = GetTypeName(type),
			Subtype = GetSubType(type),
			OptionMembers = GetOptionMembers(type),
			TypeArguments = GetTypeArguments(type),
			ArrayDimensions = (type.IsArray() ? ((ArrayTypeSymbol)type).Dimensions.ToArray() : null)
		};
	}

	private static TypeDefinition[] GetTypeArguments(TypeSymbol type)
	{
		if (!type.IsGenericType())
		{
			return null;
		}
		return ((GenericNavTypeSymbol)type).TypeArguments.Select((TypeSymbol x) => GetType(x)).ToArray();
	}

	private static TypeSymbol GetElementType(TypeSymbol type)
	{
		if (type.IsArray())
		{
			type = ((ArrayTypeSymbol)type).ElementType;
		}
		return type;
	}

	private static SymbolKind GetSymbolKindForTypeName(string typeString)
	{
		if (!Enum.TryParse<SymbolKind>(typeString, out var result))
		{
			return SymbolKind.Undefined;
		}
		return result;
	}

	private static ISymbol ResolveApplicationObjectReference(this ModuleSymbol module, SymbolKind kind, Subtype subType)
	{
		if (module == null || module.ReferenceResolver == null)
		{
			return null;
		}
		IEnumerable<ISymbol> enumerable;
		if ((object)subType != null && subType.Id.HasValue && subType.Id.Value != 0)
		{
			enumerable = module.ReferenceResolver.GetObjectSymbolsByIdAcrossModules(module, kind, subType.Id.Value).Cast<ISymbol>();
		}
		else
		{
			IEnumerable<ISymbol> enumerable2 = module.ReferenceResolver.GetObjectSymbolsByNameAcrossModules(module, kind, subType?.Name);
			enumerable = enumerable2;
		}
		IEnumerable<ISymbol> enumerable3 = enumerable;
		DebugAssertHelper.Assert((enumerable3?.Count() ?? 0) <= 1, "Kind: " + kind.ToString() + ", Name: " + (subType?.Name ?? string.Empty) + "Id: " + (subType?.Id.ToString() ?? "undefined"));
		return enumerable3?.FirstOrDefault();
	}

	private static bool TryMatchTypeWithLength(string typeString, string expectedTypeName, out int length)
	{
		RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
		Match match = new Regex(string.Format(CultureInfo.InvariantCulture, "{0}\\[(?<Length>\\d+)\\]", expectedTypeName), options).Match(typeString);
		if (match.Success)
		{
			length = int.Parse(match.Groups["Length"].Value, CultureInfo.InvariantCulture);
			return true;
		}
		length = 0;
		return false;
	}

	private static bool TryGetTypeSymbolWithLength(string typeString, out TypeSymbol symbol)
	{
		bool result = false;
		symbol = null;
		if (TryMatchTypeWithLength(typeString, NavTypeKind.Text.ToString(), out var length))
		{
			result = true;
			symbol = TextTypeSymbol.GetInstance(NavTypeKind.Text, length);
		}
		else if (TryMatchTypeWithLength(typeString, NavTypeKind.Code.ToString(), out length))
		{
			result = true;
			symbol = CodeTypeSymbol.GetInstance(NavTypeKind.Code, length);
		}
		return result;
	}

	private static string[] GetOptionMembers(TypeSymbol type)
	{
		if (!type.IsNativeOptionTypeSymbol())
		{
			return null;
		}
		return (from x in ((OptionTypeSymbol)type).GetMembersOfKind<Symbol>(SymbolKind.Option)
			select x.Name).ToArray();
	}

	private static TypeSymbol ResolveElementType(ModuleSymbol module, Symbol owner, TypeDefinition typeDefinition)
	{
		if (typeDefinition == null || typeDefinition.Name == null)
		{
			return null;
		}
		if (typeDefinition.Name == "Option")
		{
			return ReferenceOptionTypeSymbol.Create(owner, typeDefinition.OptionMembers);
		}
		if (typeDefinition.Name == "Action")
		{
			return TypeSymbol.GetNavTypeSymbol(NavTypeKind.Action);
		}
		if (typeDefinition.Name == "Label")
		{
			return TypeSymbol.GetNavTypeSymbol(NavTypeKind.Label);
		}
		SymbolKind symbolKindForTypeName = GetSymbolKindForTypeName(typeDefinition.Name);
		var (flag, result) = TryResolveAliasedElementType(module, owner, typeDefinition, symbolKindForTypeName);
		if (flag)
		{
			return result;
		}
		if (symbolKindForTypeName != SymbolKind.Undefined && (symbolKindForTypeName != SymbolKind.DotNet || (typeDefinition.Subtype != null && !typeDefinition.Subtype.IsEmpty)))
		{
			return module.ResolveApplicationObjectReference(symbolKindForTypeName, typeDefinition.Subtype) as TypeSymbol;
		}
		if (TryGetTypeSymbolWithLength(typeDefinition.Name, out var symbol))
		{
			return symbol;
		}
		NavTypeKind navTypeKind = NavTypeExtensions.GetNavTypeKind(typeDefinition.Name, throwIfUnknown: false);
		TypeSymbol result2 = null;
		if (navTypeKind != NavTypeKind.None)
		{
			result2 = ((!navTypeKind.IsGenericType()) ? TypeSymbol.GetNavTypeSymbol(navTypeKind) : ResolveGenericType(module, owner, typeDefinition, navTypeKind));
		}
		return result2;
	}

	private static (bool IsAliasedSymbol, TypeSymbol Result) TryResolveAliasedElementType(ModuleSymbol module, Symbol owner, TypeDefinition typeDefinition, SymbolKind kind)
	{
		return kind switch
		{
			SymbolKind.Record => (IsAliasedSymbol: true, Result: ResolveAliasedElementType(module, owner, typeDefinition, SymbolKind.Table, (Symbol o, TableTypeSymbol t) => module.GetRecord(t, isTemporary: false))), 
			SymbolKind.TestPage => (IsAliasedSymbol: true, Result: ResolveAliasedElementType(module, owner, typeDefinition, SymbolKind.Page, (Symbol _, PageTypeSymbol page) => module.GetTestPage(page))), 
			SymbolKind.TestRequestPage => (IsAliasedSymbol: true, Result: ResolveAliasedElementType(module, owner, typeDefinition, SymbolKind.Report, (Symbol _, ReportTypeSymbol report) => module.GetTestRequestPage(report.RequestPage))), 
			_ => (IsAliasedSymbol: false, Result: null), 
		};
	}

	private static TypeSymbol ResolveAliasedElementType<T>(ModuleSymbol module, Symbol owner, TypeDefinition typeDefinition, SymbolKind originalElementKind, Func<Symbol, T, TypeSymbol> aliasedElementFactory) where T : Symbol
	{
		T val = module.ResolveApplicationObjectReference(originalElementKind, typeDefinition.Subtype) as T;
		if (val == null)
		{
			return null;
		}
		return aliasedElementFactory(owner, val);
	}

	private static TypeSymbol ResolveGenericType(ModuleSymbol module, Symbol owner, TypeDefinition typeDefinition, NavTypeKind navTypeKind)
	{
		IGenericLanguageClassTypeSymbolFactory genericNavTypeSymbolFactory = TypeSymbol.GetGenericNavTypeSymbolFactory(navTypeKind);
		TypeSymbol[] array = new TypeSymbol[typeDefinition.TypeArguments.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ResolveType(module, owner, typeDefinition.TypeArguments[i]);
		}
		return genericNavTypeSymbolFactory.Create(array);
	}

	private static List<PageExtensionDefinition> ConvertPageExtensions(IEnumerable<IApplicationObjectTypeSymbol> pageExtensions)
	{
		List<PageExtensionDefinition> list = new List<PageExtensionDefinition>();
		foreach (IApplicationObjectTypeSymbol pageExtension in pageExtensions)
		{
			PageExtensionTypeSymbol pageExtensionTypeSymbol = pageExtension as PageExtensionTypeSymbol;
			if (pageExtensionTypeSymbol == null)
			{
				return new List<PageExtensionDefinition>();
			}
			VariableDefinition[] variables = ConvertVariables(pageExtensionTypeSymbol.GlobalVariables);
			MethodDefinition[] methods = ConvertMethods(pageExtensionTypeSymbol.GetMembers());
			ExtensionActionDefinition[] actionChanges = ConvertActionChanges(pageExtensionTypeSymbol.GetMembers());
			ExtensionControlDefinition[] controlChanges = ConvertControlChanges(pageExtensionTypeSymbol.GetMembers());
			ExtensionViewDefinition[] viewChanges = ConvertViewChanges(pageExtensionTypeSymbol.GetMembers());
			list.Add(new PageExtensionDefinition
			{
				TargetObject = pageExtensionTypeSymbol.Target?.Name,
				Id = pageExtensionTypeSymbol.Id,
				Name = pageExtensionTypeSymbol.Name,
				Variables = variables,
				Methods = methods,
				ActionChanges = actionChanges,
				ControlChanges = controlChanges,
				ViewChanges = viewChanges
			});
		}
		return list;
	}

	private static List<PageCustomizationDefinition> ConvertPageCustomizations(IEnumerable<IApplicationObjectTypeSymbol> pageCustomizations)
	{
		List<PageCustomizationDefinition> list = new List<PageCustomizationDefinition>();
		foreach (IApplicationObjectTypeSymbol pageCustomization in pageCustomizations)
		{
			PageCustomizationTypeSymbol pageCustomizationTypeSymbol = pageCustomization as PageCustomizationTypeSymbol;
			if (pageCustomizationTypeSymbol == null)
			{
				return new List<PageCustomizationDefinition>();
			}
			ExtensionActionDefinition[] actionChanges = ConvertActionChanges(pageCustomizationTypeSymbol.GetMembers());
			ExtensionControlDefinition[] controlChanges = ConvertControlChanges(pageCustomizationTypeSymbol.GetMembers());
			ExtensionViewDefinition[] viewChanges = ConvertViewChanges(pageCustomizationTypeSymbol.GetMembers());
			list.Add(new PageCustomizationDefinition
			{
				TargetObject = pageCustomizationTypeSymbol.Target?.Name,
				Id = pageCustomizationTypeSymbol.Id,
				Name = pageCustomizationTypeSymbol.Name,
				ActionChanges = actionChanges,
				ControlChanges = controlChanges,
				ViewChanges = viewChanges
			});
		}
		return list;
	}

	internal static ModuleDefinition FilterOutPageCustomizations(ModuleDefinition module, IEnumerable<ModuleDefinition> modulesPerProfile)
	{
		return new ModuleDefinition
		{
			AppId = module.AppId,
			Name = module.Name,
			PageExtensions = module.PageExtensions,
			PageCustomizations = module.PageCustomizations.Where((PageCustomizationDefinition p) => !modulesPerProfile.Any((ModuleDefinition m) => m.PageCustomizations.Contains(p))).ToArray(),
			Publisher = module.Publisher,
			Version = module.Version,
			Profiles = module.Profiles,
			Codeunits = module.Codeunits,
			Pages = module.Pages,
			Queries = module.Queries,
			Reports = module.Reports,
			TableExtensions = module.TableExtensions,
			Tables = module.Tables,
			ControlAddIns = module.ControlAddIns,
			EnumTypes = module.EnumTypes,
			DotNetPackages = module.DotNetPackages,
			XmlPorts = module.XmlPorts
		};
	}
}
