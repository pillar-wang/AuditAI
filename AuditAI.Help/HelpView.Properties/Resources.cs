using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace HelpView.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
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
				ResourceManager resourceManager = new ResourceManager("HelpView.Properties.Resources", typeof(Resources).Assembly);
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

	internal static Bitmap back32
	{
		get
		{
			object @object = ResourceManager.GetObject("back32", resourceCulture);
			return (Bitmap)@object;
		}
	}

	internal static Bitmap DocumentStructure
	{
		get
		{
			object @object = ResourceManager.GetObject("DocumentStructure", resourceCulture);
			return (Bitmap)@object;
		}
	}

	internal static Bitmap Find
	{
		get
		{
			object @object = ResourceManager.GetObject("Find", resourceCulture);
			return (Bitmap)@object;
		}
	}

	internal static Bitmap forward32
	{
		get
		{
			object @object = ResourceManager.GetObject("forward32", resourceCulture);
			return (Bitmap)@object;
		}
	}

	internal static Bitmap PdfExport
	{
		get
		{
			object @object = ResourceManager.GetObject("PdfExport", resourceCulture);
			return (Bitmap)@object;
		}
	}

	internal static Bitmap Print
	{
		get
		{
			object @object = ResourceManager.GetObject("Print", resourceCulture);
			return (Bitmap)@object;
		}
	}

	internal Resources()
	{
	}
}
