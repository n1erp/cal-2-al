using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerPageField : NavDesignerPageControl
{
	internal string FieldExpression { get; set; }

	internal new PageFieldSyntax Syntax
	{
		get
		{
			return base.Syntax as PageFieldSyntax;
		}
		set
		{
			base.Syntax = value;
		}
	}

	internal NavDesignerPageField(string fieldName, string fieldExpression)
		: base(fieldName)
	{
		FieldExpression = fieldExpression;
		Syntax = SyntaxFactory.PageField(fieldName, SyntaxFactory.IdentifierName(fieldExpression)).WithPropertyList(NavDesignerPageControl.GetDefaultPropertyList());
	}

	internal NavDesignerPageField(PageFieldSyntax fieldSyntax)
		: base(fieldSyntax)
	{
		FieldExpression = fieldSyntax?.Expression.ToString();
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((NavDesignerPageField)obj);
	}

	public bool Equals(NavDesignerPageField other)
	{
		if (base.Equals(other))
		{
			return string.Equals(FieldExpression, other.FieldExpression);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCodeHelper.CombineHashCodes(ControlName?.GetHashCode() ?? 0, FieldExpression?.GetHashCode() ?? 0);
	}

	public static bool operator ==(NavDesignerPageField left, NavDesignerPageField right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(NavDesignerPageField left, NavDesignerPageField right)
	{
		return !object.Equals(left, right);
	}
}
