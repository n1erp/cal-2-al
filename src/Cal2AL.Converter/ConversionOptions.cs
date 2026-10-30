using Cal2AL.Core.Model;
using Cal2AL.Parser.Code;

namespace Cal2AL.Converter;

/// <summary>How generated files are arranged in the target folder.</summary>
public enum OutputStructure
{
    /// <summary>Mirror the sub-folders of the source folder (Txt2Al behaviour).</summary>
    Mirror,

    /// <summary>One sub-folder per object type: Tables, Pages, Reports, …</summary>
    ByType,

    /// <summary>All files directly in the target folder.</summary>
    Flat,
}

/// <summary>Options that control conversion. Defaults favour correctness and completeness.</summary>
public sealed record ConversionOptions
{
    /// <summary>Restrict conversion to these object types (empty = all).</summary>
    public IReadOnlySet<CalObjectType> ObjectTypes { get; init; } = new HashSet<CalObjectType>();

    /// <summary>Restrict conversion to object IDs in these ranges (empty = all).</summary>
    public IReadOnlyList<(int From, int To)> IdRanges { get; init; } = [];

    /// <summary>First ID for generated tableextension/pageextension objects.</summary>
    public int ExtensionStartId { get; init; } = 70000000;

    /// <summary>Target AL runtime, e.g. 12.0. Affects scope attributes and some deprecation diagnostics.</summary>
    public Version TargetRuntime { get; init; } = new(12, 0);

    /// <summary>Code page used for files that are not UTF-8.</summary>
    public string FallbackEncoding { get; init; } = "850";

    /// <summary>Emit <c>[Scope('OnPrem')]</c> on public procedures without [External], as Txt2Al does.</summary>
    public bool EmitScopeAttributes { get; init; }

    /// <summary>Write a comment with the C/AL Version List, Date and Time above each object.</summary>
    public bool KeepObjectMetadata { get; init; } = true;

    /// <summary>Write translation (XLIFF) files for multi-language captions and text constants.</summary>
    public bool GenerateTranslations { get; init; } = true;

    /// <summary>Prefix for generated .NET type aliases.</summary>
    public string DotNetTypePrefix { get; init; } = string.Empty;

    /// <summary>File name pattern for objects. Tokens: {name} {type} {id}.</summary>
    public string ObjectFileNamePattern { get; init; } = "{name}.{type}";

    /// <summary>File name pattern for extension objects. Tokens: {name} {type} {id} {targetName} {targetId}.</summary>
    public string ExtensionFileNamePattern { get; init; } = "{name}.{type}";

    public OutputStructure Structure { get; init; } = OutputStructure.Mirror;

    /// <summary>Folder (relative to the output root) that receives extracted RDLC and Word layouts.</summary>
    public string LayoutFolder { get; init; } = "layouts";

    /// <summary>
    /// Re-parse every generated body and compare it token by token with the original; any difference beyond the
    /// intended rewrites is reported as an error. Disable only for speed on very large code bases.
    /// </summary>
    public bool VerifyCodePreservation { get; init; } = true;

    /// <summary>
    /// How day and month are ordered in C/AL date constants. C/SIDE writes them in the regional order of the machine
    /// that exported the objects (a French export contains <c>31129999D</c>), so the default detects it per file.
    /// </summary>
    public CalDateOrder CalDateOrder { get; init; } = CalDateOrder.Auto;

    /// <summary>Folder (relative to the output root) for the generated XLIFF translation files.</summary>
    public string TranslationFolder { get; init; } = "Translations";

    /// <summary>Base name of the XLIFF files: <c>{name}.{language}.xlf</c>.</summary>
    public string TranslationFileName { get; init; } = "Cal2AL";

    /// <summary>Folder (relative to the output root) for migration artifacts and the report.</summary>
    public string ArtifactFolder { get; init; } = ".cal2al";

    public bool ShouldConvert(CalObjectType type, int id)
    {
        if (ObjectTypes.Count > 0 && !ObjectTypes.Contains(type))
        {
            return false;
        }

        if (IdRanges.Count > 0 && !IdRanges.Any(r => id >= r.From && id <= r.To))
        {
            return false;
        }

        return true;
    }
}
