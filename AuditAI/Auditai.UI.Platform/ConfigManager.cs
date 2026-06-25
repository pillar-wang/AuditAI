using System.IO;
using System.Reflection;

namespace Auditai.UI.Platform;

public class ConfigManager
{
	public static readonly string PROJECTMANAGEMENT_VIEWCONFIG = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "config", "projectmanagestyle.json");

	public static readonly string PROJECT_OPERATEINFO_RECORD = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "config", "projectinfo.json");
}
