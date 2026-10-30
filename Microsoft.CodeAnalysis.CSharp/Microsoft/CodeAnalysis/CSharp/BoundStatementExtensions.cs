using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class BoundStatementExtensions
{
	[Conditional("DEBUG")]
	internal static void AssertIsLabeledStatement(this BoundStatement node)
	{
		switch (node.Kind)
		{
		case BoundKind.SwitchSection:
		case BoundKind.PatternSwitchSection:
		case BoundKind.LabelStatement:
		case BoundKind.LabeledStatement:
			return;
		}
		throw ExceptionUtilities.UnexpectedValue(node.Kind);
	}

	[Conditional("DEBUG")]
	internal static void AssertIsLabeledStatementWithLabel(this BoundStatement node, LabelSymbol label)
	{
		switch (node.Kind)
		{
		case BoundKind.SwitchSection:
			foreach (BoundSwitchLabel switchLabel in ((BoundSwitchSection)node).SwitchLabels)
			{
				if (switchLabel.Label == label)
				{
					return;
				}
			}
			throw ExceptionUtilities.Unreachable;
		case BoundKind.PatternSwitchSection:
			foreach (BoundPatternSwitchLabel switchLabel2 in ((BoundPatternSwitchSection)node).SwitchLabels)
			{
				if (switchLabel2.Label == label)
				{
					return;
				}
			}
			throw ExceptionUtilities.Unreachable;
		default:
			throw ExceptionUtilities.UnexpectedValue(node.Kind);
		case BoundKind.LabelStatement:
		case BoundKind.LabeledStatement:
			break;
		}
	}
}
