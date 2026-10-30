using System.Text;
using Cal2AL.Generator.Model;

namespace Cal2AL.Generator;

/// <summary>Formatting options for generated AL.</summary>
public sealed record AlWriterOptions
{
    public int IndentSize { get; init; } = 4;

    /// <summary>Line ending for output files. AL tooling accepts both; CRLF matches Windows/VS Code defaults.</summary>
    public string NewLine { get; init; } = "\r\n";
}

/// <summary>
/// Renders an <see cref="AlObject"/> to AL source text in the layout the AL formatter produces: four-space
/// indentation, braces on their own lines, a blank line between members.
/// </summary>
public sealed class AlWriter(AlWriterOptions? options = null)
{
    private readonly AlWriterOptions options = options ?? new AlWriterOptions();
    private readonly StringBuilder sb = new();
    private int indent;
    private bool pendingBlank;

    public string Render(AlObject obj)
    {
        sb.Clear();
        indent = 0;
        pendingBlank = false;

        foreach (var comment in obj.HeaderComments)
        {
            Line(comment);
        }

        var header = $"{obj.Kind} {obj.Id} {obj.Name}";
        if (obj.Extends is not null)
        {
            header += $" extends {obj.Extends}";
        }

        Line(header);
        Open();

        foreach (var doc in obj.Documentation)
        {
            Line(doc.Length == 0 ? "//" : "// " + doc);
        }

        if (obj.Documentation.Count > 0)
        {
            pendingBlank = true;
        }

        WriteProperties(obj.Properties);
        foreach (var section in obj.Sections)
        {
            WriteSection(section);
        }

        WriteVariables(obj.Variables);
        foreach (var trigger in obj.Triggers)
        {
            WriteMethod(trigger);
        }

        foreach (var procedure in obj.Procedures)
        {
            WriteMethod(procedure);
        }

        Close();
        return sb.ToString();
    }

    /// <summary>Renders a single method (used for migration artifacts and tests).</summary>
    public string RenderMethod(AlMethod method)
    {
        sb.Clear();
        indent = 0;
        pendingBlank = false;
        WriteMethod(method);
        return sb.ToString();
    }

    private void WriteProperties(List<AlProperty> properties)
    {
        if (properties.Count == 0)
        {
            return;
        }

        BlankIfPending();
        foreach (var p in properties)
        {
            WriteMultiline($"{p.Name} = ", p.Value, ";");
        }

        pendingBlank = true;
    }

    private void WriteSection(AlSection section)
    {
        BlankIfPending();
        foreach (var comment in section.LeadingComments)
        {
            Line(comment);
        }

        Line(section.Header);
        Open();
        WriteProperties(section.Properties);

        if (section.Lines.Count > 0)
        {
            BlankIfPending();
            foreach (var line in section.Lines)
            {
                Line(line);
            }

            pendingBlank = true;
        }

        foreach (var child in section.Children)
        {
            WriteSection(child);
        }

        foreach (var trigger in section.Triggers)
        {
            WriteMethod(trigger);
        }

        Close();
        pendingBlank = true;
    }

    private void WriteVariables(List<AlVariable> variables)
    {
        if (variables.Count == 0)
        {
            return;
        }

        BlankIfPending();
        Line("var");
        indent++;
        foreach (var v in variables)
        {
            WriteVariable(v);
        }

        indent--;
        pendingBlank = true;
    }

    private void WriteVariable(AlVariable v)
    {
        foreach (var attribute in v.Attributes)
        {
            Line(attribute);
        }

        WriteMultiline(string.Empty, v.Declaration, ";");
    }

    private void WriteMethod(AlMethod method)
    {
        BlankIfPending();
        foreach (var comment in method.LeadingComments)
        {
            Line(comment);
        }

        foreach (var attribute in method.Attributes)
        {
            Line(attribute);
        }

        var keyword = method.Kind == AlMethodKind.Trigger ? "trigger" : "procedure";
        var access = method.Access.Length > 0 ? method.Access + " " : string.Empty;
        var parameters = method.ParameterText ?? string.Join("; ", method.Parameters.Select(p => p.Declaration));
        var signature = $"{access}{keyword} {method.Name}({parameters})";
        if (method.ReturnType is not null)
        {
            signature += method.ReturnName is null ? $": {method.ReturnType}" : $" {method.ReturnName}: {method.ReturnType}";
        }

        Line(signature);
        if (method.Variables.Count > 0)
        {
            Line("var");
            indent++;
            foreach (var v in method.Variables)
            {
                WriteVariable(v);
            }

            indent--;
        }

        Line("begin");
        indent++;
        foreach (var bodyLine in method.Body)
        {
            if (bodyLine.Length == 0)
            {
                sb.Append(options.NewLine);
            }
            else
            {
                Line(bodyLine);
            }
        }

        indent--;
        Line("end;");
        pendingBlank = true;
    }

    /// <summary>Writes <paramref name="prefix"/> + value + <paramref name="suffix"/>, aligning continuation lines under the value.</summary>
    private void WriteMultiline(string prefix, string value, string suffix)
    {
        var lines = value.Split('\n');
        if (lines.Length == 1)
        {
            Line(prefix + value + suffix);
            return;
        }

        var align = new string(' ', prefix.Length);
        for (var i = 0; i < lines.Length; i++)
        {
            var text = (i == 0 ? prefix : align) + lines[i] + (i == lines.Length - 1 ? suffix : string.Empty);
            Line(text);
        }
    }

    private void BlankIfPending()
    {
        if (pendingBlank)
        {
            sb.Append(options.NewLine);
            pendingBlank = false;
        }
    }

    private void Open()
    {
        Line("{");
        indent++;
        pendingBlank = false;
    }

    private void Close()
    {
        indent--;
        Line("}");
    }

    private void Line(string text)
    {
        sb.Append(' ', indent * options.IndentSize).Append(text).Append(options.NewLine);
    }
}
