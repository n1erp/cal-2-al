using System.Collections;

namespace Cal2AL.Diagnostics;

/// <summary>A thread-safe collector of diagnostics.</summary>
public sealed class DiagnosticBag : IEnumerable<Diagnostic>
{
    private readonly List<Diagnostic> items = [];
    private readonly Lock gate = new();

    public void Add(Diagnostic diagnostic)
    {
        ArgumentNullException.ThrowIfNull(diagnostic);
        lock (gate)
        {
            items.Add(diagnostic);
        }
    }

    public void AddRange(IEnumerable<Diagnostic> diagnostics)
    {
        foreach (var d in diagnostics)
        {
            Add(d);
        }
    }

    public int Count
    {
        get
        {
            lock (gate)
            {
                return items.Count;
            }
        }
    }

    public bool HasErrors
    {
        get
        {
            lock (gate)
            {
                return items.Exists(d => d.Severity == DiagnosticSeverity.Error);
            }
        }
    }

    public IReadOnlyList<Diagnostic> ToList()
    {
        lock (gate)
        {
            return items.ToArray();
        }
    }

    public IEnumerator<Diagnostic> GetEnumerator() => ToList().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
