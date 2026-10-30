using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

public class NavAppMediaIdListing
{
	[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "The reference is immutable")]
	public static readonly NavAppMediaIdListing Empty = new NavAppMediaIdListing(null, new List<NavAppMediaSetEntry>());

	public NavAppMediaEntry Logo { get; }

	public IEnumerable<NavAppMediaSetEntry> MediaSets { get; }

	public NavAppMediaIdListing(NavAppMediaEntry logo, IEnumerable<NavAppMediaSetEntry> mediaSets)
	{
		Logo = logo;
		MediaSets = mediaSets;
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static NavAppMediaIdListing FromXml(string document)
	{
		if (string.IsNullOrWhiteSpace(document))
		{
			return Empty;
		}
		XNamespace nameSpace = "http://schemas.microsoft.com/navx/2016/mediaidlisting";
		XElement xElement = XElement.Parse(document);
		XAttribute xAttribute = xElement.Attribute(NavAppMediaIdListingXmlPart.LogoId.ToString());
		XAttribute xAttribute2 = xElement.Attribute(NavAppMediaIdListingXmlPart.LogoFileName.ToString());
		NavAppMediaEntry logo = new NavAppMediaEntry
		{
			Id = ((xAttribute == null || string.IsNullOrWhiteSpace(xAttribute.Value)) ? Guid.Empty : new Guid(xAttribute.Value)),
			FileName = ((xAttribute2 == null || string.IsNullOrWhiteSpace(xAttribute2.Value)) ? null : xAttribute2.Value),
			IsLogo = true
		};
		IEnumerable<NavAppMediaSetEntry> mediaSets = null;
		XElement xElement2 = xElement.Element(nameSpace + NavAppMediaIdListingXmlPart.MediaSetIds.ToString());
		if (xElement2 != null)
		{
			mediaSets = from x in xElement2.Elements(nameSpace + NavAppMediaIdListingXmlPart.MediaSet.ToString()).Select((XElement element) =>
				{
					XAttribute xAttribute3 = element.Attribute(NavAppMediaIdListingXmlPart.MediaSetName.ToString());
					XAttribute xAttribute4 = element.Attribute(NavAppMediaIdListingXmlPart.MediaSetId.ToString());
					IEnumerable<NavAppMediaEntry> enumerable = (from x in element.Elements(nameSpace + NavAppMediaIdListingXmlPart.Media.ToString()).Select((XElement e) =>
						{
							XAttribute xAttribute5 = e.Attribute(NavAppMediaIdListingXmlPart.MediaFileName.ToString());
							XAttribute xAttribute6 = e.Attribute(NavAppMediaIdListingXmlPart.MediaId.ToString());
							Guid result2;
							return (xAttribute6 == null || xAttribute5 == null || string.IsNullOrWhiteSpace(xAttribute5.Value) || !Guid.TryParse(xAttribute6.Value, out result2)) ? null : new NavAppMediaEntry
							{
								FileName = xAttribute5.Value,
								Id = result2
							};
						})
						where x != null
						select x).ToList();
					Guid result;
					return (xAttribute3 == null || xAttribute4 == null || enumerable == null || string.IsNullOrWhiteSpace(xAttribute3.Value) || !enumerable.Any() || !Guid.TryParse(xAttribute4.Value, out result)) ? null : new NavAppMediaSetEntry
					{
						Name = xAttribute3.Value,
						SetId = result,
						Media = enumerable
					};
				})
				where x != null
				select x;
		}
		return new NavAppMediaIdListing(logo, mediaSets);
	}

	public string ToXml()
	{
		XNamespace nameSpace = "http://schemas.microsoft.com/navx/2016/mediaidlisting";
		return new XDocument(new XElement(nameSpace + NavAppMediaIdListingXmlPart.MediaIdListing.ToString(), new XAttribute(NavAppMediaIdListingXmlPart.LogoFileName.ToString(), (Logo == null) ? string.Empty : Logo.FileName), new XAttribute(NavAppMediaIdListingXmlPart.LogoId.ToString(), (Logo == null) ? string.Empty : Logo.Id.ToString()), new XElement(nameSpace + NavAppMediaIdListingXmlPart.MediaSetIds.ToString(), (MediaSets == null) ? null : MediaSets.Select((NavAppMediaSetEntry mediaset) => new XElement(nameSpace + NavAppMediaIdListingXmlPart.MediaSet.ToString(), new XAttribute(NavAppMediaIdListingXmlPart.MediaSetName.ToString(), mediaset.Name), new XAttribute(NavAppMediaIdListingXmlPart.MediaSetId.ToString(), mediaset.SetId.ToString()), (mediaset.Media == null) ? null : mediaset.Media.Select((NavAppMediaEntry media) => new XElement(nameSpace + NavAppMediaIdListingXmlPart.Media.ToString(), new XAttribute(NavAppMediaIdListingXmlPart.MediaFileName.ToString(), media.FileName), new XAttribute(NavAppMediaIdListingXmlPart.MediaId.ToString(), media.Id))).Cast<object>().ToArray())).Cast<object>().ToArray()))).ToString();
	}
}
