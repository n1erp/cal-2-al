using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavSqlReturnCode
{
	public const int ModuleDB = 19;

	public const int ModuleEval = 41;

	public const int ModuleNdbcs = 93;

	public const int ModuleNCLCSRT = 95;

	public const int ModuleDBDataError = 23;

	public const int ModuleALRun = 101;

	public const int MsgMsgCode = 65536;

	public const int ErrorCode = 16777216;

	public const int FatalCode = 33554432;

	public const int AllowableCode = 67108864;

	public const int Ok = 0;

	public const int LockTableBadParameter = 23397458;

	public const int EvalRangeError = 19464333;

	public const int ToManyKeyFields = 23396552;

	public const int MissingBeginTransaction = 18022407;

	public const int RecordNotFoundOnPrimaryKey = 18023402;

	public const int RecordNotFound = 85132272;

	public const int KeyNotFound = 85132271;

	public const int DuplicateKey = 85132273;

	public const int RecordChangedInDatabase = 85132286;

	public const int InvalidSearchExpression = 18023424;

	public const int MustDefineFormula = 18023430;

	public const int MustSpecifyAnInterval = 18023434;

	public const int NoFilter = 18023435;

	public const int TableIsEmpty = 18023438;

	public const int TableIsALInconsistent = 18023508;

	public const int FieldTypeMismatch = 18023443;

	public const int OnlyFlowFieldsAllowedInCallsToCalcFields = 18023494;

	public const int FindFailedWithFilter = 18023495;

	public const int SumAverageOnlySupportedForNumericTypes = 18023674;

	public const int FlowFieldNotBoolean = 18023675;

	public const int FieldIsNotTextType = 18023644;

	public const int FlowFieldNotInteger = 18023676;

	public const int IllegalCompanyName = 18023729;

	public const int CannotChangeObjectType = 18023770;

	public const int NotAWriteTransaction = 18023772;

	public const int CannotChangeTransactionType = 18023779;

	public const int ModifyOnIdentityField = 18023796;

	public const int ClientReadDenied = 18023807;

	public const int ClientInsertDenied = 18023808;

	public const int ClientDeleteDenied = 18023809;

	public const int ClientModifyDenied = 18023810;

	public const int ExecuteDenied = 18023698;

	public const int ExecuteObjDenied = 18023703;

	public const int NoWaitNotSupported = 22923076;

	public const int ConnectionLost = 22924076;

	public const int BadBlobMagic = 22926086;

	public const int LockTimeout = 22926090;

	public const int Deadlock = 22926089;

	public const int BadCSideData = 22928068;

	public const int SqlObjectNotFound = 22926068;

	public const int IncompatibleSqlType = 22928069;

	public const int BadCSideCodeData = 22928070;

	public const int BadSqlIntCodeData = 22928072;

	public const int TableErrorStr = 23004237;

	public const int TableEqualValue = 23004238;

	public const int FieldTypeIncompatible = 23004244;

	public const int SqlStatementProblemNeedToRecreateConnection = 20;

	public const int SqlRollbackSeverity = 13;

	public const int SqlErrorCannotEstablishConnection = 4060;

	public const int SqlErrorShutdownInProgress = 6005;

	public const int SqlErrorTimeout = -2;

	public const int SqlErrorLockTimeout = 1222;

	public const int SqlErrorLockReleaseNotHeld = 1223;

	public const int SqlErrorDeadlock = 1205;

	public const int SqlErrorUniqueIndexViolation = 2601;

	public const int SqlErrorUniqueConstraintViolation = 2627;

	public const int SqlErrorObjectAlreadyExists = 2714;

	public const int SqlErrorIdentityModified = 8102;

	public const int TransportLevelTransientError = 10054;

	public const int DatabaseStateCannotBeChanged = 5070;

	public const int SqlAzureErrorAlterDatabaseInProgress = 40626;

	internal static readonly int[] SqlAzureOperationCanBeRetried = new int[3] { 40197, 40613, 40626 };

	internal static readonly int[] SqlAzureOperationCanBeRetriedAfterReopen = new int[1] { 40626 };

	internal static readonly int[] SqlAzureOperationMustBeShortened = new int[5] { 40549, 40550, 40551, 40552, 40553 };

	internal static readonly int[] SqlAzureKnownIssue = new int[1] { 40545 };

	internal static readonly int[] SqlAzureNeedDecoding = new int[3] { 40501, 40544, 40545 };

	internal static readonly int[] SqlAzureTransientError = new int[19]
	{
		40197, 40501, 10053, 10054, 10060, 40613, 40143, 233, 64, 20,
		40626, 4060, 49918, 49919, 49920, 10928, 10929, 11001, 3971
	};

	internal static bool IsSqlAzureConnectionLimitReached(int errorNumber)
	{
		if (errorNumber != 10928)
		{
			return errorNumber == 10929;
		}
		return true;
	}

	internal static bool IsTransientError(int errorNumber)
	{
		return SqlAzureTransientError.Any((int transientErrorNumber) => errorNumber == transientErrorNumber);
	}
}
