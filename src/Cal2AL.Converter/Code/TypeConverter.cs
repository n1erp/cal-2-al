using Cal2AL.Converter.Values;
using Cal2AL.Core.Model;
using Cal2AL.Diagnostics;
using Cal2AL.Generator;
using Cal2AL.Generator.Model;

namespace Cal2AL.Converter.Code;

/// <summary>Converts C/AL variable, parameter and return-value declarations to AL.</summary>
public sealed class TypeConverter(ObjectContext context)
{
    /// <summary>
    /// Converts a declaration. <paramref name="scopePath"/> is used for translation entries of TextConst variables
    /// (e.g. <c>Codeunit X - NamedType Text000</c>).
    /// </summary>
    public AlVariable ConvertVariable(CalVariable variable, bool isParameter, string scopePath)
    {
        var line = variable.Position.Line;
        var name = DeclareName(variable.Name);
        var type = ConvertType(variable, scopePath, line);

        var result = new AlVariable { Name = name, Type = type, IsVar = variable.IsVar };
        if (variable.InDataSet && !isParameter)
        {
            result.Attributes.Add("[InDataSet]");
        }

        if (!string.IsNullOrEmpty(variable.SecurityFiltering))
        {
            result.Attributes.Add($"[SecurityFiltering(SecurityFilter::{variable.SecurityFiltering})]");
        }

        if (!isParameter)
        {
            if (variable.RunOnClient)
            {
                result.Attributes.Add("[RunOnClient]");
            }

            if (variable.WithEvents)
            {
                result.Attributes.Add("[WithEvents]");
            }

            if (variable.SuppressDispose)
            {
                result.Attributes.Add("[SuppressDispose]");
            }

            if (variable.NonDebuggable)
            {
                result.Attributes.Add("[NonDebuggable]");
            }
        }

        return result;
    }

    /// <summary>
    /// Quotes a declared name if needed. Names that only need quotes because they are AL keywords are also recorded,
    /// so every use of them in code is quoted the same way.
    /// </summary>
    public string DeclareName(string name)
    {
        if (name.Length == 0)
        {
            return string.Empty;
        }

        var written = AlSyntax.Identifier(name);
        if (AlSyntax.IsPlainIdentifier(name) && written != name)
        {
            if (context.IdentifiersToQuote.Add(name))
            {
                context.Report(DiagnosticDescriptors.ReservedIdentifierQuoted,
                    $"'{name}' is a keyword in AL; it is written as {written} in its declaration and in all code that uses it.",
                    construct: name);
            }
        }

        return written;
    }

    public string ConvertType(CalVariable variable, string scopePath, int? line)
    {
        var baseType = ConvertBaseType(variable, scopePath, line);
        if (variable.IsTemporary)
        {
            baseType += " temporary";
        }

        if (variable.Dimensions is not null)
        {
            baseType = $"array[{variable.Dimensions.Replace(" ", string.Empty, StringComparison.Ordinal).Replace(",", ", ", StringComparison.Ordinal)}] of {baseType}";
        }

        return baseType;
    }

    private string ConvertBaseType(CalVariable variable, string scopePath, int? line)
    {
        var type = variable.Type;
        var upper = type.Name.ToUpperInvariant();
        switch (upper)
        {
            case "RECORD":
            case "CODEUNIT":
            case "PAGE":
            case "REPORT":
            case "XMLPORT":
            case "QUERY":
            case "TESTPAGE":
            case "TESTREQUESTPAGE":
            {
                var objectType = upper switch
                {
                    "RECORD" => CalObjectType.Table,
                    "TESTPAGE" => CalObjectType.Page,
                    "TESTREQUESTPAGE" => CalObjectType.Report,
                    _ => CalObjectTypeExtensions.ParseObjectType(upper),
                };
                var alTypeName = upper switch
                {
                    "RECORD" => "Record",
                    "XMLPORT" => "XmlPort",
                    "TESTPAGE" => "TestPage",
                    "TESTREQUESTPAGE" => "TestRequestPage",
                    _ => objectType.ToString(),
                };
                return $"{alTypeName} {ObjectReferences.Resolve(context, type.Subtype ?? "0", objectType, line)}";
            }

            case "FORM":
            case "DATAPORT":
                context.Report(DiagnosticDescriptors.UnsupportedDataType,
                    $"Variable '{variable.Name}' is a {type.Name}; forms and dataports do not exist in AL.",
                    line, construct: variable.ToString(),
                    resolution: type.Name.Equals("Form", StringComparison.OrdinalIgnoreCase)
                        ? "Change the variable to the Page that replaced the form."
                        : "Change the variable to the XMLport that replaced the dataport.");
                return $"{type.Name} {type.Subtype}";

            case "OPTION":
                return string.IsNullOrEmpty(type.OptionString)
                    ? "Option"
                    : "Option " + string.Join(", ", type.OptionString.Split(',').Select(m => m.Length == 0 ? string.Empty : AlSyntax.Identifier(m)));

            case "TEXTCONST":
                return "Label " + (LabelConverter.Convert(context, type.TextConstValue ?? string.Empty, $"{scopePath} - NamedType {variable.Name}", line) ?? "''");

            case "DOTNET":
            {
                context.Report(DiagnosticDescriptors.DotNetVariable,
                    $"Variable '{variable.Name}' uses the .NET type {type.ExternalTypeName}.",
                    line, construct: variable.ToString());
                var alias = type.ExternalTypeName is null ? null : context.Run.DotNet.GetAlias(type.ExternalTypeName);
                if (alias is null)
                {
                    context.Report(DiagnosticDescriptors.PropertyValueNotParsed,
                        $"The .NET type name '{type.ExternalTypeName}' could not be parsed; no alias was generated.",
                        line, construct: variable.ToString(), severity: DiagnosticSeverity.Error);
                    return $"DotNet {AlSyntax.QuotedIdentifier(type.ExternalTypeName ?? "Unknown")}";
                }

                if (variable.WithEvents)
                {
                    context.WithEventsVariables.Add(variable.Name);
                }

                return "DotNet " + alias;
            }

            case "AUTOMATION":
            case "OCX":
                context.AutomationVariables.Add(variable.Name);
                context.Report(DiagnosticDescriptors.AutomationNotSupported,
                    $"Variable '{variable.Name}' is an {type.Name} (COM) object, which AL cannot use.",
                    line, construct: $"{type.Name} {type.ExternalTypeName}", original: variable.ToString(),
                    reason: "COM automation and OCX controls were removed when the Windows client was retired.");
                return $"{type.Name} {AlSyntax.QuotedIdentifier(type.ExternalTypeName ?? string.Empty)}";

            case "FILE":
                context.Report(DiagnosticDescriptors.OnPremOnly,
                    $"Variable '{variable.Name}' is a File; the File data type is only available on-premises.",
                    line, construct: "File");
                return "File";

            case "BINARY":
                context.Report(DiagnosticDescriptors.UnsupportedDataType,
                    $"Variable '{variable.Name}' is Binary, which does not exist in AL.",
                    line, construct: variable.ToString(), resolution: "Use Text, Code or a BLOB-backed stream instead.");
                return type.Length is null ? "Binary" : $"Binary[{type.Length}]";

            case "GUID":
                return "Guid";
            case "BLOB":
                return "Blob";
            case "BIGTEXT":
                return "BigText";
            case "RECORDREF":
                return "RecordRef";
            case "FIELDREF":
                return "FieldRef";
            case "KEYREF":
                return "KeyRef";
            case "RECORDID":
                return "RecordId";
            case "INSTREAM":
                return "InStream";
            case "OUTSTREAM":
                return "OutStream";
            case "DATEFORMULA":
                return "DateFormula";
            case "DATETIME":
                return "DateTime";
            case "BIGINTEGER":
                return "BigInteger";
        }

        if (type.Length is int length)
        {
            return $"{Capitalize(type.Name)}[{length}]";
        }

        return Capitalize(type.Name);
    }

    private static string Capitalize(string name) =>
        name.Length > 0 && char.IsLower(name[0]) ? char.ToUpperInvariant(name[0]) + name[1..] : name;
}
