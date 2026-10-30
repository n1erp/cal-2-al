using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Compilation;
using System.Xml;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Reporting.WebForms;
using Microsoft.ReportingServices.Diagnostics.Internal;
using Microsoft.SqlServer.Types;

namespace Microsoft.Dynamics.Nav.Types.Report;

public static class RdlcCompiler
{
	internal struct CompilerSettings
	{
		public string SourcePath;

		public string SolutionPath;

		public string WebAppCompilerFolder;

		public string CompilerPath;
	}

	internal static class NativeMethods
	{
		[DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);
	}

	private static readonly object SourceFolderLock = new object();

	private const string WebConfigFileName = "Web.config";

	private const string WebCompileFolderMacro = "$WebCompileFolder$";

	private const string RdlcFileNameForCompiling = "Report{0}_{1}.rdlc";

	private const string RdlcFileNameFromDatabaseForCompiling = "ReportFromDatabase{0}_{1}.rdlc";

	private const string VirtualPathRdlcFileName = "/ReportLayout/Report{0}_{1}.rdlc";

	private const string VirtualPathRdlcFileNameFromDatabase = "/ReportLayout/ReportFromDatabase{0}_{1}.rdlc";

	private const string TemplateFolderName = "Template";

	internal const string ParentFolderName = "Microsoft Dynamics";

	internal const string WebCompileFolder = "WebCompileFolder";

	private const string ReportViewerCommon = "Microsoft.ReportViewer.Common.dll";

	private const string ProcessingObjectModel = "Microsoft.ReportViewer.ProcessingObjectModel.dll";

	private const string ReportViewerWebForms = "Microsoft.ReportViewer.WebForms.dll";

	private const string SqlServerTypes = "Microsoft.SqlServer.Types.dll";

	private static volatile OpenGisGeometryType openGisGeometryType = (OpenGisGeometryType)1;

	private static Type previewItemContextType;

	private static MethodInfo reportCompilerCompileReportMethod;

	private static object objectLock = new object();

	private static void CompileFile(string reportLayout)
	{
		LoadTypes();
		try
		{
			reportCompilerCompileReportMethod.Invoke(null, new object[4]
			{
				Activator.CreateInstance(previewItemContextType),
				Encoding.UTF8.GetBytes(reportLayout),
				false,
				null
			});
		}
		catch (TargetInvocationException ex)
		{
			if (ex.InnerException?.GetType().FullName == "Microsoft.Reporting.DefinitionInvalidException")
			{
				throw ex.InnerException.InnerException;
			}
			throw ex.InnerException;
		}
	}

	private static void LoadTypes()
	{
		if (!(previewItemContextType == null) && !(reportCompilerCompileReportMethod == null))
		{
			return;
		}
		lock (objectLock)
		{
			if (previewItemContextType == null || reportCompilerCompileReportMethod == null)
			{
				Assembly assembly = typeof(DataSet).Assembly;
				previewItemContextType = assembly.GetType("Microsoft.Reporting.PreviewItemContext");
				reportCompilerCompileReportMethod = assembly.GetType("Microsoft.Reporting.ReportCompiler").GetMethod("CompileReport");
			}
		}
	}

	public static bool CompileReport(bool allowExternalImages, bool allowHyperlinks, bool allowExternalAssemblies, string reportLayout, string dataSet, IEnumerable<string> parameters)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if ((int)openGisGeometryType == 1)
		{
			new DeviceInfo("", "");
			openGisGeometryType = (OpenGisGeometryType)8;
		}
		XmlDocument reportLayoutDocument = null;
		XmlNamespaceManager xmlNameSpace = null;
		if (!allowExternalImages && !CheckForExternalImages(reportLayout, ref reportLayoutDocument, ref xmlNameSpace))
		{
			return false;
		}
		if (!allowHyperlinks && !CheckForHyperlinks(reportLayout, ref reportLayoutDocument, ref xmlNameSpace))
		{
			return false;
		}
		if (!allowExternalAssemblies && !CheckForExternalAssemblies(reportLayout, ref reportLayoutDocument, ref xmlNameSpace))
		{
			return false;
		}
		List<string> reportParameters = new List<string>(parameters);
		string text = null;
		try
		{
			if (!AreDataSetFieldsValid(reportLayout, dataSet) || !AreParametersValid(reportLayout, reportParameters))
			{
				XmlDocument xmlDocument = ReportXmlHelper.ApplyReportDataSet(reportLayout, dataSet);
				ReportXmlHelper.ApplyReportParameters(xmlDocument, reportParameters);
				text = ReportXmlHelper.SaveXmlDocument(xmlDocument);
			}
		}
		catch (Exception exception)
		{
			throw RdlcCompilerException.Create(707, exception);
		}
		if (text != null)
		{
			CompileFile(text);
			try
			{
				CompileFile(reportLayout);
			}
			catch (Exception exception2)
			{
				throw RdlcCompilerException.Create(724, exception2);
			}
		}
		else
		{
			CompileFile(reportLayout);
		}
		return true;
	}

	public static bool LegacyCompileReport(int reportId, bool allowExternalImages, bool allowHyperlinks, bool allowExternalAssemblies, string reportLayout, string dataSet, IEnumerable<string> parameters, bool singleUser = false)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		Guid guid = (singleUser ? Guid.Empty : Guid.NewGuid());
		if ((int)openGisGeometryType == 1)
		{
			new DeviceInfo("", "");
			openGisGeometryType = (OpenGisGeometryType)8;
		}
		CompilerSettings compilerSettings = SetupCompileDirectory(reportId, guid, dataSet, parameters, reportLayout);
		try
		{
			XmlDocument reportLayoutDocument = null;
			XmlNamespaceManager xmlNameSpace = null;
			if (!allowExternalImages && !CheckForExternalImages(reportLayout, ref reportLayoutDocument, ref xmlNameSpace))
			{
				return false;
			}
			if (!allowHyperlinks && !CheckForHyperlinks(reportLayout, ref reportLayoutDocument, ref xmlNameSpace))
			{
				return false;
			}
			if (!allowExternalAssemblies && !CheckForExternalAssemblies(reportLayout, ref reportLayoutDocument, ref xmlNameSpace))
			{
				return false;
			}
			ClientBuildManager websiteCompiler = GetWebsiteCompiler(compilerSettings.CompilerPath);
			try
			{
				if (File.Exists(Path.Combine(compilerSettings.CompilerPath, string.Format(CultureInfo.InvariantCulture, "Report{0}_{1}.rdlc", reportId, guid))))
				{
					websiteCompiler.CompileFile(string.Format(CultureInfo.InvariantCulture, "/ReportLayout/Report{0}_{1}.rdlc", reportId, guid));
					try
					{
						websiteCompiler.CompileFile(string.Format(CultureInfo.InvariantCulture, "/ReportLayout/ReportFromDatabase{0}_{1}.rdlc", reportId, guid));
					}
					catch (Exception exception)
					{
						throw RdlcCompilerException.Create(724, exception);
					}
				}
				else
				{
					websiteCompiler.CompileFile(string.Format(CultureInfo.InvariantCulture, "/ReportLayout/ReportFromDatabase{0}_{1}.rdlc", reportId, guid));
				}
			}
			finally
			{
				UnloadCompilerAppDomain(websiteCompiler);
			}
		}
		finally
		{
			TryDeleteFile(Path.Combine(compilerSettings.CompilerPath, string.Format(CultureInfo.InvariantCulture, "ReportFromDatabase{0}_{1}.rdlc", reportId, guid)));
			TryDeleteFile(Path.Combine(compilerSettings.CompilerPath, string.Format(CultureInfo.InvariantCulture, "Report{0}_{1}.rdlc", reportId, guid)));
		}
		return true;
	}

	private static bool CheckForHyperlinks(string reportLayout, ref XmlDocument reportLayoutDocument, ref XmlNamespaceManager xmlNameSpace)
	{
		if (reportLayout.Contains("<Hyperlink>"))
		{
			CreateReportLayoutDocument(reportLayout, ref reportLayoutDocument, ref xmlNameSpace);
			XmlNode xmlNode = reportLayoutDocument.SelectSingleNode("//a:Action[a:Hyperlink]", xmlNameSpace);
			if (xmlNode != null)
			{
				XmlNode xmlNode2 = xmlNode.SelectSingleNode("../../..");
				if (xmlNode2.Name == "Image" || xmlNode2.Name == "Textbox")
				{
					throw RdlcCompilerException.Create(726, null, xmlNode2.Attributes.GetNamedItem("Name").Value);
				}
				throw RdlcCompilerException.Create(727, null, (xmlNode2.Name == "DataPoint") ? "chart " : "");
			}
		}
		return true;
	}

	private static bool CheckForExternalImages(string reportLayout, ref XmlDocument reportLayoutDocument, ref XmlNamespaceManager xmlNameSpace)
	{
		if (reportLayout.Contains("<Image"))
		{
			CreateReportLayoutDocument(reportLayout, ref reportLayoutDocument, ref xmlNameSpace);
			XmlNode xmlNode = reportLayoutDocument.SelectSingleNode("//a:Image[a:Source='External']", xmlNameSpace);
			if (xmlNode != null)
			{
				throw RdlcCompilerException.Create(725, null, xmlNode.Attributes.GetNamedItem("Name").Value);
			}
		}
		if (reportLayout.Contains("<BackgroundImage"))
		{
			CreateReportLayoutDocument(reportLayout, ref reportLayoutDocument, ref xmlNameSpace);
			XmlNode xmlNode2 = reportLayoutDocument.SelectSingleNode("//a:BackgroundImage[a:Source='External']", xmlNameSpace);
			if (xmlNode2 != null)
			{
				throw RdlcCompilerException.Create(743, null, xmlNode2.ParentNode.ParentNode.Name);
			}
		}
		return true;
	}

	private static bool CheckForExternalAssemblies(string reportLayout, ref XmlDocument reportLayoutDocument, ref XmlNamespaceManager xmlNameSpace)
	{
		if (reportLayout.Contains("<CodeModule"))
		{
			CreateReportLayoutDocument(reportLayout, ref reportLayoutDocument, ref xmlNameSpace);
			if (reportLayoutDocument.SelectSingleNode("//a:CodeModules[a:CodeModule]", xmlNameSpace) != null)
			{
				throw RdlcCompilerException.Create(734, null, string.Empty);
			}
		}
		return true;
	}

	private static void CreateReportLayoutDocument(string reportLayout, ref XmlDocument reportLayoutDocument, ref XmlNamespaceManager xmlNameSpace)
	{
		if (reportLayoutDocument == null || xmlNameSpace == null)
		{
			reportLayoutDocument = XmlHelper.SafeXmlLoader(null, reportLayout);
			xmlNameSpace = ReportXmlHelper.GetNamespaceManager(reportLayoutDocument, "http://schemas.microsoft.com/sqlserver/reporting/2010/01/reportdefinition");
		}
	}

	private static bool AreDataSetFieldsValid(string reportLayout, string metadataDataset)
	{
		XmlNode xmlNode = XmlHelper.SafeXmlLoader(null, metadataDataset).SelectSingleNode("/DataSets/DataSet/Fields");
		bool flag = xmlNode?.HasChildNodes ?? false;
		XmlReader xmlReader = XmlHelper.SafeXmlReader(new StringReader(reportLayout));
		bool flag2 = xmlReader.ReadToFollowing("Field");
		if (flag & flag2)
		{
			do
			{
				XmlNode xmlNode2 = xmlNode.SelectSingleNode("Field[@Name='" + xmlReader.GetAttribute("Name") + "']");
				if (xmlNode2 == null)
				{
					return false;
				}
				xmlNode.RemoveChild(xmlNode2);
			}
			while (xmlReader.ReadToNextSibling("Field"));
			return true;
		}
		if (!flag)
		{
			return !flag2;
		}
		return false;
	}

	private static bool AreParametersValid(string reportLayout, List<string> reportParameters)
	{
		XmlReader xmlReader = XmlHelper.SafeXmlReader(new StringReader(reportLayout));
		if (xmlReader.ReadToFollowing("ReportParameter"))
		{
			do
			{
				if (!reportParameters.Contains(xmlReader.GetAttribute("Name")))
				{
					return false;
				}
			}
			while (xmlReader.ReadToNextSibling("ReportParameter"));
			return true;
		}
		return true;
	}

	private static CompilerSettings SetupCompileDirectory(int reportId, Guid guid, string dataset, IEnumerable<string> parameters, string reportLayout)
	{
		try
		{
			CompilerSettings result = CreateCompileDirectory(guid);
			File.WriteAllText(Path.Combine(result.CompilerPath, string.Format(CultureInfo.InvariantCulture, "ReportFromDatabase{0}_{1}.rdlc", reportId, guid)), reportLayout, Encoding.UTF8);
			List<string> reportParameters = new List<string>(parameters);
			if (!AreDataSetFieldsValid(reportLayout, dataset) || !AreParametersValid(reportLayout, reportParameters))
			{
				XmlDocument xmlDocument = ReportXmlHelper.ApplyReportDataSet(reportLayout, dataset);
				ReportXmlHelper.ApplyReportParameters(xmlDocument, reportParameters);
				ReportXmlHelper.SaveXmlDocument(Path.Combine(result.CompilerPath, string.Format(CultureInfo.InvariantCulture, "Report{0}_{1}.rdlc", reportId, guid)), xmlDocument);
			}
			return result;
		}
		catch (Exception exception)
		{
			throw RdlcCompilerException.Create(707, exception);
		}
	}

	internal static void SetupCompileBinaries(string destinationPath, string solutionPath)
	{
		string directoryName = Path.GetDirectoryName(typeof(RdlcCompiler).Assembly.Location);
		string text = Path.Combine(destinationPath, "bin");
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		foreach (string item in new List<string> { "Microsoft.ReportViewer.Common.dll", "Microsoft.ReportViewer.WebForms.dll", "Microsoft.ReportViewer.ProcessingObjectModel.dll", "Microsoft.SqlServer.Types.dll" })
		{
			string sourceFileName = Path.Combine(directoryName, item);
			string text2 = Path.Combine(solutionPath, item);
			if (!File.Exists(text2))
			{
				File.Copy(sourceFileName, text2, overwrite: true);
			}
			DeployBinaryObject(text2, Path.Combine(text, item));
		}
	}

	private static void DeployBinaryObject(string sourcePath, string targetPath)
	{
		if (sourcePath == null)
		{
			throw new ArgumentNullException("sourcePath");
		}
		if (string.IsNullOrEmpty(sourcePath))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "sourcePath");
		}
		if (targetPath == null)
		{
			throw new ArgumentNullException("targetPath");
		}
		if (string.IsNullOrEmpty(targetPath))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "targetPath");
		}
		if (!File.Exists(sourcePath))
		{
			throw new FileNotFoundException(string.Format(CultureInfo.CurrentCulture, Lang._00095_50047, sourcePath), sourcePath);
		}
		string directoryName = Path.GetDirectoryName(targetPath);
		if (string.IsNullOrEmpty(directoryName))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "targetPath");
		}
		if (!Directory.Exists(directoryName))
		{
			throw new DirectoryNotFoundException(string.Format(CultureInfo.CurrentCulture, Lang.DirectoryNotFound, directoryName));
		}
		if (!File.Exists(targetPath) && !NativeMethods.CreateHardLink(targetPath, sourcePath, IntPtr.Zero))
		{
			File.Copy(sourcePath, targetPath, overwrite: true);
		}
	}

	private static CompilerSettings CreateCompileDirectory(Guid uniqueGuid)
	{
		CompilerSettings result = new CompilerSettings
		{
			SolutionPath = Path.Combine(Path.GetTempPath(), "Microsoft Dynamics", Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture))
		};
		lock (SourceFolderLock)
		{
			if (!Directory.Exists(result.SolutionPath))
			{
				Directory.CreateDirectory(result.SolutionPath);
			}
			result.SourcePath = Path.Combine(result.SolutionPath, "Template");
			if (!Directory.Exists(result.SourcePath))
			{
				Directory.CreateDirectory(result.SourcePath);
				File.WriteAllBytes(Path.Combine(result.SourcePath, "Report.rdlc"), ReportResources.ReportRdlc);
				File.WriteAllBytes(Path.Combine(result.SourcePath, "ReportLayout.sln"), ReportResources.ReportLayoutSln);
				File.WriteAllText(Path.Combine(result.SourcePath, "Report.xsd"), ReportResources.ReportXsd, Encoding.UTF8);
				File.WriteAllText(Path.Combine(result.SourcePath, "Report.xss"), string.Empty, Encoding.UTF8);
				File.WriteAllText(Path.Combine(result.SourcePath, "Report.xsc"), string.Empty, Encoding.UTF8);
				File.WriteAllText(Path.Combine(result.SourcePath, "Web.config"), ReportResources.WebConfig, Encoding.UTF8);
			}
		}
		string path = (uniqueGuid.Equals(Guid.Empty) ? result.SolutionPath : Path.Combine(result.SolutionPath, uniqueGuid.ToString("N")));
		result.WebAppCompilerFolder = Path.Combine(path, "WebCompileFolder");
		if (!Directory.Exists(result.WebAppCompilerFolder))
		{
			Directory.CreateDirectory(result.WebAppCompilerFolder);
		}
		result.CompilerPath = Path.Combine(path, "ReportCompile");
		if (!Directory.Exists(result.CompilerPath))
		{
			Directory.CreateDirectory(result.CompilerPath);
		}
		string[] files = Directory.GetFiles(result.SourcePath);
		foreach (string text in files)
		{
			string fileName = Path.GetFileName(text);
			if (!File.Exists(Path.Combine(result.CompilerPath, fileName)))
			{
				PatchFile(result.WebAppCompilerFolder, text, result.CompilerPath);
			}
		}
		SetupCompileBinaries(result.CompilerPath, result.SolutionPath);
		return result;
	}

	public static void PatchFile(string webAppCompilerFolder, string filePath, string destinationPath)
	{
		string fileName = Path.GetFileName(filePath);
		string text = Path.Combine(destinationPath, fileName);
		if (string.Equals(fileName, "Web.config", StringComparison.CurrentCultureIgnoreCase))
		{
			string text2 = File.ReadAllText(filePath, Encoding.UTF8);
			File.WriteAllText(text, text2.Replace("$WebCompileFolder$", webAppCompilerFolder), Encoding.UTF8);
		}
		else
		{
			File.Copy(filePath, text, overwrite: true);
			File.SetAttributes(text, FileAttributes.Normal);
		}
	}

	private static void TryDeleteFile(string file)
	{
		if (File.Exists(file))
		{
			try
			{
				File.Delete(file);
			}
			catch (IOException)
			{
			}
		}
	}

	private static ClientBuildManager GetWebsiteCompiler(string path)
	{
		return new ClientBuildManager("ReportLayout", path);
	}

	private static void UnloadCompilerAppDomain(ClientBuildManager websiteCompiler)
	{
		if (websiteCompiler.IsHostCreated)
		{
			websiteCompiler.Unload();
		}
	}

	static RdlcCompiler()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
	}
}
