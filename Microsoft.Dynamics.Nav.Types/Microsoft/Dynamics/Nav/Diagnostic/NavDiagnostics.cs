using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Diagnostic;

public sealed class NavDiagnostics
{
	internal class ExceptionTelemetryMessages
	{
		private readonly StringBuilder transientTelemetryBuilder;

		private readonly StringBuilder nonTransientTelemetryBuilder;

		public string TransientTelemetry => transientTelemetryBuilder.ToString();

		public string NonTransientTelemetry => nonTransientTelemetryBuilder.ToString();

		public ExceptionTelemetryMessages()
		{
			transientTelemetryBuilder = new StringBuilder();
			nonTransientTelemetryBuilder = new StringBuilder();
		}

		public ExceptionTelemetryMessages AppendLine(string message)
		{
			transientTelemetryBuilder.AppendLine(message);
			nonTransientTelemetryBuilder.AppendLine(message);
			return this;
		}

		public ExceptionTelemetryMessages AppendLine(DiagnosticParameter message)
		{
			string value = string.Format(CultureInfo.InvariantCulture, "{0}", message);
			if (!string.IsNullOrEmpty(value))
			{
				transientTelemetryBuilder.AppendLine(value);
				if (!PrivacyHelper.IsPersonalOrRestrictedData(message))
				{
					nonTransientTelemetryBuilder.AppendLine(value);
				}
			}
			return this;
		}

		public ExceptionTelemetryMessages AppendException(Exception exception, string diagnosticsContext, string callStack, ref bool couldHavePersonalData)
		{
			AggregateException ex = exception as AggregateException;
			if (ex?.InnerExceptions != null)
			{
				transientTelemetryBuilder.AppendLine(ex.Message);
				foreach (Exception innerException in ex.InnerExceptions)
				{
					AppendLine("AggregatedException:").AppendException(innerException, diagnosticsContext, null, ref couldHavePersonalData);
				}
				if (!string.IsNullOrEmpty(callStack))
				{
					AppendLine("CallerStackTrace:").AppendLine(callStack);
				}
				return this;
			}
			if (exception.InnerException == null)
			{
				AppendLine("RootException: " + exception.GetType().Name);
			}
			else
			{
				AppendLine("ParentException: " + exception.GetType().Name);
			}
			if (exception is NavBaseException ex2)
			{
				couldHavePersonalData |= ex2.ContainsPersonalOrRestrictedInformation;
				nonTransientTelemetryBuilder.AppendLine(ex2.MessageWithoutPrivateInformation);
			}
			else if (WhitelistedTypes.Contains(exception.GetType().Name))
			{
				nonTransientTelemetryBuilder.AppendLine(exception.Message);
			}
			else
			{
				couldHavePersonalData = true;
			}
			if (!string.IsNullOrEmpty(exception.Message))
			{
				transientTelemetryBuilder.AppendLine(exception.Message);
			}
			if (diagnosticsContext != null)
			{
				AppendLine(diagnosticsContext);
			}
			if (!string.IsNullOrEmpty(exception.StackTrace))
			{
				AppendLine("ExceptionStackTrace:").AppendLine(exception.StackTrace);
			}
			if (exception.InnerException != null)
			{
				AppendLine("InnerException:");
				AppendException(exception.InnerException, null, null, ref couldHavePersonalData);
			}
			if (!string.IsNullOrEmpty(callStack))
			{
				AppendLine("CallerStackTrace:").AppendLine(callStack);
			}
			return this;
		}
	}

	private sealed class TraceScopeMonitorCollection : MonitorCollection, ITraceScope, IDisposable
	{
		private readonly NavDiagnostics diagnosticsParent;

		private readonly string tag;

		private readonly string name;

		private readonly PrivacyClassification classification;

		private readonly Category category;

		private readonly Verbosity verbosity;

		private readonly Stopwatch executionTimer;

		private readonly Stopwatch suspendedTimer;

		private readonly long globalSqlStatementsExecutedStart;

		private readonly long globalSqlRowsReadStart;

		private readonly NavSqlDebuggingStatistics sqlDebuggingStatistics;

		private string resultString;

		private readonly bool verbosityWithinTraceLevel;

		private readonly ITraceScope ifxTraceScope;

		internal static readonly AsyncLocal<TraceScopeMonitorCollection> CurrentScope = new AsyncLocal<TraceScopeMonitorCollection>();

		private readonly TraceScopeMonitorCollection parentScope;

		public Guid ScopeActivityId { get; }

		public TraceScopeMonitorCollection(NavDiagnostics diagnosticsParent, string tag, DiagnosticParameter name, MonitorType monitortypes, Category category, bool createNewActivity, Verbosity verbosity)
		{
			name = name ?? DiagnosticParameter.SystemMetadata(null);
			string value = (this.name = name.Content?.ToString());
			this.category = category;
			this.verbosity = verbosity;
			classification = name.Classification;
			this.diagnosticsParent = diagnosticsParent ?? throw new ArgumentNullException("diagnosticsParent");
			verbosityWithinTraceLevel = this.diagnosticsParent.EmittingEnabledForVerbosity(verbosity);
			sqlDebuggingStatistics = SqlDebuggingStatistics;
			if (sqlDebuggingStatistics != null)
			{
				globalSqlStatementsExecutedStart = sqlDebuggingStatistics.GlobalSqlStatementsExecuted;
				globalSqlRowsReadStart = sqlDebuggingStatistics.GlobalSqlRowsRead;
			}
			parentScope = CurrentScope.Value;
			CurrentScope.Value = this;
			if (IfxTelemetryProvider != null)
			{
				try
				{
					ifxTraceScope = IfxTelemetryProvider.CreateScope(ClientActivityId, this.name);
					SetResult(NavDiagnosticsScopeResult.Success);
				}
				catch (RemotingException ex)
				{
					ifxTraceScope = null;
					this.diagnosticsParent.SendExceptionTag("00009CT", Category.Telemetry, ex, Verbosity.Warning, DiagnosticParameter.SystemMetadata(ex.Message));
				}
			}
			if (!verbosityWithinTraceLevel)
			{
				return;
			}
			executionTimer = new Stopwatch();
			suspendedTimer = new Stopwatch();
			if (name.Classification < PrivacyClassification.CustomerSupportData)
			{
				value = "nameWasClassified";
			}
			if (tag == null)
			{
				this.diagnosticsParent.TraceFailureSendTraceTagWithStackTrace("00001RZ", "A null tag made it into TraceScopeMonitorCollection");
			}
			this.tag = tag;
			if (string.IsNullOrEmpty(value))
			{
				this.diagnosticsParent.TraceFailureSendTraceTag("00001S0", "A null or empty name made it into TraceScopeMonitorCollection.", tag);
			}
			if (createNewActivity)
			{
				ScopeActivityId = Guid.NewGuid();
				if (this.diagnosticsParent.EmittingEnabledForVerbosity(Verbosity.Verbose))
				{
					TraceWriter?.WriteTelemetryActivityTransfer(new TelemetryActivityTransferEvent
					{
						Tag = tag,
						Category = EnumHelper<Category>.DefinedEnumToString(category),
						NavTenantId = this.diagnosticsParent.navTenantId,
						EventTime = DateTime.UtcNow,
						NewActivityId = ScopeActivityId,
						OldActivityId = this.diagnosticsParent.ActivityId,
						ProcessId = ProcessId,
						SessionId = this.diagnosticsParent.SessionUniqueId,
						ThreadId = Thread.CurrentThread.ManagedThreadId,
						AadTenantId = this.diagnosticsParent.aadTenantId
					});
				}
			}
			this.diagnosticsParent.SendTraceTag(tag, category, verbosity, "Starting scope \"{0}\".", name);
			executionTimer.Start();
		}

		internal void SuspendTimer()
		{
			if (verbosityWithinTraceLevel)
			{
				diagnosticsParent.SendTraceTag("000020F", category, verbosity, "Suspend \"{0}\".", DiagnosticParameter.SystemMetadata(name));
				suspendedTimer.Start();
			}
			parentScope?.SuspendTimer();
		}

		internal void ResumeTimer()
		{
			if (verbosityWithinTraceLevel)
			{
				diagnosticsParent.SendTraceTag("000020G", category, verbosity, "Resume \"{0}\".", DiagnosticParameter.SystemMetadata(name));
				suspendedTimer.Stop();
			}
			parentScope?.ResumeTimer();
		}

		public void Dispose()
		{
			if (verbosityWithinTraceLevel)
			{
				executionTimer.Stop();
				suspendedTimer.Stop();
				StringBuilder stringBuilder = new StringBuilder(100);
				stringBuilder.Append("Execution time = ").Append(executionTimer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture));
				stringBuilder.Append(", Exclusive time = ").Append((executionTimer.ElapsedMilliseconds - suspendedTimer.ElapsedMilliseconds).ToString(CultureInfo.InvariantCulture));
				if (sqlDebuggingStatistics != null)
				{
					long num = sqlDebuggingStatistics.GlobalSqlStatementsExecuted - globalSqlStatementsExecutedStart;
					long num2 = sqlDebuggingStatistics.GlobalSqlRowsRead - globalSqlRowsReadStart;
					if (num > 0 || num2 > 0)
					{
						stringBuilder.Append(", SQL Executes = ").Append(num.ToString(CultureInfo.InvariantCulture));
						stringBuilder.Append(", SQL Rows Read = ").Append(num2.ToString(CultureInfo.InvariantCulture));
					}
				}
				if (!string.IsNullOrEmpty(resultString))
				{
					stringBuilder.Append(", Result = ").Append(resultString);
				}
				diagnosticsParent.SendClassifiedTraceTag(tag, stringBuilder.ToString(), EnumHelper<Category>.DefinedEnumToString(category), verbosity, ClientSessionId, ClientActivityId, classification, "Closing scope \"{0}\".", name);
			}
			ifxTraceScope?.Dispose();
			CurrentScope.Value = parentScope;
		}

		public void SetResult(NavDiagnosticsScopeResult result)
		{
			if (verbosityWithinTraceLevel)
			{
				resultString = EnumHelper<NavDiagnosticsScopeResult>.DefinedEnumToString(result);
			}
			ifxTraceScope?.SetResult(result);
		}

		public void SetResult(Exception exception)
		{
			if (verbosityWithinTraceLevel)
			{
				resultString = exception.GetType().Name;
			}
			ifxTraceScope?.SetResult(exception);
		}
	}

	private static readonly int ProcessId = Process.GetCurrentProcess().Id;

	private static readonly bool IsServerProcess = GetIsServerProcess();

	private static readonly NavMetric3Dimensions TelemetryTagMetric = new NavMetric3Dimensions("NstTelemetryTag", "Tag", "AadTenantId", "TraceLevel");

	private static NavDiagnostics globalInstance;

	private static Func<NavDiagnostics> getMostSpecificInstanceGetter;

	public const string PersonalDataReplacementMessage = "This message was personal data and hence removed. The original may still be in transient telemetry. Find it using the transientTelemetryId.";

	public const string PersonalDataRemovedMessage = "This message had personal data removed. The original may still be in transient telemetry. Find it using the transientTelemetryId.";

	public static readonly string NoAadTenantId = Guid.Empty.ToString("D");

	private object endpointClientLockObject = new object();

	private static readonly DiagnosticParameter RedactedDiagnosticParameter = DiagnosticParameter.SystemMetadata("<redacted>");

	private readonly MonitorCollection monitorCollection;

	private readonly string alternateTenantIds;

	private readonly string navTenantId;

	private readonly string aadTenantId;

	private readonly string aadUserId;

	private Verbosity traceLevelOverride;

	private Task overrideTraceLevelTask;

	private Task enableFullALTracingTask;

	private static readonly IReadOnlyCollection<string> WhitelistedTypes = new HashSet<string>
	{
		"AccountIsLockedException", "ArgumentException", "ArgumentNullException", "AutodiscoverLocalException", "AutodiscoverRemoteException", "CommunicationException", "DuplicateNameException", "EventSourceException", "HttpException", "HttpRequestException",
		"HttpResponseException", "IndexOutOfRangeException", "InvalidCastException", "InvalidOperationException", "JsonException", "JsonReaderException", "JsonSerializationException", "KeyVaultClientException", "LicenseServiceFatalException", "NavSessionTerminatedException",
		"NotSupportedException", "NullReferenceException", "ObjectDisposedException", "ProtocolException", "ProtocolViolationException", "RemotingException", "SecurityTokenExpiredException", "SerializationException", "ServerTooBusyException", "ServiceBrokerException",
		"ServiceLocalException", "ServiceUnknownException", "ServiceResponseException", "SoapHeaderException", "SocketException", "TaskCanceledException", "TimeoutException", "TimeZoneNotFoundException", "WebException", "XmlException",
		"XPathException"
	};

	public static ITelemetryProvider IfxTelemetryProvider { get; set; }

	public static ITelemetryLogWriter TraceWriter { get; set; }

	public Verbosity TraceLevel => GetTraceLevel();

	public bool FullALFunctionTracingEnabled
	{
		get
		{
			if (enableFullALTracingTask == null || enableFullALTracingTask.IsCompleted)
			{
				return ServerUserSettings.Instance.EnableFullALFunctionTracing.Value;
			}
			return true;
		}
	}

	public static Func<NavDiagnostics> GetMostSpecificInstanceGetter
	{
		get
		{
			return getMostSpecificInstanceGetter;
		}
		internal set
		{
			getMostSpecificInstanceGetter = value;
		}
	}

	public static NavDiagnostics GetMostSpecificInstance => GetMostSpecificInstanceGetter?.Invoke() ?? GlobalInstance;

	public static NavDiagnostics GlobalInstance => globalInstance ?? (globalInstance = new NavDiagnostics(null, string.Empty, Guid.Empty, MonitorType.None, singleThreadMonitorAccess: false, string.Empty, string.Empty, string.Empty));

	internal static bool DoNotAssertOnFailure { get; set; }

	internal static Action<TelemetryEvent> TestTelemetryEventListener { get; set; }

	public Guid ActivityId { get; }

	public Guid SessionUniqueId { get; }

	public static Guid ClientSessionId => TypesThreadLocalStorage.Current?.ClientSessionId ?? Guid.Empty;

	public static Guid ClientActivityId => TypesThreadLocalStorage.Current?.ClientActivityId ?? Guid.Empty;

	public static NavSqlDebuggingStatistics SqlDebuggingStatistics => TypesThreadLocalStorage.Current?.SqlDebuggingStatics;

	public bool VerboseTraceLevelEnabled
	{
		get
		{
			if (overrideTraceLevelTask == null || overrideTraceLevelTask.IsCompleted)
			{
				return ServerUserSettings.Instance.TraceLevel.Value == Verbosity.Verbose;
			}
			return true;
		}
	}

	public void OverrideTraceLevel(Verbosity newTraceLevel, TimeSpan duration)
	{
		if (overrideTraceLevelTask == null)
		{
			traceLevelOverride = newTraceLevel;
			overrideTraceLevelTask = Task.Delay(duration).ContinueWith((Task t) =>
			{
				this?.ResetTraceLevel();
			});
		}
	}

	public void ResetTraceLevel()
	{
		overrideTraceLevelTask = null;
	}

	public void EnableFullALTracing(TimeSpan duration)
	{
		enableFullALTracingTask = Task.Delay(duration).ContinueWith((Task t) =>
		{
			this?.ResetFullALTracing();
		});
	}

	public void ResetFullALTracing()
	{
		enableFullALTracingTask = null;
	}

	public static NavDiagnostics CreateSectionDiagnostics(NavDiagnostics parent, MonitorType monitorTypes, bool singleThreadMonitorAccess)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		return CreateSectionDiagnostics(parent, parent.navTenantId, monitorTypes, singleThreadMonitorAccess, parent.alternateTenantIds, parent.aadTenantId, parent.aadUserId);
	}

	public static NavDiagnostics CreateSectionDiagnostics(NavDiagnostics parent, string navTenantId, MonitorType monitorTypes, bool singleThreadMonitorAccess, string alternateTenantIds, string aadTenantId, string aadUserId)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		return new NavDiagnostics(parent, navTenantId, Guid.NewGuid(), monitorTypes, singleThreadMonitorAccess, alternateTenantIds, aadTenantId, aadUserId);
	}

	public static NavDiagnostics CreateScopedDiagnostics(NavDiagnostics parent, Guid sessionUniqueId, MonitorType monitorTypes, bool singleThreadAccess)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		return CreateScopedDiagnostics(parent, sessionUniqueId, monitorTypes, singleThreadAccess, parent.alternateTenantIds, parent.aadUserId);
	}

	public static NavDiagnostics CreateScopedDiagnostics(NavDiagnostics parent, Guid sessionUniqueId, MonitorType monitorTypes, bool singleThreadAccess, string alternateTenantIds, string aadUserId)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		return new NavDiagnostics(parent, parent.navTenantId, sessionUniqueId, monitorTypes, singleThreadAccess, alternateTenantIds, parent.aadTenantId, aadUserId);
	}

	private NavDiagnostics(NavDiagnostics parent, string navTenantId, Guid sessionId, MonitorType monitorTypes, bool singleThreadMonitorAccess, string alternateTenantIds, string aadTenantId, string aadUserId)
	{
		if (alternateTenantIds == null)
		{
			throw new ArgumentNullException("alternateTenantIds");
		}
		this.navTenantId = navTenantId ?? throw new ArgumentNullException("navTenantId");
		SessionUniqueId = sessionId;
		ActivityId = Guid.NewGuid();
		monitorCollection = new MonitorCollection(parent?.monitorCollection, monitorTypes, singleThreadMonitorAccess);
		this.alternateTenantIds = ((alternateTenantIds.Length != 0) ? alternateTenantIds : (parent?.alternateTenantIds ?? string.Empty));
		this.aadTenantId = aadTenantId;
		this.aadUserId = aadUserId;
	}

	public void SendTraceTag(string tag, Category category, Verbosity verbosity, PrivacyClassification classification, string message)
	{
		SendClassifiedTraceTag(tag, null, EnumHelper<Category>.DefinedEnumToString(category), verbosity, ClientSessionId, ClientActivityId, classification, message);
	}

	public void SendTraceTag(string tag, Category category, Verbosity verbosity, PrivacyClassification classification, string message, params object[] parameters)
	{
		SendClassifiedTraceTag(tag, null, EnumHelper<Category>.DefinedEnumToString(category), verbosity, ClientSessionId, ClientActivityId, classification, message, parameters);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public void SendTraceTag(string tag, string category, Verbosity verbosity, DataClassification classification, string message)
	{
		PrivacyClassification classification2 = classification.ConvertDataClassificationToPrivacyClassification();
		SendClassifiedTraceTag(tag, null, category, verbosity, ClientSessionId, ClientActivityId, classification2, PrivacyHelper.MarkIfDataShouldBeScrubbed(message, classification2));
	}

	public void SendTraceTagFromManagementEndPoint(string tag, Category category, Verbosity verbosity, Guid clientSessionId, Guid clientActivityId, PrivacyClassification classification, string message, params object[] parameters)
	{
		lock (endpointClientLockObject)
		{
			SendClassifiedTraceTag(tag, null, EnumHelper<Category>.DefinedEnumToString(category), verbosity, clientSessionId, clientActivityId, classification, PrivacyHelper.MarkIfDataShouldBeScrubbed(message, classification), parameters);
		}
	}

	public void SendTraceTag(string tag, Category category, Verbosity verbosity, string message, DiagnosticParameter firstParameter, params DiagnosticParameter[] parameters)
	{
		if (TraceWriter != null && EmittingEnabledForVerbosity(verbosity))
		{
			DiagnosticParameter[] array = new DiagnosticParameter[1 + parameters.Length];
			array[0] = firstParameter;
			parameters.CopyTo(array, 1);
			SendTraceTagDiagParams(tag, category, verbosity, message, array);
		}
	}

	[Conditional("DEBUG")]
	public void SendDebugTraceTag(string tag, Category category, Verbosity verbosity, string message, params object[] parameters)
	{
		SendTraceTagCore(tag, null, EnumHelper<Category>.DefinedEnumToString(category), verbosity, ClientSessionId, ClientActivityId, Guid.Empty, navTenantId, PrivacyClassification.SystemMetadata, message, parameters);
	}

	public Exception SendExceptionTag(string tag, Category category, Exception exception, Verbosity verbosity = Verbosity.Error, DiagnosticParameter message = null, string diagnosticsContext = null, bool forceLog = false)
	{
		return ComposeAndSendExceptionTraceTag(tag, category, exception, message, verbosity, ClientSessionId, ClientActivityId, diagnosticsContext, forceLog);
	}

	public bool SendExceptionTagExceptionFilter(string tag, Category category, Exception exception, Verbosity verbosity = Verbosity.Error, DiagnosticParameter message = null, string diagnosticsContext = null, bool forceLog = false)
	{
		SendExceptionTag(tag, category, exception, verbosity, message, diagnosticsContext, forceLog);
		return false;
	}

	public Exception SendExceptionTagFromManagementEndPointClient(string tag, Category category, Exception exception, Guid clientSessionId, Guid clientActivityId, Verbosity verbosity = Verbosity.Error, DiagnosticParameter message = null, bool forceLog = false)
	{
		lock (endpointClientLockObject)
		{
			return ComposeAndSendExceptionTraceTag(tag, category, exception, message, verbosity, clientSessionId, clientActivityId, null, forceLog);
		}
	}

	private void SendTraceTagDiagParams(string tag, Category category, Verbosity verbosity, string message, params DiagnosticParameter[] parameters)
	{
		PrivacyClassification privacyClassification = PrivacyClassification.Invalid;
		foreach (DiagnosticParameter parameter in parameters)
		{
			privacyClassification = GetMessagePrivacyClassification(tag, privacyClassification, parameter);
		}
		Guid transientTelemetryId = Guid.Empty;
		if (PrivacyHelper.IsPersonalOrRestrictedData(privacyClassification))
		{
			DiagnosticParameter redactedDiagnosticParameter = RedactedDiagnosticParameter;
			transientTelemetryId = Guid.NewGuid();
			object[] array = new object[parameters.Length];
			for (int j = 0; j < parameters.Length; j++)
			{
				array[j] = (PrivacyHelper.IsPersonalOrRestrictedData(parameters[j]) ? redactedDiagnosticParameter : parameters[j]);
			}
			string message2 = message + Environment.NewLine + "This message had personal data removed. The original may still be in transient telemetry. Find it using the transientTelemetryId.";
			SendTraceTagCore(tag, null, EnumHelper<Category>.DefinedEnumToString(category), verbosity, ClientSessionId, ClientActivityId, transientTelemetryId, navTenantId, PrivacyClassification.SystemMetadata, message2, array);
		}
		SendTraceTagCore(tag, null, EnumHelper<Category>.DefinedEnumToString(category), verbosity, ClientSessionId, ClientActivityId, transientTelemetryId, navTenantId, privacyClassification, message, parameters);
	}

	private PrivacyClassification GetMessagePrivacyClassification(string tag, PrivacyClassification currentClassification, DiagnosticParameter parameter)
	{
		if (parameter == null)
		{
			TraceFailureSendTraceTag("00001V1", "Parameter with null PrivacyClassification.", tag);
			return currentClassification;
		}
		if (parameter.Classification == PrivacyClassification.Invalid)
		{
			TraceFailureSendTraceTag("00001RR", "Parameter with Invalid PrivacyClassification.", tag);
			return PrivacyClassification.AccessControlData;
		}
		return (PrivacyClassification)Math.Min((int)currentClassification, (int)parameter.Classification);
	}

	public bool EmittingEnabledForVerbosity(Verbosity verbosity)
	{
		return verbosity <= TraceLevel;
	}

	public static void SuspendCurrentTimingScopeMeasurement()
	{
		TraceScopeMonitorCollection.CurrentScope.Value?.SuspendTimer();
	}

	public static void ResumeCurrentTimingScopeMeasurement()
	{
		TraceScopeMonitorCollection.CurrentScope.Value?.ResumeTimer();
	}

	public static bool SetTraceScopeResultExceptionFilter(ITraceScope scope, Exception exception)
	{
		scope.SetResult(exception);
		return false;
	}

	private void TraceFailureSendTraceTag(string tag, string message, string failingTag)
	{
		SendTraceTag(tag, Category.Telemetry, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Programming error. Telemetry tag {0}. {1}", failingTag, message);
	}

	internal void TraceFailureSendTraceTagWithStackTrace(string tag, string message)
	{
		TraceFailureSendTraceTag(tag, message + Environment.NewLine + "Stacktrace:" + Environment.NewLine + new System.Diagnostics.StackFrame(2, fNeedFileInfo: true), string.Empty);
	}

	private void SendClassifiedTraceTag(string tag, string counterInformation, string category, Verbosity verbosity, Guid clientSessionId, Guid clientActivityId, PrivacyClassification classification, string message)
	{
		if (TraceWriter != null && EmittingEnabledForVerbosity(verbosity))
		{
			if (PrivacyHelper.IsPersonalOrRestrictedData(classification))
			{
				Guid transientTelemetryId = Guid.NewGuid();
				SendTraceTagCore(tag, counterInformation, category, verbosity, clientSessionId, clientActivityId, transientTelemetryId, navTenantId, PrivacyClassification.SystemMetadata, "This message was personal data and hence removed. The original may still be in transient telemetry. Find it using the transientTelemetryId.");
				SendTraceTagCore(tag, counterInformation, category, verbosity, clientSessionId, clientActivityId, transientTelemetryId, navTenantId, classification, PrivacyHelper.MarkIfDataShouldBeScrubbed(message, classification));
			}
			else
			{
				SendTraceTagCore(tag, counterInformation, category, verbosity, clientSessionId, clientActivityId, Guid.Empty, navTenantId, classification, PrivacyHelper.MarkIfDataShouldBeScrubbed(message, classification));
			}
		}
	}

	private void SendClassifiedTraceTag(string tag, string counterInformation, string category, Verbosity verbosity, Guid clientSessionId, Guid clientActivityId, PrivacyClassification classification, string message, params object[] parameters)
	{
		if (TraceWriter != null && EmittingEnabledForVerbosity(verbosity))
		{
			if (PrivacyHelper.IsPersonalOrRestrictedData(classification))
			{
				Guid transientTelemetryId = Guid.NewGuid();
				SendTraceTagCore(tag, counterInformation, category, verbosity, clientSessionId, clientActivityId, transientTelemetryId, navTenantId, PrivacyClassification.SystemMetadata, "This message was personal data and hence removed. The original may still be in transient telemetry. Find it using the transientTelemetryId.", parameters);
				SendTraceTagCore(tag, counterInformation, category, verbosity, clientSessionId, clientActivityId, transientTelemetryId, navTenantId, classification, PrivacyHelper.MarkIfDataShouldBeScrubbed(message, classification), parameters);
			}
			else
			{
				SendTraceTagCore(tag, counterInformation, category, verbosity, clientSessionId, clientActivityId, Guid.Empty, navTenantId, classification, PrivacyHelper.MarkIfDataShouldBeScrubbed(message, classification), parameters);
			}
		}
	}

	private void SendTraceTagCore(string tag, string counterInformation, string category, Verbosity verbosity, Guid clientSessionId, Guid clientActivityId, Guid transientTelemetryId, string tenantId, PrivacyClassification classification, string message, params object[] parameters)
	{
		if (tag == null)
		{
			TraceFailureSendTraceTagWithStackTrace("00001RS", "A null tag made it into SendTraceTagCore");
			return;
		}
		if (string.IsNullOrEmpty(message))
		{
			TraceFailureSendTraceTag("00001RT", " A null or empty message made it into SendTraceTagCore", tag);
			return;
		}
		if (parameters != null && parameters.Length != 0)
		{
			try
			{
				message = string.Format(CultureInfo.InvariantCulture, message, parameters);
			}
			catch (Exception ex)
			{
				StringBuilder stringBuilder = new StringBuilder().AppendLine("Formatting of telemetry string in SendTraceTagCore failed. Formatting string: ").AppendLine(message).AppendLine("Parameters: ")
					.AppendLine(string.Join(", ", parameters.Select((object p) => p?.GetType().Name ?? "null")))
					.AppendLine("Exception: ")
					.AppendLine(ex.Message);
				TraceFailureSendTraceTag("00001RU", stringBuilder.ToString(), tag);
				return;
			}
		}
		SendTraceTagCore(tag, counterInformation, category, verbosity, clientSessionId, clientActivityId, transientTelemetryId, tenantId, classification, message);
	}

	private void SendTraceTagCore(string tag, string counterInformation, string category, Verbosity verbosity, Guid clientSessionId, Guid clientActivityId, Guid transientTelemetryId, string tenantId, PrivacyClassification classification, string message)
	{
		if (tag == null)
		{
			TraceFailureSendTraceTagWithStackTrace("00001RS", "A null tag made it into SendTraceTagCore");
			return;
		}
		if (string.IsNullOrEmpty(message))
		{
			TraceFailureSendTraceTag("00001RT", " A null or empty message made it into SendTraceTagCore", tag);
			return;
		}
		if (classification == PrivacyClassification.Invalid)
		{
			classification = PrivacyClassification.AccessControlData;
			TraceFailureSendTraceTag("00001RV", "Telemetry message with Invalid PrivacyClassification.", tag);
		}
		Guid serverActivityId = (monitorCollection as TraceScopeMonitorCollection)?.ScopeActivityId ?? ActivityId;
		if (TraceWriter != null)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent
			{
				NavTenantId = tenantId,
				ProcessId = ProcessId,
				ThreadId = Thread.CurrentThread.ManagedThreadId,
				EventTime = DateTime.UtcNow,
				ClientSessionId = clientSessionId,
				ClientActivityId = clientActivityId,
				ServerSessionUniqueId = SessionUniqueId,
				ServerActivityId = serverActivityId,
				Category = category,
				Verbosity = verbosity,
				Message = message,
				Tag = tag,
				CounterInformation = counterInformation,
				AlternateTenantIds = alternateTenantIds,
				AadTenantId = aadTenantId,
				AadUserId = aadUserId,
				PrivacyClassification = classification,
				TransientTelemetryId = transientTelemetryId
			};
			TestTelemetryEventListener?.Invoke(telemetryEvent);
			if (FeatureSwitches.Instance.EnableSendAllTagsToIfx && verbosity <= Verbosity.Warning)
			{
				TelemetryTagMetric.LogValue(1L, tag, aadTenantId ?? NoAadTenantId, EnumHelper<Verbosity>.EnumToString(verbosity));
			}
			TraceWriter.WriteTelemetryEvent(telemetryEvent);
		}
	}

	private Exception ComposeAndSendExceptionTraceTag(string tag, Category category, Exception exception, DiagnosticParameter message, Verbosity verbosity, Guid clientSessionId, Guid clientActivityId, string diagnosticsContext, bool forceLog)
	{
		string text = navTenantId;
		if (exception == null)
		{
			TraceFailureSendTraceTag("00001RW", "SendExceptionTag was passed a null exception, which will cause the whole message to be discarded.", tag);
			return null;
		}
		message = message ?? DiagnosticParameter.SystemMetadata(null);
		if (exception is NavBaseException ex)
		{
			if (!string.IsNullOrEmpty(ex.NavTenantId))
			{
				if (!string.IsNullOrEmpty(text) && !string.Equals(text, ex.NavTenantId, StringComparison.OrdinalIgnoreCase))
				{
					SendTraceTag("00001TV", Category.Telemetry, Verbosity.Warning, PrivacyClassification.OrganizationIdentifiableInformation, "Mismatch of tenant ids in send exception, expected tenant id: " + text + " Exception tenant id: " + ex.NavTenantId);
				}
				text = ex.NavTenantId;
			}
			if (ex.DiagnosticsSuppress && !forceLog)
			{
				return exception;
			}
			ex.DiagnosticsSuppress = true;
		}
		if (exception is INavExceptionTelemetrySettings navExceptionTelemetrySettings)
		{
			verbosity = ((verbosity > navExceptionTelemetrySettings.MaxVerbosity) ? navExceptionTelemetrySettings.MaxVerbosity : verbosity);
		}
		ExceptionTelemetryMessages exceptionTelemetryMessages = ComposeExceptionTelemetryMessages(exception, diagnosticsContext, message, out var couldHavePersonalData);
		string text2 = string.Empty;
		Guid transientTelemetryId = Guid.Empty;
		if (couldHavePersonalData)
		{
			transientTelemetryId = Guid.NewGuid();
			text2 = "This message had personal data removed. The original may still be in transient telemetry. Find it using the transientTelemetryId." + Environment.NewLine;
			SendTraceTagCore(tag, null, EnumHelper<Category>.DefinedEnumToString(category), verbosity, clientSessionId, clientActivityId, transientTelemetryId, text, PrivacyClassification.CustomerContent, "(" + exception.GetType().Name + "): " + exceptionTelemetryMessages.TransientTelemetry);
		}
		SendTraceTagCore(tag, null, EnumHelper<Category>.DefinedEnumToString(category), verbosity, clientSessionId, clientActivityId, transientTelemetryId, text, PrivacyClassification.SystemMetadata, "(" + exception.GetType().Name + "): " + text2 + exceptionTelemetryMessages.NonTransientTelemetry);
		return exception;
	}

	private static ExceptionTelemetryMessages ComposeExceptionTelemetryMessages(Exception exception, string diagnosticsContext, DiagnosticParameter message, out bool couldHavePersonalData)
	{
		ExceptionTelemetryMessages exceptionTelemetryMessages = new ExceptionTelemetryMessages();
		couldHavePersonalData = PrivacyHelper.IsPersonalOrRestrictedData(message);
		exceptionTelemetryMessages.AppendLine(message);
		exceptionTelemetryMessages.AppendException(exception, diagnosticsContext, new StackTrace(3, fNeedFileInfo: true).ToString(), ref couldHavePersonalData);
		return exceptionTelemetryMessages;
	}

	public void SendTraceTagOnThreshold(string tag, Category category, Verbosity verbosity, int threshold, Action<Stopwatch> action, PrivacyClassification classification, Func<string> includedMessage, string message, Func<object[]> parameters)
	{
		SendTraceTagOnThreshold(tag, category, verbosity, threshold, (Stopwatch watch) =>
		{
			action(watch);
			return 0;
		}, classification, includedMessage, message, parameters);
	}

	public void SendTraceTagOnThreshold(string tag, Category category, Verbosity verbosity, int threshold, Action<Stopwatch> action, PrivacyClassification classification, Func<string> includedMessage, string message, params object[] parameters)
	{
		SendTraceTagOnThreshold(tag, category, verbosity, threshold, (Stopwatch watch) =>
		{
			action(watch);
			return 0;
		}, classification, includedMessage, message, parameters);
	}

	public T SendTraceTagOnThreshold<T>(string tag, Category category, Verbosity verbosity, int threshold, Func<Stopwatch, T> action, PrivacyClassification classification, Func<string> includedMessage, string message, params object[] parameters)
	{
		return SendTraceTagOnThreshold(tag, category, verbosity, threshold, action, classification, includedMessage, message, () =>
		{
			object[] array = parameters;
			return (array == null || array.Length == 0) ? new object[0] : parameters;
		});
	}

	public T SendTraceTagOnThreshold<T>(string tag, Category category, Verbosity verbosity, int threshold, Func<Stopwatch, T> action, PrivacyClassification classification, Func<string> includedMessage, string message, Func<object[]> parameters)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (message == null)
		{
			TraceFailureSendTraceTag("00001RY", "A null message made it into SendTraceTagOnThreshold.", tag);
		}
		Stopwatch stopwatch = Stopwatch.StartNew();
		try
		{
			T result = action(stopwatch);
			stopwatch.Stop();
			if (stopwatch.ElapsedMilliseconds >= threshold)
			{
				SendTraceTagThresholdMessage(tag, category, verbosity, threshold, stopwatch.ElapsedMilliseconds, classification, message, includedMessage, parameters);
			}
			return result;
		}
		catch (Exception exception) when (SendTraceTagOnThresholdExceptionFilter(exception, tag, category, threshold, stopwatch.ElapsedMilliseconds, classification, message, includedMessage, parameters))
		{
			throw;
		}
	}

	public void SendTraceTagThresholdMessage(string tag, Category category, Verbosity verbosity, int threshold, long elapsedMilliseconds, PrivacyClassification classification, string message, Func<string> includedMessage, Func<object[]> parameters)
	{
		SendTraceTag(tag, category, verbosity, classification, FormattableString.Invariant($"Action completed successfully, but it took longer than the given threshold.\r\n  Execution time: {elapsedMilliseconds} ms\r\n  Threshold: {threshold} ms\r\n  Message: {message}\r\n  {includedMessage?.Invoke() ?? string.Empty}"), parameters?.Invoke() ?? Array.Empty<object>());
	}

	public bool SendTraceTagOnThresholdExceptionFilter(Exception exception, string tag, Category category, int threshold, long elapsedMilliseconds, PrivacyClassification classification, string message, Func<string> includedMessage, Func<object[]> parameters)
	{
		if (exception is NavCSideDuplicateKeyException)
		{
			return false;
		}
		bool flag = elapsedMilliseconds >= threshold;
		string text = ((message == null) ? null : string.Format(CultureInfo.InvariantCulture, message, parameters?.Invoke() ?? Array.Empty<object>()));
		DiagnosticParameter message2 = new DiagnosticParameter(FormattableString.Invariant($"Exception happened during execution of a threshold action.\r\n  Exception type: {exception.GetType().Name},\r\n  Threshold tag: {tag},\r\n  Execution time over threshold: {flag},\r\n  Execution time: {elapsedMilliseconds} ms,\r\n  Threshold: {threshold} ms\r\n  Message: {text}\r\n  {includedMessage?.Invoke() ?? string.Empty}"), classification);
		Verbosity verbosity = (flag ? Verbosity.Warning : Verbosity.Verbose);
		SendExceptionTag("000007T", category, exception, verbosity, message2);
		return false;
	}

	public void SendTraceTagOnFailure(string tag, Category category, Action action, bool rethrow = true)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		try
		{
			action();
		}
		catch (Exception exception) when (SendExceptionTagExceptionFilter(tag, category, exception) || !rethrow)
		{
		}
	}

	public ITraceScope CreateTraceScope(string tag, DiagnosticParameter name, MonitorType monitorTypes, Category category, bool createNewActivity = true, Verbosity verbosity = Verbosity.Normal)
	{
		return new TraceScopeMonitorCollection(this, tag, name, monitorTypes, category, createNewActivity, verbosity);
	}

	public ITraceScope CreateTimingScope(string tag, Category category, DiagnosticParameter name, Verbosity verbosity = Verbosity.Normal)
	{
		return new TraceScopeMonitorCollection(this, tag, name, MonitorType.None, category, createNewActivity: false, verbosity);
	}

	private static bool GetIsServerProcess()
	{
		Assembly assembly = null;
		try
		{
			assembly = Assembly.GetEntryAssembly();
		}
		catch (InvalidOperationException)
		{
		}
		if (assembly == null)
		{
			assembly = Assembly.GetExecutingAssembly();
		}
		return assembly.FullName.Contains("Server");
	}

	private Verbosity GetTraceLevel()
	{
		try
		{
			if (VerboseTraceLevelEnabled)
			{
				return traceLevelOverride;
			}
			if (IsServerProcess)
			{
				return ServerUserSettings.Instance?.TraceLevel?.Value ?? Verbosity.Normal;
			}
			return ConfigSettings.Instance?.TraceLevel ?? Verbosity.Normal;
		}
		catch (Exception)
		{
			return Verbosity.Normal;
		}
	}
}
