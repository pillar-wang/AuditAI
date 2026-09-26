namespace Auditai.UI.Controls;

/// <summary>
/// CommonControls 旧 PNG 图标资源的字体图标替代层（Phosphor Icons Fill 实心渲染，由脚本生成）。
/// 该工程无法引用 AuditTheme，图标统一使用 DefaultColor；
/// style 统一为 Fill 实心（bold/duotone 历史值渲染结果相同）。
/// </summary>
public static class IconRes
{	public static System.Drawing.Bitmap About => global::Auditai.UI.Controls.IconLibrary.CreateBitmap("info", 32, global::Auditai.UI.Controls.IconLibrary.DefaultColor, global::Auditai.UI.Controls.IconLibrary.StyleFill);
	public static System.Drawing.Bitmap Add => global::Auditai.UI.Controls.IconLibrary.CreateBitmap("plus", 32, global::Auditai.UI.Controls.IconLibrary.DefaultColor, global::Auditai.UI.Controls.IconLibrary.StyleFill);
	public static System.Drawing.Bitmap Substract => global::Auditai.UI.Controls.IconLibrary.CreateBitmap("minus", 32, global::Auditai.UI.Controls.IconLibrary.DefaultColor, global::Auditai.UI.Controls.IconLibrary.StyleFill);
	public static System.Drawing.Bitmap Replace => global::Auditai.UI.Controls.IconLibrary.CreateBitmap("swap", 32, global::Auditai.UI.Controls.IconLibrary.DefaultColor, global::Auditai.UI.Controls.IconLibrary.StyleFill);
	public static System.Drawing.Bitmap Checked => CreateCheckboxGlyph(true);
	public static System.Drawing.Bitmap Unchecked => CreateCheckboxGlyph(false);
	public static System.Drawing.Bitmap CollectFill => global::Auditai.UI.Controls.IconLibrary.CreateBitmap("paint-bucket", 16, global::Auditai.UI.Controls.IconLibrary.DefaultColor, global::Auditai.UI.Controls.IconLibrary.StyleFill);
	public static System.Drawing.Bitmap CellCollect16 => global::Auditai.UI.Controls.IconLibrary.CreateBitmap("table", 16, global::Auditai.UI.Controls.IconLibrary.DefaultColor, global::Auditai.UI.Controls.IconLibrary.StyleFill);
	public static System.Drawing.Bitmap TableCollect16 => global::Auditai.UI.Controls.IconLibrary.CreateBitmap("database", 16, global::Auditai.UI.Controls.IconLibrary.DefaultColor, global::Auditai.UI.Controls.IconLibrary.StyleFill);
	public static System.Drawing.Bitmap ConfirmationGenerate => global::Auditai.UI.Controls.IconLibrary.CreateBitmap("clipboard-text", 32, global::Auditai.UI.Controls.IconLibrary.DefaultColor, global::Auditai.UI.Controls.IconLibrary.StyleFill);
	public static System.Drawing.Bitmap ctxAppendRow => global::Auditai.UI.Controls.IconLibrary.CreateBitmap("rows-plus-bottom", 16, global::Auditai.UI.Controls.IconLibrary.DefaultColor, global::Auditai.UI.Controls.IconLibrary.StyleFill);
	public static System.Drawing.Bitmap ctxDeleteRow => global::Auditai.UI.Controls.IconLibrary.CreateBitmap("rows", 16, global::Auditai.UI.Controls.IconLibrary.DefaultColor, global::Auditai.UI.Controls.IconLibrary.StyleFill);
	public static System.Drawing.Bitmap ctxAscending => global::Auditai.UI.Controls.IconLibrary.CreateBitmap("sort-ascending", 16, global::Auditai.UI.Controls.IconLibrary.DefaultColor, global::Auditai.UI.Controls.IconLibrary.StyleFill);
	public static System.Drawing.Bitmap ctxDescending => global::Auditai.UI.Controls.IconLibrary.CreateBitmap("sort-descending", 16, global::Auditai.UI.Controls.IconLibrary.DefaultColor, global::Auditai.UI.Controls.IconLibrary.StyleFill);
	public static System.Drawing.Bitmap ctxCopy => global::Auditai.UI.Controls.IconLibrary.CreateBitmap("copy", 16, global::Auditai.UI.Controls.IconLibrary.DefaultColor, global::Auditai.UI.Controls.IconLibrary.StyleFill);
	public static System.Drawing.Bitmap ctxFilter => global::Auditai.UI.Controls.IconLibrary.CreateBitmap("funnel", 16, global::Auditai.UI.Controls.IconLibrary.DefaultColor, global::Auditai.UI.Controls.IconLibrary.StyleFill);
	public static System.Drawing.Bitmap Filter12 => global::Auditai.UI.Controls.IconLibrary.CreateBitmap("funnel", 20, global::Auditai.UI.Controls.IconLibrary.DefaultColor, global::Auditai.UI.Controls.IconLibrary.StyleFill);
	public static System.Drawing.Bitmap SelectColumn => global::Auditai.UI.Controls.IconLibrary.CreateBitmap("columns", 16, global::Auditai.UI.Controls.IconLibrary.DefaultColor, global::Auditai.UI.Controls.IconLibrary.StyleFill);
	public static System.Drawing.Bitmap TreeDir => global::Auditai.UI.Controls.FileIcons.GetFolder(16);
	public static System.Drawing.Bitmap TreeDoc => global::Auditai.UI.Controls.FileIcons.GetWord(16);
	public static System.Drawing.Bitmap TreeGroup => global::Auditai.UI.Controls.FileIcons.GetFolder(16);
	public static System.Drawing.Bitmap TreeTable => global::Auditai.UI.Controls.FileIcons.GetTable(16);
	public static System.Drawing.Bitmap HelpCenter16 => global::Auditai.UI.Controls.IconLibrary.CreateBitmap("question", 16, global::Auditai.UI.Controls.IconLibrary.DefaultColor, global::Auditai.UI.Controls.IconLibrary.StyleFill);
	public static System.Drawing.Bitmap HelpCenter24 => global::Auditai.UI.Controls.IconLibrary.CreateBitmap("question", 24, global::Auditai.UI.Controls.IconLibrary.DefaultColor, global::Auditai.UI.Controls.IconLibrary.StyleFill);
	public static System.Drawing.Bitmap UpdateIcon => global::Auditai.UI.Controls.IconLibrary.CreateBitmap("arrows-clockwise", 32, global::Auditai.UI.Controls.IconLibrary.DefaultColor, global::Auditai.UI.Controls.IconLibrary.StyleFill);
	public static System.Drawing.Bitmap tileClose => global::Auditai.UI.Controls.IconLibrary.CreateBitmap("x", 15, global::Auditai.UI.Controls.IconLibrary.DefaultColor, global::Auditai.UI.Controls.IconLibrary.StyleFill);
	public static System.Drawing.Bitmap tileCloseDown => global::Auditai.UI.Controls.IconLibrary.CreateBitmap("x", 15, global::Auditai.UI.Controls.IconLibrary.DefaultColor, global::Auditai.UI.Controls.IconLibrary.StyleFill);
	public static System.Drawing.Bitmap tileCloseSlide => global::Auditai.UI.Controls.IconLibrary.CreateBitmap("x", 15, global::Auditai.UI.Controls.IconLibrary.DefaultColor, global::Auditai.UI.Controls.IconLibrary.StyleFill);
	public static System.Drawing.Bitmap btnDown16 => global::Auditai.UI.Controls.IconLibrary.CreateBitmap("caret-down", 16, global::Auditai.UI.Controls.IconLibrary.DefaultColor, global::Auditai.UI.Controls.IconLibrary.StyleFill);
	public static System.Drawing.Bitmap btnUp16 => global::Auditai.UI.Controls.IconLibrary.CreateBitmap("caret-up", 16, global::Auditai.UI.Controls.IconLibrary.DefaultColor, global::Auditai.UI.Controls.IconLibrary.StyleFill);
	public static System.Drawing.Bitmap btnMenu16 => global::Auditai.UI.Controls.IconLibrary.CreateBitmap("list", 16, global::Auditai.UI.Controls.IconLibrary.DefaultColor, global::Auditai.UI.Controls.IconLibrary.StyleFill);
	public static System.Drawing.Bitmap managerMark => global::Auditai.UI.Controls.IconLibrary.CreateBitmap("shield-check", 10, global::Auditai.UI.Controls.IconLibrary.DefaultColor, global::Auditai.UI.Controls.IconLibrary.StyleFill);

	/// <summary>
	/// 创建透明背景的 CheckBox 图标（替换 C1 默认带灰底的 Glyph，供 C1FlexGridEx 网格复选框使用）。
	/// 样式与 TableEditor 的自绘复选框保持一致：黑边框 + 蓝色对勾，透明底。
	/// </summary>
	private static System.Drawing.Bitmap CreateCheckboxGlyph(bool isChecked)
	{
		int size = 16;
		var bmp = new System.Drawing.Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		using (var g = System.Drawing.Graphics.FromImage(bmp))
		{
			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			var pen = new System.Drawing.Pen(System.Drawing.SystemColors.WindowText, 1.4f) { LineJoin = System.Drawing.Drawing2D.LineJoin.Round };
			g.DrawRectangle(pen, 0.7f, 0.7f, size - 1.4f, size - 1.4f);
			if (isChecked)
			{
				var checkPen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(255, 30, 100, 200), 2.2f)
				{
					StartCap = System.Drawing.Drawing2D.LineCap.Round,
					EndCap = System.Drawing.Drawing2D.LineCap.Round
				};
				g.DrawLine(checkPen, 3f, 8f, 8f, 13f);
				g.DrawLine(checkPen, 8f, 13f, 13f, 3f);
			}
		}
		return bmp;
	}
}
