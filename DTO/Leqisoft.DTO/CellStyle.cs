namespace Leqisoft.DTO;

public class CellStyle
{
	public Id64 Id { get; set; }

	public Id64 TableId { get; set; }

	public int? ForeColor { get; set; }

	public int? BackColor { get; set; }

	public int? Align { get; set; }

	public float? FontSize { get; set; }

	public int? Margin { get; set; }

	public string FontFamily { get; set; }

	public bool? Bold { get; set; }

	public bool? Italic { get; set; }

	public bool? Underline { get; set; }

	public int? DataType { get; set; }

	public string Format { get; set; }

	public long? Locked { get; set; }

	public int Status { get; set; }

	public string DefaultValue { get; set; }

	public string Comment { get; set; }
}
