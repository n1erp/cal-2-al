using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.Runtime.Designer.Images;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Image
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				ResourceManager resourceManager = new ResourceManager("Microsoft.Dynamics.Nav.Runtime.Designer.Images.Image", typeof(Image).Assembly);
				resourceMan = resourceManager;
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static Bitmap BrickIcon_DesignScope_Everybody
	{
		get
		{
			object obj = ResourceManager.GetObject("BrickIcon_DesignScope_Everybody", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap BrickIcon_DesignScope_MyProfile
	{
		get
		{
			object obj = ResourceManager.GetObject("BrickIcon_DesignScope_MyProfile", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap BrickIcon_DesignScope_MyselfOnly
	{
		get
		{
			object obj = ResourceManager.GetObject("BrickIcon_DesignScope_MyselfOnly", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap BrickIcon_DesignScope_NewProfile
	{
		get
		{
			object obj = ResourceManager.GetObject("BrickIcon_DesignScope_NewProfile", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap BrickIcon_DesignScope_Publish
	{
		get
		{
			object obj = ResourceManager.GetObject("BrickIcon_DesignScope_Publish", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap BrickIcon_Field_Boolean
	{
		get
		{
			object obj = ResourceManager.GetObject("BrickIcon_Field_Boolean", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap BrickIcon_Field_DateTime
	{
		get
		{
			object obj = ResourceManager.GetObject("BrickIcon_Field_DateTime", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap BrickIcon_Field_ExternalRef
	{
		get
		{
			object obj = ResourceManager.GetObject("BrickIcon_Field_ExternalRef", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap BrickIcon_Field_InternalRef
	{
		get
		{
			object obj = ResourceManager.GetObject("BrickIcon_Field_InternalRef", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap BrickIcon_Field_Number
	{
		get
		{
			object obj = ResourceManager.GetObject("BrickIcon_Field_Number", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap BrickIcon_Field_Option
	{
		get
		{
			object obj = ResourceManager.GetObject("BrickIcon_Field_Option", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap BrickIcon_Field_Text
	{
		get
		{
			object obj = ResourceManager.GetObject("BrickIcon_Field_Text", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal Image()
	{
	}
}
