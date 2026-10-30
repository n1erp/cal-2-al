using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.Types.Apps;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class ConverterSyntaxGenManager
{
	private List<DeltaSyntaxGen> syntaxGens = new List<DeltaSyntaxGen>();

	public ConverterSyntaxGenManager()
	{
		foreach (PersonalizationV1DeltaType value in Enum.GetValues(typeof(PersonalizationV1DeltaType)))
		{
			if (value != PersonalizationV1DeltaType.Add)
			{
				syntaxGens.Add(DeltaSyntaxGenFactory.GetSyntaxGenerator(value));
			}
		}
	}

	public ConverterResult GetSyntaxNodes(ConverterBundle converterBundle, IList<NavXmlBase> deltas)
	{
		if (converterBundle == null)
		{
			throw new ArgumentNullException("converterBundle");
		}
		if (deltas == null)
		{
			throw new ArgumentNullException("converterBundle");
		}
		Dictionary<SyntaxNodeIdentifier, SyntaxNode> dictionary = new Dictionary<SyntaxNodeIdentifier, SyntaxNode>();
		List<NavXmlDeltaConversionDiagnostics> list = new List<NavXmlDeltaConversionDiagnostics>();
		List<NavXmlDeltaConversionDiagnostics> list2 = new List<NavXmlDeltaConversionDiagnostics>();
		foreach (DeltaSyntaxGen syntaxGen in syntaxGens)
		{
			DeltaSyntaxGenResult deltaSyntaxGenResult = syntaxGen.GenerateSyntaxNodes(converterBundle, deltas);
			list2.AddRange(deltaSyntaxGenResult.ConversionWarnings);
			list.AddRange(deltaSyntaxGenResult.ConversionErrors);
			foreach (KeyValuePair<SyntaxNodeIdentifier, SyntaxNode> syntaxNode in deltaSyntaxGenResult.SyntaxNodes)
			{
				dictionary.Add(syntaxNode.Key, syntaxNode.Value);
			}
		}
		return new ConverterResult(new ConverterSyntaxNodesMerger(dictionary).MergeAllSyntaxNodes(), list2, list);
	}
}
