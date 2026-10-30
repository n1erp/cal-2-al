using System;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class ALSystemVariable
{
	public const int XmlFormat = 9;

	public static bool ALCopyStream(DataError errorLevel, NavOutStream destination, NavInStream source)
	{
		return CopyStream(errorLevel, destination, source, 0, useBytesToRead: false);
	}

	public static bool ALCopyStream(DataError errorLevel, NavOutStream destination, NavInStream source, int bytesToRead)
	{
		return CopyStream(errorLevel, destination, source, bytesToRead, useBytesToRead: true);
	}

	private static bool CopyStream(DataError errorLevel, NavOutStream destination, NavInStream source, int bytesToRead, bool useBytesToRead)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		try
		{
			if (useBytesToRead && bytesToRead < 0)
			{
				throw new NavNCLOutsidePermittedRangeException("COPYSTREAM", 3, bytesToRead, 0, int.MaxValue);
			}
			Stream internalStreamChecked = source.InternalStreamChecked;
			Stream internalStreamChecked2 = destination.InternalStreamChecked;
			int num = ((!useBytesToRead) ? NCLStreamHelper.CopyStream(internalStreamChecked, destination.InternalStreamCheckedForWrite) : NCLStreamHelper.CopyStream(internalStreamChecked, (uint)bytesToRead, destination.InternalStreamCheckedForWrite));
			if (internalStreamChecked.CanSeek)
			{
				source.Position = (int)internalStreamChecked.Position;
			}
			else
			{
				source.Position += num;
			}
			if (internalStreamChecked2.CanSeek)
			{
				destination.Position = (int)internalStreamChecked2.Position;
			}
			else
			{
				destination.Position += num;
			}
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
		catch (Exception ex2) when (ex2 is IOException || ex2 is NotSupportedException || ex2 is ObjectDisposedException)
		{
			return false;
		}
		return true;
	}

	public static bool ALIsNullGuid(Guid guid)
	{
		if (Guid.Empty.CompareTo(guid) == 0)
		{
			return true;
		}
		return false;
	}

	public static NavValue EvaluateIntoNavValue(DataError dataError, INavValueMetadata metadata, string source, int number)
	{
		if (metadata == null)
		{
			throw new ArgumentNullException("metadata");
		}
		NavValueEvaluator evaluator = NavValueEvaluator.GetEvaluator(metadata.NclType);
		if (evaluator != null)
		{
			if (!evaluator.Evaluate(dataError, out var value, metadata, source, number))
			{
				return null;
			}
			return value;
		}
		if (dataError == DataError.TrapError)
		{
			return null;
		}
		throw NavNCLNotSupportedTypeException.CreateUnknownType(CultureInfo.CurrentCulture, metadata.NavType.ToString());
	}

	public static bool ALEvaluate(DataError dataError, ByRef<int> passByRefValue, string source, int number = 0)
	{
		int value = 0;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<long> passByRefValue, string source, int number = 0)
	{
		long value = 0L;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<Decimal18> passByRefValue, string source, int number = 0)
	{
		Decimal18 value = Decimal18.Zero;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<byte> passByRefValue, string source, int number = 0)
	{
		byte value = 0;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<char> passByRefValue, string source, int number = 0)
	{
		char value = '\0';
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<bool> passByRefValue, string source, int number = 0)
	{
		bool value = false;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<Guid> passByRefValue, string source, int number = 0)
	{
		Guid value = Guid.Empty;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<FormResult> passByRefValue, string source, int number = 0)
	{
		NavFormResult value = NavFormResult.Default;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value.Value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<TransactionType> passByRefValue, string source, int number = 0)
	{
		NavTransactionType value = NavTransactionType.Default;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value.Value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<TableConnectionType> passByRefValue, string source, int number = 0)
	{
		NavTableConnectionType value = NavTableConnectionType.Default;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value.Value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<NavObjectType> passByRefValue, string source, int number = 0)
	{
		NavEnumObjectType value = NavEnumObjectType.Default;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value.Value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<ExecutionMode> passByRefValue, string source, int number = 0)
	{
		NavExecutionMode value = NavExecutionMode.Default;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value.Value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<FieldClass> passByRefValue, string source, int number = 0)
	{
		NavFieldClass value = NavFieldClass.Default;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value.Value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<NavType> passByRefValue, string source, int number = 0)
	{
		NavFieldType value = NavFieldType.Default;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value.Value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<ExecutionContext> passByRefValue, string source, int number = 0)
	{
		NavExecutionContext value = NavExecutionContext.Default;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value.Value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<NavClientType> passByRefValue, string source, int number = 0)
	{
		NavEnumClientType value = NavEnumClientType.Default;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value.Value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<SecurityFiltering> passByRefValue, string source, int number = 0)
	{
		NavSecurityFiltering value = NavSecurityFiltering.Default;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value.Value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<TextEncoding> passByRefValue, string source, int number = 0)
	{
		NavEnumTextEncoding value = NavEnumTextEncoding.Default;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value.Value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<NavReportFormat> passByRefValue, string source, int number = 0)
	{
		NavEnumReportFormat value = NavEnumReportFormat.Default;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value.Value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<NavDefaultLayout> passByRefValue, string source, int number = 0)
	{
		NavEnumDefaultLayout value = NavEnumDefaultLayout.Default;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value.Value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<NavTestPermissions> passByRefValue, string source, int number = 0)
	{
		NavEnumTestPermissions value = NavEnumTestPermissions.Default;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value.Value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<DataClassification> passByRefValue, string source, int number = 0)
	{
		NavDataClassification value = NavDataClassification.Default;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value.Value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<ALErrorType> passByRefValue, string source, int number = 0)
	{
		NavALErrorType value = NavALErrorType.Default;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value.Value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<NavALErrorInfo> passByRefValue, string source, int number = 0)
	{
		NavALErrorInfo value = NavALErrorInfo.Default;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<WebServiceActionResultCode> passByRefValue, string source, int number = 0)
	{
		NavWebServiceActionResultCode value = NavWebServiceActionResultCode.Default;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value.Value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<DataScope> passByRefValue, string source, int number = 0)
	{
		NavEnumDataScope value = NavEnumDataScope.Default;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value.Value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<ALVerbosity> passByRefValue, string source, int number = 0)
	{
		NavVerbosity value = NavVerbosity.Default;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value.Value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<TestTransactionModel> passByRefValue, string source, int number = 0)
	{
		NavTestTransactionModel value = NavTestTransactionModel.Default;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value.Value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<NotificationInfo> passByRefValue, string source, int number = 0)
	{
		NavNotification value = NavNotification.Default;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value.NotificationInfo;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<NotificationScope> passByRefValue, string source, int number = 0)
	{
		NavNotificationScope value = NavNotificationScope.Default;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value.Value;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ByRef<NavSessionSettings> passByRefValue, string source, int number = 0)
	{
		NavSessionSettings value = NavSessionSettings.Default;
		if (!ALEvaluate(dataError, ref value, source, number))
		{
			return false;
		}
		passByRefValue.Value = value;
		return true;
	}

	public static bool ALEvaluate<T>(DataError dataError, ByRef<T> passByRefValue, string source, int number = 0) where T : NavValue
	{
		NavValue value = passByRefValue.Value;
		NavValue navValue = EvaluateIntoNavValue(dataError, value, source, number);
		if (navValue == null)
		{
			return false;
		}
		passByRefValue.Value = (T)navValue;
		return true;
	}

	public static bool ALEvaluate(DataError dataError, ref long value, string source, int number)
	{
		return NavBigIntegerEvaluator.Evaluate(dataError, out value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavBigText value, string source, int number)
	{
		return NavBigTextEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavTextBuilder value, string source, int number)
	{
		return NavTextBuilderEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref bool value, string source, int number)
	{
		return NavBooleanEvaluator.Evaluate(dataError, out value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref byte value, string source, int number)
	{
		return NavByteEvaluator.Evaluate(dataError, out value, source);
	}

	public static bool ALEvaluate(DataError dataError, ref char value, string source, int number)
	{
		return NavCharEvaluator.Evaluate(dataError, out value, source);
	}

	public static bool ALEvaluate(DataError dataError, ref NavOemCode value, string source, int number)
	{
		return NavOemCodeEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavCode value, string source, int number)
	{
		return NavCodeEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavDate value, string source, int number)
	{
		return NavDateEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavDateFormula value, string source, int number)
	{
		return NavDateFormulaEvaluator.Instance.Evaluate(dataError, out value, null, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavDateTime value, string source, int number)
	{
		return NavDateTimeEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref Decimal18 value, string source, int number)
	{
		return NavDecimalEvaluator.Evaluate(dataError, out value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavDuration value, string source, int number)
	{
		return NavDurationEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref Guid value, string source, int number)
	{
		return NavGuidEvaluator.Instance.Evaluate(dataError, out value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavMedia value, string source, int number)
	{
		return NavMediaEvaluator.Instance.Evaluate(dataError, out value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavMediaSet value, string source, int number)
	{
		return NavMediaSetEvaluator.Instance.Evaluate(dataError, out value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref int value, string source, int number)
	{
		return NavIntegerEvaluator.Evaluate(dataError, out value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavOption value, string source, int number)
	{
		return NavOptionEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavRecordId value, string source, int number)
	{
		return NavRecordIdEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavTableFilter value, string source, int number)
	{
		return NavTableFilterEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavOemText value, string source, int number)
	{
		return NavOemTextEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavText value, string source, int number)
	{
		return NavTextEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavTime value, string source, int number)
	{
		return NavTimeEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavTableConnectionType value, string source, int number)
	{
		return NavTableConnectionTypeEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavDataClassification value, string source, int number)
	{
		return NavDataClassificationEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavALErrorType value, string source, int number)
	{
		return NavALErrorTypeEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavALErrorInfo value, string source, int number)
	{
		return NavALErrorInfoEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavWebServiceActionResultCode value, string source, int number)
	{
		return NavWebServiceActionResultCodeEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavEnumDataScope value, string source, int number)
	{
		return NavDataScopeEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavVerbosity value, string source, int number)
	{
		return NavVerbosityEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavTestTransactionModel value, string source, int number)
	{
		return NavTestTransactionModelEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavTransactionType value, string source, int number)
	{
		return NavTransactionTypeEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavFormResult value, string source, int number)
	{
		return NavFormResultEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavExecutionMode value, string source, int number)
	{
		return NavExecutionModeEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavFieldClass value, string source, int number)
	{
		return NavFieldClassEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavFieldType value, string source, int number)
	{
		return NavFieldTypeEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavExecutionContext value, string source, int number)
	{
		return NavExecutionContextEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavSecurityFiltering value, string source, int number)
	{
		return NavSecurityFilteringEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavEnumClientType value, string source, int number)
	{
		return NavEnumClientTypeEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavEnumObjectType value, string source, int number)
	{
		return NavEnumObjectTypeEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavEnumTextEncoding value, string source, int number)
	{
		return NavEnumTextEncodingEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavEnumReportFormat value, string source, int number)
	{
		return NavEnumReportFormatEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavEnumDefaultLayout value, string source, int number)
	{
		return NavEnumDefaultLayoutEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavEnumTestPermissions value, string source, int number)
	{
		return NavEnumTestPermissionsEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavVariant value, string source, int number)
	{
		if (dataError == DataError.TrapError)
		{
			return false;
		}
		throw NavNCLNotSupportedTypeException.CreateUnknownType(CultureInfo.CurrentCulture, source);
	}

	public static bool ALEvaluate(DataError dataError, ref NavBLOB value, string source, int number)
	{
		return NavBLOBEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavNotificationScope value, string source, int number)
	{
		return NavEnumNotificationScopeEnumEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavNotification value, string source, int number)
	{
		return NavNotificationEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool ALEvaluate(DataError dataError, ref NavSessionSettings value, string source, int number)
	{
		return NavSessionSettingsEvaluator.Instance.Evaluate(dataError, out value, value, source, number);
	}

	public static bool IsClear(NavAutomation automation)
	{
		if (automation == null)
		{
			throw new ArgumentNullException("automation");
		}
		return automation.IsClear;
	}

	public static bool Create(DataError errorLevel, NavAutomation automation)
	{
		return Create(errorLevel, automation, newServer: false);
	}

	public static bool Create(DataError errorLevel, NavAutomation automation, bool newServer)
	{
		if (automation == null)
		{
			throw new ArgumentNullException("automation");
		}
		return automation.Create(errorLevel, newServer);
	}

	public static void Clear(NavComplexValue complex)
	{
		complex?.Clear();
	}

	public static void Clear(NavSessionSettings value)
	{
		value?.Clear();
	}
}
