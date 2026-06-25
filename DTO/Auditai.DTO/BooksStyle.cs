using Newtonsoft.Json;

namespace Auditai.DTO;

public class BooksStyle
{
	private int _booksRowHeight;

	[JsonProperty(PropertyName = "FontStyle")]
	public FontSetting FontStyle { get; set; }

	[JsonProperty(PropertyName = "BooksRowHeight")]
	public int BooksRowHeight
	{
		get
		{
			if (_booksRowHeight < 10)
			{
				return 10;
			}
			if (_booksRowHeight > 2000)
			{
				return 2000;
			}
			return _booksRowHeight;
		}
		set
		{
			_booksRowHeight = value;
		}
	}

	[JsonProperty(PropertyName = "TotalDisplay")]
	public TotalFlag TotalDisplay { get; set; }

	[JsonProperty(PropertyName = "SubDisplay")]
	public TotalFlag SubDisplay { get; set; }

	[JsonProperty(PropertyName = "BalanceTo")]
	public SubOrTotal BalanceTo { get; set; }

	[JsonProperty(PropertyName = "EnableLedger")]
	public bool EnableLedger { get; set; }

	public BooksStyle()
	{
		FontStyle = new FontSetting();
		BooksRowHeight = 30;
		TotalDisplay = TotalDisplayFlags.MonthOnly;
		SubDisplay = SubDisplayFlags.DataOnly;
		BalanceTo = SubOrTotal.Subsidiary;
		EnableLedger = false;
	}
}
