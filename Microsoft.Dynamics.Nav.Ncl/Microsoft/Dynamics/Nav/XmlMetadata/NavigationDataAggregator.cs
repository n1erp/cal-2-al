using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Model.Tools.IO.Nea;
using Microsoft.Dynamics.Nav.Runtime;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Runtime.XmlMetadata;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.XmlMetadata;

internal sealed class NavigationDataAggregator
{
	private const int Uninitialized = -1;

	internal static string InternalActionGroupCaptionML = "ENU=Extension";

	internal static string InternalActionGroupCaptionName = "Extension";

	private static ActionGroupDefinition EmptyGroup = new ActionGroupDefinition();

	private readonly INavAppMetadataRetriever metadataRetriever;

	private readonly object mutex = new object();

	private IDictionary<PackageId, ActionBaseDefinition> navigationDataCache = new Dictionary<PackageId, ActionBaseDefinition>();

	internal int CachedItems => navigationDataCache?.Count ?? 0;

	internal NavigationDataAggregator(INavAppMetadataRetriever metadataRetriever)
	{
		this.metadataRetriever = metadataRetriever;
	}

	internal void InvalidateNavigationDataCache(NavTrackingEventArgs eventArgs)
	{
		IEnumerable<PackageId> enumerable = eventArgs.TrackingObjects.Select((NavTrackingObject to) => to.AppPackageId).Distinct();
		lock (mutex)
		{
			foreach (PackageId item in enumerable)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001JK", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Invalidating navigation data cache for packageId:{0}", ((WrappedValue<Guid>)(object)item).Value);
				navigationDataCache.Remove(item);
			}
		}
	}

	internal void AppendExtensionNavigationData(NavigationDefinition existingDefinitions, NavAppGroup appGroup)
	{
		NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance();
		using (mostSpecificInstance.CreateTraceScope("00001JM", DiagnosticParameter.SystemMetadata("Appending navigation data from all extensions"), MonitorType.None, Category.Extensions, createNewActivity: false))
		{
			ActionContainerDefinition actionContainerDefinition = GetContainerDefinition(existingDefinitions);
			if (actionContainerDefinition == null)
			{
				actionContainerDefinition = new ActionContainerDefinition
				{
					ActionContainerType = ActionContainerType.Departments,
					Actions = new List<ActionBaseDefinition>()
				};
				existingDefinitions.ActionContainers.Add(actionContainerDefinition);
			}
			IList<(ActionBaseDefinition, NavAppRuntimeMetadata)> navigationData = GetNavigationData(appGroup);
			if (navigationData.Count > 0)
			{
				if (actionContainerDefinition.Actions == null)
				{
					actionContainerDefinition.Actions = new List<ActionBaseDefinition>();
				}
				foreach (var item3 in navigationData)
				{
					ActionBaseDefinition item = item3.Item1;
					NavAppRuntimeMetadata item2 = item3.Item2;
					List<ActionBaseDefinition> list = item?.Actions;
					string message;
					if (list != null && list.Count > 0)
					{
						actionContainerDefinition.Actions.Add(item);
						message = string.Format(CultureInfo.InvariantCulture, "{0} search items have been appended from the package '{1}'.", list.Count, item2.TechnicalDisplayName);
					}
					else
					{
						message = string.Format(CultureInfo.InvariantCulture, "Package {0} contains no search items.", item2.TechnicalDisplayName);
					}
					mostSpecificInstance.SendTraceTag("00006ND", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, message);
				}
			}
			AppendExtensionNavigationDataFromTranslationFiles(actionContainerDefinition.Actions, appGroup);
		}
	}

	private IList<(ActionBaseDefinition, NavAppRuntimeMetadata)> GetNavigationData(NavAppGroup appGroup)
	{
		lock (mutex)
		{
			List<(ActionBaseDefinition, NavAppRuntimeMetadata)> list = new List<(ActionBaseDefinition, NavAppRuntimeMetadata)>();
			foreach (NavAppRuntimeMetadata item in appGroup.OrderedAppMetadata.Where((NavAppRuntimeMetadata m) => m.ExtensionType == NavExtensionType.ModernDev))
			{
				ActionBaseDefinition packageNavigationDataFromCache = GetPackageNavigationDataFromCache(item.PackageId);
				if (packageNavigationDataFromCache != EmptyGroup)
				{
					list.Add((packageNavigationDataFromCache.DeepClone(), item));
				}
			}
			return list;
		}
	}

	private ActionBaseDefinition GetPackageNavigationDataFromCache(PackageId packageId)
	{
		if (navigationDataCache.TryGetValue(packageId, out var value))
		{
			return value;
		}
		ActionBaseDefinition actionBaseDefinition = CreateNavigationDataFromPackage(packageId);
		navigationDataCache.Add(packageId, actionBaseDefinition);
		return actionBaseDefinition;
	}

	private ActionBaseDefinition CreateNavigationDataFromPackage(PackageId id)
	{
		NavAppPackageReader packageReader = GetPackageReader(id);
		NavAppRuntimeMetadata metadata = metadataRetriever.GetMetadata(id);
		string appName = InternalActionGroupCaptionName;
		if (metadata != null && !string.IsNullOrEmpty(metadata.Name))
		{
			appName = metadata.Name;
		}
		if (packageReader != null)
		{
			using Stream stream = packageReader.ReadNavigationFile();
			if (stream != null)
			{
				Stream stream2 = (NeaStreamReader.IsSupported(stream) ? new NeaStreamReader(stream) : stream);
				using StreamReader streamReader = new StreamReader(stream2, detectEncodingFromByteOrderMarks: true);
				string content = streamReader.ReadToEnd();
				return CreateNavigationData(content, appName);
			}
		}
		return EmptyGroup;
	}

	private NavAppPackageReader GetPackageReader(PackageId packageId)
	{
		try
		{
			Stream packageBlob = metadataRetriever.GetPackageBlob(packageId);
			return NavAppPackageReader.Create(packageBlob);
		}
		catch (Exception ex)
		{
			string message = string.Format(CultureInfo.InvariantCulture, "Cannot open the package file due to the following error: {0}", ex.Message);
			throw new InvalidOperationException(message, ex);
		}
	}

	private static ActionBaseDefinition CreateNavigationData(string content, string appName = "Extension")
	{
		NavigationDefinition navigationDefinition = TryReadNavigationDefinition(content, out var hasError);
		if (hasError)
		{
			content = Microsoft.Dynamics.Nav.Types.Metadata.XmlMetadataCleaner.CleanInvalidChars(content);
			navigationDefinition = TryReadNavigationDefinition(content, out hasError);
		}
		if (navigationDefinition != null)
		{
			ActionContainerDefinition containerDefinition = GetContainerDefinition(navigationDefinition);
			if (containerDefinition != null)
			{
				ActionBaseDefinition actionBaseDefinition = containerDefinition.Actions.Find((ActionBaseDefinition a) => a.Caption.Equals(InternalActionGroupCaptionName, StringComparison.OrdinalIgnoreCase));
				if (actionBaseDefinition != null)
				{
					actionBaseDefinition.Caption = appName;
					return actionBaseDefinition;
				}
			}
		}
		return EmptyGroup;
	}

	private static NavigationDefinition TryReadNavigationDefinition(string content, out bool hasError)
	{
		try
		{
			using XmlReader xmlReader = XmlHelper.SafeXmlReader(new StringReader(content));
			NavigationDefinition result = (NavigationDefinition)MetaPage.NavigationDefinitionSerializer.Deserialize(xmlReader);
			hasError = false;
			return result;
		}
		catch (InvalidOperationException ex) when (ex.InnerException is XmlException)
		{
			hasError = true;
			return null;
		}
	}

	private static ActionContainerDefinition GetContainerDefinition(NavigationDefinition definition)
	{
		return definition.ActionContainers.Find((ActionContainerDefinition a) => a.ActionContainerType == ActionContainerType.Departments);
	}

	private static void AppendExtensionNavigationDataFromTranslationFiles(IList<ActionBaseDefinition> definition, NavAppGroup appGroup)
	{
		NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance();
		using (mostSpecificInstance.CreateTraceScope("0000292", DiagnosticParameter.SystemMetadata("Updating navigation translations from text files"), MonitorType.None, Category.Extensions, createNewActivity: false))
		{
			int num = 0;
			if (LanguageProvider.Provider.HasData(appGroup.GroupId))
			{
				foreach (ActionBaseDefinition item in definition)
				{
					if (item is ActionDefinition actionDefinition && (actionDefinition.RunObjectType == RunObjectType.Page || actionDefinition.RunObjectType == RunObjectType.Report))
					{
						ObjectType objectType = ((actionDefinition.RunObjectType == RunObjectType.Page) ? ObjectType.Page : ObjectType.Report);
						int targetID = actionDefinition.TargetID;
						if (LanguageProvider.Provider.HasDataFor(objectType, targetID, appGroup.GroupId))
						{
							LanguageElements globalLanguageElements = LanguageProvider.Provider.GetGlobalLanguageElements(objectType, targetID, ControlType.None, -1, 0, requestPage: false);
							LanguageElements navAppLanguageElements = LanguageProvider.Provider.GetNavAppLanguageElements(objectType, targetID, ControlType.None, -1, 0, requestPage: false, appGroup.GroupId);
							string captionML = actionDefinition.CaptionML;
							string attributeValue = ElementDefinition.MergeAppLanguageStrings(captionML, navAppLanguageElements);
							string captionML2 = ElementDefinition.MergeGlobalLanguageStrings(attributeValue, globalLanguageElements);
							actionDefinition.CaptionML = captionML2;
							num++;
						}
					}
				}
			}
			mostSpecificInstance.SendTraceTag("0000291", Category.Extensions, Verbosity.Normal, "{0} navigation data items had updates to translations.", DiagnosticParameter.SystemMetadata(num));
		}
	}
}
