using Auditai.UI.Platform;
using Newtonsoft.Json;

namespace Auditai.DTO;

public class UserConfig
{
	private int _recentProjectCount;

	[JsonProperty(PropertyName = "BooksStyle")]
	public BooksStyle BooksStyle { get; set; }

	[JsonProperty(PropertyName = "TableStyle")]
	public TableSetting TableStyle { get; set; }

	[JsonProperty(PropertyName = "DocStyle")]
	public DocStyle DocStyle { get; set; }

	[JsonProperty(PropertyName = "SignatureStyle")]
	public SignatureStyle SignatureStyle { get; set; }

	[JsonProperty(PropertyName = "CollectSetting")]
	public CollectSetting CollectSetting { get; set; }

	[JsonProperty(PropertyName = "NavigateViewType")]
	public NavigateViewType NavigateViewType { get; set; }

	[JsonProperty(PropertyName = "IsTitleFitTableName")]
	public bool IsTitleFitTableName { get; set; }

	[JsonProperty(PropertyName = "HideTab")]
	public bool HideTab { get; set; }

	[JsonProperty(PropertyName = "HideGroupLable")]
	public bool HideGroupLable { get; set; }

	[JsonProperty(PropertyName = "GuidDisplay")]
	public bool GuidDisplay { get; set; } = true;


	[JsonProperty(PropertyName = "RecentProjectCount")]
	public int RecentProjectCount
	{
		get
		{
			if (_recentProjectCount >= 0)
			{
				return _recentProjectCount;
			}
			return 0;
		}
		set
		{
			_recentProjectCount = value;
		}
	}

	[JsonProperty(PropertyName = "AutoRowAdd")]
	public bool AutoRowAdd { get; set; }

	[JsonProperty(PropertyName = "AutoDecimalSet")]
	public bool AutoDecimalSet { get; set; }

	[JsonProperty(PropertyName = "AutoAreaMerge")]
	public bool AutoAreaMerge { get; set; }

	[JsonProperty(PropertyName = "ShowNumber")]
	public bool ShowNumber { get; set; } = true;


	[JsonProperty(PropertyName = "AutoNumSet")]
	public bool SelectionStatsEnabled { get; set; }

	[JsonProperty(PropertyName = "AutoSpellCheck")]
	public bool AutoSpellCheck { get; set; }

	[JsonProperty(PropertyName = "CurrentTheme")]
	public string CurrentTheme { get; set; }

	[JsonProperty(PropertyName = "RowsApplyFormulaAuto")]
	public int RowsApplyFormulaAuto { get; set; } = 1000;


	[JsonProperty(PropertyName = "UserName")]
	public string UserName { get; set; }

	[JsonProperty(PropertyName = "Password")]
	public string Password { get; set; }

	[JsonProperty(PropertyName = "PhoneNumber")]
	public string PhoneNumber { get; set; }

	[JsonProperty(PropertyName = "LoginType")]
	public int LoginType { get; set; }

	[JsonProperty(PropertyName = "Machine")]
	public string Machine { get; set; }

	[JsonProperty(PropertyName = "Tooltip")]
	public bool Tooltip { get; set; }

	[JsonProperty(PropertyName = "ProjectMembersViewMode")]
	public ListTileViewMode ProjectMembersViewMode { get; set; } = ListTileViewMode.Tile;


	[JsonProperty(PropertyName = "TeamUsersViewMode")]
	public ListTileViewMode TeamUsersViewMode { get; set; } = ListTileViewMode.Tile;


	[JsonProperty(PropertyName = "AppEdition")]
	public int AppEdition { get; set; } = 1;


	[JsonProperty(PropertyName = "IsLoginByPhoneNumber")]
	public bool IsLoginByPhoneNumber { get; set; }

	[JsonProperty(PropertyName = "HideFootRow")]
	public bool HideFootRow { get; set; } = true;

	public UserConfig()
	{
		BooksStyle = new BooksStyle();
		TableStyle = new TableSetting();
		DocStyle = new DocStyle();
		SignatureStyle = new SignatureStyle();
		CollectSetting = new CollectSetting();
		IsTitleFitTableName = true;
		HideTab = false;
		HideGroupLable = false;
		RecentProjectCount = 5;
		AutoRowAdd = true;
		AutoDecimalSet = true;
		AutoAreaMerge = true;
		SelectionStatsEnabled = true;
		AutoSpellCheck = true;
		CurrentTheme = "3";
	}

	public string SaveConfig()
	{
		try
		{
			return JsonConvert.SerializeObject(this);
		}
		catch
		{
			return JsonConvert.SerializeObject(new UserConfig());
		}
	}

	public void LoadConfig(string serialize)
	{
		TableStyle.SubTitleContent.Clear();
		JsonConvert.PopulateObject(serialize, this);
	}
}
