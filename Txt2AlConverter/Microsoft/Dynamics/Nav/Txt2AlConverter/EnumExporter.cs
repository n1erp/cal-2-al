using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal class EnumExporter
{
	internal void Export(EnumTypeDefinition[] enumDefinitions, IAlWriter writer)
	{
		foreach (EnumTypeDefinition e in enumDefinitions)
		{
			writer.WriteSection($"enum {e.Id} {AlUtilities.QuoteIdentifierIfNeeded(e.Name)}", () =>
			{
				if (e.Properties != null)
				{
					PropertyDefinition[] properties = e.Properties;
					foreach (PropertyDefinition propertyDefinition in properties)
					{
						writer.WriteProperty(propertyDefinition.Name, propertyDefinition.Value);
					}
				}
				EnumValueDefinition[] values = e.Values;
				foreach (EnumValueDefinition value in values)
				{
					writer.WriteSection($"value({value.Id};{AlUtilities.QuoteIdentifierIfNeeded(value.Name)})", () =>
					{
						if (value.Properties != null)
						{
							PropertyDefinition[] properties2 = value.Properties;
							foreach (PropertyDefinition propertyDefinition2 in properties2)
							{
								writer.WriteProperty(propertyDefinition2.Name, propertyDefinition2.Value);
							}
						}
					});
				}
			});
		}
	}
}
