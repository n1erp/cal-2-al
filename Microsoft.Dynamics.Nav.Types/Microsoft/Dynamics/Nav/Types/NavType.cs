using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public enum NavType
{
	[EnumMember]
	None = 0,
	Action = 26120,
	[EnumMember]
	Automation = 1,
	[EnumMember]
	DotNet = 2,
	[EnumMember]
	BigInteger = 36096,
	[EnumMember]
	BigText = 37888,
	[EnumMember]
	NotSupported_Binary = 33792,
	[EnumMember]
	BLOB = 33794,
	[EnumMember]
	Boolean = 34048,
	[EnumMember]
	Byte = 34816,
	[EnumMember]
	OemCode = 35072,
	[EnumMember]
	Codeunit = 26368,
	[EnumMember]
	Connection = 49408,
	[EnumMember]
	Date = 11776,
	[EnumMember]
	DateFormula = 11798,
	[EnumMember]
	DateTime = 37376,
	[EnumMember]
	Decimal = 12800,
	[EnumMember]
	Dialog = 26119,
	[EnumMember]
	Duration = 36864,
	[EnumMember]
	FieldRef = 26138,
	[EnumMember]
	FieldClass = 4870,
	[EnumMember]
	File = 26115,
	[EnumMember]
	Form = 26117,
	[EnumMember]
	GUID = 37120,
	[EnumMember]
	InStream = 33796,
	[EnumMember]
	Integer = 34560,
	[EnumMember]
	KeyRef = 26139,
	[EnumMember]
	Option = 35584,
	[EnumMember]
	OutStream = 33798,
	[EnumMember]
	Record = 26114,
	[EnumMember]
	RecordID = 4989,
	[EnumMember]
	RecordRef = 26137,
	[EnumMember]
	Report = 4876,
	[EnumMember]
	TableFilter = 4913,
	[EnumMember]
	OemText = 11520,
	[EnumMember]
	TextConst = 26127,
	[EnumMember]
	Time = 11777,
	[EnumMember]
	TransactionType = 4911,
	[EnumMember]
	TypeId = 2304,
	[EnumMember]
	XmlPort = 5018,
	[EnumMember]
	Query = 5034,
	[EnumMember]
	Variant = 30976,
	[EnumMember]
	Text = 31489,
	[EnumMember]
	Char = 31488,
	[EnumMember]
	Code = 31490,
	[EnumMember]
	TestPage = 26147,
	[EnumMember]
	ExecutionMode = 26156,
	[EnumMember]
	ExecutionContext = 26157,
	[EnumMember]
	SecurityFiltering = 26158,
	[EnumMember]
	ClientType = 26160,
	[EnumMember]
	TableConnectionType = 26192,
	[EnumMember]
	FilterPageBuilder = 26194,
	[EnumMember]
	Media = 26208,
	[EnumMember]
	MediaSet = 26209,
	[EnumMember]
	Notification = 26210,
	[EnumMember]
	NotificationScope = 26211,
	[EnumMember]
	SessionSettings = 26222,
	[EnumMember]
	Version = 26223,
	[EnumMember]
	InstalledAppInfo = 26224,
	[EnumMember]
	AppDependencyInfo = 26225,
	[EnumMember]
	String = 9999999,
	[EnumMember]
	Stream = 9999998,
	[EnumMember]
	ClrByteArray = 9999997,
	[EnumMember]
	RecordReference = 9999996,
	[EnumMember]
	CodeunitReference = 9999995,
	[EnumMember]
	FormReference = 9999994,
	[EnumMember]
	ReportReference = 9999993,
	[EnumMember]
	XmlPortReference = 9999992,
	[EnumMember]
	QueryReference = 9999991,
	[EnumMember]
	TestPageReference = 9999990,
	[EnumMember]
	TestField = 9999989,
	[EnumMember]
	TestAction = 9999988,
	[EnumMember]
	ObjectType = 26161,
	[EnumMember]
	TextEncoding = 26159,
	[EnumMember]
	DataScope = 26162,
	[EnumMember]
	ReportFormat = 26189,
	[EnumMember]
	DefaultLayout = 26190,
	[EnumMember]
	TestPermissions = 26219,
	[EnumMember]
	DataClassification = 26228,
	[EnumMember]
	Verbosity = 26229,
	[EnumMember]
	TestTransactionModel = 26230,
	[EnumMember]
	PageExtension = 9999987,
	[EnumMember]
	TableExtension = 9999986,
	[EnumMember]
	HttpClient = 9999970,
	[EnumMember]
	HttpRequestMessage = 9999971,
	[EnumMember]
	HttpResponseMessage = 9999972,
	[EnumMember]
	HttpContent = 9999973,
	[EnumMember]
	HttpHeaders = 9999974,
	[EnumMember]
	JsonToken = 9999975,
	[EnumMember]
	JsonValue = 9999976,
	[EnumMember]
	JsonObject = 9999977,
	[EnumMember]
	JsonArray = 9999978,
	[EnumMember]
	TextBuilder = 9999979,
	[EnumMember]
	XmlAttribute = 76801,
	[EnumMember]
	XmlAttributeCollection = 76802,
	[EnumMember]
	XmlComment = 76803,
	[EnumMember]
	XmlCData = 76804,
	[EnumMember]
	XmlDeclaration = 76805,
	[EnumMember]
	XmlDocument = 76806,
	[EnumMember]
	XmlDocumentType = 76807,
	[EnumMember]
	XmlElement = 76808,
	[EnumMember]
	XmlNameTable = 76809,
	[EnumMember]
	XmlNamespaceManager = 76810,
	[EnumMember]
	XmlNode = 76811,
	[EnumMember]
	XmlNodeList = 76812,
	[EnumMember]
	XmlProcessingInstruction = 76813,
	[EnumMember]
	XmlText = 76814,
	[EnumMember]
	XmlReadOptions = 76815,
	[EnumMember]
	XmlWriteOptions = 76816,
	[EnumMember]
	List = 77057,
	[EnumMember]
	Dictionary = 77058,
	[EnumMember]
	WebServiceActionContext = 77313,
	[EnumMember]
	WebServiceActionResultCode = 77314,
	[EnumMember]
	ALErrorInfo = 77315,
	[EnumMember]
	ALErrorType = 77316,
	[EnumMember]
	Enum = 77569
}
