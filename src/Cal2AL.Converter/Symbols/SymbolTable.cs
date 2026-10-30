using System.Text.RegularExpressions;
using Cal2AL.Core.Model;

namespace Cal2AL.Converter.Symbols;

/// <summary>A table field known from the converted object set.</summary>
public sealed record FieldSymbol(int No, string Name, string DataType, string? OptionString);

/// <summary>
/// Everything Cal2AL knows about the objects being converted: names by ID (to turn <c>Record 18</c> into
/// <c>Record Customer</c>) and table fields (to recognise Boolean filters). Built once from all parsed objects
/// before any object is converted.
/// </summary>
public sealed partial class SymbolTable
{
    private readonly Dictionary<(CalObjectType, int), string> namesById = [];
    private readonly Dictionary<(CalObjectType, string), int> idsByName = [];
    private readonly Dictionary<int, List<FieldSymbol>> tableFields = [];
    private readonly Dictionary<int, string> pageSourceTables = [];

    public static SymbolTable Empty { get; } = new();

    public int Count => namesById.Count;

    public void Add(CalObject obj)
    {
        if (obj.Type == CalObjectType.Modification || obj.Type == CalObjectType.Unknown)
        {
            return;
        }

        namesById[(obj.Type, obj.Id)] = obj.Name;
        idsByName[(obj.Type, obj.Name.ToUpperInvariant())] = obj.Id;

        if (obj.Type == CalObjectType.Table)
        {
            var fields = new List<FieldSymbol>();
            foreach (var item in obj.GetItems("FIELDS"))
            {
                if (int.TryParse(item.GetPositional(0).Trim(), out var no))
                {
                    fields.Add(new FieldSymbol(no, item.GetPositional(2).Trim(), item.GetPositional(3).Trim(), item.Properties["OptionString"]));
                }
            }

            tableFields[obj.Id] = fields;
        }

        if (obj.Type == CalObjectType.Page && obj.Properties["SourceTable"] is { } sourceTable)
        {
            pageSourceTables[obj.Id] = sourceTable.Trim();
        }
    }

    /// <summary>The source table of a page in the converted set, by page ID or name.</summary>
    public int? GetPageSourceTableId(string pageReference)
    {
        var trimmed = pageReference.Trim();
        var m = PageNumberRegex().Match(trimmed);
        int? pageId = m.Success ? int.Parse(m.Groups[1].Value) : int.TryParse(trimmed, out var n) ? n : GetId(CalObjectType.Page, trimmed);
        return pageId is int id && pageSourceTables.TryGetValue(id, out var table) ? ResolveTableId(table) : null;
    }

    [GeneratedRegex(@"^Page\s*(\d+)$", RegexOptions.IgnoreCase)]
    private static partial Regex PageNumberRegex();

    public static SymbolTable Build(IEnumerable<CalObject> objects)
    {
        var table = new SymbolTable();
        foreach (var obj in objects)
        {
            table.Add(obj);
        }

        return table;
    }

    public string? GetName(CalObjectType type, int id) => namesById.GetValueOrDefault((Normalize(type), id));

    public int? GetId(CalObjectType type, string name) =>
        idsByName.TryGetValue((Normalize(type), Unquote(name).ToUpperInvariant()), out var id) ? id : null;

    public IReadOnlyList<FieldSymbol>? GetFields(int tableId) => tableFields.GetValueOrDefault(tableId);

    /// <summary>Finds a table's ID from a reference that may be a number, <c>Table18</c>, a name or a quoted name.</summary>
    public int? ResolveTableId(string reference)
    {
        var trimmed = reference.Trim();
        if (int.TryParse(trimmed, out var id))
        {
            return id;
        }

        var m = TableNumberRegex().Match(trimmed);
        if (m.Success)
        {
            return int.Parse(m.Groups[1].Value);
        }

        return GetId(CalObjectType.Table, trimmed);
    }

    public FieldSymbol? FindField(int tableId, string fieldName)
    {
        var fields = GetFields(tableId);
        var name = Unquote(fieldName);
        return fields?.FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    private static CalObjectType Normalize(CalObjectType type) => type;

    private static string Unquote(string value)
    {
        var t = value.Trim();
        return t.Length >= 2 && t[0] == '"' && t[^1] == '"' ? t[1..^1].Replace("\"\"", "\"") : t;
    }

    [GeneratedRegex(@"^Table\s*(\d+)$", RegexOptions.IgnoreCase)]
    private static partial Regex TableNumberRegex();
}
