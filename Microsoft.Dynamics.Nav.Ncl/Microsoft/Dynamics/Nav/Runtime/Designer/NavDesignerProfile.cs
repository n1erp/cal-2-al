using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerProfile : NavDesignerObject
{
	private readonly PropertySyntax descriptionSyntax;

	private readonly PropertySyntax roleCenterPropertySyntax;

	private readonly List<string> pageCustomizationsList = new List<string>();

	private PropertySyntax pageCustomizationSyntax;

	private readonly SyntaxList<MemberSyntax> memberlistSyntaxList;

	private ProfileSyntax profileSyntax;

	public string Name { get; }

	internal override ObjectSyntax Syntax => profileSyntax;

	protected NavDesignerProfile(ProfileSyntax syntax)
		: base(syntax.Name.Identifier.ToString().UnquoteIdentifier())
	{
		profileSyntax = syntax;
		Name = syntax.Name.Identifier.ToString().UnquoteIdentifier();
		descriptionSyntax = GetProperty(PropertyKind.Description);
		roleCenterPropertySyntax = GetProperty(PropertyKind.RoleCenter);
		pageCustomizationSyntax = GetProperty(PropertyKind.Customizations);
		CommaSeparatedPropertyValueSyntax obj = pageCustomizationSyntax?.Value as CommaSeparatedPropertyValueSyntax;
		if (obj != null)
		{
			obj.Values.ForEach((IdentifierNameSyntax v) =>
			{
				pageCustomizationsList.Add(v.Identifier.ValueText);
			});
		}
	}

	protected NavDesignerProfile(string name, string description, string roleCenterId)
		: base(name)
	{
		Name = name;
		descriptionSyntax = SyntaxFactory.Property(PropertyKind.Description, SyntaxFactory.StringPropertyValue(SyntaxFactory.StringLiteralValue(SyntaxFactory.Literal(description))));
		roleCenterPropertySyntax = SyntaxFactory.Property(PropertyKind.RoleCenter, SyntaxFactory.ObjectReferencePropertyValue(SyntaxFactory.ObjectNameOrId(SyntaxFactory.IdentifierName(roleCenterId))));
		profileSyntax = CreateModifiedSyntax();
	}

	public static NavDesignerProfile Create(string name, string description, string rolecenterId)
	{
		return new NavDesignerProfile(name, description, rolecenterId);
	}

	public static NavDesignerProfile Create(ProfileSyntax syntax)
	{
		if (syntax == null)
		{
			return null;
		}
		return new NavDesignerProfile(syntax);
	}

	public static IEnumerable<NavDesignerProfile> Create(string objectSource)
	{
		SyntaxTree syntaxTree = SyntaxTree.ParseObjectText(objectSource);
		IEnumerable<ProfileSyntax> source = syntaxTree.GetRoot().ChildNodes().OfType<ProfileSyntax>();
		return source.Select(Create);
	}

	public void AddPageCustomization(string pageCustomizationId)
	{
		pageCustomizationsList.Add(pageCustomizationId);
		profileSyntax = CreateModifiedSyntax();
	}

	public void RemovePageCustomization(string pageCustomizationId)
	{
		pageCustomizationsList.Remove(pageCustomizationId);
		profileSyntax = CreateModifiedSyntax();
	}

	private ProfileSyntax CreateModifiedSyntax()
	{
		pageCustomizationSyntax = SyntaxFactory.Property(PropertyKind.Customizations, SyntaxFactory.CommaSeparatedPropertyValue().AddValues(pageCustomizationsList.Select((string p) => SyntaxFactory.IdentifierName(p)).ToArray()));
		SyntaxList<PropertySyntaxOrEmpty> properties = default(SyntaxList<PropertySyntaxOrEmpty>).Add(descriptionSyntax).Add(roleCenterPropertySyntax).Add(pageCustomizationSyntax);
		PropertyListSyntax propertyList = SyntaxFactory.PropertyList().WithProperties(properties);
		return SyntaxFactory.Profile(SyntaxFactory.IdentifierName(Name), propertyList, memberlistSyntaxList);
	}

	protected PropertySyntax GetProperty(PropertyKind propertyKind)
	{
		return profileSyntax.GetProperty(propertyKind.ToString());
	}
}
