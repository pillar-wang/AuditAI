namespace Auditai.UI.Platform;

/// <summary>
/// phosphor 语义名 → 语义色查询表：按 icon-map.csv 的 color 列生成（13 色语义板），
/// 供命令栏等直接调用 IconLibrary.DrawGlyph 绘制的调用方按图标语义取色。
/// neutral/空/未知色名统一回落 IconLibrary.DefaultColor（与界面文字色一致）。
/// 由 gen-iconres.ps1 按 icon-map.csv 生成，请勿手工编辑属性区。
/// </summary>
public static class IconPalette
{
	public static System.Drawing.Color Get(string phosphorName)
	{
		switch (phosphorName)
		{
			case "folder-plus": return AuditTheme.SuccessText;
			case "folder-open": return AuditTheme.Brand;
			case "pencil": return AuditTheme.Brand;
			case "trash": return AuditTheme.ErrorText;
			case "copy": return AuditTheme.Brand;
			case "export": return AuditTheme.WarningText;
			case "floppy-disk": return AuditTheme.SuccessText;
			case "paper-plane-tilt": return AuditTheme.Brand;
			case "shield-check": return AuditTheme.SuccessText;
			case "archive": return AuditTheme.WarningText;
			case "users": return AuditTheme.Teal;
			case "user": return AuditTheme.Indigo;
			case "list": return AuditTheme.Brand;
			case "list-bullets": return AuditTheme.Brand;
			case "key": return AuditTheme.WarningText;
			case "browsers": return AuditTheme.Brand;
			case "arrow-counter-clockwise": return AuditTheme.SuccessText;
			case "trash-simple": return AuditTheme.Rose;
			case "share": return AuditTheme.Brand;
			case "cloud-arrow-up": return AuditTheme.Brand;
			case "folders": return AuditTheme.Indigo;
			case "file-text": return AuditTheme.Indigo;
			case "folder": return AuditTheme.Brand;
			case "file": return AuditTheme.Teal;
			case "image": return AuditTheme.Indigo;
			case "file-pdf": return AuditTheme.Rose;
			case "table": return AuditTheme.Brand;
			case "calculator": return AuditTheme.Brand;
			case "tray-arrow-down": return AuditTheme.WarningText;
			case "check-circle": return AuditTheme.SuccessText;
			case "file-doc": return AuditTheme.WarningText;
			case "file-xls": return AuditTheme.WarningText;
			case "book": return AuditTheme.Brand;
			case "clipboard-text": return AuditTheme.SuccessText;
			case "function": return AuditTheme.Brand;
			case "cursor-click": return AuditTheme.Brand;
			case "database": return AuditTheme.WarningText;
			case "crown": return AuditTheme.WarningText;
			case "lightbulb": return AuditTheme.WarningText;
			case "signature": return AuditTheme.Brand;
			case "upload": return AuditTheme.WarningText;
			case "users-three": return AuditTheme.Slate;
			case "user-plus": return AuditTheme.SuccessText;
			case "plus": return AuditTheme.SuccessText;
			case "sign-in": return AuditTheme.Brand;
			case "user-minus": return AuditTheme.ErrorText;
			case "crosshair": return AuditTheme.Brand;
			case "file-plus": return AuditTheme.SuccessText;
			case "arrow-u-up-left": return AuditTheme.ErrorText;
			// —— 补充：审计场景常用图标 ——
			case "magnifying-glass": return AuditTheme.Slate;
			case "sliders-horizontal": return AuditTheme.Slate;
			case "download": return AuditTheme.SuccessText;
			case "print": return AuditTheme.Slate;
			case "gear": return AuditTheme.Slate;
			case "bell": return AuditTheme.WarningText;
			case "chart-bar": return AuditTheme.Brand;
			case "chart-pie": return AuditTheme.Brand;
			case "wallet": return AuditTheme.SuccessText;
			case "file-search": return AuditTheme.Brand;
			case "files": return AuditTheme.Indigo;
			case "house": return AuditTheme.Brand;
			case "identification-card": return AuditTheme.Slate;
			default: return global::Auditai.UI.Controls.IconLibrary.DefaultColor;
		}
	}
}
