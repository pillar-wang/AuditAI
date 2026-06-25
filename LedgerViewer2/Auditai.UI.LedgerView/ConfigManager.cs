using System.IO;
using System.Reflection;

namespace Auditai.UI.LedgerView;

public class ConfigManager
{
	public static readonly string RECENTLEDGERPATH = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "config", "recentledger.json");

	public static readonly string USERCONFIGFILE = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "config", "ledgerView.json");
}
