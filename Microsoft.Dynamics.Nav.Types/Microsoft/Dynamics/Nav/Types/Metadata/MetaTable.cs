using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class MetaTable : MetaBase
{
	internal const string PropertyName = "Name";

	internal const string PropertyId = "ID";

	internal const string PropertyFields = "Fields";

	internal const string PropertyKeys = "Keys";

	internal const string PropertyFieldGroups = "FieldGroups";

	internal const string PropertyLookupPageId = "LookupPageID";

	internal const string PropertyDrillDownPageId = "DrillDownPageID";

	internal const string PropertyLookupFormId = "LookupFormID";

	internal const string PropertyDrillDownFormId = "DrillDownFormID";

	internal const string PropertyCaptionMultiLanguage = "CaptionML";

	internal const string PropertyDataCaptionFields = "DataCaptionFields";

	internal const string PropertyIsVirtual = "IsVirtual";

	internal const string PropertyDataPerCompany = "DataPerCompany";

	internal const string PropertyXmlns = "xmlns";

	internal const string PropertyIsLinked = "LinkedObject";

	internal const string PropertyTableType = "TableType";

	internal const string PropertyExternalName = "ExternalName";

	internal const string PropertyExternalSchema = "ExternalSchema";

	internal const string PropertyPasteIsValid = "PasteIsValid";

	internal const string PropertySourceAppId = "SourceAppId";

	internal const string PropertySourceExtensionType = "SourceExtensionType";

	internal const string PropertyObsoleteState = "ObsoleteState";

	internal const string PropertyObsoleteReason = "ObsoleteReason";

	internal const string PropertyDataClassification = "DataClassification";

	internal const string PropertyReplicateData = "ReplicateData";

	internal const string PropertyCompressionType = "CompressionType";

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private string name;

	private int id;

	private int metadataAppGroupId;

	private int languageAppGroupId;

	private int lookupFormId;

	private int drillDownFormId;

	private long versionNumber;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private string caption;

	private string mergedCaptionMultiLanguage;

	private string captionML;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private bool isVirtual;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private List<int> dataCaptionFields = new List<int>();

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private List<MetaKey> keys = new List<MetaKey>();

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private List<MetaField> fields = new List<MetaField>();

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private List<MetaFieldGroup> fieldGroups = new List<MetaFieldGroup>();

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private bool isDataPerCompany;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private bool isLinked;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private TableType tableType;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private string externalName;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private string externalSchema;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private bool pasteIsValid;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private string sourceAppId;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private int sourceExtensionType;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private ObsoleteState obsoleteState;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private string obsoleteReason;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private ALDataClassification dataClassification;

	internal bool SkipValidatingEvents;

	private Dictionary<int, MetaField> fieldsById = new Dictionary<int, MetaField>();

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private bool replicateData;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private CompressionType compressionType;

	public static Func<int, int, IEnumerable<int>> GetValidateEventSubscriptionsOnTable { get; set; }

	private Dictionary<int, MetaField> FieldsById => fieldsById;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public long VersionNumber
	{
		get
		{
			return versionNumber;
		}
		set
		{
			versionNumber = value;
		}
	}

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public long Timestamp { get; set; }

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public int LookupFormId
	{
		get
		{
			return lookupFormId;
		}
		set
		{
			lookupFormId = value;
		}
	}

	public List<int> DataCaptionFields
	{
		get
		{
			return dataCaptionFields;
		}
		set
		{
			dataCaptionFields = value;
		}
	}

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public int DrillDownFormId
	{
		get
		{
			return drillDownFormId;
		}
		set
		{
			drillDownFormId = value;
		}
	}

	[DefaultValue(0)]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public int Id
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}

	public int MetadataAppGroupId => metadataAppGroupId;

	public int LanguageAppGroupId => languageAppGroupId;

	[DefaultValue("")]
	[XmlAttribute]
	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	[DefaultValue(false)]
	[XmlAttribute]
	public bool IsVirtual
	{
		get
		{
			return isVirtual;
		}
		set
		{
			isVirtual = value;
		}
	}

	[DefaultValue("")]
	[XmlAttribute]
	public string Caption
	{
		get
		{
			return caption;
		}
		set
		{
			caption = value;
		}
	}

	[XmlIgnore]
	[XmlAttribute]
	public string MergedCaptionMultiLanguage => mergedCaptionMultiLanguage;

	[XmlIgnore]
	[XmlAttribute]
	public string CaptionML
	{
		get
		{
			return captionML;
		}
		set
		{
			captionML = value;
		}
	}

	[XmlAttribute]
	public bool IsDataPerCompany
	{
		get
		{
			return isDataPerCompany;
		}
		set
		{
			isDataPerCompany = value;
		}
	}

	[DefaultValue(true)]
	[XmlAttribute]
	public bool PasteIsValid
	{
		get
		{
			return pasteIsValid;
		}
		set
		{
			pasteIsValid = value;
		}
	}

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public string SourceAppId
	{
		get
		{
			return sourceAppId;
		}
		set
		{
			sourceAppId = value;
		}
	}

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public int SourceExtensionType
	{
		get
		{
			return sourceExtensionType;
		}
		set
		{
			sourceExtensionType = value;
		}
	}

	public List<MetaFieldGroup> FieldGroups => fieldGroups;

	public List<MetaKey> Keys => keys;

	public List<MetaField> Fields => fields;

	public bool IsLinked => isLinked;

	public TableType TableType
	{
		get
		{
			return tableType;
		}
		set
		{
			tableType = value;
		}
	}

	public string ExternalName
	{
		get
		{
			return externalName;
		}
		set
		{
			externalName = value;
		}
	}

	public string ExternalSchema => externalSchema;

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public ObsoleteState ObsoleteState
	{
		get
		{
			return obsoleteState;
		}
		set
		{
			obsoleteState = value;
		}
	}

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public string ObsoleteReason
	{
		get
		{
			return obsoleteReason;
		}
		set
		{
			obsoleteReason = value ?? string.Empty;
		}
	}

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public ALDataClassification DataClassification
	{
		get
		{
			return dataClassification;
		}
		set
		{
			dataClassification = value;
		}
	}

	[DefaultValue(false)]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public bool ReplicateData
	{
		get
		{
			return replicateData;
		}
		set
		{
			replicateData = value;
		}
	}

	[DefaultValue(CompressionType.Unspecified)]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public CompressionType CompressionType
	{
		get
		{
			return compressionType;
		}
		set
		{
			compressionType = value;
		}
	}

	public bool IsExternalDataSource
	{
		get
		{
			if (!IsLinked)
			{
				return TableType != TableType.Normal;
			}
			return true;
		}
	}

	private void Initialize(XmlNode root)
	{
		isDataPerCompany = true;
		pasteIsValid = true;
		foreach (XmlAttribute attribute in root.Attributes)
		{
			switch (attribute.Name)
			{
			case "Name":
				name = attribute.Value;
				break;
			case "ID":
				id = MetaBase.Int32Value(attribute.Value);
				break;
			case "LookupPageID":
			case "LookupFormID":
				lookupFormId = MetaBase.Int32Value(attribute.Value);
				break;
			case "DataCaptionFields":
			{
				string[] array = attribute.Value.Replace("Field", "").Split(new char[1] { ',' }, StringSplitOptions.None);
				for (int i = 0; i < array.Length; i++)
				{
					if (int.TryParse(array[i], out var result))
					{
						dataCaptionFields.Add(result);
					}
				}
				break;
			}
			case "DrillDownPageID":
			case "DrillDownFormID":
				drillDownFormId = MetaBase.Int32Value(attribute.Value);
				break;
			case "IsVirtual":
				isVirtual = MetaBase.BooleanValue(attribute.Value);
				break;
			case "CaptionML":
				captionML = attribute.Value;
				mergedCaptionMultiLanguage = attribute.Value;
				break;
			case "DataPerCompany":
				isDataPerCompany = MetaBase.BooleanValue(attribute.Value);
				break;
			case "LinkedObject":
				isLinked = MetaBase.Int32Value(attribute.Value) == 1;
				break;
			case "TableType":
				tableType = (TableType)Enum.Parse(typeof(TableType), attribute.Value);
				break;
			case "ExternalName":
				externalName = attribute.Value;
				break;
			case "ExternalSchema":
				externalSchema = attribute.Value;
				break;
			case "PasteIsValid":
				pasteIsValid = MetaBase.BooleanValue(attribute.Value);
				break;
			case "SourceAppId":
				sourceAppId = attribute.Value;
				break;
			case "SourceExtensionType":
				sourceExtensionType = MetaBase.Int32Value(attribute.Value);
				break;
			case "ObsoleteState":
				ObsoleteState = (ObsoleteState)Enum.Parse(typeof(ObsoleteState), attribute.Value);
				break;
			case "ObsoleteReason":
				ObsoleteReason = attribute.Value;
				break;
			case "DataClassification":
				DataClassification = (ALDataClassification)Enum.Parse(typeof(ALDataClassification), attribute.Value);
				break;
			case "ReplicateData":
				replicateData = MetaBase.BooleanValue(attribute.Value);
				break;
			case "CompressionType":
				CompressionType = (CompressionType)Enum.Parse(typeof(CompressionType), attribute.Value);
				break;
			}
		}
	}

	public override object Clone()
	{
		MetaTable metaTable = (MetaTable)base.Clone();
		if (DataCaptionFields != null)
		{
			metaTable.DataCaptionFields = new List<int>(DataCaptionFields);
		}
		if (Keys != null)
		{
			metaTable.keys = new List<MetaKey>();
			foreach (MetaKey key in Keys)
			{
				metaTable.Keys.Add((MetaKey)key.Clone());
			}
		}
		if (Fields != null)
		{
			metaTable.fields = new List<MetaField>();
			foreach (MetaField field in Fields)
			{
				metaTable.fields.Add((MetaField)field.Clone());
			}
		}
		if (FieldGroups != null)
		{
			metaTable.fieldGroups = new List<MetaFieldGroup>();
			foreach (MetaFieldGroup fieldGroup in FieldGroups)
			{
				metaTable.FieldGroups.Add((MetaFieldGroup)fieldGroup.Clone());
			}
		}
		metaTable.fieldsById = new Dictionary<int, MetaField>();
		return metaTable;
	}

	public MetaField GetFieldsById(int identifier)
	{
		bool flag = FieldsById.Count == Fields.Count;
		if (!flag)
		{
			fieldsById = Fields.ToDictionary((MetaField c) => c.Id, (MetaField c) => c);
		}
		fieldsById.TryGetValue(identifier, out var value);
		if ((value == null) & flag)
		{
			fieldsById = Fields.ToDictionary((MetaField c) => c.Id, (MetaField c) => c);
			fieldsById.TryGetValue(identifier, out value);
		}
		return value;
	}

	public static MetaTable CreateMetaTableFromXml(XmlNode root, int appGroupId = 0, long timestamp = 0L)
	{
		if (root == null)
		{
			throw new ArgumentNullException("root");
		}
		MetaTable metaTable = new MetaTable();
		metaTable.metadataAppGroupId = appGroupId;
		metaTable.languageAppGroupId = appGroupId;
		metaTable.Timestamp = timestamp;
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(root.OwnerDocument.NameTable);
		xmlNamespaceManager.AddNamespace("a", root.NamespaceURI);
		XmlNode xmlNode = root.SelectSingleNode("a:Fields", xmlNamespaceManager);
		if (xmlNode != null)
		{
			foreach (XmlNode childNode in xmlNode.ChildNodes)
			{
				try
				{
					MetaField item = new MetaField(childNode);
					metaTable.fields.Add(item);
				}
				catch (NavNCLNotSupportedTypeException innerException)
				{
					if (root.Attributes["ID"] != null)
					{
						metaTable.id = MetaBase.Int32Value(root.Attributes["ID"].Value);
						if (childNode != null)
						{
							XmlNode namedItem = childNode.Attributes.GetNamedItem("ID");
							if (namedItem != null && int.TryParse(namedItem.Value, out var result))
							{
								throw new NavNSInvalidTableDefinitionException(metaTable.id, result, innerException);
							}
						}
						throw new NavNSInvalidTableDefinitionException(metaTable.id, innerException);
					}
					throw;
				}
			}
		}
		xmlNode = root.SelectSingleNode("a:Keys", xmlNamespaceManager);
		if (xmlNode != null)
		{
			foreach (XmlNode childNode2 in xmlNode.ChildNodes)
			{
				metaTable.keys.Add(new MetaKey(childNode2, metaTable));
			}
		}
		xmlNode = root.SelectSingleNode("a:FieldGroups", xmlNamespaceManager);
		if (xmlNode != null)
		{
			foreach (XmlNode childNode3 in xmlNode.ChildNodes)
			{
				metaTable.fieldGroups.Add(new MetaFieldGroup(childNode3));
			}
		}
		metaTable.Initialize(root);
		metaTable.MergeMultiLanguageTexts(appGroupId, includeFields: true);
		return metaTable;
	}

	public void ApplyLanguage(string language, string primaryLanguage)
	{
		if (MergedCaptionMultiLanguage != null)
		{
			caption = MultiLanguage.GetLanguageValue(language, primaryLanguage, MergedCaptionMultiLanguage);
		}
		foreach (MetaField field in Fields)
		{
			field.ApplyLanguage(this, language, primaryLanguage);
		}
		foreach (MetaFieldGroup fieldGroup in FieldGroups)
		{
			fieldGroup.ApplyLanguage(language, primaryLanguage);
		}
	}

	internal void MergeMultiLanguageTexts(int appGroupId, bool includeFields, bool force = false)
	{
		languageAppGroupId = appGroupId;
		if (!force && !LanguageProvider.Provider.HasDataFor(ObjectType.Table, Id, appGroupId))
		{
			return;
		}
		mergedCaptionMultiLanguage = MultiLanguageTextMerge(captionML, 0, ObjectType.Table, Id, -1, appGroupId);
		if (!includeFields)
		{
			return;
		}
		foreach (MetaField field in Fields)
		{
			field.MergeMultiLanguageTexts(Id, appGroupId);
		}
		foreach (MetaFieldGroup fieldGroup in FieldGroups)
		{
			fieldGroup.MergeMultiLanguageTexts(this);
		}
	}

	internal static string MultiLanguageTextMerge(string attributeValue, int attributeId, ObjectType objType, int objId, int fieldId, int languageAppGroupId)
	{
		if (!LanguageProvider.Provider.HasDataFor(objType, objId, languageAppGroupId))
		{
			return attributeValue;
		}
		ControlType controlType = ControlType.Field;
		switch (objType)
		{
		case ObjectType.Query:
			controlType = ControlType.Column;
			break;
		case ObjectType.Report:
			controlType = ControlType.Label;
			break;
		}
		LanguageElements globalLanguageElements = LanguageProvider.Provider.GetGlobalLanguageElements(objType, objId, controlType, fieldId, attributeId, requestPage: false);
		LanguageElements navAppLanguageElements = LanguageProvider.Provider.GetNavAppLanguageElements(objType, objId, controlType, fieldId, attributeId, requestPage: false, languageAppGroupId);
		return ElementDefinition.MergeGlobalLanguageStrings(ElementDefinition.MergeAppLanguageStrings(attributeValue, navAppLanguageElements), globalLanguageElements);
	}

	[OnDeserialized]
	private void OnMetaTableDeserialized(StreamingContext context)
	{
		fieldsById = new Dictionary<int, MetaField>();
	}

	[OnSerializing]
	private void OnSerializing(StreamingContext context)
	{
		if (SkipValidatingEvents || TableType == TableType.Query || GetValidateEventSubscriptionsOnTable == null)
		{
			return;
		}
		foreach (int item in GetValidateEventSubscriptionsOnTable(Id, MetadataAppGroupId))
		{
			if (FieldsById.TryGetValue(item, out var value))
			{
				value.Validation = true;
			}
		}
	}

	public int CalcIndexFromKeyFields(string keyFields)
	{
		int num = 0;
		if (!string.IsNullOrEmpty(keyFields))
		{
			num = Keys.FindIndex((MetaKey key) => key.Name == keyFields);
			if (num < 0)
			{
				return num;
			}
		}
		return num + 1;
	}

	public static int[] GetKeyFieldIds(string keyFields)
	{
		if (string.IsNullOrEmpty(keyFields))
		{
			throw new ArgumentNullException("keyFields");
		}
		List<int> list = new List<int>();
		string[] array = keyFields.Split(',');
		foreach (string text in array)
		{
			if (int.TryParse(text.Remove(0, "Field".Length), out var _))
			{
				list.Add(int.Parse(text.Remove(0, "Field".Length), CultureInfo.InvariantCulture));
				continue;
			}
			throw new ArgumentOutOfRangeException("keyFields");
		}
		return list.ToArray();
	}
}
