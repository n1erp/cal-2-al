using System;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavValue : NavObject, INavValueMetadata, IEquatable<NavValue>, IComparable<NavValue>
{
	public virtual int NavDefinedLengthMetadata
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public virtual NCLOptionMetadata NavOptionMetadata
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public abstract int GetBytesSize { get; }

	public virtual bool IsWrapper => false;

	public virtual bool IsInitialized => true;

	public virtual bool IsMutable => false;

	public virtual bool IsNull => false;

	public virtual NavValue InnerValue
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public abstract bool IsZeroOrEmpty { get; }

	public virtual object ValueAsObject
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public virtual object ClientObject => ValueAsObject;

	public virtual object GetSqlWhereClauseValue(SqlDataType sqlDataType)
	{
		return ValueAsObject;
	}

	public virtual object GetSqlWritableValue(SqlDataType sqlDataType, int maxLength, bool compressed = false)
	{
		return ValueAsObject;
	}

	public abstract override int GetHashCode();

	public virtual byte[] GetBytes()
	{
		throw new NotSupportedException();
	}

	public virtual byte ToByte()
	{
		int num = ToInt32();
		if (num < 0 || num > 255)
		{
			throw new OverflowException(Lang.ToByteOverflow);
		}
		return (byte)num;
	}

	public virtual bool ToBoolean()
	{
		return ToInt32() != 0;
	}

	public virtual int ToInt32()
	{
		throw new NavNCLConversionException(GetType(), typeof(int));
	}

	public virtual long ToInt64()
	{
		return ToInt32();
	}

	public virtual DateTime ToDateTime()
	{
		throw new NavNCLConversionException(GetType(), typeof(DateTime));
	}

	public virtual Decimal18 ToDecimal()
	{
		return ToInt64();
	}

	public override string ToString()
	{
		return NavFormatEvaluateHelper.Format(this);
	}

	public virtual string ToString(bool invariant)
	{
		if (!invariant)
		{
			return NavFormatEvaluateHelper.Format(this);
		}
		return NavFormatEvaluateHelper.Format(this, 0, 9, NavSession.InvariantFormatSettings);
	}

	public virtual Guid ToGuid()
	{
		throw new NavNCLConversionException(GetType(), typeof(Guid));
	}

	public virtual bool Equals(NavValue other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (NavType != other.NavType)
		{
			return false;
		}
		return ThrowUnableToCompareException(other);
	}

	public virtual int CompareTo(NavValue other)
	{
		ThrowUnableToCompareException(other);
		return 0;
	}

	protected bool ThrowUnableToCompareException(object unsupportedInstance)
	{
		throw CreateUnableToCompareException(GetType(), unsupportedInstance);
	}

	internal static NavNCLInvalidComparisonException CreateUnableToCompareException(Type target, object unsupportedInstance)
	{
		string unsupportedTypeName = ((unsupportedInstance == null) ? "<NULL>" : unsupportedInstance.GetType().Name);
		return NavNCLInvalidComparisonException.Create(CultureInfo.InvariantCulture, target.Name, unsupportedTypeName);
	}

	internal static NavNCLArgumentException CreateUnableToCreateFromObjectException(Type target, object unsupportedObject)
	{
		string arg = ((unsupportedObject == null) ? "<NULL>" : unsupportedObject.GetType().Name);
		return new NavNCLArgumentException(string.Format(CultureInfo.CurrentCulture, Lang.UnableToCreateFromObject, arg, target.Name));
	}

	public bool CompareTypeAndMetadata(INavValueMetadata otherMetadata)
	{
		if (otherMetadata == null)
		{
			throw new ArgumentNullException("otherMetadata");
		}
		return NavValueMetadata.CompareTypeAndMetadata(this, otherMetadata);
	}

	public static object PrepareNavFieldForBusinessApp(NavValue field)
	{
		if (field == null)
		{
			throw new ArgumentNullException("field");
		}
		switch (field.NclType)
		{
		case NavNclType.NavBoolean:
		case NavNclType.NavByte:
		case NavNclType.NavInteger:
		case NavNclType.NavBigInteger:
		case NavNclType.NavDecimal:
		case NavNclType.NavChar:
		case NavNclType.NavGuid:
		case NavNclType.NavFieldClass:
			return field.ValueAsObject;
		default:
			return field;
		}
	}

	public static NavValue CreateNavValueFromObject(INavValueMetadata metadata, object value)
	{
		if (metadata == null)
		{
			throw new ArgumentNullException("metadata");
		}
		NavNclType nclType = metadata.NclType;
		for (NavValue navValue = value as NavValue; navValue != null; navValue = navValue.InnerValue)
		{
			if (navValue.CompareTypeAndMetadata(metadata))
			{
				return navValue;
			}
			if (navValue.NclType == nclType)
			{
				value = navValue;
				break;
			}
			if (!navValue.IsWrapper || !navValue.IsInitialized)
			{
				break;
			}
		}
		if (value == DBNull.Value)
		{
			value = null;
		}
		try
		{
			switch (nclType)
			{
			case NavNclType.NavBigInteger:
				return NavBigInteger.CreateFromObject(value);
			case NavNclType.NavBigText:
				return NavBigText.CreateFromObject(value);
			case NavNclType.NavBoolean:
				return NavBoolean.CreateFromObject(value);
			case NavNclType.NavByte:
				return NavByte.CreateFromObject(value);
			case NavNclType.NavOemCode:
				return NavOemCode.CreateFromObject(metadata.NavDefinedLengthMetadata, value);
			case NavNclType.NavDate:
				return NavDate.CreateFromObject(value);
			case NavNclType.NavDateFormula:
				return NavDateFormula.CreateFromObject(value);
			case NavNclType.NavDateTime:
				return NavDateTime.CreateFromObject(value);
			case NavNclType.NavDecimal:
				return NavDecimal.CreateFromObject(value);
			case NavNclType.NavDuration:
				return NavDuration.CreateFromObject(value);
			case NavNclType.NavGuid:
				return NavGuid.CreateFromObject(value);
			case NavNclType.NavInteger:
				return NavInteger.CreateFromObject(value);
			case NavNclType.NavOption:
				return NavOption.CreateFromObject(metadata.NavOptionMetadata, value);
			case NavNclType.NavRecordId:
				return NavRecordId.CreateFromObject(value);
			case NavNclType.NavOemText:
				return NavOemText.CreateFromObject(metadata.NavDefinedLengthMetadata, value);
			case NavNclType.NavTime:
				return NavTime.CreateFromObject(value);
			case NavNclType.NavClrObject:
				return NavByteArray.CreateFromObject(value);
			case NavNclType.NavTableFilter:
				return NavTableFilter.CreateFromObject(value);
			case NavNclType.NavText:
				return NavText.CreateFromObject(metadata.NavDefinedLengthMetadata, value);
			case NavNclType.NavChar:
				return NavChar.CreateFromObject(value);
			case NavNclType.NavCode:
				return NavCode.CreateFromObject(metadata.NavDefinedLengthMetadata, value);
			case NavNclType.NavBlob:
				return NavBLOB.CreateFromObject(value);
			case NavNclType.NavMedia:
				return NavMedia.CreateFromObject(value);
			case NavNclType.NavMediaSet:
				return NavMediaSet.CreateFromObject(value);
			case NavNclType.NavNotification:
				return NavNotification.CreateFromObject(value);
			case NavNclType.NavJsonArray:
				return NavJsonArray.CreateFromObject(value);
			case NavNclType.NavJsonObject:
				return NavJsonObject.CreateFromObject(value);
			case NavNclType.NavJsonToken:
				return NavJsonToken.CreateFromObject(value);
			case NavNclType.NavJsonValue:
				return NavJsonValue.CreateFromObject(value);
			case NavNclType.NavXmlAttribute:
				return NavXmlNode.CreateFromObject(value);
			case NavNclType.NavXmlAttributeCollection:
				return NavXmlAttributeCollection.CreateFromObject(value);
			case NavNclType.NavXmlCData:
				return NavXmlNode.CreateFromObject(value);
			case NavNclType.NavXmlComment:
				return NavXmlNode.CreateFromObject(value);
			case NavNclType.NavXmlDeclaration:
				return NavXmlNode.CreateFromObject(value);
			case NavNclType.NavXmlDocument:
				return NavXmlNode.CreateFromObject(value);
			case NavNclType.NavXmlDocumentType:
				return NavXmlNode.CreateFromObject(value);
			case NavNclType.NavXmlElement:
				return NavXmlNode.CreateFromObject(value);
			case NavNclType.NavXmlNamespaceManager:
				return NavXmlNamespaceManager.CreateFromObject(value);
			case NavNclType.NavXmlNameTable:
				return NavXmlNameTable.CreateFromObject(value);
			case NavNclType.NavXmlNode:
				return NavXmlNode.CreateFromObject(value);
			case NavNclType.NavXmlNodeList:
				return NavXmlNodeList.CreateFromObject(value);
			case NavNclType.NavXmlProcessingInstruction:
				return NavXmlNode.CreateFromObject(value);
			case NavNclType.NavXmlReadOptions:
				return NavXmlReadOptions.CreateFromObject(value);
			case NavNclType.NavXmlText:
				return NavXmlNode.CreateFromObject(value);
			case NavNclType.NavXmlWriteOptions:
				return NavXmlWriteOptions.CreateFromObject(value);
			case NavNclType.NavTextBuilder:
				return NavTextBuilder.CreateFromObject(value);
			case NavNclType.NavErrorInfo:
				return NavALErrorInfo.CreateFromObject(value);
			}
		}
		catch (NotSupportedException)
		{
		}
		catch (FormatException)
		{
		}
		throw NavNCLNotSupportedTypeException.CreateUnknownType(CultureInfo.CurrentCulture, nclType.ToString());
	}

	public static int ReadNavValueFromBytes(byte[] data, int offset, out NavValue value)
	{
		NavType navType = (NavType)BitConverter.ToInt32(data, offset);
		int num = BitConverter.ToInt32(data, offset + 4);
		offset += 8;
		switch (navType)
		{
		case NavType.BigInteger:
			value = NavBigInteger.Create(data, offset, num);
			break;
		case NavType.BigText:
			value = NavBigText.CreateFromBytes(data, offset, num);
			break;
		case NavType.Boolean:
			value = NavBoolean.CreateFromBytes(data, offset, num);
			break;
		case NavType.Byte:
			value = NavByte.CreateFromBytes(data, offset, num);
			break;
		case NavType.Char:
			value = NavChar.CreateFromBytes(data, offset, num);
			break;
		case NavType.OemCode:
			value = NavOemCode.CreateFromBytes(NavValueMetadata.DefaultMetadata(navType).NavDefinedLengthMetadata, data, offset, num);
			break;
		case NavType.Date:
			value = NavDate.CreateFromBytes(data, offset, num);
			break;
		case NavType.DateFormula:
			value = NavDateFormula.CreateFromBytes(data, offset, num);
			break;
		case NavType.DateTime:
			value = NavDateTime.CreateFromBytes(data, offset, num);
			break;
		case NavType.Decimal:
			value = NavDecimal.Create(data, offset, num);
			break;
		case NavType.Duration:
			value = NavDuration.CreateFromBytes(data, offset, num);
			break;
		case NavType.GUID:
			value = NavGuid.CreateFromBytes(data, offset, num);
			break;
		case NavType.Integer:
			value = NavInteger.Create(data, offset, num);
			break;
		case NavType.Option:
			value = NavOption.CreateFromBytes(NavValueMetadata.DefaultMetadata(navType).NavOptionMetadata, data, offset, num);
			break;
		case NavType.RecordID:
			value = NavRecordId.CreateFromBytes(data, offset, num);
			break;
		case NavType.OemText:
			value = NavOemText.CreateFromBytes(NavValueMetadata.DefaultMetadata(navType).NavDefinedLengthMetadata, data, offset, num);
			break;
		case NavType.Time:
			value = NavTime.CreateFromBytes(data, offset, num);
			break;
		case NavType.ClrByteArray:
			value = NavByteArray.CreateFromBytes(data, offset, num);
			break;
		case NavType.TableFilter:
			value = NavTableFilter.CreateFromBytes(data, offset, num);
			break;
		case NavType.Text:
			value = NavText.CreateFromBytes(NavValueMetadata.DefaultMetadata(navType).NavDefinedLengthMetadata, data, offset, num);
			break;
		case NavType.Code:
			value = NavCode.CreateFromBytes(NavValueMetadata.DefaultMetadata(navType).NavDefinedLengthMetadata, data, offset, num);
			break;
		default:
			throw NavNCLNotSupportedTypeException.CreateReadFromBytesNotSupportedType(CultureInfo.CurrentUICulture, navType);
		}
		return num + 8;
	}

	public virtual void StoreToBytes(BinaryWriter writer)
	{
		writer.Write((int)NavType);
		byte[] bytes = GetBytes();
		writer.Write(bytes.Length);
		writer.Write(bytes);
	}

	public static NavValue CreateNavValueFromBytes(INavValueMetadata metadata, byte[] bytes)
	{
		return CreateNavValueFromObject(metadata, bytes);
	}

	public static NavValue CreateNavValueFromBytes(INavValueMetadata metadata, byte[] data, int offset, int length)
	{
		if (metadata == null)
		{
			throw new ArgumentNullException("metadata");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return metadata.NclType switch
		{
			NavNclType.NavBigInteger => (NavValue)NavBigInteger.Create(data, offset, length), 
			NavNclType.NavBigText => NavBigText.CreateFromBytes(data, offset, length), 
			NavNclType.NavBoolean => NavBoolean.CreateFromBytes(data, offset, length), 
			NavNclType.NavByte => NavByte.CreateFromBytes(data, offset, length), 
			NavNclType.NavOemCode => NavOemCode.CreateFromBytes(metadata.NavDefinedLengthMetadata, data, offset, length), 
			NavNclType.NavDate => NavDate.CreateFromBytes(data, offset, length), 
			NavNclType.NavDateFormula => NavDateFormula.CreateFromBytes(data, offset, length), 
			NavNclType.NavDateTime => NavDateTime.CreateFromBytes(data, offset, length), 
			NavNclType.NavDecimal => NavDecimal.Create(data, offset, length), 
			NavNclType.NavDuration => NavDuration.CreateFromBytes(data, offset, length), 
			NavNclType.NavGuid => NavGuid.CreateFromBytes(data, offset, length), 
			NavNclType.NavInteger => NavInteger.Create(data, offset, length), 
			NavNclType.NavOption => NavOption.CreateFromBytes(metadata.NavOptionMetadata, data, offset, length), 
			NavNclType.NavRecordId => NavRecordId.CreateFromBytes(data, offset, length), 
			NavNclType.NavOemText => NavOemText.CreateFromBytes(metadata.NavDefinedLengthMetadata, data, offset, length), 
			NavNclType.NavTime => NavTime.CreateFromBytes(data, offset, length), 
			NavNclType.NavTableFilter => NavTableFilter.CreateFromBytes(data, offset, length), 
			NavNclType.NavText => NavText.CreateFromBytes(metadata.NavDefinedLengthMetadata, data, offset, length), 
			NavNclType.NavCode => NavCode.CreateFromBytes(metadata.NavDefinedLengthMetadata, data, offset, length), 
			_ => throw NavNCLNotSupportedTypeException.CreateUnknownType(CultureInfo.CurrentCulture, metadata.NclType.ToString()), 
		};
	}

	public static object GetObjectFromNavObject(object navInstance)
	{
		if (navInstance == null)
		{
			return DBNull.Value;
		}
		NavType navTypeFromType = NavNclTypeHelper.GetNavTypeFromType(navInstance.GetType());
		switch (navTypeFromType)
		{
		case NavType.BigInteger:
			return navInstance;
		case NavType.BigText:
			return ((NavBigText)navInstance).Value;
		case NavType.Boolean:
			return navInstance;
		case NavType.Byte:
			return navInstance;
		case NavType.Char:
			return navInstance;
		case NavType.OemCode:
			return ((NavOemCode)navInstance).Value;
		case NavType.Date:
		{
			DateTime value3 = ((NavDate)navInstance).Value;
			if (value3 == NavDate.Undefined.Value)
			{
				return DBNull.Value;
			}
			return value3;
		}
		case NavType.DateFormula:
			return ((NavDateFormula)navInstance).Value;
		case NavType.DateTime:
		{
			DateTime value2 = ((NavDateTime)navInstance).Value;
			if (value2 == NavDateTime.Undefined.Value)
			{
				return DBNull.Value;
			}
			return value2;
		}
		case NavType.Decimal:
			return ((NavDecimal)navInstance).Value;
		case NavType.Duration:
			return ((NavDuration)navInstance).Value;
		case NavType.GUID:
			return navInstance;
		case NavType.Integer:
			return navInstance;
		case NavType.Option:
			return ((NavOption)navInstance).Value;
		case NavType.RecordID:
			return null;
		case NavType.String:
			return navInstance;
		case NavType.TableFilter:
			return ((NavTableFilter)navInstance).Value;
		case NavType.OemText:
			return ((NavOemText)navInstance).Value;
		case NavType.TextConst:
			return ((NavTextConstant)navInstance).Value;
		case NavType.Time:
		{
			DateTime value = ((NavTime)navInstance).Value;
			if (value == NavTime.Undefined.Value)
			{
				return DBNull.Value;
			}
			return value;
		}
		case NavType.Text:
			return ((NavText)navInstance).Value;
		case NavType.Code:
			return ((NavCode)navInstance).Value;
		case NavType.Notification:
			return navInstance as NavNotification;
		default:
			throw NavNCLNotSupportedTypeException.CreateUnknownType(CultureInfo.CurrentCulture, navTypeFromType.ToString());
		}
	}

	internal static NavValue GetDefaultNavValue(INavValueMetadata metadata, bool nullSupport = false)
	{
		NavValue fixedDefaultNavValue;
		if (!nullSupport)
		{
			fixedDefaultNavValue = NavNclTypeHelper.GetFixedDefaultNavValue(metadata);
			if (fixedDefaultNavValue != null)
			{
				return fixedDefaultNavValue;
			}
		}
		fixedDefaultNavValue = metadata.NclType switch
		{
			NavNclType.NavBoolean => (NavValue)(nullSupport ? NavBoolean.Null : NavBoolean.Default), 
			NavNclType.NavBigInteger => nullSupport ? NavBigInteger.Null : NavBigInteger.Default, 
			NavNclType.NavDate => nullSupport ? NavDate.Null : NavDate.Default, 
			NavNclType.NavDateTime => nullSupport ? NavDateTime.Null : NavDateTime.Default, 
			NavNclType.NavDecimal => nullSupport ? NavDecimal.Null : NavDecimal.Default, 
			NavNclType.NavGuid => nullSupport ? NavGuid.Null : NavGuid.Default, 
			NavNclType.NavInteger => nullSupport ? NavInteger.Null : NavInteger.Default, 
			NavNclType.NavOption => nullSupport ? NavOption.Null(metadata.NavOptionMetadata) : metadata.NavOptionMetadata.CreateNavOption(0), 
			NavNclType.NavText => nullSupport ? NavText.NullWithMaxLength(metadata.NavDefinedLengthMetadata) : NavText.Default(metadata.NavDefinedLengthMetadata), 
			NavNclType.NavCode => nullSupport ? NavCode.NullWithMaxLength(metadata.NavDefinedLengthMetadata) : NavCode.Default(metadata.NavDefinedLengthMetadata), 
			NavNclType.NavBigText => NavBigText.Default, 
			NavNclType.NavByte => NavByte.Default, 
			NavNclType.NavOemCode => NavOemCode.Default(metadata.NavDefinedLengthMetadata), 
			NavNclType.NavDateFormula => NavDateFormula.Default, 
			NavNclType.NavDuration => NavDuration.Default, 
			NavNclType.NavRecordId => NavRecordId.Default, 
			NavNclType.NavOemText => NavOemText.Default(metadata.NavDefinedLengthMetadata), 
			NavNclType.NavTime => NavTime.Default, 
			NavNclType.NavTableFilter => NavTableFilter.Default, 
			NavNclType.NavChar => NavChar.Default, 
			NavNclType.NavVersion => NavVersion.Default, 
			NavNclType.NavBlob => NavBLOB.Default(), 
			NavNclType.NavMedia => NavMedia.Default, 
			NavNclType.NavMediaSet => NavMediaSet.Default, 
			NavNclType.NavJsonToken => NavJsonToken.Default, 
			NavNclType.NavJsonValue => NavJsonValue.Default, 
			NavNclType.NavJsonArray => NavJsonArray.Default, 
			NavNclType.NavJsonObject => NavJsonObject.Default, 
			NavNclType.NavXmlAttribute => NavXmlAttribute.Default, 
			NavNclType.NavXmlAttributeCollection => NavXmlAttributeCollection.Default, 
			NavNclType.NavXmlComment => NavXmlComment.Default, 
			NavNclType.NavXmlCData => NavXmlCData.Default, 
			NavNclType.NavXmlDeclaration => NavXmlDeclaration.Default, 
			NavNclType.NavXmlDocument => NavXmlDocument.Default, 
			NavNclType.NavXmlDocumentType => NavXmlDocumentType.Default, 
			NavNclType.NavXmlElement => NavXmlElement.Default, 
			NavNclType.NavXmlNamespaceManager => NavXmlNamespaceManager.Default, 
			NavNclType.NavXmlNameTable => NavXmlNameTable.Default, 
			NavNclType.NavXmlNode => NavXmlNode.Default, 
			NavNclType.NavXmlNodeList => NavXmlNodeList.Default, 
			NavNclType.NavXmlProcessingInstruction => NavXmlProcessingInstruction.Default, 
			NavNclType.NavXmlText => NavXmlText.Default, 
			NavNclType.NavXmlWriteOptions => NavXmlWriteOptions.Default, 
			NavNclType.NavXmlReadOptions => NavXmlReadOptions.Default, 
			NavNclType.NavList => NavList<object>.Default, 
			NavNclType.NavDictionary => NavDictionary<object, object>.Default, 
			_ => throw NavNCLNotSupportedTypeException.CreateUnknownType(CultureInfo.CurrentCulture, metadata.NclType.ToString()), 
		};
		if (nullSupport && !fixedDefaultNavValue.IsNull && fixedDefaultNavValue.NclType != NavNclType.NavBlob)
		{
			throw NavNCLNotSupportedTypeException.CreateUnknownType(CultureInfo.CurrentCulture, metadata.NclType.ToString());
		}
		return fixedDefaultNavValue;
	}
}
