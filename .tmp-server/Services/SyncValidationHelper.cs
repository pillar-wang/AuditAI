namespace AuditApiServer.Services;

/// <summary>
/// 同步服务验证工具类。集中管理 Protobuf Action 字段的验证逻辑。
/// </summary>
public static class SyncValidationHelper
{
    /// <summary>
    /// 合法 Action 取值：1=New, 2=Modify, 3=Delete（与客户端约定一致）。
    /// </summary>
    private static readonly HashSet<int> ValidActions = new HashSet<int> { 1, 2, 3 };

    /// <summary>
    /// 批量验证多个实体的 Action 字段是否合法。
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="entities">实体集合</param>
    /// <param name="entityName">实体名称（用于错误消息）</param>
    /// <param name="actionSelector">Action 字段选择器</param>
    /// <param name="idSelector">实体 ID 选择器（用于错误消息）</param>
    /// <exception cref="ArgumentException">存在非法 Action 值时抛出</exception>
    public static void ValidateActions<T>(
        IEnumerable<T> entities,
        string entityName,
        Func<T, int> actionSelector,
        Func<T, long> idSelector)
    {
        foreach (var entity in entities)
        {
            var action = actionSelector(entity);
            if (!ValidActions.Contains(action))
                throw new ArgumentException($"{entityName} {idSelector(entity)} 含非法 Action={action}（合法值 1=New/2=Modify/3=Delete）");
        }
    }

    /// <summary>
    /// 批量验证多个实体的 Action 字段是否合法（使用字符串 ID）。
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="entities">实体集合</param>
    /// <param name="entityName">实体名称（用于错误消息）</param>
    /// <param name="actionSelector">Action 字段选择器</param>
    /// <param name="idSelector">实体 ID 选择器（用于错误消息，返回字符串）</param>
    /// <exception cref="ArgumentException">存在非法 Action 值时抛出</exception>
    public static void ValidateActions<T>(
        IEnumerable<T> entities,
        string entityName,
        Func<T, int> actionSelector,
        Func<T, string> idSelector)
    {
        foreach (var entity in entities)
        {
            var action = actionSelector(entity);
            if (!ValidActions.Contains(action))
                throw new ArgumentException($"{entityName} {idSelector(entity)} 含非法 Action={action}（合法值 1=New/2=Modify/3=Delete）");
        }
    }
}
