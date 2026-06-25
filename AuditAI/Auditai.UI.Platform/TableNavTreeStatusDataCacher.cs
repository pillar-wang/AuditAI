namespace Auditai.UI.Platform;

/// <summary>
/// 可选的表格导航树状态缓存辅助类。
/// 当前所有方法为空存根，用于在不启用缓存功能时提供无操作的回退。
/// 如需启用缓存，可在各方法中实现实际的持久化逻辑（如存入字典、文件或数据库）。
/// </summary>
public static class TableNavTreeStatusDataCacher
{
	// 缓存存根：保存导航树状态数据（可选实现）
	public static void SaveNavTreeStatusData(object tableId, object navGrid) { }
	// 缓存存根：恢复导航树状态数据（可选实现）
	public static void RestoreNavTreeStatusData(object tableId, object navGrid) { }
	// 缓存存根：移除表格缓存（可选实现）
	public static void RemoveTable(object project) { }
}