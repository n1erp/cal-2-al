using System;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DiagnosticRules;

internal class RuleSetLoader
{
	internal virtual string ResolvePath(string pathToParent, string includePath)
	{
		string text = PathUtilities.ResolveRelativePath(includePath, pathToParent, null, null, (string p) => true);
		if (text != null)
		{
			text = PathUtilities.TryNormalizeAbsolutePath(text);
		}
		if (text == null)
		{
			string message = ((pathToParent != null) ? string.Format(CultureInfo.InvariantCulture, CodeAnalysisResources.ERR_FailedToResolveRelativePathToRuleSet, includePath, pathToParent) : string.Format(CultureInfo.InvariantCulture, CodeAnalysisResources.ERR_FailedToResolveAbsolutePathToRuleSet, includePath));
			throw new FileNotFoundException(message);
		}
		return text;
	}

	internal virtual ExternalRuleSet LoadRuleSet(string pathToIncludedRuleSet)
	{
		try
		{
			DebugAssertHelper.Assert(PathUtilities.IsAbsolute(pathToIncludedRuleSet));
			using StreamReader reader = File.OpenText(pathToIncludedRuleSet);
			using JsonTextReader reader2 = new JsonTextReader(reader);
			return new JsonSerializer
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			}.Deserialize<ExternalRuleSet>(reader2);
		}
		catch (Exception ex)
		{
			throw new InvalidRuleSetException(ex.Message, ex);
		}
	}
}
