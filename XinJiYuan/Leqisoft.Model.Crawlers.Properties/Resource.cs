using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Leqisoft.Model.Crawlers.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resource
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
				ResourceManager resourceManager = new ResourceManager("Leqisoft.Model.Crawlers.Properties.Resource", typeof(Resource).Assembly);
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

	internal static Bitmap Icon
	{
		get
		{
			object @object = ResourceManager.GetObject("Icon", resourceCulture);
			return (Bitmap)@object;
		}
	}

	internal static Bitmap Logo
	{
		get
		{
			object @object = ResourceManager.GetObject("Logo", resourceCulture);
			return (Bitmap)@object;
		}
	}

	internal Resource()
	{
	}
}
