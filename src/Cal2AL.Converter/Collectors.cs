using System.Globalization;
using System.Text.RegularExpressions;
using Cal2AL.Generator;

namespace Cal2AL.Converter;

/// <summary>One translatable text: its source (ENU) value and its translations.</summary>
public sealed record TranslationUnit(
    string ObjectType,
    string ObjectName,
    string Path,
    string Source,
    string? Comment,
    IReadOnlyDictionary<string, string> Translations);

/// <summary>Collects translatable texts from all objects, for XLIFF generation. Thread-safe.</summary>
public sealed class TranslationCollector
{
    private readonly List<TranslationUnit> units = [];
    private readonly Lock gate = new();

    public void Add(TranslationUnit unit)
    {
        lock (gate)
        {
            units.Add(unit);
        }
    }

    public IReadOnlyList<TranslationUnit> Units
    {
        get
        {
            lock (gate)
            {
                return units.ToArray();
            }
        }
    }

    /// <summary>All target languages (culture names) that have at least one translation.</summary>
    public IReadOnlyList<string> Languages => Units.SelectMany(u => u.Translations.Keys).Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase).ToArray();
}

/// <summary>A .NET type used by DotNet variables, with the alias generated for it.</summary>
public sealed record DotNetTypeDeclaration(string AssemblyName, string? Version, string? Culture, string? PublicKeyToken, string TypeName, string Alias);

/// <summary>
/// Collects .NET types referenced by DotNet variables and assigns each a unique AL alias, so that a
/// <c>dotnet { assembly(...) { type(...) } }</c> package can be generated. Thread-safe.
/// </summary>
public sealed partial class DotNetCollector(string aliasPrefix = "")
{
    private readonly Dictionary<string, DotNetTypeDeclaration> byQualifiedName = new(StringComparer.Ordinal);
    private readonly HashSet<string> aliases = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock gate = new();

    /// <summary>Public key tokens of .NET Framework assemblies (ECMA and Microsoft).</summary>
    private static readonly HashSet<string> FrameworkTokens = new(StringComparer.OrdinalIgnoreCase) { "b77a5c561934e089", "b03f5f7f11d50a3a" };

    // 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'.System.String
    [GeneratedRegex(@"^'(?<assembly>[^']*)'\.(?<type>.+)$")]
    private static partial Regex QualifiedNameRegex();

    public bool HasTypes
    {
        get
        {
            lock (gate)
            {
                return byQualifiedName.Count > 0;
            }
        }
    }

    public IReadOnlyList<DotNetTypeDeclaration> Types
    {
        get
        {
            lock (gate)
            {
                return byQualifiedName.Values.Distinct().ToArray(); // unified framework types share one declaration
            }
        }
    }

    /// <summary>Registers a type and returns its alias (quoted if needed), or null when the name cannot be parsed.</summary>
    public string? GetAlias(string assemblyQualifiedName)
    {
        var m = QualifiedNameRegex().Match(assemblyQualifiedName.Trim());
        if (!m.Success)
        {
            return null;
        }

        lock (gate)
        {
            if (byQualifiedName.TryGetValue(assemblyQualifiedName, out var existing))
            {
                return AlSyntax.Identifier(existing.Alias);
            }

            var parts = m.Groups["assembly"].Value.Split(',', StringSplitOptions.TrimEntries);
            string? Part(string key) => parts.Skip(1).Select(p => p.Split('=', 2)).FirstOrDefault(kv => kv.Length == 2 && kv[0].Equals(key, StringComparison.OrdinalIgnoreCase))?[1];

            var typeName = m.Groups["type"].Value;
            var version = Part("Version");
            var token = Part("PublicKeyToken");

            // .NET Framework assemblies are unified by the runtime (mscorlib 2.0 and 4.0 load the same 4.0 assembly), so
            // one declaration per type is enough; NAV 2009–2013 objects often still reference the 2.0 versions.
            if (token is not null && FrameworkTokens.Contains(token) && version is not null
                && (version.StartsWith("2.0.", StringComparison.Ordinal) || version.StartsWith("4.0.", StringComparison.Ordinal)))
            {
                var unified = byQualifiedName.Values.FirstOrDefault(d =>
                    d.TypeName == typeName && string.Equals(d.AssemblyName, parts[0], StringComparison.OrdinalIgnoreCase)
                    && string.Equals(d.PublicKeyToken, token, StringComparison.OrdinalIgnoreCase));
                if (unified is not null)
                {
                    byQualifiedName[assemblyQualifiedName] = unified;
                    return AlSyntax.Identifier(unified.Alias);
                }

                if (version.StartsWith("2.0.", StringComparison.Ordinal))
                {
                    version = "4.0.0.0";
                }
            }
            var shortName = typeName.Split('.', '+').Last();
            var tick = shortName.IndexOf('`');
            if (tick > 0)
            {
                shortName = shortName[..tick];
            }

            var baseAlias = aliasPrefix + AlSyntax.ToIdentifierText(shortName);
            var alias = baseAlias;
            var n = 2;
            while (!aliases.Add(alias))
            {
                alias = baseAlias + n.ToString(CultureInfo.InvariantCulture);
                n++;
            }

            var decl = new DotNetTypeDeclaration(parts[0], version, Part("Culture"), token, typeName, alias);
            byQualifiedName[assemblyQualifiedName] = decl;
            return AlSyntax.Identifier(alias);
        }
    }
}

/// <summary>An enum generated from an extensible C/AL option field.</summary>
public sealed record EnumDeclaration(int Id, string Name, bool Extensible, IReadOnlyList<string> Members, IReadOnlyList<string?> Captions);

/// <summary>Collects enums generated from extensible option fields. Thread-safe.</summary>
public sealed class EnumCollector
{
    private readonly Dictionary<int, EnumDeclaration> enums = [];
    private readonly Lock gate = new();

    /// <summary>Adds an enum; returns an error message when an enum with the same ID or name but different members exists.</summary>
    public string? Add(EnumDeclaration declaration)
    {
        lock (gate)
        {
            if (enums.TryGetValue(declaration.Id, out var existing))
            {
                return existing.Members.SequenceEqual(declaration.Members, StringComparer.OrdinalIgnoreCase)
                    && string.Equals(existing.Name, declaration.Name, StringComparison.OrdinalIgnoreCase)
                    ? null
                    : $"Enum {declaration.Id} '{declaration.Name}' is defined differently in another field ('{existing.Name}').";
            }

            if (enums.Values.Any(e => string.Equals(e.Name, declaration.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return $"Enum name '{declaration.Name}' is used with a different ID by another field.";
            }

            enums[declaration.Id] = declaration;
            return null;
        }
    }

    public IReadOnlyList<EnumDeclaration> Enums
    {
        get
        {
            lock (gate)
            {
                return enums.Values.OrderBy(e => e.Id).ToArray();
            }
        }
    }
}

/// <summary>Assigns IDs to generated extension objects in a deterministic order.</summary>
public sealed class ExtensionIdAllocator(int start)
{
    private int next = start;
    private readonly Lock gate = new();

    public int Next()
    {
        lock (gate)
        {
            return next++;
        }
    }
}
