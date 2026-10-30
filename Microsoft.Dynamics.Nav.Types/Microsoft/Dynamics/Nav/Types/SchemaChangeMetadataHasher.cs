using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types;

public static class SchemaChangeMetadataHasher
{
	private class TableSchemaChangeMetadataContext : IEquatable<TableSchemaChangeMetadataContext>
	{
		public string TableName { get; set; }

		public bool IsDataPerCompany { get; set; }

		public CompressionType CompressionType { get; set; }

		public IList<FieldSchemaChangeContext> Fields { get; set; }

		public IList<KeySchemaChangeContext> Keys { get; set; }

		public static TableSchemaChangeMetadataContext Create(MetaTable table)
		{
			return new TableSchemaChangeMetadataContext
			{
				TableName = table.Name,
				IsDataPerCompany = table.IsDataPerCompany,
				CompressionType = table.CompressionType,
				Fields = table.Fields.Select((MetaField f) => new FieldSchemaChangeContext
				{
					Name = f.Name,
					AutoIncrement = f.AutoIncrement,
					Compressed = f.Compressed,
					Length = f.Length,
					SqlDataType = f.SqlDataType,
					Type = f.Type,
					IsActive = f.Enabled,
					IsUserDefinedTimestamp = f.Timestamp,
					IsNormal = (f.FieldClass == FieldClass.Normal)
				}).ToList(),
				Keys = table.Keys.Select((MetaKey k) => new KeySchemaChangeContext
				{
					IsClustered = k.Clustered,
					IsEnabled = k.Enabled,
					MaintainSiftIndex = k.MaintainSiftIndex,
					MaintainSqlIndex = k.MaintainSqlIndex,
					Fields = k.FieldRelations.ToList(),
					SumIndexFields = k.SumIndexFields.ToList(),
					Unique = k.Unique
				}).ToList()
			};
		}

		public override int GetHashCode()
		{
			int hash = GetFNVHashCode(TableName);
			ComputeHash(IsDataPerCompany, ref hash);
			ComputeHash((int)CompressionType, ref hash);
			foreach (FieldSchemaChangeContext field in Fields)
			{
				hash = Combine(hash, field.GetHashCode());
			}
			foreach (KeySchemaChangeContext key in Keys)
			{
				hash = Combine(hash, key.GetHashCode());
			}
			return hash;
		}

		public bool Equals(TableSchemaChangeMetadataContext other)
		{
			if (other == null)
			{
				return false;
			}
			if (TableName == null && other.TableName != null)
			{
				return false;
			}
			if (!TableName.Equals(other.TableName))
			{
				return false;
			}
			if (IsDataPerCompany != other.IsDataPerCompany)
			{
				return false;
			}
			if (CompressionType != other.CompressionType)
			{
				return false;
			}
			if (AreFieldsEqual(other.Fields))
			{
				return AreKeysEqual(other.Keys);
			}
			return false;
		}

		private bool AreFieldsEqual(IList<FieldSchemaChangeContext> other)
		{
			if (Fields.Count != other.Count)
			{
				return false;
			}
			for (int i = 0; i < Fields.Count; i++)
			{
				FieldSchemaChangeContext fieldSchemaChangeContext = Fields[i];
				FieldSchemaChangeContext other2 = other[i];
				if (!fieldSchemaChangeContext.Equals(other2))
				{
					return false;
				}
			}
			return true;
		}

		private bool AreKeysEqual(IList<KeySchemaChangeContext> other)
		{
			if (Keys.Count != other.Count)
			{
				return false;
			}
			for (int i = 0; i < Keys.Count; i++)
			{
				KeySchemaChangeContext keySchemaChangeContext = Keys[i];
				KeySchemaChangeContext other2 = other[i];
				if (!keySchemaChangeContext.Equals(other2))
				{
					return false;
				}
			}
			return true;
		}
	}

	private class FieldSchemaChangeContext : IEquatable<FieldSchemaChangeContext>
	{
		public string Name { get; set; }

		public NavType Type { get; set; }

		public int Length { get; set; }

		public string SqlDataType { get; set; }

		public bool AutoIncrement { get; set; }

		public bool Compressed { get; set; }

		public bool IsActive { get; set; }

		public bool IsUserDefinedTimestamp { get; set; }

		public bool IsNormal { get; set; }

		public override int GetHashCode()
		{
			int hash = GetFNVHashCode(Name);
			ComputeHash((int)Type, ref hash);
			ComputeHash(Length, ref hash);
			ComputeHash(SqlDataType, ref hash);
			ComputeHash(AutoIncrement, ref hash);
			ComputeHash(Compressed, ref hash);
			ComputeHash(IsActive, ref hash);
			ComputeHash(IsUserDefinedTimestamp, ref hash);
			ComputeHash(IsNormal, ref hash);
			return hash;
		}

		public bool Equals(FieldSchemaChangeContext other)
		{
			if (other == null)
			{
				return false;
			}
			if (Name == null && other.Name != null)
			{
				return false;
			}
			if (!Name.Equals(other.Name))
			{
				return false;
			}
			if (Type != other.Type)
			{
				return false;
			}
			if (Length != other.Length)
			{
				return false;
			}
			if (SqlDataType == null && other.SqlDataType != null)
			{
				return false;
			}
			if (!SqlDataType.Equals(other.SqlDataType))
			{
				return false;
			}
			if (AutoIncrement != other.AutoIncrement)
			{
				return false;
			}
			if (Compressed != other.Compressed)
			{
				return false;
			}
			if (IsActive != other.IsActive)
			{
				return false;
			}
			if (IsUserDefinedTimestamp != other.IsUserDefinedTimestamp)
			{
				return false;
			}
			if (IsNormal != other.IsNormal)
			{
				return false;
			}
			return true;
		}
	}

	private class KeySchemaChangeContext : IEquatable<KeySchemaChangeContext>
	{
		public bool IsEnabled { get; set; }

		public bool MaintainSqlIndex { get; set; }

		public bool MaintainSiftIndex { get; set; }

		public bool IsClustered { get; set; }

		public bool Unique { get; set; }

		public IList<FieldMetadataRelation> Fields { get; set; }

		public IList<SumIndexField> SumIndexFields { get; set; }

		public override int GetHashCode()
		{
			int hash = GetFNVHashCode(BitConverter.GetBytes(IsEnabled));
			ComputeHash(MaintainSqlIndex, ref hash);
			ComputeHash(MaintainSiftIndex, ref hash);
			ComputeHash(IsClustered, ref hash);
			ComputeHash(Unique, ref hash);
			foreach (FieldMetadataRelation field in Fields)
			{
				ComputeHash(field.Id, ref hash);
				ComputeHash(field.Name, ref hash);
			}
			foreach (SumIndexField sumIndexField in SumIndexFields)
			{
				ComputeHash(sumIndexField.Id, ref hash);
				ComputeHash(sumIndexField.Name, ref hash);
			}
			return hash;
		}

		public bool Equals(KeySchemaChangeContext other)
		{
			if (other == null)
			{
				return false;
			}
			if (IsEnabled != other.IsEnabled)
			{
				return false;
			}
			if (MaintainSqlIndex != other.MaintainSqlIndex)
			{
				return false;
			}
			if (IsClustered != other.IsClustered)
			{
				return false;
			}
			if (Unique != other.Unique)
			{
				return false;
			}
			if (AreFieldsEqual(other.Fields))
			{
				return AreSumIndexFieldsEqual(other.SumIndexFields);
			}
			return false;
		}

		private bool AreFieldsEqual(IList<FieldMetadataRelation> other)
		{
			if (Fields.Count != other.Count)
			{
				return false;
			}
			for (int i = 0; i < Fields.Count; i++)
			{
				FieldMetadataRelation fieldMetadataRelation = Fields[i];
				FieldMetadataRelation fieldMetadataRelation2 = other[i];
				if (fieldMetadataRelation.Id != fieldMetadataRelation2.Id)
				{
					return false;
				}
				if (!fieldMetadataRelation.Name.Equals(fieldMetadataRelation2.Name))
				{
					return false;
				}
			}
			return true;
		}

		private bool AreSumIndexFieldsEqual(IList<SumIndexField> other)
		{
			if (SumIndexFields.Count != other.Count)
			{
				return false;
			}
			for (int i = 0; i < SumIndexFields.Count; i++)
			{
				SumIndexField sumIndexField = SumIndexFields[i];
				SumIndexField sumIndexField2 = other[i];
				if (sumIndexField.Id != sumIndexField2.Id)
				{
					return false;
				}
				if (!sumIndexField.Name.Equals(sumIndexField2.Name))
				{
					return false;
				}
			}
			return true;
		}
	}

	private const int FnvOffsetBias = -2128831035;

	private const int FnvPrime = 16777619;

	public static int ComputeHash(string metadata)
	{
		if (string.IsNullOrEmpty(metadata))
		{
			return 0;
		}
		XmlDocument document = XmlHelper.SafeXmlDocument();
		try
		{
			document.Load(XmlHelper.SafeXmlReader(new StringReader(metadata)));
		}
		catch (XmlException)
		{
			XmlMetadataCleaner.CleanForInvalidCharacters(metadata, ref document);
		}
		return ComputeHash(document.DocumentElement);
	}

	public static int ComputeHash(Stream metadata)
	{
		if (metadata == null)
		{
			return 0;
		}
		XmlDocument document = XmlHelper.SafeXmlDocument();
		try
		{
			document.Load(XmlHelper.SafeXmlReader(metadata));
		}
		catch (XmlException)
		{
			XmlMetadataCleaner.CleanForInvalidCharacters(metadata, null, ref document);
		}
		return ComputeHash(document.DocumentElement);
	}

	private static int ComputeHash(XmlElement root)
	{
		MetaTable metaTable = MetaTable.CreateMetaTableFromXml(root, 0, 0L);
		if (metaTable == null)
		{
			return 0;
		}
		return TableSchemaChangeMetadataContext.Create(metaTable).GetHashCode();
	}

	private static int Combine(int h1, int h2)
	{
		return ((h1 << 5) + h1 + (h1 >> 27)) ^ h2;
	}

	private static void ComputeHash(int data, ref int hash)
	{
		int fNVHashCode = GetFNVHashCode(BitConverter.GetBytes(data));
		hash = Combine(hash, fNVHashCode);
	}

	private static void ComputeHash(string data, ref int hash)
	{
		int fNVHashCode = GetFNVHashCode(data);
		hash = Combine(hash, fNVHashCode);
	}

	private static void ComputeHash(bool data, ref int hash)
	{
		int fNVHashCode = GetFNVHashCode(BitConverter.GetBytes(data));
		hash = Combine(hash, fNVHashCode);
	}

	private static int GetFNVHashCode(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return 0;
		}
		byte[] array = new byte[s.Length * 2];
		Buffer.BlockCopy(s.ToCharArray(), 0, array, 0, array.Length);
		return GetFNVHashCode(array);
	}

	private static int GetFNVHashCode(byte[] data)
	{
		int num = -2128831035;
		for (int i = 0; i < data.Length; i++)
		{
			num = (num ^ data[i]) * 16777619;
		}
		return num;
	}
}
