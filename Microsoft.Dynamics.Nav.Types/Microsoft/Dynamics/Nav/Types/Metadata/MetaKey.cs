using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class MetaKey : MetaBase
{
	private const string PropertyKey = "Key";

	private const string PropertyKeyName = "KeyName";

	private const string PropertyEnabled = "Enabled";

	private const string PropertySumIndexFields = "SumIndexFields";

	private const string PropertyMaintainSQLIndex = "MaintainSQLIndex";

	private const string PropertyMaintainSIFTIndex = "MaintainSIFTIndex";

	private const string PropertySqlIndex = "SQLIndex";

	private const string PropertyClustered = "Clustered";

	private const string PropertyUnique = "Unique";

	private const string PropertySourceAppId = "SourceAppId";

	private const string PropertySourceExtensionType = "SourceExtensionType";

	private const string PropertyObsoleteState = "ObsoleteState";

	private const string PropertyObsoleteReason = "ObsoleteReason";

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private List<FieldMetadataRelation> fieldRelations = new List<FieldMetadataRelation>();

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private List<SumIndexField> sumIndexFields = new List<SumIndexField>();

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private List<FieldMetadataRelation> sqlIndexFields = new List<FieldMetadataRelation>();

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private string name;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private string keyName;

	[DataMember(IsRequired = false, EmitDefaultValue = true)]
	private bool enabled;

	[DataMember(IsRequired = false, EmitDefaultValue = true)]
	private bool maintainSqlIndex;

	[DataMember(IsRequired = false, EmitDefaultValue = true)]
	private bool maintainSiftIndex;

	[DataMember(IsRequired = false, EmitDefaultValue = true)]
	private bool clustered;

	[DataMember(IsRequired = false, EmitDefaultValue = true)]
	private bool unique;

	private string sourceAppId;

	private int sourceExtensionType;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private ObsoleteState obsoleteState;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private string obsoleteReason;

	public List<FieldMetadataRelation> FieldRelations => fieldRelations;

	public List<SumIndexField> SumIndexFields => sumIndexFields;

	public string Name => name;

	public string KeyName => keyName;

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

	public bool MaintainSqlIndex => maintainSqlIndex;

	public bool MaintainSiftIndex => maintainSiftIndex;

	public bool Clustered
	{
		[DebuggerStepThrough]
		get
		{
			return clustered;
		}
	}

	public bool Unique
	{
		[DebuggerStepThrough]
		get
		{
			return unique;
		}
	}

	public List<FieldMetadataRelation> SqlIndexFields => sqlIndexFields;

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

	public MetaKey()
	{
		Enabled = true;
		maintainSqlIndex = true;
		maintainSiftIndex = true;
	}

	public override object Clone()
	{
		MetaKey metaKey = (MetaKey)base.Clone();
		if (FieldRelations != null)
		{
			metaKey.fieldRelations = new List<FieldMetadataRelation>();
			foreach (FieldMetadataRelation fieldRelation in FieldRelations)
			{
				metaKey.FieldRelations.Add((FieldMetadataRelation)fieldRelation.Clone());
			}
		}
		if (SumIndexFields != null)
		{
			metaKey.sumIndexFields = new List<SumIndexField>();
			foreach (SumIndexField sumIndexField in SumIndexFields)
			{
				metaKey.SumIndexFields.Add((SumIndexField)sumIndexField.Clone());
			}
		}
		if (sqlIndexFields != null)
		{
			metaKey.sqlIndexFields = new List<FieldMetadataRelation>(sqlIndexFields.ToArray());
		}
		return metaKey;
	}

	public MetaKey(XmlNode root, MetaTable table)
		: base(root)
	{
		Initialize(root, table);
	}

	private void Initialize(XmlNode keyNode, MetaTable table)
	{
		foreach (XmlAttribute attribute in keyNode.Attributes)
		{
			switch (attribute.Name)
			{
			case "Key":
			{
				string text = (name = attribute.Value);
				Enabled = keyNode.Attributes["Enabled"] == null || keyNode.Attributes["Enabled"].Value == "1";
				text = text.Replace("Field", "");
				string[] array = text.Split(new char[1] { ',' }, StringSplitOptions.None);
				foreach (string s3 in array)
				{
					MetaField fieldsById3 = table.GetFieldsById(int.Parse(s3, CultureInfo.InvariantCulture));
					FieldMetadataRelation fieldMetadataRelation2 = new FieldMetadataRelation();
					fieldMetadataRelation2.Name = fieldsById3.Name;
					fieldMetadataRelation2.Id = fieldsById3.Id;
					fieldRelations.Add(fieldMetadataRelation2);
				}
				break;
			}
			case "KeyName":
				keyName = attribute.Value;
				break;
			case "SumIndexFields":
			{
				string[] array = attribute.Value.Replace("Field", "").Split(new char[1] { ',' }, StringSplitOptions.None);
				foreach (string s2 in array)
				{
					MetaField fieldsById2 = table.GetFieldsById(int.Parse(s2, CultureInfo.InvariantCulture));
					SumIndexField sumIndexField = new SumIndexField();
					sumIndexField.Name = fieldsById2.Name;
					sumIndexField.Id = fieldsById2.Id;
					sumIndexFields.Add(sumIndexField);
				}
				break;
			}
			case "SQLIndex":
			{
				string[] array = attribute.Value.Replace("Field", "").Split(new char[1] { ',' }, StringSplitOptions.None);
				foreach (string s in array)
				{
					MetaField fieldsById = table.GetFieldsById(int.Parse(s, CultureInfo.InvariantCulture));
					FieldMetadataRelation fieldMetadataRelation = new FieldMetadataRelation();
					fieldMetadataRelation.Name = fieldsById.Name;
					fieldMetadataRelation.Id = fieldsById.Id;
					sqlIndexFields.Add(fieldMetadataRelation);
				}
				break;
			}
			case "MaintainSQLIndex":
				maintainSqlIndex = MetaBase.BooleanValue(attribute.Value);
				break;
			case "MaintainSIFTIndex":
				maintainSiftIndex = MetaBase.BooleanValue(attribute.Value);
				break;
			case "Clustered":
				clustered = MetaBase.BooleanValue(attribute.Value);
				break;
			case "Unique":
				unique = MetaBase.BooleanValue(attribute.Value);
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
			}
		}
	}
}
