using System.Globalization;
using System.Security;
using System.Text;
using Cal2AL.Generator;
using Cal2AL.Generator.Model;

namespace Cal2AL.Converter.Engine;

/// <summary>Writes the run-level files: the .NET package, generated enums and XLIFF translations.</summary>
public static class PackageWriters
{
    /// <summary>
    /// A <c>dotnet</c> package declaring every .NET type used by DotNet variables, grouped by assembly.
    /// </summary>
    public static string DotNetPackage(IEnumerable<DotNetTypeDeclaration> types, string newLine = "\r\n")
    {
        var sb = new StringBuilder();
        void Line(int level, string text) => sb.Append(new string(' ', level * 4)).Append(text).Append(newLine);

        Line(0, "dotnet");
        Line(0, "{");
        var first = true;
        foreach (var assembly in types.GroupBy(t => (t.AssemblyName, t.Version, t.Culture, t.PublicKeyToken))
                     .OrderBy(g => g.Key.AssemblyName, StringComparer.OrdinalIgnoreCase))
        {
            if (!first)
            {
                sb.Append(newLine);
            }

            first = false;
            Line(1, $"assembly({AlSyntax.String(assembly.Key.AssemblyName)})");
            Line(1, "{");
            if (assembly.Key.Version is { Length: > 0 } version)
            {
                Line(2, $"Version = {AlSyntax.String(version)};");
            }

            if (assembly.Key.Culture is { Length: > 0 } culture)
            {
                Line(2, $"Culture = {AlSyntax.String(culture)};");
            }

            if (assembly.Key.PublicKeyToken is { Length: > 0 } token)
            {
                Line(2, $"PublicKeyToken = {AlSyntax.String(token)};");
            }

            sb.Append(newLine);
            foreach (var type in assembly.OrderBy(t => t.TypeName, StringComparer.Ordinal))
            {
                Line(2, $"type({AlSyntax.String(type.TypeName)}; {AlSyntax.Identifier(type.Alias)})");
                Line(2, "{");
                Line(2, "}");
            }

            Line(1, "}");
        }

        Line(0, "}");
        return sb.ToString();
    }

    /// <summary>An <c>enum</c> object for an extensible option field.</summary>
    public static AlObject EnumObject(EnumDeclaration declaration)
    {
        var obj = new AlObject { Kind = "enum", Id = declaration.Id, Name = AlSyntax.Identifier(declaration.Name) };
        if (declaration.Extensible)
        {
            obj.Properties.Add(new AlProperty("Extensible", "true"));
        }

        for (var i = 0; i < declaration.Members.Count; i++)
        {
            var member = declaration.Members[i].Trim();
            var value = new AlSection { Header = $"value({i}; {(member.Length == 0 ? "\" \"" : AlSyntax.Identifier(member))})" };
            if (i < declaration.Captions.Count && declaration.Captions[i] is { } caption)
            {
                value.Properties.Add(new AlProperty("Caption", AlSyntax.String(caption)));
            }

            obj.Sections.Add(value);
        }

        return obj;
    }

    /// <summary>
    /// An XLIFF 1.2 file with the translations of one language. Each unit carries its C/AL path in a developer
    /// note, so it can be matched to the units of the compiler-generated <c>.g.xlf</c>.
    /// </summary>
    public static string Xliff(IEnumerable<TranslationUnit> units, string targetLanguage, string original)
    {
        var sb = new StringBuilder();
        sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n");
        sb.Append("<xliff version=\"1.2\" xmlns=\"urn:oasis:names:tc:xliff:document:1.2\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"urn:oasis:names:tc:xliff:document:1.2 xliff-core-1.2-transitional.xsd\">\r\n");
        sb.Append("  <file datatype=\"xml\" source-language=\"en-US\" target-language=\"").Append(Escape(targetLanguage))
          .Append("\" original=\"").Append(Escape(original)).Append("\">\r\n");
        sb.Append("    <body>\r\n");
        sb.Append("      <group id=\"body\">\r\n");
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var unit in units)
        {
            if (!unit.Translations.TryGetValue(targetLanguage, out var target))
            {
                continue;
            }

            var id = UniqueId(ids, unit.Path);
            sb.Append("        <trans-unit id=\"").Append(Escape(id)).Append("\" size-unit=\"char\" translate=\"yes\" xml:space=\"preserve\">\r\n");
            sb.Append("          <source>").Append(Escape(unit.Source)).Append("</source>\r\n");
            sb.Append("          <target>").Append(Escape(target)).Append("</target>\r\n");
            if (unit.Comment is { Length: > 0 } comment)
            {
                sb.Append("          <note from=\"Developer\" annotates=\"general\" priority=\"2\">").Append(Escape(comment)).Append("</note>\r\n");
            }

            sb.Append("          <note from=\"Xliff Generator\" annotates=\"general\" priority=\"3\">")
              .Append(Escape(unit.Path)).Append("</note>\r\n");
            sb.Append("        </trans-unit>\r\n");
        }

        sb.Append("      </group>\r\n    </body>\r\n  </file>\r\n</xliff>\r\n");
        return sb.ToString();
    }

    private static string UniqueId(HashSet<string> ids, string id)
    {
        var candidate = id;
        for (var n = 2; !ids.Add(candidate); n++)
        {
            candidate = string.Create(CultureInfo.InvariantCulture, $"{id} #{n}");
        }

        return candidate;
    }

    private static string Escape(string value) => SecurityElement.Escape(value) ?? string.Empty;
}
