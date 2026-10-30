using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Dynamics.Nav.Runtime.Data.Graph;

internal class RecordBufferJsonConverter : JsonConverter
{
	private const string LastModifiedDateTimeFieldName = "lastModifiedDateTime";

	private const string OdataTypeFieldName = "@odata.type";

	private const string CollectionIdentifier = "Collection(";

	private readonly NCLMetaTable table;

	private readonly NavDiagnostics diagnostics;

	public RecordBufferJsonConverter(NCLMetaTable metaTable, NavDiagnostics diagnostics)
	{
		table = metaTable;
		this.diagnostics = diagnostics;
	}

	public override void WriteJson(JsonWriter writer, object recordBufferObject, JsonSerializer serializer)
	{
		diagnostics.SendTraceTag("00000DA", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Begin RecordBufferSerializer.WriteJson: \nTableName {0} - ExternalName {1}", table.TableName, table.ExternalName);
		using (diagnostics.CreateTraceScope("00000L9", DiagnosticParameter.SystemMetadata("GraphDataProvider.Serialization.WriteJson"), MonitorType.None, Category.Runtime))
		{
			MutableRecordBuffer mutableBuffer = recordBufferObject as MutableRecordBuffer;
			if (mutableBuffer == null)
			{
				throw new ArgumentNullException("recordBufferObject");
			}
			JObject jObject = new JObject();
			JArray jArray = new JArray();
			bool flag = false;
			if (table.KeyCount > 0)
			{
				flag = table.PrimaryKey.DefinedKeyFields.Any((INavFieldMetadata t) => mutableBuffer.ReadOnlyBuffer[t.ColumnIndex] == null && !mutableBuffer.IsChangedValueSameAsOriginalValue(t.ColumnIndex));
			}
			for (int num = 0; num < table.FieldCount; num++)
			{
				NCLMetaField fieldByIndex = table.GetFieldByIndex(num);
				if (!fieldByIndex.FieldActive || fieldByIndex.FieldObsoleted || string.IsNullOrEmpty(fieldByIndex.ExternalName) || fieldByIndex.ExternalName.StartsWith("/", StringComparison.Ordinal) || "DeltaToken".Equals(fieldByIndex.ExternalName, StringComparison.OrdinalIgnoreCase) || fieldByIndex.FieldClass == FieldClass.FlowField)
				{
					continue;
				}
				NavValue navValue = mutableBuffer[num];
				if (navValue != null)
				{
					bool flag2 = navValue.Equals(fieldByIndex.InitValue) && fieldByIndex.FieldNclType != NavNclType.NavBoolean;
					bool flag3 = mutableBuffer.ReadOnlyBuffer[num] != null && mutableBuffer.IsChangedValueSameAsOriginalValue(num);
					if ((flag && !flag2) || (!flag && !flag3) || fieldByIndex.ExternalName.Equals("@odata.type", StringComparison.OrdinalIgnoreCase))
					{
						JToken jToken = CreateJTokenValueFromNavValue(fieldByIndex, navValue);
						if (jToken != null)
						{
							JProperty content = new JProperty(fieldByIndex.ExternalName, jToken);
							jObject.Add(content);
						}
					}
				}
				if (ExtensionsHelper.IsSingleValueExtendedProperties(fieldByIndex))
				{
					string text = Encoding.UTF8.GetString(navValue.GetBytes()).TrimEnd(default(char));
					if (!string.IsNullOrEmpty(text))
					{
						jArray.Add(JToken.Parse(text));
					}
				}
			}
			if (jArray.Count > 0)
			{
				jObject.Add("SingleValueExtendedProperties", jArray);
			}
			jObject.WriteTo(writer);
		}
		diagnostics.SendTraceTag("00000DB", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Done RecordBufferSerializer.WriteJson: \nTableName {0} - ExternalName {1}", table.TableName, table.ExternalName);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		diagnostics.SendTraceTag("00000CS", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Begin RecordBufferSerializer.ReadJson: \nTableName {0} - ExternalName {1}", table.TableName, table.ExternalName);
		NavValue[] array = ((!(existingValue is IRecordBuffer recordBuffer)) ? new NavValue[table.FieldCount] : recordBuffer.ToArray());
		using (diagnostics.CreateTraceScope("00000LA", DiagnosticParameter.SystemMetadata("GraphDataProvider.Serialization.ReadJson"), MonitorType.None, Category.Runtime))
		{
			JObject jObject = JObject.Load(reader);
			if (jObject.TryGetValue("lastModifiedDateTime", StringComparison.OrdinalIgnoreCase, out var value))
			{
				array[0] = NavBigInteger.Create(value.Value<DateTime>().Ticks);
			}
			else
			{
				array[0] = NavBigInteger.Create(DateTime.UtcNow.Ticks);
			}
			for (int i = 1; i < table.FieldCount; i++)
			{
				NCLMetaField fieldByIndex = table.GetFieldByIndex(i);
				if (!fieldByIndex.FieldActive || fieldByIndex.FieldObsoleted || string.IsNullOrEmpty(fieldByIndex.ExternalName))
				{
					array[i] = NavValue.GetDefaultNavValue(fieldByIndex, nullSupport: true);
					continue;
				}
				if (fieldByIndex.FieldClass == FieldClass.FlowField)
				{
					array[i] = NavValue.GetDefaultNavValue(fieldByIndex, nullSupport: true);
					continue;
				}
				JToken value2 = null;
				if (ExtensionsHelper.IsOpenTypeExtension(fieldByIndex))
				{
					string extensionSearchPath = GetExtensionSearchPath("Extensions", "ExtensionName", fieldByIndex.ExternalName);
					value2 = jObject.SelectToken(extensionSearchPath);
				}
				else if (ExtensionsHelper.IsSingleValueExtendedProperties(fieldByIndex))
				{
					string extensionSearchPath2 = GetExtensionSearchPath("SingleValueExtendedProperties", "PropertyId", fieldByIndex.ExternalName);
					value2 = jObject.SelectToken(extensionSearchPath2);
				}
				else if (!jObject.TryGetValue(fieldByIndex.ExternalName, StringComparison.Ordinal, out value2))
				{
					value2 = jObject.SelectToken(fieldByIndex.ExternalName);
				}
				if (value2 == null || value2.Type == JTokenType.Null)
				{
					diagnostics.SendTraceTag("00000CT", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "No value was found for the table field: {0}.{1} using ExternalName: {2}. Using default value specified in field metadata.", fieldByIndex.Parent.TableName, fieldByIndex.FieldName, fieldByIndex.ExternalName);
					array[i] = NavValue.GetDefaultNavValue(fieldByIndex, nullSupport: true);
				}
				else
				{
					array[i] = CreateNavValueFromJToken(fieldByIndex, value2, diagnostics);
				}
			}
		}
		diagnostics.SendTraceTag("00000CU", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Done RecordBufferSerializer.ReadJson: \nTableName {0} - ExternalName {1}", table.TableName, table.ExternalName);
		return new ReadOnlyRecordBuffer(table, array);
	}

	internal static JToken CreateJTokenValueFromNavValue(NCLMetaField metaField, NavValue navValue)
	{
		if (metaField.ExternalType == null)
		{
			return null;
		}
		switch (metaField.ExternalType.ToUpperInvariant())
		{
		case "EDM.BOOLEAN":
			return new JValue(navValue.ToBoolean());
		case "EDM.STRING":
			if (metaField.FieldNclType == NavNclType.NavBlob)
			{
				string valueFromBlob2 = GetValueFromBlob(metaField, navValue, BlobSubType.Memo);
				if (!string.IsNullOrWhiteSpace(valueFromBlob2))
				{
					return new JValue(valueFromBlob2);
				}
				return string.Empty;
			}
			return new JValue(navValue.ToString());
		case "EDM.DATE":
		case "EDM.DATETIME":
			if (navValue.IsNull || navValue.IsZeroOrEmpty)
			{
				return JValue.CreateNull();
			}
			return new JValue(navValue.ToDateTime().ToUniversalTime());
		case "EDM.DATETIMEOFFSET":
		case "EDM.TIMEOFDAY":
			if (navValue.IsNull || navValue.IsZeroOrEmpty)
			{
				return JValue.CreateNull();
			}
			return new JValue(new DateTimeOffset(navValue.ToDateTime().ToUniversalTime()));
		case "EDM.INT32":
			return new JValue(navValue.ToInt32());
		case "EDM.INT64":
			return new JValue(navValue.ToInt64());
		case "EDM.DECIMAL":
		case "EDM.DOUBLE":
			return new JValue((double)navValue.ToDecimal());
		case "EDM.GUID":
			return new JValue(navValue.ToGuid());
		case "EDM.DURATION":
			if (navValue.IsNull)
			{
				return new JValue(NavDuration.Default.Format(null, 9));
			}
			return new JValue(((NavDuration)navValue).Format(null, 9));
		default:
		{
			if (metaField.FieldNclType == NavNclType.NavOption)
			{
				return new JValue(metaField.FieldOptionMetadata.GetOptionFromIndex(navValue.ToInt32()));
			}
			string valueFromBlob = GetValueFromBlob(metaField, navValue, BlobSubType.Json);
			if (ExtensionsHelper.IsOpenTypeExtension(metaField) || ExtensionsHelper.IsSingleValueExtendedProperties(metaField))
			{
				return null;
			}
			if (!string.IsNullOrWhiteSpace(valueFromBlob))
			{
				return JToken.Parse(valueFromBlob);
			}
			if (!metaField.ExternalType.StartsWith("Collection(", StringComparison.OrdinalIgnoreCase))
			{
				return new JObject();
			}
			return new JArray();
		}
		}
	}

	private static string GetValueFromBlob(NCLMetaField metaField, NavValue navValue, BlobSubType allowedBlobSubType)
	{
		if (metaField.BlobSubType != allowedBlobSubType)
		{
			throw new NavNCLNotSupportedTypeException(string.Format(CultureInfo.CurrentCulture, Lang.GraphUnsupportedBlobSubType, metaField.BlobSubType));
		}
		if (!(navValue is NavBLOB navBLOB) || (navBLOB.IsInMemory && navBLOB.GetBytesSize <= 0))
		{
			throw new NavNCLTypeException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidNavNCLTypeAndValue, navValue, navValue.NclType));
		}
		return Encoding.UTF8.GetString(navBLOB.GetBytes()).TrimEnd(default(char));
	}

	internal static NavValue CreateNavValueFromJToken(NCLMetaField metaField, JToken entityValue, NavDiagnostics diagnostics = null)
	{
		try
		{
			bool flag = entityValue.IsNull();
			switch (metaField.ExternalType.ToUpperInvariant())
			{
			case "EDM.BOOLEAN":
				return flag ? NavBoolean.Default : NavBoolean.Create(entityValue.Value<bool>());
			case "EDM.STRING":
			{
				string text = (flag ? string.Empty : entityValue.Value<string>());
				switch (metaField.FieldNclType)
				{
				case NavNclType.NavText:
					return flag ? NavText.Default(metaField.FieldDefinedLength) : new NavText(metaField.FieldDefinedLength, TrimText(text, metaField, diagnostics));
				case NavNclType.NavCode:
					return flag ? NavCode.Default(metaField.FieldDefinedLength) : new NavCode(metaField.FieldDefinedLength, TrimText(text, metaField, diagnostics));
				case NavNclType.NavBlob:
					return CreateNavBlobFromJToken(metaField, entityValue, applyFormattingOnToString: false, BlobSubType.Memo);
				}
				break;
			}
			case "EDM.DATE":
			case "EDM.DATETIME":
			case "EDM.DATETIMEOFFSET":
			case "EDM.TIMEOFDAY":
			{
				DateTime initValue = (flag ? DateTime.MinValue : entityValue.Value<DateTime>());
				switch (metaField.FieldNclType)
				{
				case NavNclType.NavDate:
					return flag ? NavDate.Undefined : NavDate.Create(initValue);
				case NavNclType.NavDateTime:
					return flag ? NavDateTime.Undefined : NavDateTime.Create(initValue);
				case NavNclType.NavTime:
					return flag ? NavTime.Undefined : NavTime.Create(initValue);
				}
				break;
			}
			case "EDM.INT32":
				return flag ? NavInteger.Default : NavInteger.CreateFromObject(entityValue.Value<int>());
			case "EDM.INT64":
				return flag ? NavBigInteger.Default : NavBigInteger.CreateFromObject(entityValue.Value<long>());
			case "EDM.DECIMAL":
				return flag ? NavDecimal.Default : NavDecimal.CreateFromObject(entityValue.Value<decimal>());
			case "EDM.DOUBLE":
				return flag ? NavDecimal.Default : NavDecimal.CreateFromObject(entityValue.Value<double>());
			case "EDM.GUID":
				return flag ? NavGuid.Default : NavGuid.CreateFromObject(entityValue.Value<Guid>());
			case "EDM.DURATION":
				return flag ? NavDuration.Default : NavDuration.CreateFromObject(XmlConvert.ToTimeSpan(entityValue.Value<string>()));
			default:
				if (metaField.FieldNclType == NavNclType.NavOption)
				{
					return flag ? NavValue.GetDefaultNavValue(metaField) : NavOption.CreateFromObject(metaField.FieldOptionMetadata, metaField.FieldOptionMetadata.GetIndexFromOption(entityValue.Value<string>()));
				}
				if (metaField.FieldNclType != NavNclType.NavBlob)
				{
					string message = string.Format(CultureInfo.CurrentCulture, Lang.NavTypeIsNotSupported, metaField.FieldNclType);
					throw new NavNCLNotSupportedTypeException(message);
				}
				return CreateNavBlobFromJToken(metaField, entityValue, applyFormattingOnToString: true, BlobSubType.Json);
			}
			throw new NavGraphException(string.Format(CultureInfo.CurrentCulture, Lang.GraphUnsupportedExternalType, metaField.ExternalType, metaField.FieldName, metaField.FieldNclType, string.Empty));
		}
		catch (Exception ex) when (!(ex is NavBaseException))
		{
			throw new NavGraphException(string.Format(CultureInfo.CurrentCulture, Lang.GraphUnsupportedExternalType, metaField.ExternalType, metaField.FieldName, metaField.FieldNclType, ex.Message));
		}
	}

	private static NavBLOB CreateNavBlobFromJToken(NCLMetaField metaField, JToken entityValue, bool applyFormattingOnToString, BlobSubType allowedBlobSubType)
	{
		if (metaField.BlobSubType != allowedBlobSubType)
		{
			throw new NavNCLNotSupportedTypeException(string.Format(CultureInfo.CurrentCulture, Lang.GraphUnsupportedBlobSubType, metaField.BlobSubType));
		}
		if (entityValue.IsNull())
		{
			return NavBLOB.Default();
		}
		return new NavBLOB(Encoding.UTF8, applyFormattingOnToString ? entityValue.ToString(Newtonsoft.Json.Formatting.None) : entityValue.ToString());
	}

	public override bool CanConvert(Type objectType)
	{
		if (!(objectType == typeof(ReadOnlyRecordBuffer)))
		{
			return objectType == typeof(MutableRecordBuffer);
		}
		return true;
	}

	private static string GetExtensionSearchPath(string rootElementName, string propertyName, string propertyValue)
	{
		return string.Format(CultureInfo.InvariantCulture, "$.{0}[?(@.{1} == '{2}')]", rootElementName, propertyName, propertyValue);
	}

	private static string TrimText(string text, NCLMetaField metaField, NavDiagnostics diagnostics = null)
	{
		if (text == null || text.Length < metaField.FieldDefinedLength)
		{
			return text;
		}
		string result = text.Substring(0, metaField.FieldDefinedLength);
		diagnostics?.SendTraceTag("00000CV", Category.Runtime, Verbosity.Verbose, "Trimming text to a length of {3}, text length was {0}, in field {2}.{1}.", DiagnosticParameter.SystemMetadata(text.Length), DiagnosticParameter.SystemMetadata(metaField.FieldName), DiagnosticParameter.SystemMetadata(metaField.Parent?.TableName), DiagnosticParameter.SystemMetadata(metaField.FieldDefinedLength));
		return result;
	}
}
