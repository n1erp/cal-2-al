using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

public static class PropertiesSolveHelper
{
	public static void SolvePropertiesDefaulting(MasterPage masterPage, MetaTable metaTable, Dictionary<int, MetaTable> expressionsTables)
	{
		if (masterPage != null)
		{
			if (string.IsNullOrEmpty(masterPage.Caption))
			{
				masterPage.Caption = masterPage.Name;
			}
			SolvePageProperties(masterPage);
			SolvePropertiesDefaultingControls(masterPage.FindAll(FindAllControls), metaTable, masterPage);
			SolveExpressions(masterPage.Expressions, expressionsTables);
		}
	}

	private static void SolveExpressions(List<DataFieldDefinition> expressions, Dictionary<int, MetaTable> expressionsTables)
	{
		if (expressions == null || expressionsTables == null)
		{
			return;
		}
		foreach (DataFieldDefinition expression in expressions)
		{
			if (expression.TableNo != 0 && expression.FieldNo != 0 && expressionsTables.TryGetValue(expression.TableNo, out var value))
			{
				MetaField fieldsById = value.GetFieldsById(expression.FieldNo);
				if (fieldsById == null)
				{
					throw NavMetaFieldNotFoundException.Create(CultureInfo.CurrentCulture, expression.FieldNo, expression.TableNo);
				}
				expression.DataLength = fieldsById.Length;
				expression.Datatype = CommonTypeInformation.NavTypeToDataType(fieldsById.Type);
				string[] fieldOptionsArray = GetFieldOptionsArray(fieldsById);
				if (fieldOptionsArray != null)
				{
					expression.OptionString = string.Join(','.ToString(), fieldOptionsArray);
				}
				if (fieldsById.Type == NavType.BLOB)
				{
					expression.SubType = fieldsById.BlobSubType.ToString();
				}
			}
		}
	}

	private static void SolvePageProperties(MasterPage masterPage)
	{
		if (masterPage.PageProperties != null)
		{
			if (!masterPage.PageProperties.SourceObject.AutoSplitKeySpecified && masterPage.PageProperties.SourceObject.MultipleNewLines)
			{
				masterPage.PageProperties.SourceObject.AutoSplitKey = true;
			}
			masterPage.ShowFilterPane = (masterPage.PageProperties.SourceObject.ShowFilterSpecified ? masterPage.PageProperties.SourceObject.ShowFilter : masterPage.ShowFilterPane);
			SetDefaultChunks(masterPage);
		}
	}

	private static bool IsActionDefinition(ElementDefinition element)
	{
		return element is ActionDefinition;
	}

	private static void SetDefaultChunks(MasterPage masterPage)
	{
		if (string.IsNullOrEmpty(masterPage.PageProperties.PromotedActionCategoriesML) || string.IsNullOrEmpty(masterPage.PageProperties.PromotedActionCategories))
		{
			masterPage.PageProperties.PromotedActionCategories = string.Empty;
		}
		if (masterPage.CommandBar == null || !string.IsNullOrEmpty(masterPage.PageProperties.PromotedActionCategoriesOrder))
		{
			return;
		}
		string[] array = masterPage.PageProperties.PromotedActionCategories.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		List<ElementDefinition> list = masterPage.CommandBar.FindAll(IsActionDefinition);
		PromotedCategory[] array2 = new PromotedCategory[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			string caption = array[i];
			array2[i] = new PromotedCategory(PromotedCategory.Types[i], caption);
		}
		int[] array3 = new int[array2.Length];
		foreach (ElementDefinition item in list)
		{
			if (!(item is ActionDefinition { Promoted: not false } actionDefinition))
			{
				continue;
			}
			PromotedCategory promotedCategory = FindCategoryByType(array2, actionDefinition.PromotedCategory);
			if (promotedCategory != null)
			{
				if (actionDefinition.PromotedIsBig)
				{
					promotedCategory.ActionGuidsByOrder.Insert(array3[Array.IndexOf(array2, promotedCategory)]++, actionDefinition.ControlGUID);
				}
				else
				{
					promotedCategory.ActionGuidsByOrder.Add(actionDefinition.ControlGUID);
				}
			}
		}
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(PromotedCategory[]));
		StringBuilder stringBuilder = new StringBuilder();
		XmlWriter xmlWriter = XmlWriter.Create(stringBuilder);
		xmlSerializer.Serialize(xmlWriter, array2);
		masterPage.PageProperties.PromotedActionCategoriesOrder = stringBuilder.ToString();
	}

	private static PromotedCategory FindCategoryByType(PromotedCategory[] promotedCategories, PromotedCategoryType promotedCategoryType)
	{
		foreach (PromotedCategory promotedCategory in promotedCategories)
		{
			if (promotedCategory.CategoryType == promotedCategoryType)
			{
				return promotedCategory;
			}
		}
		return null;
	}

	private static void SolvePropertiesDefaultingControls(List<ElementDefinition> controls, MetaTable metaTable, MasterPage masterPage)
	{
		foreach (ElementDefinition control in controls)
		{
			MetaField metaField = null;
			if (control is ControlDataboundDefinition controlDataboundDefinition)
			{
				if (metaTable != null && int.TryParse(controlDataboundDefinition.DataColumnName, out var result))
				{
					metaField = metaTable.GetFieldsById(result);
				}
				if (control is ControlDefinition { AccessByObjectID: not 0, AccessByObjectType: not AccessByObjectType.None } controlDefinition)
				{
					if (controlDefinition.AccessByPermissionMask != 0)
					{
						RelatedObjectDefinition relatedObjectDefinition = new RelatedObjectDefinition();
						relatedObjectDefinition.ObjectID = controlDefinition.AccessByObjectID;
						relatedObjectDefinition.ObjectType = controlDefinition.AccessByObjectType;
						relatedObjectDefinition.PermissionMask = controlDefinition.AccessByPermissionMask;
						relatedObjectDefinition.RelatedBy = RelationType.AccessByPermission;
						if (controlDataboundDefinition.RelatedObjects == null)
						{
							controlDataboundDefinition.RelatedObjects = new List<RelatedObjectDefinition>();
						}
						controlDataboundDefinition.RelatedObjects.Add(relatedObjectDefinition);
					}
				}
				else if (metaField != null && metaField.AccessByObjectType != AccessByObjectType.None && metaField.AccessByObjectID != 0)
				{
					RelatedObjectDefinition relatedObjectDefinition2 = new RelatedObjectDefinition();
					relatedObjectDefinition2.ObjectID = metaField.AccessByObjectID;
					relatedObjectDefinition2.ObjectType = metaField.AccessByObjectType;
					relatedObjectDefinition2.PermissionMask = metaField.AccessByPermissionMask;
					relatedObjectDefinition2.RelatedBy = RelationType.AccessByPermission;
					if (controlDataboundDefinition.RelatedObjects == null)
					{
						controlDataboundDefinition.RelatedObjects = new List<RelatedObjectDefinition>();
					}
					controlDataboundDefinition.RelatedObjects.Add(relatedObjectDefinition2);
				}
			}
			control.SolveProperties(metaField, metaTable, masterPage);
		}
	}

	internal static void SolveClosingDates(ControlBaseDefinition control, MetaField field)
	{
		if (!control.ClosingDatesSpecified && field != null)
		{
			control.ClosingDates = field.ClosingDates;
		}
	}

	internal static void SolveDateFormula(ControlBaseDefinition control, MetaField field)
	{
		if (field != null && field.DateFormula)
		{
			control.DateFormula = field.DateFormula;
		}
	}

	internal static void SolveProgressControl(ControlDataboundDefinition progressControl, MetaField field)
	{
		decimal result = 0m;
		bool useField = false;
		bool useControl;
		if ((useControl = decimal.TryParse(progressControl.MinValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2)) | (field != null && (useField = decimal.TryParse(field.MinValue, NumberStyles.Float, CultureInfo.InvariantCulture, out result))))
		{
			progressControl.MinValue = SolveGenericMinMax(result2, result, useControl, useField, lowBound: true).ToString(CultureInfo.InvariantCulture);
		}
		if ((useControl = decimal.TryParse(progressControl.MaxValue, NumberStyles.Float, CultureInfo.InvariantCulture, out result2)) | (field != null && (useField = decimal.TryParse(field.MaxValue, NumberStyles.Float, CultureInfo.InvariantCulture, out result))))
		{
			progressControl.MaxValue = SolveGenericMinMax(result2, result, useControl, useField, lowBound: false).ToString(CultureInfo.InvariantCulture);
		}
		else
		{
			progressControl.MaxValue = 10000.ToString(CultureInfo.InvariantCulture);
		}
	}

	internal static void SolveBlankNumbers(ControlDefinition control, MetaField field)
	{
		if (field != null && !control.BlankNumbersSpecified)
		{
			control.BlankNumbers = field.BlankNumbers;
		}
	}

	internal static void SolveBlankZero(ControlDefinition control, MetaField field)
	{
		if (field != null && !control.BlankZeroSpecified)
		{
			control.BlankZero = field.BlankZero;
		}
	}

	internal static void SolveNotBlank(ControlDefinition control, MetaField field)
	{
		if (field != null)
		{
			control.NotBlank = control.NotBlank || field.NotBlank;
		}
	}

	internal static void SolveLookup(ControlDefinition control, MetaField field)
	{
		if (!control.LookupSpecified)
		{
			if (control.Triggers?.Find(FindLookup) != null)
			{
				control.Lookup = true;
			}
			else
			{
				control.Lookup = (field != null && field.Lookup) || (field?.Relations != null && field.Relations.Count != 0) || (control.TableRelations != null && control.TableRelations.Count != 0);
			}
		}
	}

	internal static void SolveMinMax(ControlDataboundDefinition control, MetaField field)
	{
		if (control.ExtendedDatatype == ExtendedDatatype.Ratio)
		{
			SolveProgressControl(control, field);
		}
		else if (field != null)
		{
			switch (field.Type)
			{
			case NavType.BigInteger:
				SolveBigIntegerMinMax(control, field);
				break;
			case NavType.Integer:
				SolveBigIntegerMinMax(control, field);
				break;
			case NavType.Decimal:
				SolveDecimalMinMax(control, field);
				break;
			case NavType.Option:
				SolveBigIntegerMinMax(control, field);
				break;
			case NavType.Time:
				SolveTimeMinMax(control, field);
				break;
			case NavType.Date:
				SolveDateTimeMinMax(control, field);
				break;
			case NavType.DateTime:
				SolveDateTimeMinMax(control, field);
				break;
			case NavType.Duration:
				SolveBigIntegerMinMax(control, field);
				break;
			}
		}
	}

	private static void SolveTimeMinMax(ControlDataboundDefinition control, MetaField field)
	{
		bool useControl;
		bool useField;
		if ((useControl = TimeSpan.TryParse(control.MinValue, out var result)) | (useField = TimeSpan.TryParse(field.MinValue, out var result2)))
		{
			control.MinValue = SolveGenericMinMax(result, result2, useControl, useField, lowBound: true).ToString();
		}
		if ((useControl = TimeSpan.TryParse(control.MaxValue, out result)) | (useField = TimeSpan.TryParse(field.MaxValue, out result2)))
		{
			control.MaxValue = SolveGenericMinMax(result, result2, useControl, useField, lowBound: false).ToString();
		}
	}

	private static void SolveDateTimeMinMax(ControlDataboundDefinition control, MetaField field)
	{
		bool useControl;
		bool useField;
		if ((useControl = DateTime.TryParse(control.MinValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)) | (useField = DateTime.TryParse(field.MinValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result2)))
		{
			control.MinValue = SolveGenericMinMax(result, result2, useControl, useField, lowBound: true).ToString(CultureInfo.InvariantCulture);
		}
		if ((useControl = DateTime.TryParse(control.MaxValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out result)) | (useField = DateTime.TryParse(field.MaxValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out result2)))
		{
			control.MaxValue = SolveGenericMinMax(result, result2, useControl, useField, lowBound: false).ToString(CultureInfo.InvariantCulture);
		}
	}

	private static T SolveGenericMinMax<T>(T controlValue, T fieldValue, bool useControl, bool useField, bool lowBound) where T : IComparable
	{
		if (!useField)
		{
			return controlValue;
		}
		if (!useControl)
		{
			return fieldValue;
		}
		if (lowBound)
		{
			if (controlValue.CompareTo(fieldValue) > 0)
			{
				return controlValue;
			}
			return fieldValue;
		}
		if (controlValue.CompareTo(fieldValue) < 0)
		{
			return controlValue;
		}
		return fieldValue;
	}

	internal static void SolveCaptionClass(ControlBaseDefinition control, MetaField field, MasterPage masterPage)
	{
		if (!string.IsNullOrEmpty(control.CaptionClass) || (field != null && !string.IsNullOrEmpty(field.CaptionClass)))
		{
			control.CaptionColumnName = GenerateCaptionColumnName(control.ID);
			AddMasterPageExpression(masterPage, control.CaptionColumnName);
		}
	}

	internal static void SolveDecimalMinMax(ControlDataboundDefinition control, MetaField field)
	{
		bool useControl;
		bool useField;
		if ((useControl = decimal.TryParse(control.MinValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)) | (useField = decimal.TryParse(field.MinValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2)))
		{
			control.MinValue = SolveGenericMinMax(result, result2, useControl, useField, lowBound: true).ToString(CultureInfo.InvariantCulture);
		}
		if ((useControl = decimal.TryParse(control.MaxValue, NumberStyles.Float, CultureInfo.InvariantCulture, out result)) | (useField = decimal.TryParse(field.MaxValue, NumberStyles.Float, CultureInfo.InvariantCulture, out result2)))
		{
			control.MaxValue = SolveGenericMinMax(result, result2, useControl, useField, lowBound: false).ToString(CultureInfo.InvariantCulture);
		}
	}

	internal static void SolveBigIntegerMinMax(ControlDataboundDefinition control, MetaField field)
	{
		bool useControl;
		bool useField;
		if ((useControl = long.TryParse(control.MinValue, out var result)) | (useField = long.TryParse(field.MinValue, out var result2)))
		{
			control.MinValue = SolveGenericMinMax(result, result2, useControl, useField, lowBound: true).ToString(CultureInfo.InvariantCulture);
		}
		if ((useControl = long.TryParse(control.MaxValue, out result)) | (useField = long.TryParse(field.MaxValue, out result2)))
		{
			control.MaxValue = SolveGenericMinMax(result, result2, useControl, useField, lowBound: false).ToString(CultureInfo.InvariantCulture);
		}
	}

	internal static void SolveDrillDown(ControlDefinition control, MetaField field)
	{
		if (!control.DrillDownSpecified)
		{
			if (control.Triggers != null && control.Triggers.Find(FindDrillDown) != null)
			{
				control.DrillDown = true;
			}
			else if (field != null && field.FieldClass == FieldClass.FlowField)
			{
				control.DrillDown = true;
			}
			else
			{
				control.DrillDown = control.DrillDownFormIDSpecified;
			}
		}
	}

	internal static void SolveAssistEdit(ControlDefinition control)
	{
		if (!control.AssistEditSpecified)
		{
			if (control.Triggers != null && control.Triggers.Find(FindAssistEdit) != null)
			{
				control.AssistEdit = true;
			}
			else
			{
				control.AssistEdit = false;
			}
		}
	}

	private static bool FindAssistEdit(ControlTriggerDefinition controlTriggerDefinition)
	{
		return controlTriggerDefinition.TriggerType == ControlTriggerType.OnAssistEdit;
	}

	private static bool FindDrillDown(ControlTriggerDefinition controlTriggerDefinition)
	{
		return controlTriggerDefinition.TriggerType == ControlTriggerType.OnDrillDown;
	}

	private static bool FindLookup(ControlTriggerDefinition controlTriggerDefinition)
	{
		return controlTriggerDefinition.TriggerType == ControlTriggerType.OnLookup;
	}

	internal static void SolveDropDown(ControlBaseDefinition control, MetaField field)
	{
		if (!control.DropDownSpecified && field != null)
		{
			control.DropDown = field.Type == NavType.Option;
		}
	}

	internal static void SolveExtendedDatatype(ControlDataboundDefinition control, MetaField field)
	{
		if (!control.ExtendedDatatypeSpecified && field != null)
		{
			control.ExtendedDatatype = field.ExtendedDatatype;
		}
	}

	internal static void SolveRelatedObjects(ControlDataboundDefinition control, MetaField field)
	{
		if (field != null && field.CalcFormula != null && field.CalcFormula.TableId != 0)
		{
			RelatedObjectDefinition relatedObjectDefinition = new RelatedObjectDefinition();
			relatedObjectDefinition.ObjectID = field.CalcFormula.TableId;
			relatedObjectDefinition.ObjectType = AccessByObjectType.TableData;
			relatedObjectDefinition.PermissionMask = 32;
			relatedObjectDefinition.RelatedBy = RelationType.CalcFormula;
			if (control.RelatedObjects == null)
			{
				control.RelatedObjects = new List<RelatedObjectDefinition>();
			}
			control.RelatedObjects.Add(relatedObjectDefinition);
		}
		if (field == null || field.Relations == null || field.Relations.Count <= 0 || (control.TableRelations != null && control.TableRelations.Count != 0))
		{
			return;
		}
		foreach (MetaFieldRelation relation in field.Relations)
		{
			RelatedObjectDefinition relatedObjectDefinition2 = new RelatedObjectDefinition();
			relatedObjectDefinition2.ObjectID = relation.TableId;
			relatedObjectDefinition2.ObjectType = AccessByObjectType.TableData;
			relatedObjectDefinition2.PermissionMask = 32;
			relatedObjectDefinition2.RelatedBy = RelationType.TableRelation;
			if (control.RelatedObjects == null)
			{
				control.RelatedObjects = new List<RelatedObjectDefinition>();
			}
			control.RelatedObjects.Add(relatedObjectDefinition2);
		}
	}

	internal static void SolveEditable(ControlDataboundDefinition control, MetaField field, MasterPage masterPage)
	{
		if (!control.SourceExpressionIsAssignable)
		{
			control.Editable = false.ToString(CultureInfo.InvariantCulture);
		}
		else if (control.Editable == null)
		{
			if (field != null)
			{
				control.Editable = field.Editable.ToString(CultureInfo.InvariantCulture);
			}
			else
			{
				control.Editable = "true";
			}
		}
		else if (!PropertyHelper.PropertyIsFalse(control.Editable) && masterPage != null && masterPage.PageProperties != null && !masterPage.PageProperties.Editable)
		{
			control.Editable = false.ToString(CultureInfo.InvariantCulture);
		}
	}

	internal static void SolveNumeric(ControlBaseDefinition control, MetaField field)
	{
		if (!control.NumericSpecified && field != null)
		{
			control.Numeric = field.Numeric;
		}
	}

	internal static void SolveValuesAllowed(ControlDefinition control, MetaField field)
	{
		if (string.IsNullOrEmpty(control.ValuesAllowed) && field != null)
		{
			control.ValuesAllowed = field.ValuesAllowed;
		}
	}

	internal static void SolveCharAllowed(ControlDefinition control, MetaField field)
	{
		if (string.IsNullOrEmpty(control.CharAllowed) && field != null)
		{
			control.CharAllowed = field.CharAllowed;
		}
	}

	internal static void SolveCaption(NamedElementDefinition control, MetaField field)
	{
		if (string.IsNullOrEmpty(control.Caption))
		{
			if (field != null && !string.IsNullOrEmpty(field.Caption))
			{
				control.Caption = field.Caption;
			}
			else
			{
				control.Caption = control.Name;
			}
		}
	}

	internal static void SolveName(NamedElementDefinition control, MetaField field)
	{
		if (!string.IsNullOrEmpty(control.Name))
		{
			control.CachedALIdentifier = control.Name;
		}
		else if (field != null)
		{
			control.Name = field.Name;
		}
		else
		{
			control.Name = string.Format(CultureInfo.InvariantCulture, "Control{0}", control.ID);
		}
	}

	private static bool FindAllControls(ElementDefinition element)
	{
		return element is ControlBaseDefinition;
	}

	private static string GenerateCaptionColumnName(int id)
	{
		return string.Format(CultureInfo.InvariantCulture, "Control{0}_DynamicCaption", id);
	}

	private static void AddMasterPageExpression(MasterPage masterPage, string captionColumnName)
	{
		DataFieldDefinition dataFieldDefinition = new DataFieldDefinition();
		dataFieldDefinition.Name = captionColumnName;
		dataFieldDefinition.Datatype = DataType.Text;
		if (masterPage.Expressions == null)
		{
			masterPage.Expressions = new List<DataFieldDefinition>();
		}
		masterPage.Expressions.Add(dataFieldDefinition);
	}

	internal static void SolveOptionString(ControlDefinition control, MetaField field)
	{
		string[] array = GetFieldOptionsArray(field);
		if (array != null)
		{
			if (!string.IsNullOrEmpty(control.OptionCaption))
			{
				array = OverrideOptionCaptions(array, control.OptionCaption.Split(','));
			}
			control.OptionCaption = string.Join(','.ToString(), array);
		}
	}

	internal static void SolveDataboundControlImage(ControlDataboundDefinition control)
	{
		if (control.Image == null || string.Compare(control.Image, "Stack", StringComparison.OrdinalIgnoreCase) == 0)
		{
			control.Image = string.Empty;
		}
		else if (string.Compare(control.Image, "None", StringComparison.OrdinalIgnoreCase) == 0)
		{
			control.Image = null;
		}
		else
		{
			control.Image = "Cue_" + control.Image + "_56x46";
		}
	}

	private static string[] GetFieldOptionsArray(MetaField field)
	{
		string[] array = null;
		if (field != null)
		{
			if (!string.IsNullOrEmpty(field.OptionString))
			{
				array = field.OptionString.Split(',');
			}
			if (!string.IsNullOrEmpty(field.OptionCaption))
			{
				string[] array2 = field.OptionCaption.Split(',');
				array = ((array != null) ? OverrideOptionCaptions(array, array2) : array2);
			}
		}
		return array;
	}

	private static string[] OverrideOptionCaptions(string[] target, string[] captions)
	{
		if (captions != null && captions.Length != 0)
		{
			for (int i = 0; i < target.Length; i++)
			{
				if (!string.IsNullOrEmpty(target[i]))
				{
					if (i >= captions.Length)
					{
						target[i] = string.Empty;
					}
					else
					{
						target[i] = captions[i];
					}
				}
			}
		}
		return target;
	}

	internal static bool SolveBoolean(string booleanText)
	{
		if (string.Compare(booleanText, "1", StringComparison.OrdinalIgnoreCase) != 0 && string.Compare(booleanText, "Yes", StringComparison.OrdinalIgnoreCase) != 0)
		{
			return string.Compare(booleanText, "True", StringComparison.OrdinalIgnoreCase) == 0;
		}
		return true;
	}

	public static void SolvePropertiesDefaulting(ElementDefinition element)
	{
		SolvePropertiesDefaultingControls(element.FindAll(FindAllControls), null, null);
	}
}
