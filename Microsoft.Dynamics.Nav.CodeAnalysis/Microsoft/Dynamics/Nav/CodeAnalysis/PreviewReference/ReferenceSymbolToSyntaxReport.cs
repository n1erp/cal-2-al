using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.PreviewReference;

internal class ReferenceSymbolToSyntaxReport : ReferenceSymbolToSyntax
{
	protected override ApplicationObjectSyntax ConvertApplicationSymbol(ISymbol symbol)
	{
		return ReferenceSymbolToSyntax.ConvertApplicationSymbol<ReportTypeSymbol>(symbol, ConvertReport);
	}

	private ReportSyntax ConvertReport(ReportTypeSymbol report)
	{
		ReportSyntax reportSyntax = SyntaxFactory.Report(SyntaxFactory.ObjectId(report.Id), SyntaxFactory.IdentifierName(report.Name));
		ImmutableArray<Symbol> members = report.GetMembers();
		if (!report.Properties.IsDefaultOrEmpty)
		{
			ReportSyntax reportSyntax2 = reportSyntax;
			PropertySyntaxOrEmpty[] items = ReferenceSymbolToSyntax.ConvertProperties(report.Properties, report);
			reportSyntax = reportSyntax2.AddPropertyListProperties(items);
		}
		if (members != null && !members.IsEmpty)
		{
			reportSyntax = reportSyntax.WithMembers(ConvertMembers(members));
		}
		return reportSyntax;
	}
}
