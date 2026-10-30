using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class MetaField : MetaBase
{
	public const char OptionsSeparator = ',';

	private const string PropertyFieldClass = "FieldClass";

	internal const string PropertyId = "ID";

	private const string PropertyLookup = "OnLookup";

	private const string PropertyCaption = "Caption";

	private const string PropertyCaptionMultiLanguage = "CaptionML";

	private const string PropertyName = "Name";

	private const string PropertyLength = "DataLength";

	private const string PropertyClosingDates = "ClosingDates";

	private const string PropertyDateFormula = "DateFormula";

	private const string PropertyOptionString = "OptionString";

	private const string PropertyOptionCaptionMultiLanguage = "OptionCaptionML";

	private const string PropertyOptionValues = "OptionValues";

	private const string PropertyValidation = "OnValidate";

	private const string PropertyType = "Datatype";

	private const string PropertyCalcFormula = "CalculationFormula";

	private const string PropertyValidateTableRelation = "ValidateTableRelation";

	private const string PropertyExtendedDatatype = "ExtendedDatatype";

	private const string PropertyBlobSubType = "SubType";

	private const string PropertyFilterTableID = "FilterTableID";

	private const string PropertyMetaFieldRelation = "TableRelations";

	private const string PropertyEditable = "Editable";

	private const string PropertyMinValue = "MinValue";

	private const string PropertyMaxValue = "MaxValue";

	private const string PropertyBlankNumbers = "BlankNumbers";

	private const string PropertyNotBlank = "NotBlank";

	private const string PropertyBlankZero = "BlankZero";

	private const string PropertyCharAllowed = "CharAllowed";

	private const string PropertyValuesAllowed = "ValuesAllowed";

	private const string PropertyNumeric = "Numeric";

	private const string PropertyTitle = "Title";

	private const string PropertyInitValue = "InitValue";

	private const string PropertyCaptionClassText = "CaptionClassText";

	private const string PropertyCaptionClass = "CaptionClass";

	private const string PropertyCaptionClassCode = "CaptionClassCode";

	private const string PropertySignDisplacement = "SignDisplacement";

	private const string PropertyEnabled = "Enabled";

	private const string PropertyWidth = "Width";

	private const string PropertyTestTableRelation = "TestTableRelation";

	private const string PropertyAutoIncrement = "AutoIncrement";

	private const string PropertyTimestamp = "SQL_Timestamp";

	private const string PropertyCompressed = "Compressed";

	private const string PropertySqlDataType = "SQL_Data_Type";

	private const string PropertyAccessByObjectType = "AccessByObjectType";

	private const string PropertyAccessByObjectID = "AccessByObjectID";

	private const string PropertyAccessByPermissionMask = "AccessByPermissionMask";

	private const string PropertyExternalName = "ExternalName";

	private const string PropertyExternalType = "ExternalType";

	private const string PropertyExternalAccess = "ExternalAccess";

	private const string PropertyOptionOrdinalValues = "OptionOrdinalValues";

	private const string PropertySourceAppId = "SourceAppId";

	private const string PropertySourceExtensionType = "SourceExtensionType";

	private const string PropertyObsoleteState = "ObsoleteState";

	private const string PropertyObsoleteReason = "ObsoleteReason";

	private const string PropertyDataClassification = "DataClassification";

	private const string PropertyEnumTypeName = "EnumTypeName";

	private const string PropertyEnumTypeId = "EnumTypeId";

	private const string PropertyEnumExtensible = "Extensible";

	private FieldClass fieldClass;

	private int id;

	private bool lookup;

	private NavType type;

	private string name;

	private string caption;

	private string mergedCaptionMultiLanguage;

	private string captionML;

	private int length;

	private bool closingDates;

	private bool dateFormula;

	private string dataColumnName;

	private string clrType;

	private bool validation;

	private MetaCalcFormula calcFormula;

	private ExtendedDatatype extendedDatatype;

	private BlobSubType blobSubType;

	private int filterTableID;

	private bool editable = true;

	private static readonly ConcurrentDictionary<string, ExtendedDatatype> extendedDatatypesTable = new ConcurrentDictionary<string, ExtendedDatatype>(new Dictionary<string, ExtendedDatatype>
	{
		{
			"None",
			ExtendedDatatype.None
		},
		{
			"E-Mail",
			ExtendedDatatype.EMail
		},
		{
			"Phone No.",
			ExtendedDatatype.PhoneNo
		},
		{
			"URL",
			ExtendedDatatype.URL
		},
		{
			"Ratio",
			ExtendedDatatype.Ratio
		},
		{
			"Masked",
			ExtendedDatatype.Masked
		},
		{
			"Person",
			ExtendedDatatype.Person
		},
		{
			"Resource",
			ExtendedDatatype.Resource
		}
	});

	private string optionCaption;

	private string mergedOptionCaptionML;

	private string optionCaptionML;

	private string optionString;

	private List<int> optionValues;

	private string minValue;

	private string maxValue;

	private BlankNumbers blankNumbers;

	private bool blankZero;

	private bool notBlank;

	private string charAllowed;

	private string valuesAllowed;

	private bool numeric;

	private bool title;

	private string initValue;

	private string captionClass;

	private string signDisplacement;

	private bool enabled = true;

	private int width;

	private static readonly ConcurrentDictionary<string, AccessByObjectType> accessByObjectTable = new ConcurrentDictionary<string, AccessByObjectType>(new Dictionary<string, AccessByObjectType>
	{
		{
			"None",
			AccessByObjectType.None
		},
		{
			"Page",
			AccessByObjectType.Page
		},
		{
			"Report",
			AccessByObjectType.Report
		},
		{
			"Codeunit",
			AccessByObjectType.Codeunit
		},
		{
			"XMLport",
			AccessByObjectType.XMLport
		},
		{
			"TableData",
			AccessByObjectType.TableData
		},
		{
			"Table",
			AccessByObjectType.Table
		},
		{
			"System",
			AccessByObjectType.System
		},
		{
			"PageExtension",
			AccessByObjectType.PageExtension
		},
		{
			"TableExtension",
			AccessByObjectType.TableExtension
		}
	});

	private AccessByObjectType accessByObjectType;

	private int accessByObjectID;

	private int accessByPermissionMask;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private bool autoIncrement;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private bool timestamp;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private string sqlDataType;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private bool compressed = true;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private List<MetaFieldRelation> relations = new List<MetaFieldRelation>();

	private bool validateRelations;

	private string sourceAppId;

	private int sourceExtensionType;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private bool testRelations;

	private string obsoleteReason;

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

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public string OptionString
	{
		get
		{
			return optionString;
		}
		set
		{
			optionString = value;
		}
	}

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public string OptionCaption
	{
		get
		{
			return optionCaption;
		}
		set
		{
			optionCaption = value;
		}
	}

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public List<int> OptionValues
	{
		get
		{
			if (optionValues == null || optionValues.Count == 0)
			{
				optionValues = GenerateOptionValues(optionString);
			}
			return optionValues;
		}
		set
		{
			optionValues = value;
		}
	}

	[XmlIgnore]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public string MergedOptionCaptionML
	{
		get
		{
			return mergedOptionCaptionML;
		}
		set
		{
			mergedOptionCaptionML = value;
		}
	}

	[XmlIgnore]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public string OptionCaptionML
	{
		get
		{
			return optionCaptionML;
		}
		set
		{
			optionCaptionML = value;
		}
	}

	[DefaultValue(0)]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public FieldClass FieldClass
	{
		get
		{
			return fieldClass;
		}
		set
		{
			fieldClass = value;
		}
	}

	[DefaultValue(false)]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public bool Lookup
	{
		get
		{
			return lookup;
		}
		set
		{
			lookup = value;
		}
	}

	[DefaultValue(true)]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public bool Editable
	{
		get
		{
			return editable;
		}
		set
		{
			editable = value;
		}
	}

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public string MinValue
	{
		get
		{
			return minValue;
		}
		set
		{
			minValue = value;
		}
	}

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public string MaxValue
	{
		get
		{
			return maxValue;
		}
		set
		{
			maxValue = value;
		}
	}

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public string InitValue
	{
		get
		{
			return initValue;
		}
		set
		{
			initValue = value;
		}
	}

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public BlankNumbers BlankNumbers
	{
		get
		{
			return blankNumbers;
		}
		set
		{
			blankNumbers = value;
		}
	}

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public bool NotBlank
	{
		get
		{
			return notBlank;
		}
		set
		{
			notBlank = value;
		}
	}

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public bool BlankZero
	{
		get
		{
			return blankZero;
		}
		set
		{
			blankZero = value;
		}
	}

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public string CharAllowed
	{
		get
		{
			return charAllowed;
		}
		set
		{
			charAllowed = value;
		}
	}

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public string ValuesAllowed
	{
		get
		{
			return valuesAllowed;
		}
		set
		{
			valuesAllowed = value;
		}
	}

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public string CaptionClass
	{
		get
		{
			return captionClass;
		}
		set
		{
			captionClass = value;
		}
	}

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public string SignDisplacement
	{
		get
		{
			return signDisplacement;
		}
		set
		{
			signDisplacement = value;
		}
	}

	[DefaultValue(true)]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public bool Enabled
	{
		get
		{
			return enabled;
		}
		set
		{
			enabled = value;
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

	public bool AutoIncrement
	{
		get
		{
			return autoIncrement;
		}
		set
		{
			autoIncrement = value;
		}
	}

	public bool Timestamp => timestamp;

	public string SqlDataType => sqlDataType;

	public bool Compressed => compressed;

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public bool Numeric
	{
		get
		{
			return numeric;
		}
		set
		{
			numeric = value;
		}
	}

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public bool Title
	{
		get
		{
			return title;
		}
		set
		{
			title = value;
		}
	}

	public List<MetaFieldRelation> Relations
	{
		get
		{
			return relations;
		}
		set
		{
			relations = value;
		}
	}

	public AccessByObjectType AccessByObjectType => accessByObjectType;

	public int AccessByObjectID => accessByObjectID;

	public int AccessByPermissionMask => accessByPermissionMask;

	public bool ValidateRelation => validateRelations;

	public bool TestRelation => testRelations;

	[DefaultValue("")]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public string ClrType
	{
		get
		{
			return clrType;
		}
		set
		{
			clrType = value;
		}
	}

	[DefaultValue(0)]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public NavType Type
	{
		get
		{
			return type;
		}
		set
		{
			type = value;
		}
	}

	[DefaultValue("")]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
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

	[DefaultValue("")]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
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

	[DefaultValue("")]
	[XmlAttribute]
	public string DataColumnName
	{
		get
		{
			if (dataColumnName == null)
			{
				dataColumnName = Id.ToString(CultureInfo.InvariantCulture);
			}
			return dataColumnName;
		}
	}

	[XmlIgnore]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public string MergedCaptionMultiLanguage
	{
		get
		{
			return mergedCaptionMultiLanguage;
		}
		set
		{
			mergedCaptionMultiLanguage = value;
		}
	}

	[XmlIgnore]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
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

	[DefaultValue(false)]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public bool ClosingDates
	{
		get
		{
			return closingDates;
		}
		set
		{
			closingDates = value;
		}
	}

	[DefaultValue(false)]
	[XmlAttribute]
	public bool DateFormula
	{
		get
		{
			return dateFormula;
		}
		set
		{
			dateFormula = value;
		}
	}

	[DefaultValue(0)]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public int Length
	{
		get
		{
			return length;
		}
		set
		{
			length = value;
		}
	}

	[DefaultValue(false)]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public bool Validation
	{
		get
		{
			return validation;
		}
		set
		{
			validation = value;
		}
	}

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public MetaCalcFormula CalcFormula
	{
		get
		{
			return calcFormula;
		}
		set
		{
			calcFormula = value;
		}
	}

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public int FilterTableID
	{
		get
		{
			return filterTableID;
		}
		set
		{
			filterTableID = value;
		}
	}

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public BlobSubType BlobSubType
	{
		get
		{
			return blobSubType;
		}
		set
		{
			blobSubType = value;
		}
	}

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public ExtendedDatatype ExtendedDatatype
	{
		get
		{
			return extendedDatatype;
		}
		set
		{
			extendedDatatype = value;
		}
	}

	[DefaultValue(0)]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public int Width
	{
		get
		{
			return width;
		}
		set
		{
			width = value;
		}
	}

	[DefaultValue("")]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public string ExternalName { get; set; }

	[DefaultValue("")]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public string ExternalType { get; set; }

	[DefaultValue(0)]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public ExternalAccess ExternalAccess { get; set; }

	[DefaultValue(null)]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public int[] OptionOrdinalValues { get; set; }

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public ObsoleteState ObsoleteState { get; set; }

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
	public ALDataClassification DataClassification { get; set; }

	[DefaultValue("")]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public string EnumTypeName { get; set; }

	[DefaultValue(0)]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public int EnumTypeId { get; set; }

	[DefaultValue(false)]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public bool Extensible { get; set; }

	public override object Clone()
	{
		MetaField metaField = (MetaField)base.Clone();
		if (calcFormula != null)
		{
			metaField.CalcFormula = (MetaCalcFormula)CalcFormula.Clone();
		}
		if (relations != null)
		{
			metaField.relations = new List<MetaFieldRelation>();
			foreach (MetaFieldRelation relation in Relations)
			{
				metaField.Relations.Add((MetaFieldRelation)relation.Clone());
			}
		}
		return metaField;
	}

	public MetaField()
	{
	}

	public MetaField(XmlNode root)
		: base(root)
	{
		Initialize(root);
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(root.OwnerDocument.NameTable);
		xmlNamespaceManager.AddNamespace("a", root.NamespaceURI);
		XmlNode xmlNode = root.SelectSingleNode("a:CalculationFormula", xmlNamespaceManager);
		if (xmlNode != null)
		{
			calcFormula = new MetaCalcFormula(xmlNode);
		}
		XmlNodeList xmlNodeList = root.SelectNodes("a:TableRelations", xmlNamespaceManager);
		if (xmlNodeList == null)
		{
			return;
		}
		foreach (XmlNode item in xmlNodeList)
		{
			relations.Add(new MetaFieldRelation(item));
			if (root.Attributes["ValidateTableRelation"] != null)
			{
				validation |= root.Attributes["ValidateTableRelation"].Value == "1";
			}
		}
		if (root.Attributes["ValidateTableRelation"] != null)
		{
			validateRelations = MetaBase.BooleanValue(root.Attributes["ValidateTableRelation"].Value);
		}
		if (root.Attributes["TestTableRelation"] != null)
		{
			testRelations = MetaBase.BooleanValue(root.Attributes["TestTableRelation"].Value);
		}
	}

	private void Initialize(XmlNode root)
	{
		foreach (XmlAttribute attribute in root.Attributes)
		{
			switch (attribute.Name)
			{
			case "FieldClass":
				fieldClass = (FieldClass)Enum.Parse(typeof(FieldClass), attribute.Value);
				break;
			case "Editable":
				editable = MetaBase.BooleanValue(attribute.Value);
				break;
			case "InitValue":
				initValue = attribute.Value;
				break;
			case "MinValue":
				minValue = attribute.Value;
				break;
			case "MaxValue":
				maxValue = attribute.Value;
				break;
			case "ExtendedDatatype":
				extendedDatatype = extendedDatatypesTable[attribute.Value];
				break;
			case "SubType":
				try
				{
					blobSubType = (BlobSubType)Enum.Parse(typeof(BlobSubType), attribute.Value);
				}
				catch
				{
					throw NavNCLNotSupportedTypeException.CreateUnsupportedType(CultureInfo.CurrentUICulture, attribute.Value);
				}
				break;
			case "FilterTableID":
				FilterTableID = MetaBase.Int32Value(attribute.Value);
				break;
			case "ID":
				id = MetaBase.Int32Value(attribute.Value);
				break;
			case "Width":
				width = MetaBase.Int32Value(attribute.Value);
				break;
			case "OnLookup":
				lookup = !string.IsNullOrEmpty(attribute.Value) && string.Compare(attribute.Value, "undefined", StringComparison.OrdinalIgnoreCase) != 0;
				break;
			case "Datatype":
				try
				{
					type = (NavType)Enum.Parse(typeof(NavType), attribute.Value);
				}
				catch
				{
					throw NavNCLNotSupportedTypeException.CreateUnsupportedType(CultureInfo.CurrentUICulture, attribute.Value);
				}
				break;
			case "Name":
				name = attribute.Value;
				break;
			case "OnValidate":
				validation = attribute.Value != null && !attribute.Value.ToUpperInvariant().Equals("UNDEFINED") && !string.IsNullOrEmpty(attribute.Value);
				break;
			case "Caption":
				caption = attribute.Value;
				break;
			case "CaptionML":
				mergedCaptionMultiLanguage = attribute.Value;
				captionML = attribute.Value;
				break;
			case "DataLength":
				length = MetaBase.Int32Value(attribute.Value);
				break;
			case "ClosingDates":
				closingDates = MetaBase.BooleanValue(attribute.Value);
				break;
			case "DateFormula":
				dateFormula = MetaBase.BooleanValue(attribute.Value);
				break;
			case "BlankNumbers":
				blankNumbers = (BlankNumbers)Enum.Parse(typeof(BlankNumbers), attribute.Value);
				break;
			case "NotBlank":
				notBlank = MetaBase.BooleanValue(attribute.Value);
				break;
			case "BlankZero":
				blankZero = MetaBase.BooleanValue(attribute.Value);
				break;
			case "CharAllowed":
				charAllowed = attribute.Value;
				break;
			case "ValuesAllowed":
				valuesAllowed = attribute.Value;
				break;
			case "CaptionClassText":
			case "CaptionClass":
				captionClass = attribute.Value;
				break;
			case "AccessByObjectType":
				accessByObjectType = accessByObjectTable[attribute.Value];
				break;
			case "AccessByObjectID":
				accessByObjectID = MetaBase.Int32Value(attribute.Value);
				break;
			case "AccessByPermissionMask":
				accessByPermissionMask = MetaBase.Int32Value(attribute.Value);
				break;
			case "SignDisplacement":
				signDisplacement = attribute.Value;
				break;
			case "Enabled":
				enabled = MetaBase.BooleanValue(attribute.Value);
				break;
			case "Numeric":
				numeric = MetaBase.BooleanValue(attribute.Value);
				break;
			case "Title":
				title = MetaBase.BooleanValue(attribute.Value);
				break;
			case "OptionCaptionML":
				mergedOptionCaptionML = attribute.Value;
				optionCaptionML = attribute.Value;
				break;
			case "OptionString":
				optionString = attribute.Value;
				break;
			case "OptionValues":
			{
				optionValues = new List<int>();
				string value = attribute.Value;
				if (string.IsNullOrEmpty(value))
				{
					break;
				}
				string[] array = value.Split(',');
				for (int i = 0; i < array.Length; i++)
				{
					if (int.TryParse(array[i], out var result))
					{
						optionValues.Add(result - 1);
						continue;
					}
					optionValues.Clear();
					break;
				}
				break;
			}
			case "AutoIncrement":
				autoIncrement = MetaBase.BooleanValue(attribute.Value);
				break;
			case "SQL_Timestamp":
				timestamp = MetaBase.BooleanValue(attribute.Value);
				break;
			case "SQL_Data_Type":
				sqlDataType = attribute.Value;
				break;
			case "Compressed":
				compressed = MetaBase.BooleanValue(attribute.Value);
				break;
			case "ExternalName":
				ExternalName = attribute.Value;
				break;
			case "ExternalType":
				ExternalType = attribute.Value;
				break;
			case "ExternalAccess":
				ExternalAccess = ((attribute.Value != null) ? ((ExternalAccess)Enum.Parse(typeof(ExternalAccess), attribute.Value)) : ExternalAccess.Full);
				break;
			case "OptionOrdinalValues":
				OptionOrdinalValues = attribute.Value.Split(';').Select(int.Parse).ToArray();
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
			case "EnumTypeName":
				EnumTypeName = attribute.Value;
				break;
			case "EnumTypeId":
				EnumTypeId = MetaBase.Int32Value(attribute.Value);
				break;
			case "Extensible":
				Extensible = MetaBase.BooleanValue(attribute.Value);
				break;
			}
		}
		if (Type == NavType.Date)
		{
			validation = true;
		}
		clrType = CommonTypeInformation.ResolveClrType(type).ToString();
	}

	public static List<int> GenerateOptionValues(string optionString)
	{
		List<int> list = new List<int>();
		if (!string.IsNullOrEmpty(optionString))
		{
			string[] array = optionString.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				list.Add(i);
			}
		}
		return list;
	}

	public void ApplyLanguage(MetaTable parent, string language, string primaryLanguage)
	{
		if (parent.Id == 2000000007)
		{
			caption = GetDateSystemTableCaption(Id);
		}
		if (MergedCaptionMultiLanguage != null)
		{
			caption = MultiLanguage.GetLanguageValue(language, primaryLanguage, MergedCaptionMultiLanguage);
		}
		if (Type == NavType.Option)
		{
			if (parent.Id == 2000000007 && Id == 1)
			{
				optionCaption = Lang.DatePeriodTypeCaptions;
			}
			else
			{
				optionCaption = MultiLanguage.GetLanguageValue(language, primaryLanguage, mergedOptionCaptionML);
			}
		}
	}

	internal void MergeMultiLanguageTexts(int parentId, int languageAppGroupId)
	{
		mergedCaptionMultiLanguage = MetaTable.MultiLanguageTextMerge(captionML, 0, ObjectType.Table, parentId, Id, languageAppGroupId);
		mergedOptionCaptionML = MetaTable.MultiLanguageTextMerge(optionCaptionML, 2, ObjectType.Table, parentId, Id, languageAppGroupId);
	}

	internal static string GetDateSystemTableCaption(int fieldId)
	{
		return fieldId switch
		{
			1 => Lang.DatePeriodTypeCaption, 
			2 => Lang.DatePeriodStartCaption, 
			3 => Lang.DatePeriodEndCaption, 
			4 => Lang.DatePeriodNoCaption, 
			5 => Lang.DatePeriodNameCaption, 
			_ => string.Empty, 
		};
	}
}
