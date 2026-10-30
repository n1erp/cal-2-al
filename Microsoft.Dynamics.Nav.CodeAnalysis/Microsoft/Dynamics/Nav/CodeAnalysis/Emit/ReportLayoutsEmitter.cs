using System;
using System.IO;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal class ReportLayoutsEmitter : PreMetadataEmitter<SourceReportTypeSymbol>
{
	public ReportLayoutsEmitter(SourceReportTypeSymbol symbol, ModuleBuilder moduleBuilder)
		: base(symbol, moduleBuilder)
	{
	}

	internal override void Emit(DiagnosticBag diagnostics)
	{
		if ((Symbol.ContainingModule.DeclaringCompilation.Options.CompilationGenerateOptions & CompilationGenerationOptions.ReportLayout) != 0)
		{
			GenerateLayout(diagnostics, PropertyKind.WordLayout, ReportWordUtilities.GenerateWordLayout, "Word");
			GenerateLayout(diagnostics, PropertyKind.RDLCLayout, ReportRdlcUtilities.GenerateRdlcLayout, "RDL");
		}
	}

	private void GenerateLayout(DiagnosticBag diagnostics, PropertyKind propertyKind, Action<SourceReportTypeSymbol, IFileSystem, string> generator, string layoutType)
	{
		string text = Symbol.GetBoundPropertyValue<BoundLiteralPropertyValue>(propertyKind)?.Value.StringValue;
		if (text == null)
		{
			return;
		}
		try
		{
			generator(Symbol, Symbol.DeclaringCompilation.FileSystem, text);
		}
		catch (FileNotFoundException ex)
		{
			if (ex.FileName != "DocumentFormat.OpenXml, Version=2.7.2.0, Culture=neutral, PublicKeyToken=8fb06cb64d019a17")
			{
				throw;
			}
		}
		catch (IOException)
		{
			diagnostics.Add(ErrorCode.ERR_FileInUse, Location.None, text);
		}
		catch (UnauthorizedAccessException)
		{
			if (Symbol.DeclaringCompilation.Options.ReadonlyErrorsAsWarnings)
			{
				diagnostics.Add(ErrorCode.WRN_FileIsReadonly, Location.None, text);
			}
			else
			{
				diagnostics.Add(ErrorCode.ERR_FileIsReadonly, Location.None, text);
			}
		}
		catch (Exception ex4) when (ex4 is InvalidDataException || ex4 is XmlException)
		{
			diagnostics.Add(ErrorCode.ERR_MalformedReportLayout, Location.None, layoutType, text, ex4.Message);
		}
	}
}
