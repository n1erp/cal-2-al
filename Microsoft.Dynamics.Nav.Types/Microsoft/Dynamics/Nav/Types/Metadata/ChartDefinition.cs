using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[GeneratedCode("xsd", "2.0.50727.42")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[XmlRoot(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects", IsNullable = false)]
public class ChartDefinition
{
	public const int UndefinedNumber = int.MinValue;

	public const string UndefinedString = "";

	public const bool UndefinedBoolean = false;

	[NonSerialized]
	private CultureInfo culture = CultureInfo.CurrentUICulture;

	private MultiLanguageTextDefinition[] titleField;

	private MultiLanguageTextDefinition[] descriptionField;

	private ChartTableDefinition tableField;

	private QueryDefinition queryField;

	private DimensionAxisDefinition xAxisField;

	private DimensionAxisDefinition zAxisField;

	private MeasureAxisDefinition yAxisField;

	private ChartType typeField;

	public bool HasXDimension
	{
		get
		{
			if (XAxis != null && XAxis.Field != null)
			{
				if (XAxis.Field.ID == 0)
				{
					return XAxis.Field.Name != null;
				}
				return true;
			}
			return false;
		}
	}

	private bool HasXDimensionTitle
	{
		get
		{
			if (XAxis != null)
			{
				return XAxis.Title != null;
			}
			return false;
		}
	}

	private bool HasXDimensionCaption
	{
		get
		{
			if (XAxis != null)
			{
				return XAxis.Caption != null;
			}
			return false;
		}
	}

	public bool HasZDimension
	{
		get
		{
			if (ZAxis != null && ZAxis.Field != null)
			{
				if (ZAxis.Field.ID == 0)
				{
					return ZAxis.Field.Name != null;
				}
				return true;
			}
			return false;
		}
	}

	private bool HasZDimensionTitle
	{
		get
		{
			if (ZAxis != null)
			{
				return ZAxis.Title != null;
			}
			return false;
		}
	}

	private bool HasZDimensionCaption
	{
		get
		{
			if (ZAxis != null)
			{
				return ZAxis.Caption != null;
			}
			return false;
		}
	}

	public bool HasYAxis => YAxis != null;

	private bool HasYAxisTitle
	{
		get
		{
			if (YAxis != null)
			{
				return YAxis.Title != null;
			}
			return false;
		}
	}

	public bool HasQuery => Query != null;

	public int QueryFilterCount => QueryFilters.Length;

	public bool HasTable => Table != null;

	public int TableFilterCount => TableFilters.Length;

	public int MeasureCount
	{
		get
		{
			if (YAxis == null || YAxis.Measures == null)
			{
				return 0;
			}
			return YAxis.Measures.Length;
		}
	}

	private ChartFilterDefinition[] TableFilters
	{
		get
		{
			if (Table == null || Table.Filters == null)
			{
				return new ChartFilterDefinition[0];
			}
			return Table.Filters;
		}
		set
		{
			InitTableDefinition();
			Table.Filters = value;
		}
	}

	private ChartFilterDefinition[] QueryFilters
	{
		get
		{
			if (Query == null || Query.Filters == null)
			{
				return new ChartFilterDefinition[0];
			}
			return Query.Filters;
		}
		set
		{
			InitQueryDefinition();
			Query.Filters = value;
		}
	}

	private MeasureDefinition[] Measures
	{
		get
		{
			if (YAxis != null)
			{
				return YAxis.Measures;
			}
			return null;
		}
		set
		{
			if (YAxis == null)
			{
				YAxis = new MeasureAxisDefinition();
			}
			YAxis.Measures = value;
		}
	}

	[XmlArrayItem("Text", IsNullable = false)]
	public MultiLanguageTextDefinition[] Title
	{
		get
		{
			return titleField;
		}
		set
		{
			titleField = value;
		}
	}

	[XmlArrayItem("Text", IsNullable = false)]
	public MultiLanguageTextDefinition[] Description
	{
		get
		{
			return descriptionField;
		}
		set
		{
			descriptionField = value;
		}
	}

	public ChartTableDefinition Table
	{
		get
		{
			return tableField;
		}
		set
		{
			tableField = value;
		}
	}

	public QueryDefinition Query
	{
		get
		{
			return queryField;
		}
		set
		{
			queryField = value;
		}
	}

	public DimensionAxisDefinition XAxis
	{
		get
		{
			return xAxisField;
		}
		set
		{
			xAxisField = value;
		}
	}

	public DimensionAxisDefinition ZAxis
	{
		get
		{
			return zAxisField;
		}
		set
		{
			zAxisField = value;
		}
	}

	public MeasureAxisDefinition YAxis
	{
		get
		{
			return yAxisField;
		}
		set
		{
			yAxisField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(ChartType.Column)]
	public ChartType Type
	{
		get
		{
			return typeField;
		}
		set
		{
			typeField = value;
		}
	}

	public static bool IsXmlSchemaValid(Stream chartDefinition)
	{
		return GetXmlSchemaErrors(chartDefinition).Length == 0;
	}

	public static StringBuilder GetXmlSchemaErrors(Stream chartDefinition)
	{
		if (chartDefinition.CanSeek)
		{
			chartDefinition.Seek(0L, SeekOrigin.Begin);
		}
		StringBuilder errors = new StringBuilder();
		XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		xmlReaderSettings.DtdProcessing = DtdProcessing.Prohibit;
		xmlReaderSettings.XmlResolver = null;
		xmlReaderSettings.ValidationType = ValidationType.Schema;
		xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
		xmlReaderSettings.ValidationEventHandler += (object sender, ValidationEventArgs e) =>
		{
			XmlSchemaException exception = e.Exception;
			errors.AppendFormat(CultureInfo.CurrentCulture, Lang.XmlSchemaError, exception.LineNumber, exception.LinePosition, exception.Message);
		};
		XmlSchemaSet xmlSchemaSet = new XmlSchemaSet();
		using (StringReader reader = new StringReader(NonLocalizableFiles.ChartMetadata))
		{
			XmlSchema schema = XmlSchema.Read(reader, null);
			xmlSchemaSet.Add(schema);
			xmlReaderSettings.Schemas = xmlSchemaSet;
		}
		using (XmlReader xmlReader = XmlReader.Create(chartDefinition, xmlReaderSettings))
		{
			try
			{
				while (xmlReader.Read())
				{
				}
			}
			catch (XmlException ex)
			{
				errors.AppendLine(ex.Message);
			}
		}
		if (chartDefinition.CanSeek)
		{
			chartDefinition.Seek(0L, SeekOrigin.Begin);
		}
		return errors;
	}

	public bool IsValid()
	{
		if (IsDefinitionValid())
		{
			return IsXmlSchemaValid();
		}
		return false;
	}

	public bool IsXmlSchemaValid()
	{
		using Stream chartDefinition = GetAsStream();
		return IsXmlSchemaValid(chartDefinition);
	}

	public bool IsDefinitionValid()
	{
		return ValidateChartDefinition(removeInvalidElements: false);
	}

	public void RemoveValidationErrors()
	{
		ValidateChartDefinition(removeInvalidElements: true);
	}

	public StringBuilder GetValidationErrors()
	{
		return GetChartDefinitionValidationErrorsInternal(removeInvalidElements: false);
	}

	public StringBuilder GetXmlSchemaErrors()
	{
		using Stream chartDefinition = GetAsStream();
		return GetXmlSchemaErrors(chartDefinition);
	}

	private bool ValidateChartDefinition(bool removeInvalidElements)
	{
		return GetChartDefinitionValidationErrorsInternal(removeInvalidElements).Length == 0;
	}

	private StringBuilder GetChartDefinitionValidationErrorsInternal(bool removeInvalidElements)
	{
		StringBuilder stringBuilder = new StringBuilder();
		HandleMeasureValidationErrorsInternal(removeInvalidElements, stringBuilder);
		HandleDimensionValidationErrorsInternal(removeInvalidElements, stringBuilder);
		return stringBuilder;
	}

	private void HandleDimensionValidationErrorsInternal(bool removeInvalidElements, StringBuilder stringBuilder)
	{
		if (XAxis == null || XAxis.Field == null || ZAxis == null || ZAxis.Field == null)
		{
			return;
		}
		int iD = XAxis.Field.ID;
		int iD2 = ZAxis.Field.ID;
		string name = XAxis.Field.Name;
		string name2 = ZAxis.Field.Name;
		bool flag = true;
		if (iD != 0 && iD2 != 0)
		{
			flag = iD != iD2;
		}
		else if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(name2))
		{
			flag = !name.Equals(name2);
		}
		if (!flag)
		{
			stringBuilder.AppendLine(Lang.ChartDefinitionDimensionsMustNotBeEqual);
			if (removeInvalidElements)
			{
				ZAxis.Field.ID = 0;
				ZAxis.Field.Name = null;
			}
		}
	}

	private void HandleMeasureValidationErrorsInternal(bool removeInvalidElements, StringBuilder stringBuilder)
	{
		if (MeasureCount <= 0)
		{
			return;
		}
		List<int> invalidMeasureDefintions = new List<int>();
		List<string> measureNames = new List<string>();
		List<int> measureIds = new List<int>();
		for (int i = 0; i < YAxis.Measures.Length; i++)
		{
			MeasureDefinition measureDefinition = YAxis.Measures[i];
			if (IsCountMeasureInvalidInternal(measureDefinition, removeInvalidElements, stringBuilder, i, invalidMeasureDefintions, HasTable))
			{
				if (!removeInvalidElements)
				{
					break;
				}
			}
			else if (IsMeasureNameInvalidInternal(measureDefinition, removeInvalidElements, stringBuilder, i, invalidMeasureDefintions, measureIds, measureNames, HasTable) && !removeInvalidElements)
			{
				break;
			}
		}
		UpdateMeasureDefinitionsFromErrorsFound(removeInvalidElements, invalidMeasureDefintions);
	}

	private static bool IsMeasureNameInvalidInternal(MeasureDefinition measureDefinition, bool removeInvalidElements, StringBuilder stringBuilder, int index, List<int> invalidMeasureDefintions, List<int> measureIds, List<string> measureNames, bool hasTable)
	{
		string text = string.Empty;
		int num = -1;
		if (measureDefinition.Field != null)
		{
			text = measureDefinition.Field.Name;
			num = measureDefinition.Field.ID;
		}
		if (num == 0 && string.IsNullOrEmpty(text) && !IsValidCountMeasure(measureDefinition, hasTable))
		{
			stringBuilder.AppendLine(Lang.ChartDefinitionFieldsMustBeSpecifiedForMeasures);
			if (!removeInvalidElements)
			{
				return true;
			}
			invalidMeasureDefintions.Add(index);
			return true;
		}
		bool num2 = measureIds.Contains(num) || measureNames.Contains(text);
		AddMeasureIdAndName(measureIds, measureNames, text, num);
		if (num2)
		{
			stringBuilder.AppendLine(Lang.ChartDefinitionAllFieldsMustBeUniqueForMeasures);
			if (!removeInvalidElements)
			{
				return true;
			}
			invalidMeasureDefintions.Add(index);
			return true;
		}
		return false;
	}

	private static void AddMeasureIdAndName(List<int> measureIds, List<string> measureNames, string name, int id)
	{
		if (id != 0)
		{
			measureIds.Add(id);
		}
		if (!string.IsNullOrEmpty(name))
		{
			measureNames.Add(name);
		}
	}

	private static bool IsCountMeasureInvalidInternal(MeasureDefinition measureDefinition, bool removeInvalidElements, StringBuilder stringBuilder, int index, List<int> invalidMeasureDefintions, bool hasTable)
	{
		if (measureDefinition.Operator != ChartMeasureOperator.Count)
		{
			return false;
		}
		if (IsValidCountMeasure(measureDefinition, hasTable))
		{
			return false;
		}
		stringBuilder.AppendLine(Lang.ChartDefinitionFieldsMustBeSpecifiedForMeasuresExceptCount);
		if (!removeInvalidElements)
		{
			return true;
		}
		invalidMeasureDefintions.Add(index);
		return true;
	}

	private static bool IsMeasureFieldDefined(MeasureDefinition measureDefinition)
	{
		if (measureDefinition.Field != null)
		{
			if (measureDefinition.Field.ID == 0)
			{
				return !string.IsNullOrEmpty(measureDefinition.Field.Name);
			}
			return true;
		}
		return false;
	}

	private static bool IsValidCountMeasure(MeasureDefinition measureDefinition, bool hasTable)
	{
		if (hasTable && measureDefinition.Operator == ChartMeasureOperator.Count)
		{
			return true;
		}
		if (!hasTable && measureDefinition.Operator == ChartMeasureOperator.Count && IsMeasureFieldDefined(measureDefinition))
		{
			return true;
		}
		return false;
	}

	private void UpdateMeasureDefinitionsFromErrorsFound(bool removeInvalidElements, List<int> invalidMeasureDefintions)
	{
		if (removeInvalidElements && invalidMeasureDefintions.Count > 0)
		{
			List<MeasureDefinition> measuresForModification = GetMeasuresForModification();
			for (int num = invalidMeasureDefintions.Count - 1; num >= 0; num--)
			{
				int index = invalidMeasureDefintions[num];
				measuresForModification.RemoveAt(index);
			}
			Measures = measuresForModification.ToArray();
		}
	}

	public void SetCurrentTitle(string value)
	{
		Title = SetValueAndGetMultilanguageDefinition(GetCulture(), Title, value);
	}

	public string GetCurrentTitle()
	{
		return GetMultilanguageDefinition(GetCulture(), Title);
	}

	public void SetCurrentDescription(string value)
	{
		Description = SetValueAndGetMultilanguageDefinition(GetCulture(), Description, value);
	}

	public string GetCurrentDescription()
	{
		return GetMultilanguageDefinition(GetCulture(), Description);
	}

	public void SetXDimensionName(string value)
	{
		InitXAxis();
		XAxis.Field.Name = value;
	}

	public string GetXDimensionName()
	{
		if (!HasXDimension)
		{
			return "";
		}
		return XAxis.Field.Name;
	}

	public void SetXDimensionId(int value)
	{
		InitXAxis();
		XAxis.Field.ID = value;
	}

	public int GetXDimensionId()
	{
		if (!HasXDimension)
		{
			return int.MinValue;
		}
		return XAxis.Field.ID;
	}

	public void SetXDimensionCaption(string value)
	{
		InitXAxis();
		XAxis.Caption = SetValueAndGetMultilanguageDefinition(GetCulture(), XAxis.Caption, value);
	}

	public string GetXDimensionCaption()
	{
		if (!HasXDimensionCaption)
		{
			return "";
		}
		return GetMultilanguageDefinition(GetCulture(), XAxis.Caption);
	}

	public void SetXDimensionMultilanguageCaption(MultiLanguageTextDefinition[] value)
	{
		InitXAxis();
		XAxis.Caption = value;
	}

	public MultiLanguageTextDefinition[] GetXDimensionMultilanguageCaption()
	{
		if (!HasXDimension)
		{
			InitXAxis();
		}
		if (!HasXDimensionCaption)
		{
			InitXAxis();
		}
		return XAxis.Caption;
	}

	public void SetXDimensionTitle(string value)
	{
		InitXAxis();
		XAxis.Title = SetValueAndGetMultilanguageDefinition(GetCulture(), XAxis.Title, value);
	}

	public string GetXDimensionTitle()
	{
		if (!HasXDimensionTitle)
		{
			return "";
		}
		return GetMultilanguageDefinition(GetCulture(), XAxis.Title);
	}

	public void SetXDimensionMultilanguageTitle(MultiLanguageTextDefinition[] value)
	{
		InitXAxis();
		XAxis.Title = value;
	}

	public MultiLanguageTextDefinition[] GetXDimensionMultilanguageTitle()
	{
		if (!HasXDimension)
		{
			InitXAxis();
		}
		if (!HasXDimensionTitle)
		{
			InitXAxis();
		}
		return XAxis.Title;
	}

	public void SetShowXDimensionTitle(bool value)
	{
		InitXAxis();
		XAxis.ShowTitle = value;
	}

	public bool GetShowXDimensionTitle()
	{
		if (!HasXDimension)
		{
			return false;
		}
		return XAxis.ShowTitle;
	}

	public void SetZDimensionName(string value)
	{
		InitZAxis();
		ZAxis.Field.Name = value;
	}

	public string GetZDimensionName()
	{
		if (!HasZDimension)
		{
			return "";
		}
		return ZAxis.Field.Name;
	}

	public void SetZDimensionId(int value)
	{
		InitZAxis();
		ZAxis.Field.ID = value;
	}

	public int GetZDimensionId()
	{
		if (!HasZDimension)
		{
			return int.MinValue;
		}
		return ZAxis.Field.ID;
	}

	public void SetZDimensionCaption(string value)
	{
		InitZAxis();
		ZAxis.Caption = SetValueAndGetMultilanguageDefinition(GetCulture(), ZAxis.Caption, value);
	}

	public string GetZDimensionCaption()
	{
		if (!HasZDimensionCaption)
		{
			return "";
		}
		return GetMultilanguageDefinition(GetCulture(), ZAxis.Caption);
	}

	public void SetZDimensionMultilanguageCaption(MultiLanguageTextDefinition[] value)
	{
		InitXAxis();
		ZAxis.Caption = value;
	}

	public MultiLanguageTextDefinition[] GetZDimensionMultilanguageCaption()
	{
		if (!HasZDimension)
		{
			InitZAxis();
		}
		if (!HasZDimensionCaption)
		{
			InitZAxis();
		}
		return ZAxis.Caption;
	}

	public void SetZDimensionTitle(string value)
	{
		InitZAxis();
		ZAxis.Title = SetValueAndGetMultilanguageDefinition(GetCulture(), ZAxis.Title, value);
	}

	public void SetZDimensionMultilanguageTitle(MultiLanguageTextDefinition[] value)
	{
		InitZAxis();
		ZAxis.Title = value;
	}

	public MultiLanguageTextDefinition[] GetZDimensionMultilanguageTitle()
	{
		if (!HasZDimension)
		{
			InitZAxis();
		}
		return ZAxis.Title;
	}

	public string GetZDimensionTitle()
	{
		if (!HasZDimensionTitle)
		{
			return "";
		}
		return GetMultilanguageDefinition(GetCulture(), ZAxis.Title);
	}

	public void SetShowZDimensionTitle(bool value)
	{
		InitZAxis();
		ZAxis.ShowTitle = value;
	}

	public bool GetShowZDimensionTitle()
	{
		if (!HasZDimension)
		{
			return false;
		}
		return ZAxis.ShowTitle;
	}

	public void SetYAxisTitle(string value)
	{
		InitYAxis();
		YAxis.Title = SetValueAndGetMultilanguageDefinition(GetCulture(), YAxis.Title, value);
	}

	public string GetYAxisTitle()
	{
		if (!HasYAxis)
		{
			return "";
		}
		return GetMultilanguageDefinition(GetCulture(), YAxis.Title);
	}

	public void SetYAxisMultilanguageTitle(MultiLanguageTextDefinition[] value)
	{
		InitYAxis();
		YAxis.Title = value;
	}

	public MultiLanguageTextDefinition[] GetYAxisMultilanguageTitle()
	{
		if (!HasYAxisTitle)
		{
			return null;
		}
		return YAxis.Title;
	}

	public void SetShowYAxisTitle(bool value)
	{
		InitYAxis();
		YAxis.ShowTitle = value;
	}

	public bool GetShowYAxisTitle()
	{
		if (!HasYAxis)
		{
			return false;
		}
		return YAxis.ShowTitle;
	}

	public void SetQueryId(int value)
	{
		InitQueryDefinition();
		Query.ID = value;
	}

	public int GetQueryId()
	{
		if (!HasQuery)
		{
			return int.MinValue;
		}
		return Query.ID;
	}

	public void SetQueryName(string value)
	{
		InitQueryDefinition();
		Query.Name = value;
	}

	public string GetQueryName()
	{
		if (!HasQuery)
		{
			return "";
		}
		return Query.Name;
	}

	public void ClearQueryFilters()
	{
		QueryFilters = new ChartFilterDefinition[0];
	}

	public void RemoveQueryFilter(int index)
	{
		List<ChartFilterDefinition> queryFiltersForModification = GetQueryFiltersForModification();
		if (queryFiltersForModification.Count > 0)
		{
			queryFiltersForModification.RemoveAt(index);
			QueryFilters = queryFiltersForModification.ToArray();
		}
	}

	public void AddQueryFilter(int id, string name, string value)
	{
		List<ChartFilterDefinition> queryFiltersForModification = GetQueryFiltersForModification();
		ChartFilterDefinition chartFilterDefinition = new ChartFilterDefinition();
		chartFilterDefinition.Field = new ChartFieldDefinition();
		chartFilterDefinition.Field.ID = id;
		chartFilterDefinition.Field.Name = name;
		chartFilterDefinition.Value = value;
		queryFiltersForModification.Add(chartFilterDefinition);
		QueryFilters = queryFiltersForModification.ToArray();
	}

	public int GetQueryFilterFieldId(int index)
	{
		if (QueryFilters.Length != 0)
		{
			return QueryFilters[index].Field.ID;
		}
		return int.MinValue;
	}

	public string GetQueryFilterFieldName(int index)
	{
		if (QueryFilters.Length != 0)
		{
			return QueryFilters[index].Field.Name;
		}
		return "";
	}

	public string GetQueryFilterValue(int index)
	{
		if (QueryFilters.Length != 0)
		{
			return QueryFilters[index].Value;
		}
		return "";
	}

	public void SetQueryFilterFieldId(int index, int id)
	{
		if (QueryFilters.Length != 0)
		{
			QueryFilters[index].Field.ID = id;
		}
	}

	public void SetQueryFilterFieldName(int index, string name)
	{
		if (QueryFilters.Length != 0)
		{
			QueryFilters[index].Field.Name = name;
		}
	}

	public void SetQueryFilterValue(int index, string value)
	{
		if (QueryFilters.Length != 0)
		{
			QueryFilters[index].Value = value;
		}
	}

	public void SetTableId(int value)
	{
		InitTableDefinition();
		Table.ID = value;
	}

	public int GetTableId()
	{
		if (!HasTable)
		{
			return int.MinValue;
		}
		return Table.ID;
	}

	public void SetTableName(string value)
	{
		InitTableDefinition();
		Table.Name = value;
	}

	public string GetTableName()
	{
		if (!HasTable)
		{
			return "";
		}
		return Table.Name;
	}

	public void ClearTableFilters()
	{
		TableFilters = new ChartFilterDefinition[0];
	}

	public void RemoveTableFilter(int index)
	{
		List<ChartFilterDefinition> tableFiltersForModification = GetTableFiltersForModification();
		if (tableFiltersForModification.Count > 0)
		{
			tableFiltersForModification.RemoveAt(index);
			TableFilters = tableFiltersForModification.ToArray();
		}
	}

	public void AddTableFilter(int id, string name, string value)
	{
		List<ChartFilterDefinition> tableFiltersForModification = GetTableFiltersForModification();
		ChartFilterDefinition chartFilterDefinition = new ChartFilterDefinition();
		chartFilterDefinition.Field = new ChartFieldDefinition();
		chartFilterDefinition.Field.ID = id;
		chartFilterDefinition.Field.Name = name;
		chartFilterDefinition.Value = value;
		tableFiltersForModification.Add(chartFilterDefinition);
		TableFilters = tableFiltersForModification.ToArray();
	}

	public int GetTableFilterFieldId(int index)
	{
		if (TableFilters.Length != 0)
		{
			return TableFilters[index].Field.ID;
		}
		return int.MinValue;
	}

	public string GetTableFilterFieldName(int index)
	{
		if (TableFilters.Length != 0)
		{
			return TableFilters[index].Field.Name;
		}
		return "";
	}

	public string GetTableFilterValue(int index)
	{
		if (TableFilters.Length != 0)
		{
			return TableFilters[index].Value;
		}
		return "";
	}

	public void SetTableFilterFieldId(int index, int id)
	{
		if (TableFilters.Length != 0)
		{
			TableFilters[index].Field.ID = id;
		}
	}

	public void SetTableFilterFieldName(int index, string name)
	{
		if (TableFilters.Length != 0)
		{
			TableFilters[index].Field.Name = name;
		}
	}

	public void SetTableFilterValue(int index, string value)
	{
		if (TableFilters.Length != 0)
		{
			TableFilters[index].Value = value;
		}
	}

	public void AddMeasure(ChartType chartType, ChartMeasureOperator chartMeasureOperator)
	{
		MeasureDefinition item = new MeasureDefinition
		{
			SeriesChartType = chartType,
			Operator = chartMeasureOperator
		};
		List<MeasureDefinition> measuresForModification = GetMeasuresForModification();
		measuresForModification.Add(item);
		Measures = measuresForModification.ToArray();
	}

	public void AddMeasure(int id, string name, ChartType chartType, ChartMeasureOperator chartMeasureOperator)
	{
		MeasureDefinition item = new MeasureDefinition
		{
			Field = new ChartFieldDefinition
			{
				ID = id,
				Name = name
			},
			SeriesChartType = chartType,
			Operator = chartMeasureOperator
		};
		List<MeasureDefinition> measuresForModification = GetMeasuresForModification();
		measuresForModification.Add(item);
		Measures = measuresForModification.ToArray();
	}

	public void AddMeasure(int id, string name, string caption, ChartType chartType, ChartMeasureOperator chartMeasureOperator)
	{
		MeasureDefinition item = new MeasureDefinition
		{
			Field = new ChartFieldDefinition
			{
				ID = id,
				Name = name
			},
			Caption = SetValueAndGetMultilanguageDefinition(GetCulture(), null, caption),
			SeriesChartType = chartType,
			Operator = chartMeasureOperator
		};
		List<MeasureDefinition> measuresForModification = GetMeasuresForModification();
		measuresForModification.Add(item);
		Measures = measuresForModification.ToArray();
	}

	public void AddMeasure(int id, string name, MultiLanguageTextDefinition[] caption, ChartType chartType, ChartMeasureOperator chartMeasureOperator)
	{
		MeasureDefinition item = new MeasureDefinition
		{
			Field = new ChartFieldDefinition
			{
				ID = id,
				Name = name
			},
			Caption = caption,
			SeriesChartType = chartType,
			Operator = chartMeasureOperator
		};
		List<MeasureDefinition> measuresForModification = GetMeasuresForModification();
		measuresForModification.Add(item);
		Measures = measuresForModification.ToArray();
	}

	public void RemoveMeasure(int index)
	{
		List<MeasureDefinition> measuresForModification = GetMeasuresForModification();
		measuresForModification.RemoveAt(index);
		Measures = measuresForModification.ToArray();
	}

	public void ClearMeasures()
	{
		Measures = new MeasureDefinition[0];
	}

	public bool HasMeasureField(int index)
	{
		if (YAxis != null && YAxis.Measures != null && YAxis.Measures.Length > index)
		{
			return YAxis.Measures[index].Field != null;
		}
		return false;
	}

	public string GetMeasureName(int index)
	{
		if (!HasMeasureField(index))
		{
			return "";
		}
		return YAxis.Measures[index].Field.Name;
	}

	public string GetMeasureCaption(int index)
	{
		if (!HasMeasureField(index))
		{
			return "";
		}
		return GetMultilanguageDefinition(GetCulture(), YAxis.Measures[index].Caption);
	}

	public MultiLanguageTextDefinition[] GetMultilanguageMeasureCaption(int index)
	{
		if (!HasMeasureField(index))
		{
			return null;
		}
		return YAxis.Measures[index].Caption;
	}

	public void SetMeasureCaption(int index, string caption)
	{
		if (HasMeasureField(index))
		{
			YAxis.Measures[index].Caption = SetValueAndGetMultilanguageDefinition(GetCulture(), YAxis.Measures[index].Caption, caption);
		}
	}

	public void SetMultilanguageMeasureCaption(int index, MultiLanguageTextDefinition[] caption)
	{
		if (HasMeasureField(index))
		{
			YAxis.Measures[index].Caption = caption;
		}
	}

	public void ModifyMeasure(int index, int id, string name, string caption, ChartType chartType, ChartMeasureOperator chartMeasureOperator)
	{
		List<MeasureDefinition> measuresForModification = GetMeasuresForModification();
		if (measuresForModification.Count == 0 || index >= measuresForModification.Count)
		{
			throw new ArgumentException(Lang.ExceptionMeasureIndexDoesNotExist, "index");
		}
		MeasureDefinition measureDefinition = measuresForModification[index];
		measureDefinition.Field = new ChartFieldDefinition
		{
			ID = id,
			Name = name
		};
		measureDefinition.Caption = SetValueAndGetMultilanguageDefinition(GetCulture(), measureDefinition.Caption, caption);
		measureDefinition.SeriesChartType = chartType;
		measureDefinition.Operator = chartMeasureOperator;
		Measures = measuresForModification.ToArray();
	}

	public void ModifyMeasure(int index, int id, string name, MultiLanguageTextDefinition[] caption, ChartType chartType, ChartMeasureOperator chartMeasureOperator)
	{
		List<MeasureDefinition> measuresForModification = GetMeasuresForModification();
		if (measuresForModification.Count == 0 || index >= measuresForModification.Count)
		{
			throw new ArgumentException(Lang.ExceptionMeasureIndexDoesNotExist, "index");
		}
		MeasureDefinition measureDefinition = measuresForModification[index];
		measureDefinition.Field = new ChartFieldDefinition
		{
			ID = id,
			Name = name
		};
		measureDefinition.Caption = caption;
		measureDefinition.SeriesChartType = chartType;
		measureDefinition.Operator = chartMeasureOperator;
		Measures = measuresForModification.ToArray();
	}

	public int GetMeasureId(int index)
	{
		if (!HasMeasureField(index))
		{
			return int.MinValue;
		}
		return YAxis.Measures[index].Field.ID;
	}

	public ChartType GetMeasureChartType(int index)
	{
		return YAxis.Measures[index].SeriesChartType;
	}

	public ChartMeasureOperator GetMeasureOperator(int index)
	{
		return YAxis.Measures[index].Operator;
	}

	public ChartDefinition Clone()
	{
		return FromBlob(GetAsBlob());
	}

	public byte[] GetAsBlob()
	{
		using MemoryStream memoryStream = MemoryStreamManager.GetStream();
		new XmlSerializer(typeof(ChartDefinition)).Serialize(memoryStream, this);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		return memoryStream.ToArray();
	}

	public Stream GetAsStream()
	{
		MemoryStream stream = MemoryStreamManager.GetStream();
		StreamWriter textWriter = new StreamWriter(stream, Encoding.UTF8);
		new XmlSerializer(typeof(ChartDefinition)).Serialize(textWriter, this);
		stream.Seek(0L, SeekOrigin.Begin);
		return stream;
	}

	public string GetAsXml()
	{
		StringBuilder stringBuilder = new StringBuilder();
		using StringWriter stringWriter = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);
		new XmlSerializer(typeof(ChartDefinition)).Serialize(stringWriter, this);
		stringWriter.Close();
		return stringBuilder.ToString();
	}

	public static ChartDefinition FromBlob(byte[] blob)
	{
		if (blob == null || blob.Length == 0)
		{
			return null;
		}
		using MemoryStream stream = MemoryStreamManager.GetStream(blob);
		return (ChartDefinition)new XmlSerializer(typeof(ChartDefinition)).Deserialize(stream);
	}

	public static ChartDefinition FromAsciiXml(string xml)
	{
		if (string.IsNullOrEmpty(xml))
		{
			return Empty();
		}
		return FromBlob(new ASCIIEncoding().GetBytes(xml));
	}

	public static ChartDefinition Empty()
	{
		ChartDefinition chartDefinition = new ChartDefinition();
		chartDefinition.InitYAxis();
		return chartDefinition;
	}

	private List<ChartFilterDefinition> GetTableFiltersForModification()
	{
		return new List<ChartFilterDefinition>(TableFilters);
	}

	private List<ChartFilterDefinition> GetQueryFiltersForModification()
	{
		return new List<ChartFilterDefinition>(QueryFilters);
	}

	private List<MeasureDefinition> GetMeasuresForModification()
	{
		if (Measures == null)
		{
			return new List<MeasureDefinition>();
		}
		return new List<MeasureDefinition>(Measures);
	}

	private void InitYAxis()
	{
		if (YAxis == null)
		{
			YAxis = new MeasureAxisDefinition();
		}
	}

	private void InitXAxis()
	{
		if (XAxis == null)
		{
			XAxis = new DimensionAxisDefinition();
		}
		if (XAxis.Field == null)
		{
			XAxis.Field = new ChartFieldDefinition();
		}
	}

	private void InitZAxis()
	{
		if (ZAxis == null)
		{
			ZAxis = new DimensionAxisDefinition();
		}
		if (ZAxis.Field == null)
		{
			ZAxis.Field = new ChartFieldDefinition();
		}
	}

	private void InitTableDefinition()
	{
		if (Table == null)
		{
			Table = new ChartTableDefinition();
		}
		if (Table.Filters == null)
		{
			Table.Filters = new ChartFilterDefinition[0];
		}
	}

	private void InitQueryDefinition()
	{
		if (Query == null)
		{
			Query = new QueryDefinition();
		}
		if (Query.Filters == null)
		{
			Query.Filters = new ChartFilterDefinition[0];
		}
	}

	private static string GetMultilanguageDefinition(CultureInfo culture, MultiLanguageTextDefinition[] multiLanguageTextDefinitions)
	{
		if (multiLanguageTextDefinitions == null || multiLanguageTextDefinitions.Length == 0)
		{
			return "";
		}
		CultureInfo cultureInfo = culture.Clone() as CultureInfo;
		string text;
		while (true)
		{
			text = MultiLanguageTextDefinition.GetText(multiLanguageTextDefinitions, cultureInfo);
			if (text != null)
			{
				break;
			}
			CultureInfo parent = cultureInfo.Parent;
			if (parent == null || parent == cultureInfo)
			{
				break;
			}
			cultureInfo = parent;
		}
		if (text != null)
		{
			return text;
		}
		text = MultiLanguageTextDefinition.GetText(multiLanguageTextDefinitions, CultureInfo.InvariantCulture);
		if (text != null)
		{
			return text;
		}
		return "";
	}

	private static MultiLanguageTextDefinition[] SetValueAndGetMultilanguageDefinition(CultureInfo culture, MultiLanguageTextDefinition[] existingValues, string value)
	{
		if (existingValues == null)
		{
			existingValues = new MultiLanguageTextDefinition[0];
		}
		MultiLanguageTextDefinition.SetText(ref existingValues, culture, value);
		return existingValues;
	}

	public void SetCulture(CultureInfo value)
	{
		culture = value;
	}

	public CultureInfo GetCulture()
	{
		return culture;
	}

	public ChartDefinition()
	{
		typeField = ChartType.Column;
	}
}
