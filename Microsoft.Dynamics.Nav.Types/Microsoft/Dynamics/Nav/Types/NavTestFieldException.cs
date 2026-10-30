using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public sealed class NavTestFieldException : NavException
{
	private string fieldName;

	private string tableName;

	private const string FieldNameString = "FieldName";

	private const string TableNameString = "TableName";

	public string FieldName
	{
		get
		{
			return fieldName;
		}
		set
		{
			fieldName = value;
		}
	}

	public string TableName
	{
		get
		{
			return tableName;
		}
		set
		{
			tableName = value;
		}
	}

	private NavTestFieldException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		fieldName = info.GetString("FieldName");
		tableName = info.GetString("TableName");
		DiagnosticsSuppress = true;
	}

	public NavTestFieldException()
	{
		DiagnosticsSuppress = true;
	}

	public NavTestFieldException(string message)
		: base(message)
	{
		DiagnosticsSuppress = true;
	}

	public NavTestFieldException(string message, Exception innerException)
		: base(message, innerException)
	{
		if (innerException == null)
		{
			throw new ArgumentNullException("innerException");
		}
		DiagnosticsSuppress = true;
	}

	public NavTestFieldException(CultureInfo culture, string message, params DiagnosticParameter[] args)
		: base(culture, message, args)
	{
	}

	public static NavTestFieldException CreateMustBeEqualTo(CultureInfo culture, string fieldName, string tableName, string shouldBeValue, string currentValue, string primaryKeyValues)
	{
		return new NavTestFieldException(culture, Lang.FieldMustBeEqualTo, DiagnosticParameter.SystemMetadata(fieldName), DiagnosticParameter.SystemMetadata(tableName), DiagnosticParameter.CustomerContent(shouldBeValue), DiagnosticParameter.CustomerContent(currentValue), DiagnosticParameter.CustomerContent(primaryKeyValues))
		{
			fieldName = fieldName,
			tableName = tableName
		};
	}

	public static NavTestFieldException CreateMustBeUri(CultureInfo culture, string fieldName, string tableName, string currentValue)
	{
		return new NavTestFieldException(culture, Lang.FieldMustBeURI, DiagnosticParameter.SystemMetadata(fieldName), DiagnosticParameter.SystemMetadata(tableName), DiagnosticParameter.CustomerContent(currentValue))
		{
			fieldName = fieldName,
			tableName = tableName
		};
	}

	public static NavTestFieldException CreateNonblank(CultureInfo culture, string fieldName, string tableName, string primaryKeyValues)
	{
		return new NavTestFieldException(culture, Lang.FieldMustBeNonblank, DiagnosticParameter.SystemMetadata(fieldName), DiagnosticParameter.SystemMetadata(tableName), DiagnosticParameter.CustomerContent(primaryKeyValues))
		{
			fieldName = fieldName,
			tableName = tableName
		};
	}

	public static NavTestFieldException CreateNonblankOption(CultureInfo culture, string fieldName, string optionCaption, string tableName, string primaryKeyValues)
	{
		return new NavTestFieldException(culture, Lang.FieldMustBeNonblankOption, DiagnosticParameter.SystemMetadata(fieldName), DiagnosticParameter.SystemMetadata(optionCaption), DiagnosticParameter.SystemMetadata(tableName), DiagnosticParameter.CustomerContent(primaryKeyValues))
		{
			fieldName = fieldName,
			tableName = tableName
		};
	}

	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("FieldName", fieldName);
		info.AddValue("TableName", tableName);
		base.GetObjectData(info, context);
	}

	protected override void UpdateCloneCore(NavBaseException exceptionClone)
	{
		base.UpdateCloneCore(exceptionClone);
		NavTestFieldException ex = (NavTestFieldException)exceptionClone;
		ex.fieldName = fieldName;
		ex.tableName = tableName;
	}
}
