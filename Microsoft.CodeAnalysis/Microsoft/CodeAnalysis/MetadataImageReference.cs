using System.Diagnostics;
using System.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal sealed class MetadataImageReference : PortableExecutableReference
{
	private readonly string _display;

	private readonly Metadata _metadata;

	public override string Display
	{
		get
		{
			string text = _display;
			if (text == null)
			{
				text = FilePath;
				if (text == null)
				{
					if (Properties.Kind != MetadataImageKind.Assembly)
					{
						return CodeAnalysisResources.InMemoryModule;
					}
					text = CodeAnalysisResources.InMemoryAssembly;
				}
			}
			return text;
		}
	}

	internal MetadataImageReference(Metadata metadata, MetadataReferenceProperties properties, DocumentationProvider documentation, string filePath, string display)
		: base(properties, filePath, documentation ?? DocumentationProvider.Default)
	{
		_display = display;
		_metadata = metadata;
	}

	protected override Metadata GetMetadataImpl()
	{
		return _metadata;
	}

	protected override DocumentationProvider CreateDocumentationProvider()
	{
		throw ExceptionUtilities.Unreachable;
	}

	protected override PortableExecutableReference WithPropertiesImpl(MetadataReferenceProperties properties)
	{
		return new MetadataImageReference(_metadata, properties, DocumentationProvider, FilePath, _display);
	}

	private string GetDebuggerDisplay()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((Properties.Kind == MetadataImageKind.Module) ? "Module" : "Assembly");
		if (!Properties.Aliases.IsEmpty)
		{
			stringBuilder.Append(" Aliases={");
			stringBuilder.Append(string.Join(", ", Properties.Aliases));
			stringBuilder.Append("}");
		}
		if (Properties.EmbedInteropTypes)
		{
			stringBuilder.Append(" Embed");
		}
		if (FilePath != null)
		{
			stringBuilder.Append(" Path='");
			stringBuilder.Append(FilePath);
			stringBuilder.Append("'");
		}
		if (_display != null)
		{
			stringBuilder.Append(" Display='");
			stringBuilder.Append(_display);
			stringBuilder.Append("'");
		}
		return stringBuilder.ToString();
	}
}
