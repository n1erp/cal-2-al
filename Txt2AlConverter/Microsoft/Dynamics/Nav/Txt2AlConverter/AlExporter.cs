using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference.DotNet;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.IO;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Modification;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Page;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Query;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Report;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.XmlPort;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

public sealed class AlExporter : IApplicationObjectExporter
{
	private readonly AlExportOptions options;

	private readonly IDotNetDeclarationExtractor dotNetExtractor;

	private readonly ITranslationExtractor translationExtractor;

	private readonly ITypeResolver typeResolver;

	private readonly IEnumDeclarationExtractor enumDeclarationExtractor;

	private readonly IFileNameExporter fileNameExporter;

	public AlExporter()
		: this(AlExportOptions.Default)
	{
	}

	public AlExporter(AlExportOptions options, IDotNetDeclarationExtractor dotNetExtractor = null, ITranslationExtractor translationExtractor = null, IEnumDeclarationExtractor enumDeclarationExtractor = null, IFileNameExporter fileNameExporter = null)
	{
		this.dotNetExtractor = dotNetExtractor ?? new NullDotNetDeclarationExtractor(quoteName: true);
		this.translationExtractor = translationExtractor ?? NullTranslationExtractor.Instance;
		this.enumDeclarationExtractor = enumDeclarationExtractor ?? NullEnumDeclarationExtractor.Instance;
		this.options = options ?? AlExportOptions.Default;
		this.fileNameExporter = fileNameExporter ?? new BaseFileNameExporter();
		if (this.options.ResolveTypeNames)
		{
			typeResolver = new UnresolvedTypeExporter(this.dotNetExtractor, this.translationExtractor);
		}
		else
		{
			typeResolver = new BaseTypeResolver(this.dotNetExtractor, this.translationExtractor);
		}
	}

	public static string ConvertAppManifest(string manifestFilePath)
	{
		string text = File.ReadAllText(manifestFilePath);
		if (string.IsNullOrWhiteSpace(text))
		{
			return string.Empty;
		}
		return NavAppManifest.FromXml(text).ToString();
	}

	public static string GetV1ExtensionManifestFileName()
	{
		return "manifest.xml";
	}

	public static string GetV2ExtensionManifestFileName()
	{
		return "app.json";
	}

	public void ExportObject(ApplicationObject applicationObject, Stream stream)
	{
		ExportObjects(new ApplicationObject[1] { applicationObject }, stream);
	}

	public string ExportObjectToString(ApplicationObject applicationObject)
	{
		using MemoryStream memoryStream = new MemoryStream();
		ExportObject(applicationObject, memoryStream);
		memoryStream.Position = 0L;
		using StreamReader streamReader = new StreamReader(memoryStream, Encoding.Unicode);
		return streamReader.ReadToEnd();
	}

	public string ExportDotNetToString(IDotNetDeclarationExtractor extractor)
	{
		if (!extractor.HasDeclarations)
		{
			return string.Empty;
		}
		using MemoryStream memoryStream = new MemoryStream();
		ExportDotNet(extractor, memoryStream);
		memoryStream.Position = 0L;
		using StreamReader streamReader = new StreamReader(memoryStream, Encoding.Unicode);
		return streamReader.ReadToEnd();
	}

	public string ExportEnumToString(IEnumDeclarationExtractor extractor)
	{
		if (!extractor.HasDeclarations)
		{
			return string.Empty;
		}
		using MemoryStream memoryStream = new MemoryStream();
		ExportEnum(extractor, memoryStream);
		memoryStream.Position = 0L;
		using StreamReader streamReader = new StreamReader(memoryStream, Encoding.Unicode);
		return streamReader.ReadToEnd();
	}

	public string ExportObjectToString(IEnumerable<ApplicationObject> applicationObjects)
	{
		using MemoryStream memoryStream = new MemoryStream();
		ExportObjects(applicationObjects, memoryStream);
		memoryStream.Position = 0L;
		using StreamReader streamReader = new StreamReader(memoryStream, Encoding.Unicode);
		return streamReader.ReadToEnd();
	}

	public void ExportObjects(IEnumerable<ApplicationObject> applicationObjects, Stream stream, Action progressAction = null)
	{
		using StreamWriter streamWriter = GetStreamWriter(stream);
		AlWriter alWriter = GetAlWriter(streamWriter);
		foreach (ApplicationObject applicationObject in applicationObjects)
		{
			if (IsSupportedObjectType(applicationObject))
			{
				GetApplicationObjectExporter(applicationObject.ElementTypeInfo.ElementType).Export(applicationObject, alWriter);
			}
			progressAction?.Invoke();
		}
		streamWriter.Flush();
	}

	public void ExportRdlc(IEnumerable<ApplicationObject> applicationObjects, Stream stream)
	{
		RdlcLayoutExporter rdlcLayoutExporter = new RdlcLayoutExporter();
		using StreamWriter streamWriter = GetStreamWriter(stream);
		AlWriter alWriter = GetAlWriter(streamWriter);
		foreach (ApplicationObject applicationObject in applicationObjects)
		{
			if (applicationObject.RootElement.HasRdlc())
			{
				rdlcLayoutExporter.Export(applicationObject.RootElement, alWriter);
			}
		}
		streamWriter.Flush();
	}

	public MemoryStream ExportWordLayout(IEnumerable<ApplicationObject> applicationObjects)
	{
		WordLayoutExporter wordLayoutExporter = new WordLayoutExporter();
		MemoryStream memoryStream = new MemoryStream();
		foreach (ApplicationObject applicationObject in applicationObjects)
		{
			if (applicationObject.RootElement.HasWordLayout())
			{
				wordLayoutExporter.Export(applicationObject.RootElement, memoryStream);
			}
		}
		memoryStream.Flush();
		memoryStream.Position = 0L;
		return memoryStream;
	}

	public void ExportDotNet(IDotNetDeclarationExtractor extractor, Stream stream)
	{
		if (!extractor.HasDeclarations)
		{
			return;
		}
		DotNetExporter dotNetExporter = new DotNetExporter(options.DotNetAddInsInjector);
		using StreamWriter streamWriter = new StreamWriter(stream, Encoding.Unicode, 32768, leaveOpen: true);
		AlWriter writer = new AlWriter(streamWriter, options, new BaseFileNameExporter());
		dotNetExporter.Export(extractor.GetDeclaration(), writer);
		streamWriter.Flush();
	}

	public void ExportEnum(IEnumDeclarationExtractor extractor, Stream stream)
	{
		if (!extractor.HasDeclarations)
		{
			return;
		}
		EnumExporter enumExporter = new EnumExporter();
		using StreamWriter streamWriter = new StreamWriter(stream, Encoding.Unicode, 32768, leaveOpen: true);
		AlWriter writer = new AlWriter(streamWriter, options, new BaseFileNameExporter());
		enumExporter.Export(extractor.GetDeclarations(), writer);
		streamWriter.Flush();
	}

	private static StreamWriter GetStreamWriter(Stream stream)
	{
		return new StreamWriter(stream, Encoding.Unicode, 32768, leaveOpen: true);
	}

	private static bool IsSupportedObjectType(ApplicationObject ao)
	{
		switch (ao.ElementTypeInfo.ElementType)
		{
		case ElementType.Report:
		case ElementType.XmlPort:
		case ElementType.Codeunit:
		case ElementType.Table:
		case ElementType.Page:
		case ElementType.Query:
			return true;
		case ElementType.Modification:
			return IsChangeTypeSupported(ao.Id.ObjectType);
		default:
			return false;
		}
	}

	private static bool IsChangeTypeSupported(ElementType targetType)
	{
		if ((uint)(targetType - 20) <= 1u)
		{
			return true;
		}
		return false;
	}

	private AlWriter GetAlWriter(StreamWriter destinationStreamWriter)
	{
		return new AlWriter(destinationStreamWriter, options, fileNameExporter, dotNetExtractor, translationExtractor, typeResolver, enumDeclarationExtractor);
	}

	private ApplicationObjectExporter GetApplicationObjectExporter(ElementType applicationObjectType)
	{
		return applicationObjectType switch
		{
			ElementType.Table => new ApplicationObjectExporter(new TableContentExporter(options), options), 
			ElementType.Codeunit => new ApplicationObjectExporter(new CodeunitContentExporter(options), options), 
			ElementType.Report => new ApplicationObjectExporter(new ReportContentExporter(options), options), 
			ElementType.Page => new ApplicationObjectExporter(new PageContentExporter(options), options), 
			ElementType.Query => new ApplicationObjectExporter(new QueryContentExporter(options), options), 
			ElementType.XmlPort => new ApplicationObjectExporter(new XmlPortContentExporter(options), options), 
			ElementType.Modification => new ApplicationObjectExporter(new ModificationContentExporter(options), options), 
			_ => throw new ArgumentOutOfRangeException("Unsupported application object type:" + applicationObjectType), 
		};
	}
}
