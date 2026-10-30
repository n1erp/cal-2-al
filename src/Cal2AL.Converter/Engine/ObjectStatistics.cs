using Cal2AL.Core.Model;
using Cal2AL.Diagnostics;

namespace Cal2AL.Converter.Engine;

/// <summary>Counts the parts of a parsed C/AL object, for per-object statistics.</summary>
public static class ObjectStatistics
{
    public static ObjectMetrics Compute(CalObject obj)
    {
        var counter = new Counter();
        counter.Visit(obj);
        foreach (var procedure in obj.Code.Procedures)
        {
            counter.Procedure(procedure);
        }

        return new ObjectMetrics
        {
            Fields = counter.Fields,
            Keys = counter.Keys,
            Controls = counter.Controls,
            Actions = counter.Actions,
            DataItems = counter.DataItems,
            Procedures = counter.Procedures,
            Triggers = counter.Triggers,
            Variables = obj.Code.Variables.Count + counter.Variables,
            CodeLines = counter.CodeLines,
        };
    }

    private sealed class Counter
    {
        public int Fields;
        public int Keys;
        public int Controls;
        public int Actions;
        public int DataItems;
        public int Procedures;
        public int Triggers;
        public int Variables;
        public int CodeLines;

        public void Visit(CalNode node)
        {
            foreach (var trigger in node.Triggers)
            {
                // Delta OriginalCode is the base object's code, not code of this object.
                if (trigger.Name.Equals("OriginalCode", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                Triggers++;
                CodeLines += Lines(trigger.Body);
            }

            foreach (var section in node.Sections)
            {
                Variables += section.Variables.Count;
                foreach (var procedure in section.Procedures)
                {
                    Procedure(procedure);
                }

                foreach (var item in section.Items)
                {
                    Count(section.Name, item);
                    Visit(item);
                }
            }
        }

        public void Procedure(CalProcedure procedure)
        {
            Procedures++;
            CodeLines += Lines(procedure.Body);
        }

        private void Count(string sectionName, CalElement item)
        {
            switch (sectionName.ToUpperInvariant())
            {
                case "FIELDS":
                case "FIELDCOLLECTION":
                    Fields++;
                    break;
                case "KEYS":
                case "KEYCOLLECTION":
                    Keys++;
                    break;
                case "CONTROLS":
                case "CONTROLCOLLECTION":
                    Controls++;
                    break;
                case "ACTIONS":
                case "ACTIONLIST":
                case "ACTIONCOLLECTION":
                    Actions++;
                    break;
                case "DATASET":
                case "ELEMENTS":
                    if (item.Positional.Any(p => p.Trim().Equals("DataItem", StringComparison.OrdinalIgnoreCase))
                        || item.Properties.Contains("SourceTable"))
                    {
                        DataItems++;
                    }

                    break;
            }
        }

        private static int Lines(CalCodeBody body) => body.Lines.Count(l => !string.IsNullOrWhiteSpace(l.Text));
    }
}
