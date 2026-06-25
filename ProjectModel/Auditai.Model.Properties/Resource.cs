using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Auditai.Model.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
public class Resource
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				ResourceManager resourceManager = new ResourceManager("Auditai.Model.Properties.Resource", typeof(Resource).Assembly);
				resourceMan = resourceManager;
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static CultureInfo Culture
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

	public static string DefaultSectPr => ResourceManager.GetString("DefaultSectPr", resourceCulture);

	public static string DefaultStyleXml => ResourceManager.GetString("DefaultStyleXml", resourceCulture);

	public static Bitmap FolderCollapsed
	{
		get
		{
			object @object = ResourceManager.GetObject("FolderCollapsed", resourceCulture);
			return (Bitmap)@object;
		}
	}

	public static Bitmap FolderExpanded
	{
		get
		{
			object @object = ResourceManager.GetObject("FolderExpanded", resourceCulture);
			return (Bitmap)@object;
		}
	}

	public static Bitmap GraphDir
	{
		get
		{
			object @object = ResourceManager.GetObject("GraphDir", resourceCulture);
			return (Bitmap)@object;
		}
	}

	public static Bitmap GraphDirBg
	{
		get
		{
			object @object = ResourceManager.GetObject("GraphDirBg", resourceCulture);
			return (Bitmap)@object;
		}
	}

	public static Bitmap GraphDoc
	{
		get
		{
			object @object = ResourceManager.GetObject("GraphDoc", resourceCulture);
			return (Bitmap)@object;
		}
	}

	public static Bitmap GraphImage
	{
		get
		{
			object @object = ResourceManager.GetObject("GraphImage", resourceCulture);
			return (Bitmap)@object;
		}
	}

	public static Bitmap GraphPdf
	{
		get
		{
			object @object = ResourceManager.GetObject("GraphPdf", resourceCulture);
			return (Bitmap)@object;
		}
	}

	public static Bitmap GraphTable
	{
		get
		{
			object @object = ResourceManager.GetObject("GraphTable", resourceCulture);
			return (Bitmap)@object;
		}
	}

	public static Bitmap GraphTableBg
	{
		get
		{
			object @object = ResourceManager.GetObject("GraphTableBg", resourceCulture);
			return (Bitmap)@object;
		}
	}

	internal Resource()
	{
	}
}
