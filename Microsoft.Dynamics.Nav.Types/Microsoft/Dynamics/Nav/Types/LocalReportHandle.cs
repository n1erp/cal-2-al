using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Data;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Reporting.WinForms;

namespace Microsoft.Dynamics.Nav.Types;

public sealed class LocalReportHandle : ILocalReportHandle, IDisposable
{
	internal sealed class LocalReportProxy : MarshalByRefObject, IDisposable
	{
		[NonSerialized]
		private LocalReport localReport = new LocalReport();

		[NonSerialized]
		private static bool SqlServerSpatialTypesLoadRequired = true;

		private bool enableAppDomainMonitor;

		private List<MemoryStream> reportStreams = new List<MemoryStream>();

		public bool IsInRemoteAppDomain { get; set; }

		public string Name { get; internal set; }

		internal bool EnableAppDomainMonitor
		{
			get
			{
				return enableAppDomainMonitor;
			}
			private set
			{
				enableAppDomainMonitor = value;
				if (EnableAppDomainMonitor && !AppDomain.MonitoringIsEnabled)
				{
					AppDomain.MonitoringIsEnabled = true;
				}
			}
		}

		public long MonitoringTotalAllocatedMemorySize
		{
			get
			{
				if (!EnableAppDomainMonitor)
				{
					return 0L;
				}
				return AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize;
			}
		}

		public long MonitoringSurvivedMemorySize
		{
			get
			{
				if (!EnableAppDomainMonitor)
				{
					return 0L;
				}
				return AppDomain.CurrentDomain.MonitoringSurvivedMemorySize;
			}
		}

		public TimeSpan MonitoringTotalProcessorTime
		{
			get
			{
				if (!EnableAppDomainMonitor)
				{
					return TimeSpan.Zero;
				}
				return AppDomain.CurrentDomain.MonitoringTotalProcessorTime;
			}
		}

		public override object InitializeLifetimeService()
		{
			return null;
		}

		public LocalReportProxy(bool enableMonitoring)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected Obj, but got Unknown
			EnableAppDomainMonitor = enableMonitoring;
			if (SqlServerSpatialTypesLoadRequired)
			{
				HandleSqlServerSpatialTypes.LoadNativeBinaries(AppDomain.CurrentDomain.BaseDirectory);
				SqlServerSpatialTypesLoadRequired = false;
			}
			ReportAppDomain.IncrementOpenLocalReport();
		}

		internal void InitializeLocalReport(byte[] reportLayout, bool enableExternalImage, bool enableHyperlinks, bool enableExternalAssemblies, bool previousBehavior, bool enableReportViewerInternalAppDomainIsolation, IList<StrongName> strongNames)
		{
			localReport.EnableHyperlinks = enableHyperlinks;
			localReport.EnableExternalImages = enableExternalImage;
			if (previousBehavior)
			{
				if (enableExternalAssemblies | enableReportViewerInternalAppDomainIsolation)
				{
					if (ReportAppDomain.LegacyCasEnabled)
					{
						localReport.ExecuteReportInSandboxAppDomain();
					}
					if (enableExternalAssemblies)
					{
						PermissionSet basePermissionsForSandboxAppDomain = new PermissionSet(PermissionState.Unrestricted);
						localReport.SetBasePermissionsForSandboxAppDomain(basePermissionsForSandboxAppDomain);
						foreach (StrongName strongName in strongNames)
						{
							localReport.AddFullTrustModuleInSandboxAppDomain(strongName);
						}
					}
				}
				else if (ReportAppDomain.LegacyCasEnabled)
				{
					localReport.ExecuteReportInCurrentAppDomain(Assembly.GetExecutingAssembly().Evidence);
				}
			}
			else if (enableExternalAssemblies)
			{
				if (ReportAppDomain.LegacyCasEnabled)
				{
					localReport.ExecuteReportInSandboxAppDomain();
				}
				PermissionSet basePermissionsForSandboxAppDomain2 = new PermissionSet(PermissionState.Unrestricted);
				localReport.SetBasePermissionsForSandboxAppDomain(basePermissionsForSandboxAppDomain2);
				foreach (StrongName strongName2 in strongNames)
				{
					localReport.AddFullTrustModuleInSandboxAppDomain(strongName2);
				}
			}
			else if (ReportAppDomain.LegacyCasEnabled)
			{
				localReport.AddTrustedCodeModuleInCurrentAppDomain(typeof(ReportAppDomain).Assembly.FullName);
			}
			((Report)localReport).LoadReportDefinition((Stream)new MemoryStream(reportLayout));
		}

		private Stream CreateStream()
		{
			MemoryStream memoryStream = new MemoryStream();
			reportStreams.Add(memoryStream);
			return memoryStream;
		}

		public string[] Render(string format, string deviceInfo, CreateStreamCallback createStream, CultureInfo parentCurrentCulture, CultureInfo parentCurrentUiCulture, TimeZoneInfo parentTimeZone)
		{
			reportStreams.Clear();
			Warning[] warnings = null;
			CreateStreamCallback val3 = default;
			QueueUserWorkItemAndWaitForCompletion((object state) =>
			{
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Expected Obj, but got Unknown
				//IL_0035: Expected Obj, but got Unknown
				LocalReport val = localReport;
				string text = format;
				string text2 = deviceInfo;
				CreateStreamCallback val2 = val3;
				if (val2 == null)
				{
					CreateStreamCallback val4 = (string name, string extension, Encoding encoding, string mimeType, bool willSeek) => createStream();
					CreateStreamCallback val5 = val4;
					val3 = val4;
					val2 = val5;
				}
				val.Render(text, text2, val2, ref warnings);
			}, parentCurrentCulture, parentCurrentUiCulture, parentTimeZone);
			reportStreams.Clear();
			return FindErrors(warnings);
		}

		public string[] Render(string format, string deviceInfo, out MemoryStream[] streams, CultureInfo parentCurrentCulture, CultureInfo parentCurrentUiCulture, TimeZoneInfo parentTimeZone)
		{
			reportStreams.Clear();
			Warning[] warnings = null;
			CreateStreamCallback val3 = default;
			QueueUserWorkItemAndWaitForCompletion((object state) =>
			{
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Expected Obj, but got Unknown
				//IL_0035: Expected Obj, but got Unknown
				LocalReport val = localReport;
				string text = format;
				string text2 = deviceInfo;
				CreateStreamCallback val2 = val3;
				if (val2 == null)
				{
					CreateStreamCallback val4 = (string name, string extension, Encoding encoding, string mimeType, bool willSeek) => CreateStream();
					CreateStreamCallback val5 = val4;
					val3 = val4;
					val2 = val5;
				}
				val.Render(text, text2, val2, ref warnings);
			}, parentCurrentCulture, parentCurrentUiCulture, parentTimeZone);
			streams = reportStreams.ToArray();
			reportStreams.Clear();
			return FindErrors(warnings);
		}

		public byte[] Render(string format, string deviceInfo, out string mimeType, out string encoding, out string fileNameExtension, out string[] streams, out string[] errors, CultureInfo parentCurrentCulture, CultureInfo parentCurrentUiCulture, TimeZoneInfo parentTimeZone)
		{
			Warning[] warnings = null;
			byte[] output = null;
			string localMimeType = null;
			string localEncoding = null;
			string localFileNameExtension = null;
			string[] localStreams = null;
			QueueUserWorkItemAndWaitForCompletion((object state) =>
			{
				output = ((Report)localReport).Render(format, deviceInfo, ref localMimeType, ref localEncoding, ref localFileNameExtension, ref localStreams, ref warnings);
			}, parentCurrentCulture, parentCurrentUiCulture, parentTimeZone);
			mimeType = localMimeType;
			encoding = localEncoding;
			fileNameExtension = localFileNameExtension;
			streams = localStreams;
			errors = FindErrors(warnings);
			return output;
		}

		public void Dispose()
		{
			if (localReport != null)
			{
				ReportAppDomain.DecrementOpenLocalReport();
				localReport.Dispose();
				localReport = null;
			}
		}

		public void ClearDataSources()
		{
			((Collection<ReportDataSource>)(object)localReport.DataSources).Clear();
		}

		public void AddDataSource(string name, IEnumerable dataSource)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Expected Obj, but got Unknown
			((Collection<ReportDataSource>)(object)localReport.DataSources).Add(new ReportDataSource(name, dataSource));
		}

		public void AddDataSource(string name, DataTable dataSource)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Expected Obj, but got Unknown
			((Collection<ReportDataSource>)(object)localReport.DataSources).Add(new ReportDataSource(name, dataSource));
		}

		public void SetParameter(string key, string value)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Expected Obj, but got Unknown
			((Report)localReport).SetParameters(new ReportParameter(key, value));
		}

		public KeyValuePair<string, IList<string>>[] GetParameters()
		{
			List<KeyValuePair<string, IList<string>>> list = new List<KeyValuePair<string, IList<string>>>();
			foreach (ReportParameterInfo item in (ReadOnlyCollection<ReportParameterInfo>)(object)((Report)localReport).GetParameters())
			{
				list.Add(new KeyValuePair<string, IList<string>>(item.Name, item.Values));
			}
			return list.ToArray();
		}

		public T GetDataSource<T>(string name) where T : class
		{
			ReportDataSource val = localReport.DataSources[name];
			return ((val != null) ? val.Value : null) as T;
		}

		public ReportPageSettings GetDefaultPageSettings()
		{
			return ((Report)localReport).GetDefaultPageSettings();
		}
	}

	private object lockObject = new object();

	private int refCounter;

	private byte[] rdlcData;

	private IList<StrongName> strongNames;

	private bool hasNoSharedValuesInCode;

	private IEnumerable<string> parameterNames;

	private LocalReportProxy localReportProxy;

	private const string SharedVariableBasicRegExPattern = "(^|[\\W])Shared[\\W]";

	private static readonly Regex findSharedTokenRegex = new Regex("(^|[\\W])Shared[\\W]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	public bool EnableAppDomainMonitor { get; }

	private bool Disposed
	{
		get
		{
			lock (lockObject)
			{
				return refCounter < 0;
			}
		}
	}

	internal static Regex FindSharedTokenRegex => findSharedTokenRegex;

	public MemoryStream ReportLayout => new MemoryStream(rdlcData);

	public byte[] ReportLayoutBytes => rdlcData;

	internal LocalReportProxy LocalReport
	{
		get
		{
			if (Disposed)
			{
				throw new ObjectDisposedException("LocalReport");
			}
			if (localReportProxy == null)
			{
				lock (lockObject)
				{
					if (localReportProxy == null)
					{
						localReportProxy = ReportAppDomain.CreateLocalReportProxy(EnableReportViewerInternalAppDomainIsolation, EnableAppDomainMonitor);
						localReportProxy.InitializeLocalReport(rdlcData, EnableExternalImages, EnableHyperlinks, EnableExternalAssemblies, ServerUserSettings.Instance.PreviousReportAppDomainIsolation.Value, EnableReportViewerInternalAppDomainIsolation, StrongNames);
					}
				}
			}
			return localReportProxy;
		}
	}

	public bool EnableHyperlinks { get; private set; }

	public bool EnableExternalImages { get; private set; }

	public bool EnableExternalAssemblies { get; }

	public bool EnableReportViewerInternalAppDomainIsolation { get; }

	internal IEnumerable<string> ParameterNames => parameterNames;

	public IList<StrongName> StrongNames => strongNames;

	private static string[] FindErrors(Warning[] warnings)
	{
		List<string> list = new List<string>();
		if (warnings != null)
		{
			list.AddRange(from warning in warnings.Where((Warning warning) =>
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					//IL_0007: Invalid comparison between Unknown and I4
					return (int)warning.Severity == 1;
				})
				select warning.Message);
		}
		return list.ToArray();
	}

	private void DisposeLocalReportAndReset()
	{
		if (localReportProxy != null)
		{
			localReportProxy.Dispose();
			localReportProxy = null;
		}
	}

	private void RenderingCompleted()
	{
		if (!hasNoSharedValuesInCode || localReportProxy.IsInRemoteAppDomain)
		{
			DisposeLocalReportAndReset();
		}
	}

	public LocalReportHandle(byte[] reportLayout, bool enableExternalImages, bool enableHyperlinks, bool enableExternalAssemblies, bool enableReportViewerInternalAppDomainIsolation, bool enableAppDomainMonitor)
	{
		EnableAppDomainMonitor = enableAppDomainMonitor;
		rdlcData = reportLayout ?? throw new ArgumentNullException("reportLayout");
		EnableExternalAssemblies = enableExternalAssemblies;
		EnableExternalImages = enableExternalImages;
		EnableHyperlinks = enableHyperlinks;
		EnableReportViewerInternalAppDomainIsolation = enableReportViewerInternalAppDomainIsolation;
		InitializePropertiesFromRDL();
	}

	public void AddReference()
	{
		lock (lockObject)
		{
			refCounter++;
		}
	}

	public void Dispose()
	{
		lock (lockObject)
		{
			ClearDataSources();
			if (--refCounter < 0)
			{
				DisposeLocalReportAndReset();
			}
		}
	}

	public void AddDataSource(string name, IList<NavDataRow> dataSource)
	{
		LocalReport.AddDataSource(name, dataSource);
	}

	public void AddDataSource(string name, NavDataRowCollection dataSource)
	{
		LocalReport.AddDataSource(name, dataSource);
	}

	public void AddDataSource(string name, DataTable dataSource)
	{
		LocalReport.AddDataSource(name, dataSource);
	}

	public void SetReportParameters(KeyValuePair<string, string>[] reportParameters)
	{
		if (reportParameters == null)
		{
			return;
		}
		IDictionary<string, string> dictionary = new Dictionary<string, string>();
		for (int i = 0; i < reportParameters.Length; i++)
		{
			KeyValuePair<string, string> keyValuePair = reportParameters[i];
			dictionary[keyValuePair.Key] = keyValuePair.Value;
		}
		foreach (string parameterName in ParameterNames)
		{
			if (dictionary.TryGetValue(parameterName, out var value))
			{
				LocalReport.SetParameter(parameterName, value);
			}
		}
	}

	public ReportPageSettings GetDefaultPageSettings()
	{
		return localReportProxy.GetDefaultPageSettings();
	}

	internal static void QueueUserWorkItemAndWaitForCompletion(WaitCallback callBack)
	{
		QueueUserWorkItemAndWaitForCompletion(callBack, Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentUICulture, CurrentTimeZoneManagerBase.CurrentTimeZone);
	}

	internal static void QueueUserWorkItemAndWaitForCompletion(WaitCallback callBack, CultureInfo parentCurrentCulture, CultureInfo parentCurrentUiCulture, TimeZoneInfo parentTimeZone)
	{
		ManualResetEvent waitForCompletion = new ManualResetEvent(initialState: false);
		Exception exceptionOnWorkItem = null;
		ThreadPool.QueueUserWorkItem((object state) =>
		{
			try
			{
				CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
				CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;
				TimeZoneInfo currentTimeZone = CurrentTimeZoneManagerBase.CurrentTimeZone;
				Thread.CurrentThread.CurrentCulture = parentCurrentCulture;
				Thread.CurrentThread.CurrentUICulture = parentCurrentUiCulture;
				CurrentTimeZoneManagerBase.CurrentTimeZone = parentTimeZone;
				IPrincipal currentPrincipal = Thread.CurrentPrincipal;
				Thread.CurrentPrincipal = null;
				try
				{
					callBack(state);
				}
				finally
				{
					Thread.CurrentPrincipal = currentPrincipal;
					Thread.CurrentThread.CurrentCulture = currentCulture;
					Thread.CurrentThread.CurrentUICulture = currentUICulture;
					CurrentTimeZoneManagerBase.CurrentTimeZone = currentTimeZone;
				}
			}
			catch (Exception ex)
			{
				exceptionOnWorkItem = ex;
			}
			finally
			{
				waitForCompletion.Set();
			}
		});
		waitForCompletion.WaitOne();
		if (exceptionOnWorkItem != null)
		{
			throw exceptionOnWorkItem;
		}
	}

	public string[] Render(string format, string deviceInfo, CreateStreamCallback createStream)
	{
		try
		{
			if (!ReportAppDomain.LegacyCasEnabled && !ReportAppDomain.ForceToCurrentAppDomain)
			{
				throw new NavNCLInvalidOperationException("Report Viewer should not be called in non legacy CAS mode, serialization would fail.");
			}
			using (NavDiagnostics.GetMostSpecificInstance.CreateTraceScope("00009AA", DiagnosticParameter.SystemMetadata(string.Format(CultureInfo.InvariantCulture, "Report Render format:{0}, domain:{1}", format, LocalReport.Name)), MonitorType.None, Category.Runtime))
			{
				long monitoringTotalAllocatedMemorySize = LocalReport.MonitoringTotalAllocatedMemorySize;
				TimeSpan monitoringTotalProcessorTime = LocalReport.MonitoringTotalProcessorTime;
				string[] result = LocalReport.Render(format, deviceInfo, createStream, Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentUICulture, CurrentTimeZoneManagerBase.CurrentTimeZone);
				NavDiagnostics.GetMostSpecificInstance.SendTraceTag("00009A9", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, $"MonitoringTotalAllocatedMemorySize={monitoringTotalAllocatedMemorySize - LocalReport.MonitoringTotalAllocatedMemorySize}, {monitoringTotalProcessorTime - LocalReport.MonitoringTotalProcessorTime}");
				return result;
			}
		}
		finally
		{
			RenderingCompleted();
		}
	}

	public string[] Render(string format, string deviceInfo, out MemoryStream[] streams)
	{
		try
		{
			streams = null;
			using (NavDiagnostics.GetMostSpecificInstance.CreateTraceScope("00009AB", DiagnosticParameter.SystemMetadata(string.Format(CultureInfo.InvariantCulture, "Report Render format:{0}, domain:{1}", format, LocalReport.Name)), MonitorType.None, Category.Runtime))
			{
				return LocalReport.Render(format, deviceInfo, out streams, Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentUICulture, CurrentTimeZoneManagerBase.CurrentTimeZone);
			}
		}
		finally
		{
			RenderingCompleted();
		}
	}

	public byte[] Render(string format, string deviceInfo, out string mimeType, out string encoding, out string fileNameExtension, out string[] streams, out string[] errors)
	{
		try
		{
			using (NavDiagnostics.GetMostSpecificInstance.CreateTraceScope("00009AC", DiagnosticParameter.SystemMetadata(string.Format(CultureInfo.InvariantCulture, "Report Render format:{0}, domain:{1}", format, LocalReport.Name)), MonitorType.None, Category.Runtime))
			{
				return LocalReport.Render(format, deviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out errors, Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentUICulture, CurrentTimeZoneManagerBase.CurrentTimeZone);
			}
		}
		finally
		{
			RenderingCompleted();
		}
	}

	public void ClearDataSources()
	{
		if (localReportProxy != null)
		{
			LocalReport.ClearDataSources();
		}
	}

	private void InitializePropertiesFromRDL()
	{
		XmlDocument reportLayoutDocument = GetReportLayoutDocument(ReportLayout, out var xmlNameSpace);
		strongNames = GetStrongNames(xmlNameSpace, reportLayoutDocument);
		hasNoSharedValuesInCode = GetHasNoSharedValuesInCode(xmlNameSpace, reportLayoutDocument);
		parameterNames = GetReportParameters(xmlNameSpace, reportLayoutDocument);
	}

	internal static bool GetHasNoSharedValuesInCode(XmlNamespaceManager xmlNameSpace, XmlDocument reportLayoutDocument)
	{
		foreach (XmlNode item in reportLayoutDocument.SelectNodes("//a:Code", xmlNameSpace))
		{
			if (FindSharedTokenRegex.Match(item.InnerText).Success)
			{
				return false;
			}
		}
		return true;
	}

	internal static IList<StrongName> GetStrongNames(XmlNamespaceManager xmlNameSpace, XmlDocument reportLayoutDocument)
	{
		XmlNode xmlNode = reportLayoutDocument.SelectSingleNode("./a:Report/a:CodeModules", xmlNameSpace);
		IList<StrongName> list = new List<StrongName>();
		if (xmlNode != null)
		{
			foreach (XmlNode childNode in xmlNode.ChildNodes)
			{
				StrongName item = CreateStrongName(Assembly.ReflectionOnlyLoad(childNode.InnerText));
				list.Add(item);
			}
		}
		return list;
	}

	internal static IList<string> GetReportParameters(XmlNamespaceManager xmlNameSpace, XmlDocument reportLayoutDocument)
	{
		XmlNodeList xmlNodeList = reportLayoutDocument.SelectNodes("/a:Report/a:ReportParameters/a:ReportParameter/@Name", xmlNameSpace);
		IList<string> list = new List<string>();
		foreach (XmlNode item in xmlNodeList)
		{
			list.Add(item.Value);
		}
		return list;
	}

	internal static XmlDocument GetReportLayoutDocument(Stream rdlStream, out XmlNamespaceManager xmlNameSpace)
	{
		XmlDocument xmlDocument = XmlHelper.SafeXmlDocument();
		try
		{
			xmlDocument.Load(XmlHelper.SafeXmlReader(rdlStream));
		}
		catch (XmlException)
		{
		}
		xmlNameSpace = GetXmlNameSpace(xmlDocument);
		if (rdlStream.CanSeek)
		{
			rdlStream.Seek(0L, SeekOrigin.Begin);
		}
		return xmlDocument;
	}

	private static XmlNamespaceManager GetXmlNameSpace(XmlDocument reportLayoutDocument)
	{
		string text = reportLayoutDocument?.DocumentElement?.NamespaceURI;
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(reportLayoutDocument.NameTable);
		xmlNamespaceManager.AddNamespace("a", text);
		return xmlNamespaceManager;
	}

	private static StrongName CreateStrongName(Assembly assembly)
	{
		if (assembly == null)
		{
			throw new ArgumentNullException("assembly");
		}
		AssemblyName name = assembly.GetName();
		byte[] publicKey = name.GetPublicKey();
		if (publicKey == null || publicKey.Length == 0)
		{
			throw new InvalidOperationException("Assembly is not strongly named");
		}
		return new StrongName(new StrongNamePublicKeyBlob(publicKey), name.Name, name.Version);
	}
}
