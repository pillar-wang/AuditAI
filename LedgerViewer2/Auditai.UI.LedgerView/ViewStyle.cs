using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Auditai.UI.LedgerView;

[JsonObject("ViewStyle")]
public class ViewStyle
{
	[JsonProperty("GridStyleCollection")]
	public List<GridStyle> GridStyleCollection { get; set; }

	public GridStyle this[string name] => GridStyleCollection.FirstOrDefault((GridStyle t) => t.Name == name);

	[JsonProperty("FontSize")]
	public float FontSize { get; set; }

	[JsonProperty("FamilyName")]
	public string FamilyName { get; set; }

	[JsonProperty("Height")]
	public float Height { get; set; }

	[JsonProperty("AmountWidth")]
	public int AmountWidth { get; set; }

	[JsonProperty("RatioWidth")]
	public int RatioWidth { get; set; }

	[JsonProperty("DateWidth")]
	public int DateWidth { get; set; }

	public ViewStyle()
	{
		GridStyleCollection = new List<GridStyle>();
		FamilyName = "微软雅黑";
		AmountWidth = 0;
		RatioWidth = 0;
		Height = 20f;
		DateWidth = 0;
	}

	public static ViewStyle Load(string path)
	{
		ViewStyle viewStyle = new ViewStyle();
		if (!File.Exists(path))
		{
			return viewStyle;
		}
		try
		{
			string value = File.ReadAllText(path);
			viewStyle = JsonConvert.DeserializeObject<ViewStyle>(value);
			viewStyle = viewStyle ?? new ViewStyle();
		}
		catch (Exception)
		{
			viewStyle = viewStyle ?? new ViewStyle();
		}
		return viewStyle;
	}

	public static void Save(string path, ViewStyle style)
	{
		string directoryName = Path.GetDirectoryName(path);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		try
		{
			style = style ?? new ViewStyle();
			string contents = JsonConvert.SerializeObject(style);
			File.WriteAllText(path, contents);
		}
		catch (Exception)
		{
		}
	}
}
