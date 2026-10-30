using System;
using System.IO;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public class RuleSetInclude
{
	private readonly string _includePath;

	private readonly ReportDiagnostic _action;

	public string IncludePath => _includePath;

	public ReportDiagnostic Action => _action;

	public RuleSetInclude(string includePath, ReportDiagnostic action)
	{
		_includePath = includePath;
		_action = action;
	}

	public RuleSet LoadRuleSet(RuleSet parent)
	{
		RuleSet result = null;
		string includePath = _includePath;
		try
		{
			includePath = GetIncludePath(parent);
			result = RuleSetProcessor.LoadFromFile(includePath);
		}
		catch (FileNotFoundException)
		{
		}
		catch (Exception ex2)
		{
			throw new InvalidRuleSetException(string.Format(CodeAnalysisResources.InvalidRuleSetInclude, includePath, ex2.Message));
		}
		return result;
	}

	private string GetIncludePath(RuleSet parent)
	{
		return Path.GetFullPath(ResolveIncludePath(_includePath, parent?.FilePath) ?? throw new FileNotFoundException(string.Format(CodeAnalysisResources.FailedToResolveRuleSetName, _includePath), _includePath));
	}

	private static string ResolveIncludePath(string includePath, string parentRulesetPath)
	{
		string text = ResolveIncludePathCore(includePath, parentRulesetPath);
		if (text == null && PathUtilities.IsUnixLikePlatform)
		{
			includePath = includePath.Replace('\\', Path.DirectorySeparatorChar);
			text = ResolveIncludePathCore(includePath, parentRulesetPath);
		}
		return text;
	}

	private static string ResolveIncludePathCore(string includePath, string parentRulesetPath)
	{
		includePath = Environment.ExpandEnvironmentVariables(includePath);
		if (Path.IsPathRooted(includePath))
		{
			if (File.Exists(includePath))
			{
				return includePath;
			}
		}
		else if (!string.IsNullOrEmpty(parentRulesetPath))
		{
			includePath = PathUtilities.CombinePathsUnchecked(Path.GetDirectoryName(parentRulesetPath), includePath);
			if (File.Exists(includePath))
			{
				return includePath;
			}
		}
		return null;
	}
}
