using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Runtime;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.XmlMetadata;

public static class ControlAddInMetadataProvider
{
	private class ControlAddInData
	{
		private readonly List<ControlAddInResourceDefinition> extractedResources = new List<ControlAddInResourceDefinition>();

		internal string CtrlName { get; set; }

		internal string PublicKeyToken { get; set; }

		internal string VersionNumber { get; set; }

		internal byte[] Resource { get; set; }

		internal byte[] Manifest { get; set; }

		internal string ManifestVersion { get; set; }

		internal ControlAddInManifest ParsedManifest { get; set; }

		internal List<ControlAddInResourceDefinition> ExtractedResources => extractedResources;

		internal string ErrorMessage { get; set; }

		internal string ControlAddInName => CtrlName + ";PublicKeyToken=" + PublicKeyToken;
	}

	public static ICollection<AddInRegistration> GetDotNetControlAddInRegistrations()
	{
		return (from controlAddIn in GetControlAddInDatasFromDatabaseTables(includeJavaScript: false)
			let strToken = controlAddIn.PublicKeyToken
			where !string.IsNullOrEmpty(strToken)
			let token = (from x in Enumerable.Range(0, strToken.Length)
				where x % 2 == 0
				select ToByte(controlAddIn, strToken.Substring(x, Math.Min(2, strToken.Length - x)), 16)).ToArray()
			select new AddInRegistration(controlAddIn.CtrlName, token, controlAddIn.VersionNumber)).ToList();
	}

	private static byte ToByte(ControlAddInData controlAddIn, string value, int fromBase)
	{
		try
		{
			return Convert.ToByte(value, fromBase);
		}
		catch (FormatException)
		{
			controlAddIn.ErrorMessage = string.Format(CultureInfo.CurrentCulture, Lang.ControlAddInInvalidPublicKeyToken, controlAddIn.ControlAddInName);
			return 0;
		}
	}

	public static void SolveControlAddInProperties(MasterPage masterPage)
	{
		if (masterPage == null)
		{
			throw new ArgumentNullException("masterPage");
		}
		List<ElementDefinition> controlDefinitions = masterPage.ContentArea.FindAll(IsControlAddIn);
		SolveControlAddInProperties(controlDefinitions);
	}

	private static bool IsControlAddIn(ElementDefinition element)
	{
		if (element is ControlDefinition controlDefinition)
		{
			return !string.IsNullOrEmpty(controlDefinition.ControlAddIn);
		}
		return false;
	}

	private static void SolveControlAddInProperties(IEnumerable<ElementDefinition> controlDefinitions)
	{
		List<ControlAddInData> list = new List<ControlAddInData>();
		foreach (ControlDefinition controlDefinition in controlDefinitions.Where((ElementDefinition c) => c is ControlDefinition).Cast<ControlDefinition>())
		{
			ControlAddInData controlAddInData = null;
			if (!string.IsNullOrEmpty(controlDefinition.ControlAddIn))
			{
				controlAddInData = list.FirstOrDefault((ControlAddInData addIn) => addIn.ControlAddInName.Equals(controlDefinition.ControlAddIn));
				if (controlAddInData == null)
				{
					list.AddRange(GetControlAddInDatasFromDatabaseTables(includeJavaScript: true, includeFromNavApps: true, controlDefinition.ControlAddIn));
					controlAddInData = list.LastOrDefault((ControlAddInData addIn) => addIn.ControlAddInName.Equals(controlDefinition.ControlAddIn));
				}
			}
			if (controlAddInData == null)
			{
				controlDefinition.ControlAddIn = string.Format(CultureInfo.CurrentCulture, "{0}, {1}", controlDefinition.ControlAddIn, Lang.NonAllowedControlAddIn);
			}
			else
			{
				UpdateControlAddInProperty(controlDefinition, controlAddInData);
			}
		}
	}

	private static List<ControlAddInData> GetControlAddInDatasFromDatabaseTables(bool includeJavaScript = true, bool includeFromNavApps = true, string addInName = null)
	{
		NavSession session = NavCurrentThread.Session;
		List<ControlAddInData> list = new List<ControlAddInData>();
		list.AddRange(GetControlAddInDataFromSystemTable(session, includeJavaScript, addInName));
		if (includeFromNavApps)
		{
			list.AddRange(GetControlAddInDataFromTenantTable(session, includeJavaScript, addInName));
		}
		return list;
	}

	private static List<ControlAddInData> GetControlAddInDataFromSystemTable(NavSession session, bool includeJavaScript, string addInName = null)
	{
		List<ControlAddInData> list = new List<ControlAddInData>();
		using (new MaximizedPermissionScope(session, new int[1] { 2000000069 }))
		{
			using NavRecord navRecord = new NavRecord(session, 2000000069, SecurityFiltering.Ignored);
			if (includeJavaScript)
			{
				navRecord.ALSetFilter(15, "<>%1", NavInteger.Create(2));
			}
			else
			{
				navRecord.ALSetFilter(15, "%1 | %2", NavInteger.Create(1), NavInteger.Create(3));
			}
			if (!string.IsNullOrEmpty(addInName))
			{
				NavAppAddInAssemblyName navAppAddInAssemblyName = NavAppAddInAssemblyName.CreateFromString(addInName, AssemblyNameSeparatorToken.Semicolon);
				navRecord.ALSetFilter(1, navAppAddInAssemblyName.Name);
				navRecord.ALSetFilter(5, navAppAddInAssemblyName.PublicKeyToken);
			}
			bool flag = navRecord.ALFindSet(DataError.TrapError);
			while (flag)
			{
				ControlAddInData controlAddInData = new ControlAddInData
				{
					CtrlName = navRecord.GetFieldValue(1).ToString(),
					PublicKeyToken = navRecord.GetFieldValue(5).ToString(),
					VersionNumber = navRecord.GetFieldValue(8).ToString()
				};
				if (includeJavaScript && navRecord.GetFieldValue(15).ToInt32() == 0)
				{
					navRecord.CalcFields(DataError.TrapError, new int[1] { 25 });
					byte[] bytes = navRecord.GetFieldValue(25).GetBytes();
					if (bytes != null && bytes.Length != 0)
					{
						controlAddInData.Resource = bytes;
					}
				}
				list.Add(controlAddInData);
				flag = navRecord.ALNext() != 0;
			}
			return list;
		}
	}

	private static List<ControlAddInData> GetControlAddInDataFromTenantTable(NavSession session, bool includeJavaScript, string addInName = null)
	{
		List<ControlAddInData> list = new List<ControlAddInData>();
		using (new MaximizedPermissionScope(session, new int[1] { 2000000169 }))
		{
			using NavRecord navRecord = new NavRecord(session, 2000000169, SecurityFiltering.Ignored);
			if (includeJavaScript)
			{
				navRecord.ALSetFilter(5, "<>%1", NavInteger.Create(2));
			}
			else
			{
				navRecord.ALSetFilter(5, "%1 | %2", NavInteger.Create(1), NavInteger.Create(3));
			}
			if (!string.IsNullOrEmpty(addInName))
			{
				NavAppAddInAssemblyName navAppAddInAssemblyName = NavAppAddInAssemblyName.CreateFromString(addInName, AssemblyNameSeparatorToken.Semicolon);
				navRecord.ALSetFilter(2, navAppAddInAssemblyName.Name);
				navRecord.ALSetFilter(3, navAppAddInAssemblyName.PublicKeyToken);
			}
			bool flag = navRecord.ALFindSet(DataError.TrapError);
			while (flag)
			{
				ControlAddInData controlAddInData = new ControlAddInData
				{
					CtrlName = navRecord.GetFieldValue(2).ToString(),
					PublicKeyToken = navRecord.GetFieldValue(3).ToString(),
					VersionNumber = navRecord.GetFieldValue(4).ToString()
				};
				if (includeJavaScript && navRecord.GetFieldValue(5).ToInt32() == 0)
				{
					navRecord.CalcFields(DataError.TrapError, new int[1] { 7 });
					byte[] bytes = navRecord.GetFieldValue(7).GetBytes();
					if (bytes != null && bytes.Length != 0)
					{
						controlAddInData.Resource = bytes;
					}
				}
				list.Add(controlAddInData);
				flag = navRecord.ALNext() != 0;
			}
			return list;
		}
	}

	private static void UpdateControlAddInProperty(ControlDefinition controlDefinition, ControlAddInData controlAddIn)
	{
		if (controlAddIn != null)
		{
			if (!string.IsNullOrEmpty(controlAddIn.VersionNumber))
			{
				string controlAddIn2 = controlAddIn.ControlAddInName + ", Version=" + controlAddIn.VersionNumber;
				controlDefinition.ControlAddIn = controlAddIn2;
			}
			if (controlAddIn.Resource != null && !IsControlAddInManifestUnpacked(controlAddIn))
			{
				UnpackControlAddInManifest(controlAddIn);
				if (ParseControlAddInManifest(controlAddIn))
				{
					UnpackControlAddInResources(controlAddIn);
				}
			}
			if (!string.IsNullOrEmpty(controlAddIn.ErrorMessage))
			{
				controlDefinition.ControlAddInManifest = new ControlAddInManifestDefinition
				{
					ErrorMessage = controlAddIn.ErrorMessage
				};
			}
			else if (controlAddIn.ParsedManifest != null)
			{
				ControlAddInManifestDefinition controlAddInManifest = new ControlAddInManifestDefinition
				{
					RequestedWidth = controlAddIn.ParsedManifest.RequestedWidth,
					RequestedHeight = controlAddIn.ParsedManifest.RequestedHeight,
					MinimumWidth = controlAddIn.ParsedManifest.MinimumWidth,
					MinimumHeight = controlAddIn.ParsedManifest.MinimumHeight,
					MaximumWidth = controlAddIn.ParsedManifest.MaximumWidth,
					MaximumHeight = controlAddIn.ParsedManifest.MaximumHeight,
					Resources = controlAddIn.ExtractedResources,
					Script = controlAddIn.ParsedManifest.Script,
					RecreateScript = controlAddIn.ParsedManifest.RecreateScript,
					RefreshScript = controlAddIn.ParsedManifest.RefreshScript,
					ScriptUrls = controlAddIn.ParsedManifest.ScriptUrls.ToList(),
					StyleSheetUrls = controlAddIn.ParsedManifest.StyleSheetUrls.ToList(),
					VerticalShrink = controlAddIn.ParsedManifest.VerticalShrink,
					HorizontalShrink = controlAddIn.ParsedManifest.HorizontalShrink,
					VerticalStretch = controlAddIn.ParsedManifest.VerticalStretch,
					HorizontalStretch = controlAddIn.ParsedManifest.HorizontalStretch,
					AllowCaption = controlAddIn.ParsedManifest.AllowCaption,
					Version = controlAddIn.ManifestVersion
				};
				string optionCaption = controlDefinition.OptionCaption;
				string text = ParseControlAddInManifestOverride(optionCaption, controlAddInManifest);
				if (!string.IsNullOrEmpty(text))
				{
					controlAddInManifest = new ControlAddInManifestDefinition
					{
						ErrorMessage = text
					};
				}
				controlDefinition.ControlAddInManifest = controlAddInManifest;
			}
		}
		else
		{
			controlDefinition.ControlAddIn = null;
			controlDefinition.ControlAddInManifest = null;
		}
	}

	private static bool ParseControlAddInManifest(ControlAddInData controlAddIn)
	{
		if (controlAddIn.Manifest == null)
		{
			return false;
		}
		ControlAddInManifestParser controlAddInManifestParser = new ControlAddInManifestParser(controlAddIn.Manifest);
		if (controlAddInManifestParser.Parse())
		{
			controlAddIn.ParsedManifest = controlAddInManifestParser.Manifest;
		}
		else
		{
			controlAddIn.ParsedManifest = null;
			controlAddIn.ErrorMessage = controlAddInManifestParser.ErrorMessage;
		}
		return controlAddIn.ParsedManifest != null;
	}

	private static string ParseControlAddInManifestOverride(string controlAddInManifestOverride, ControlAddInManifestDefinition controlAddInManifest)
	{
		string text = ControlAddInManifestParser.ParseOverride(controlAddInManifestOverride, controlAddInManifest);
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return null;
	}

	private static bool IsControlAddInManifestUnpacked(ControlAddInData controlAddIn)
	{
		if (controlAddIn.Resource != null)
		{
			if (controlAddIn.Manifest == null)
			{
				return controlAddIn.ErrorMessage != null;
			}
			return true;
		}
		return false;
	}

	private static void UnpackControlAddInManifest(ControlAddInData controlAddIn)
	{
		try
		{
			using (ControlAddInZipArchiveReader controlAddInZipArchiveReader = new ControlAddInZipArchiveReader(controlAddIn.Resource))
			{
				controlAddIn.Manifest = controlAddInZipArchiveReader.ReadManifest();
			}
			if (controlAddIn.Manifest == null)
			{
				controlAddIn.ErrorMessage = string.Format(CultureInfo.CurrentCulture, Lang.ManifestParser_MissingResource, "manifest.xml");
			}
		}
		catch (InvalidDataException)
		{
			controlAddIn.ErrorMessage = string.Format(CultureInfo.CurrentCulture, Lang.ControlAddInInvalidResource, controlAddIn.ControlAddInName);
		}
	}

	private static void UnpackControlAddInResources(ControlAddInData controlAddIn)
	{
		try
		{
			using ControlAddInZipArchiveReader controlAddInZipArchiveReader = new ControlAddInZipArchiveReader(controlAddIn.Resource);
			UnpackControlAddInResources(controlAddInZipArchiveReader, controlAddIn, controlAddIn.ParsedManifest.ScriptResourceNames, ControlAddInResourceType.Script);
			UnpackControlAddInResources(controlAddInZipArchiveReader, controlAddIn, controlAddIn.ParsedManifest.StyleSheetResourceNames, ControlAddInResourceType.StyleSheet);
			UnpackControlAddInResources(controlAddInZipArchiveReader, controlAddIn, controlAddIn.ParsedManifest.ImageResourceNames, ControlAddInResourceType.Image);
			DateTimeOffset dateTimeOffset = controlAddInZipArchiveReader.WriteTime;
			if (dateTimeOffset == DateTimeOffset.MinValue)
			{
				dateTimeOffset = DateTimeOffset.Now;
			}
			controlAddIn.ManifestVersion = dateTimeOffset.UtcTicks.ToString(CultureInfo.InvariantCulture);
		}
		catch (InvalidDataException)
		{
			controlAddIn.ErrorMessage = string.Format(CultureInfo.CurrentCulture, Lang.ControlAddInInvalidResource, controlAddIn.ControlAddInName);
		}
	}

	private static void UnpackControlAddInResources(ControlAddInZipArchiveReader reader, ControlAddInData controlAddIn, IEnumerable<string> resourceNames, ControlAddInResourceType resourceType)
	{
		foreach (string resourceName in resourceNames)
		{
			if (controlAddIn.ErrorMessage != null)
			{
				break;
			}
			byte[] array = ((controlAddIn.ParsedManifest.Version <= 1) ? reader.ReadResource(resourceType, resourceName) : reader.ReadResource(resourceName));
			if (array != null)
			{
				controlAddIn.ExtractedResources.Add(new ControlAddInResourceDefinition
				{
					ResourceName = resourceName,
					ResourceType = resourceType,
					ResourceValue = array
				});
			}
			else
			{
				controlAddIn.ErrorMessage = string.Format(CultureInfo.CurrentCulture, Lang.ManifestParser_MissingResource, resourceName);
			}
		}
	}
}
