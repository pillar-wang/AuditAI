using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Auditai.PlatformResource.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
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
				ResourceManager resourceManager = new ResourceManager("Auditai.PlatformResource.Properties.Resource", typeof(Resource).Assembly);
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

	internal static Icon 业务管控平台
	{
		get
		{
			object obj = ResourceManager.GetObject("业务管控平台", resourceCulture);
			return (Icon)obj;
		}
	}

	internal static Icon 审计协作平台
	{
		get
		{
			object obj = ResourceManager.GetObject("审计协作平台", resourceCulture);
			return (Icon)obj;
		}
	}

	internal static Icon 报表开发平台
	{
		get
		{
			object obj = ResourceManager.GetObject("报表开发平台", resourceCulture);
			return (Icon)obj;
		}
	}

	internal static Icon 集团报表平台
	{
		get
		{
			object obj = ResourceManager.GetObject("集团报表平台", resourceCulture);
			return (Icon)obj;
		}
	}

	internal Resource()
	{
	}
}
