using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class FeatureExtensions
{
	public static Version RequiredVersion(this Feature feature)
	{
		switch (feature)
		{
		case Feature.DotNet:
		case Feature.Enum:
		case Feature.FieldGroupExtension:
			return RuntimeVersion.Fall2018;
		case Feature.RunQueryObject:
		case Feature.Views:
			return RuntimeVersion.Spring2019;
		default:
			throw ExceptionUtilities.UnexpectedValue(feature);
		}
	}

	public static string Localize(this Feature feature)
	{
		return ErrorFacts.GetMessage(feature, CultureInfo.CurrentCulture);
	}
}
