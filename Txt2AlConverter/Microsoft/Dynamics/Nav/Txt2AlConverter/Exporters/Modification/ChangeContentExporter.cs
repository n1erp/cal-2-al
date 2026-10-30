using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Modification;

internal class ChangeContentExporter : IElementExporter
{
	private readonly AlExportOptions options;

	private static ProcedureExporter procedureExporter;

	private static CommentedCodeDeltaExporter commentedCodeExporter;

	public ChangeContentExporter(AlExportOptions options)
	{
		this.options = options;
	}

	public void Export(IElement element, IAlWriter writer)
	{
		IElementExporter elementExporter = null;
		switch (ModificationContentExporter.GetExtensionTargetType(element))
		{
		case ElementType.Page:
			elementExporter = new PageExtensionExporter(options);
			break;
		case ElementType.Table:
			elementExporter = new TableExtensionExporter(options);
			break;
		}
		procedureExporter = new ProcedureExporter();
		commentedCodeExporter = new CommentedCodeDeltaExporter();
		elementExporter?.Export(element, writer);
	}

	internal static IEnumerable<IElement> GetChangesOfTypeAndTarget(IElement element, ElementType elementType, params ChangeTypeKind[] changeTypes)
	{
		IList<IElement> changes = new List<IElement>();
		element.GetSubElements(ElementType.Change).ForEach((IElement change) =>
		{
			if (GetChangeTargetType(change) == elementType && changeTypes.Contains(change.GetChangeType()))
			{
				changes.Add(change);
			}
		});
		return changes;
	}

	internal static IEnumerable<IElement> GetChangesOfType(IElement element, ElementType elementType)
	{
		IList<IElement> changes = new List<IElement>();
		element.GetSubElements(ElementType.Change).ForEach((IElement change) =>
		{
			if (change.GetSubElements(elementType).Any() || GetChangeTargetType(change) == elementType)
			{
				changes.Add(change);
			}
		});
		return changes;
	}

	internal static IEnumerable<IElement> GetVariablesChanges(IElement element)
	{
		IList<IElement> changes = new List<IElement>();
		element.GetSubElements(ElementType.Change).ForEach((IElement change) =>
		{
			if ((GetChangeTargetType(change) == ElementType.Variable || change.GetSubElements(ElementType.Variable).Any()) && IsGlobalVariableChange(change))
			{
				changes.Add(change);
			}
		});
		return changes;
	}

	private static bool IsGlobalVariableChange(IElement change)
	{
		ElementType changeTargetType = GetChangeTargetType(change);
		ElementType changeTargetContextType = GetChangeTargetContextType(change);
		if (changeTargetType != ElementType.Procedure && changeTargetType != ElementType.Event && changeTargetContextType != ElementType.Procedure)
		{
			return changeTargetContextType != ElementType.Event;
		}
		return false;
	}

	internal static IEnumerable<IElement> GetTriggerChanges(IElement element)
	{
		IList<IElement> changes = new List<IElement>();
		element.GetSubElements(ElementType.Change).ForEach((IElement change) =>
		{
			string text = CollectTargetInformation(change);
			if (text != null && Enum.TryParse<ElementType>(text, out var result) && AlUtilities.IsTriggerElementType(result))
			{
				changes.Add(change);
			}
		});
		return changes;
	}

	private static string CollectTargetInformation(IElement element)
	{
		element.TryGetStringProperty(PropertyType.Target, out var value);
		if (value != null)
		{
			return value;
		}
		element.TryGetStringProperty(PropertyType.TargetValue, out value);
		if (value != null)
		{
			return value;
		}
		List<IElement> list = element.GetSubElements().ToList();
		if (list.Count > 0)
		{
			return list.ToList().First().ToString();
		}
		return null;
	}

	internal static string GetSectionHeader(ElementType sectionTargetType)
	{
		return sectionTargetType switch
		{
			ElementType.Control => SyntaxKind.PageLayoutKeyword.GetText(), 
			ElementType.Action => SyntaxKind.ActionsKeyword.GetText(), 
			ElementType.Field => SyntaxKind.FieldsKeyword.GetText(), 
			ElementType.Key => SyntaxKind.KeysKeyword.GetText(), 
			_ => string.Empty, 
		};
	}

	internal static ChangeType GetChangeType(IElement element)
	{
		switch (element.GetChangeType())
		{
		case ChangeTypeKind.Insertion:
			return ChooseInsertionChangeType(element);
		case ChangeTypeKind.PropertyInsertion:
		case ChangeTypeKind.PropertyModification:
			return ChangeType.PropertyChange;
		case ChangeTypeKind.Move:
			return ChangeType.MoveAfter;
		case ChangeTypeKind.Deletion:
			return ChangeType.Deletion;
		case ChangeTypeKind.PropertyDeletion:
			return ChangeType.PropertyDeletion;
		case ChangeTypeKind.CodeModification:
			return ChangeType.CodeModification;
		default:
			return ChangeType.Unknown;
		}
	}

	private static ChangeType ChooseInsertionChangeType(IElement element)
	{
		element.TryGetStringProperty(PropertyType.InsertAfter, out var value);
		if (value != null)
		{
			return ChangeType.InsertAfter;
		}
		element.TryGetStringProperty(PropertyType.TargetValue, out value);
		if (value != null)
		{
			return ChangeType.CodeModification;
		}
		element.TryGetStringProperty(PropertyType.Target, out value);
		if (value != null)
		{
			return ChangeType.CodeModification;
		}
		element.TryGetStringProperty(PropertyType.AddedChildren, out value);
		if (value != null)
		{
			return ChangeType.CollectionInsertion;
		}
		return ChangeType.InsertAfter;
	}

	internal static ElementType GetChangeTargetType(IElement element)
	{
		return ModificationContentExporter.GetExtensionTargetType(element);
	}

	private static ElementType GetChangeTargetContextType(IElement element)
	{
		return ModificationContentExporter.GetExtensionTargetContextType(element);
	}

	internal static void ExportUnsupportedChanges(IEnumerable<IElement> changes, IAlWriter writer)
	{
		IEnumerable<IElement> source = (changes as IElement[]) ?? changes.ToArray();
		if (source.Any())
		{
			source.Where((IElement c) => !IgnoredUnsupportedChange(c)).ForEach((IElement change) =>
			{
				new WarningExporter().Export(change, writer);
			});
		}
	}

	private static bool IgnoredUnsupportedChange(IElement change)
	{
		string stringProperty = change.GetStringProperty(PropertyType.Property);
		if (stringProperty != null && stringProperty == "Version List")
		{
			return true;
		}
		return false;
	}

	internal static void ExportVariables(IEnumerable<IElement> changes, IAlWriter writer)
	{
		IEnumerable<IElement> enumerable = (changes as IElement[]) ?? changes.ToArray();
		if (enumerable.Any())
		{
			enumerable.ForEach((IElement change) =>
			{
				ExportVariablesSubelements(change, writer);
			});
		}
	}

	private static void ExportVariablesSubelements(IElement element, IAlWriter writer)
	{
		switch (element.GetChangeType())
		{
		case ChangeTypeKind.Insertion:
			new VariableSectionExporter().Export(element, writer);
			break;
		case ChangeTypeKind.PropertyModification:
			commentedCodeExporter.Export(element, writer);
			break;
		}
	}

	internal static void ExportProcedures(IEnumerable<IElement> changes, IAlWriter writer)
	{
		IEnumerable<IElement> enumerable = (changes as IElement[]) ?? changes.ToArray();
		if (enumerable.Any())
		{
			enumerable.ForEach((IElement change) =>
			{
				ExportProcedureSubelements(change, writer);
			});
		}
	}

	private static void ExportProcedureSubelements(IElement element, IAlWriter writer)
	{
		switch (element.GetChangeType())
		{
		case ChangeTypeKind.Insertion:
			element.GetSubElements().ForEach((IElement change) =>
			{
				ExportProcedureInsertionElement(change, element, writer);
			});
			break;
		case ChangeTypeKind.CodeModification:
			commentedCodeExporter.Export(element, writer);
			break;
		case ChangeTypeKind.PropertyInsertion:
		case ChangeTypeKind.PropertyModification:
		case ChangeTypeKind.PropertyDeletion:
			new WarningExporter().Export(element, writer);
			break;
		case ChangeTypeKind.Deletion:
			break;
		}
	}

	private static void ExportProcedureInsertionElement(IElement change, IElement parent, IAlWriter writer)
	{
		ElementType elementType = change.ElementTypeInfo.ElementType;
		if (elementType == ElementType.Variable || (uint)(elementType - 5) <= 1u)
		{
			new WarningExporter(elementType.ToString() + " Insertion " + change.ToString()).Export(parent, writer);
		}
		else
		{
			procedureExporter.Export(change, writer);
		}
	}

	internal static void ExportTriggers(IEnumerable<IElement> changes, IAlWriter writer)
	{
		IEnumerable<IElement> enumerable = (changes as IElement[]) ?? changes.ToArray();
		if (enumerable.Any())
		{
			enumerable.ForEach((IElement change) =>
			{
				commentedCodeExporter.Export(change, writer);
			});
		}
	}

	internal static void ExportDocumentationChanges(IEnumerable<IElement> changes, IAlWriter writer)
	{
		IEnumerable<IElement> enumerable = (changes as IElement[]) ?? changes.ToArray();
		if (!enumerable.Any())
		{
			return;
		}
		enumerable.ForEach((IElement change) =>
		{
			switch (change.GetChangeType())
			{
			case ChangeTypeKind.Insertion:
				ElementWithDocumentationExporter.ExportDocumentationLines(change.GetFirstSubElement(ElementType.Documentation), writer);
				break;
			case ChangeTypeKind.CodeModification:
				ElementWithDocumentationExporter.ExportDocumentationLines(change.GetFirstSubElement(ElementType.ModifiedCode), writer);
				break;
			}
		});
	}
}
