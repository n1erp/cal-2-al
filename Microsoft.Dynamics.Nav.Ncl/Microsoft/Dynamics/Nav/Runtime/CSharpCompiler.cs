using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class CSharpCompiler
{
	private readonly IList<MetadataReference> metadataReferences;

	private readonly SyntaxTree fileVersionInfoSyntaxTree;

	public FileVersionInfo ExecutingAssemblyFileVersionInfo { get; }

	public CSharpCompiler()
	{
		string location = Assembly.GetExecutingAssembly().Location;
		ExecutingAssemblyFileVersionInfo = FileVersionInfo.GetVersionInfo(location);
		fileVersionInfoSyntaxTree = CSharpSyntaxTree.ParseText(string.Format(CultureInfo.InvariantCulture, "using System.Reflection; [assembly: AssemblyVersion(\"{0}\")] [assembly: AssemblyFileVersion(\"{0}\")]", ExecutingAssemblyFileVersionInfo.FileVersion));
		metadataReferences = CreateAssemblyReferences(location);
	}

	public NavAppCSharpCompilerResult CompileCSharpFiles(string assemblyName, IEnumerable<string> sourceFiles, bool enableDebugging)
	{
		string pdbFilePath = Path.ChangeExtension(assemblyName, "pdb");
		List<Tuple<SourceText, string>> source = sourceFiles.Select((string f) =>
		{
			byte[] array = File.ReadAllBytes(f);
			return Tuple.Create(SourceText.From(array, array.Length, Encoding.UTF8, SourceHashAlgorithm.Sha1, throwIfBinaryDetected: false, canBeEmbedded: true), f);
		}).ToList();
		List<SyntaxTree> list = source.Select((Tuple<SourceText, string> tp) => CSharpSyntaxTree.ParseText(tp.Item1, null, tp.Item2)).ToList();
		List<EmbeddedText> list2 = new List<EmbeddedText>();
		if (enableDebugging)
		{
			list2 = source.Select((Tuple<SourceText, string> tp) => EmbeddedText.FromSource(tp.Item2, tp.Item1)).ToList();
		}
		list.Add(fileVersionInfoSyntaxTree);
		CSharpCompilation cSharpCompilation = CreateCompilation(assemblyName, list, enableDebugging);
		MemoryStream memoryStream = new MemoryStream();
		MemoryStream memoryStream2 = (enableDebugging ? new MemoryStream() : null);
		using (Stream win32Resources = cSharpCompilation.CreateDefaultWin32Resources(versionResource: true, noManifest: false, null, null))
		{
			EmitOptions options = new EmitOptions(metadataOnly: false, DebugInformationFormat.PortablePdb, pdbFilePath, null, 0, 0uL);
			IEnumerable<EmbeddedText> embeddedTexts = list2;
			EmitResult emitResult = cSharpCompilation.Emit(memoryStream, memoryStream2, null, win32Resources, null, options, null, null, embeddedTexts);
			if (!emitResult.Success)
			{
				return new NavAppCSharpCompilerResult(null, null, GetErrorsAsString(emitResult.Diagnostics));
			}
		}
		return new NavAppCSharpCompilerResult(memoryStream.ToArray(), memoryStream2?.ToArray(), null);
	}

	private CSharpCompilation CreateCompilation(string assemblyName, IList<SyntaxTree> syntaxTrees, bool enableDebugging)
	{
		OptimizationLevel optimizationLevel = ((!enableDebugging) ? OptimizationLevel.Release : OptimizationLevel.Debug);
		CSharpCompilationOptions cSharpCompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, reportSuppressedDiagnostics: false, null, null, null, null, optimizationLevel, checkOverflow: true, allowUnsafe: false, null, null, default, null, Platform.AnyCpu, ReportDiagnostic.Suppress, 0);
		IEnumerable<MetadataReference> references = metadataReferences;
		CSharpCompilationOptions options = cSharpCompilationOptions;
		return CSharpCompilation.Create(assemblyName, syntaxTrees, references, options);
	}

	private static string GetErrorsAsString(IEnumerable<Microsoft.CodeAnalysis.Diagnostic> errors)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Microsoft.CodeAnalysis.Diagnostic error in errors)
		{
			stringBuilder.AppendLine(error.ToString());
		}
		return stringBuilder.ToString();
	}

	private static IList<MetadataReference> CreateAssemblyReferences(string strPath)
	{
		string directoryName = Path.GetDirectoryName(strPath);
		string[] source = new string[3]
		{
			Path.Combine(directoryName, "Microsoft.Dynamics.Nav.Ncl.dll"),
			Path.Combine(directoryName, "Microsoft.Dynamics.Nav.Types.dll"),
			Path.Combine(directoryName, "Microsoft.Dynamics.Nav.Language.dll")
		};
		IList<MetadataReference> list = source.Select((string p) => MetadataReference.CreateFromFile(p)).Cast<MetadataReference>().ToList();
		string directoryName2 = Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location);
		list.Add(MetadataReference.CreateFromFile(Path.Combine(directoryName2, "mscorlib.dll")));
		list.Add(MetadataReference.CreateFromFile(typeof(IStreamProvider).GetTypeInfo().Assembly.Location));
		list.Add(MetadataReference.CreateFromFile(typeof(Uri).GetTypeInfo().Assembly.Location));
		return list;
	}
}
