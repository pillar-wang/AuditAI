﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using Newtonsoft.Json;

namespace AuditApiServer.Models;

// 与客户端 DTO/Auditai.DTO 序列化格式兼容的 POCO（不引用 net452 的 DTO.dll，避免框架冲突）

public class UserDto
{
    public long Id { get; set; }
    public string? UserName { get; set; }
    [JsonIgnore]
    public string? Password { get; set; }
    public string? Email { get; set; }
    public string? Sex { get; set; }
    public string? Name { get; set; }
    public string? Company { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public int Role { get; set; } // UserRole: 0=Manager,1=Assistant,2=Checker,3=Editor,4=User
    public string? QQId { get; set; }
    public string? WechatId { get; set; }
    [JsonIgnore]
    public string? Salt { get; set; }
    public Guid TeamId { get; set; }
    public bool IsDataAdmin { get; set; }
    public byte[]? Picture { get; set; }
    public DateTime LicenseDate { get; set; }
    public long? GroupId { get; set; }
    public string? JobTitle { get; set; }
    [JsonIgnore]
    public object? UserGroup { get; set; }
    public bool IsTeamAdmin { get; set; }
    public bool IsSystemAdmin { get; set; }
    public bool IsActive { get; set; }
    public object? Permissions { get; set; }

    // 阶段 9 Task 9.4：密码安全字段（不暴露给客户端）
    /// <summary>密码 Salt（Base64），用于 SHA256+Salt 校验</summary>
    [JsonIgnore]
    public string? PasswordSalt { get; set; }

    /// <summary>密码哈希算法版本：0=Legacy 原样存储，1=SHA256+Salt</summary>
    [JsonIgnore]
    public int PasswordHashAlgorithm { get; set; }

    /// <summary>设备码：注册/首次登录时绑定，后续登录校验一致性（不暴露给客户端）</summary>
    [JsonIgnore]
    public string? MachineCode { get; set; }
}

public class UserTokenDto
{
    public long UserId { get; set; }
    public string? LastToken { get; set; }
    public string? TokenValue { get; set; }
    public string? UpdateToken { get; set; }
    public DateTime UpdateTime { get; set; }
    public object? Cookie { get; set; }

    /// <summary>Token 过期时间（UTC ISO8601）。null 表示 Legacy 无过期。</summary>
    [JsonIgnore]
    public DateTime? ExpiresAt { get; set; }

    /// <summary>最近一次刷新时间（UTC ISO8601）。</summary>
    [JsonIgnore]
    public DateTime? LastRefreshAt { get; set; }
}

public class ProjectDto
{
    public Guid Id { get; set; }
    public string? Number { get; set; }
    public string? Name { get; set; }
    public string? Category { get; set; }
    public string? Auditee { get; set; }
    public string? Note { get; set; }
    public Guid? ParentId { get; set; }
    public UserDto? Creator { get; set; }
    public int Version { get; set; }
    public int Type { get; set; }      // ProjectType
    public int ChargeType { get; set; }
    public IEnumerable<UserDto>? Users { get; set; }
    public bool TeamVisible { get; set; }
    public Guid? TemplateId { get; set; }
    public Guid? TeamId { get; set; }
    public int DemoId { get; set; }
    public bool SystemBuild { get; set; }
    public DateTime CreateTime { get; set; }
    public int ProjectChargeType { get; set; }
    public DateTime ProjectLicenseDate { get; set; } = DateTime.Now;
    public string? CustomFillConfig { get; set; }
    public int TemplateVersion { get; set; }
    public int ReviewStatus { get; set; }   // 0=未上报 1=审批中 2=已通过 3=已退回
    public bool IsArchived { get; set; }
}

public class TeamPushResultDto
{
    public Guid TeamId { get; set; }
    public bool Success { get; set; }
    public Guid ProjectId { get; set; }
    public int TemplateVersion { get; set; }
    public string? Message { get; set; }
}

public class TeamDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public int Level { get; set; }       // TeamLevel: 0=None,1=Standard,2=Professional,3=Ultimate
    public int PayStatus { get; set; }   // PayStatus: 0=Trial,1=Payed,2=Expired,3=Free
    public DateTime LicenseDate { get; set; }
    public long OwnerUserId { get; set; }
    public int EnterpriseId { get; set; }
    public int PlanType { get; set; }
}

public class UserGroupDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public long? ParentId { get; set; }
    public Guid TeamId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProjectMemberDto
{
    public long Id { get; set; }
    public Guid ProjectId { get; set; }
    public long UserId { get; set; }
    public int Role { get; set; }  // 0=成员, 1=管理员
    public DateTime JoinedAt { get; set; }
    public UserDto? User { get; set; }  // 导航属性
}

public class TableSchemaDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string? Name { get; set; }
    public int Version { get; set; }
    public int Mask { get; set; }
    public byte[]? ColumnsData { get; set; }
    public byte[]? RowsData { get; set; }
    public byte[]? CellsData { get; set; }
    public byte[]? CellStylesData { get; set; }
    public byte[]? MergesData { get; set; }
    public byte[]? CellPropsData { get; set; }
    public byte[]? PageSetup { get; set; }
    public byte[]? BorderStyle { get; set; }
    public int HeaderMode { get; set; }
    public int FrozenCols { get; set; }
    public string? CollectSource { get; set; }
    public string? Locker { get; set; }
    public string? FilterInfo { get; set; }
    public string? Foot { get; set; }
    public string? ControlFormula { get; set; }
    public DateTime UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
}

public class DocumentDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string? Name { get; set; }
    public int Version { get; set; }
    public byte[]? ParagraphsData { get; set; }
    public DateTime UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
}

public class VersionHistoryDto
{
    public long Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid TargetId { get; set; }
    public string? TargetType { get; set; }  // 'Table', 'Document', 'Image', 'Pdf'
    public int Version { get; set; }
    public string? ChangeType { get; set; }  // 'Push', 'Revert'
    public byte[]? Snapshot { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
}

public class ServerTaskDto
{
    public string? Id { get; set; }  // GUID（内部主键）
    public long TaskId { get; set; }  // 自增 long 任务 Id（客户端使用的对外 ID，对应 SQLite rowid）
    public long UserId { get; set; }
    public string? TaskType { get; set; }  // 'CreateProject', 'PushProject', ...
    public int Status { get; set; }  // 0=Created, 1=Running, 2=Completed, 3=Failed
    public double Progress { get; set; }
    public string? InputFilePath { get; set; }
    public string? OutputFilePath { get; set; }
    public string? Description { get; set; }
    public string? Result { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class TaskStatusDto
{
    // 强制 camelCase 序列化：客户端 WebApiClient.WaitingServerTaskRunOver 通过 jObject["progressValue"] 等键读取
    [JsonProperty("progressValue")]
    public double ProgressValue { get; set; }
    [JsonProperty("isTaskEnd")]
    public bool IsTaskEnd { get; set; }
    [JsonProperty("isTaskSuccess")]
    public bool IsTaskSuccess { get; set; }
    [JsonProperty("isTimeOut")]
    public bool IsTimeOut { get; set; }
    [JsonProperty("taskDesc")]
    public string? TaskDesc { get; set; }
    [JsonProperty("taskResult")]
    public string? TaskResult { get; set; }
}

public class ValidateCodeDto
{
    public long Id { get; set; }
    public string? Key { get; set; }  // phone 或 delete_{phone}
    public string? Code { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LoginLogDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string? Version { get; set; }
    public string? IPAddress { get; set; }
    public string? MachineCode { get; set; }
    public int HasProcess { get; set; }
    public DateTime LoginAt { get; set; }
    public DateTime? LogoutAt { get; set; }
}

public class ProjectFileDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string? FileName { get; set; }
    public long FileSize { get; set; }
    public string? ContentType { get; set; }
    public string? StoragePath { get; set; }
    public long UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class DataDictionaryDto
{
    public long Id { get; set; }
    public string? DicType { get; set; }  // 'TableCollect', 'CellCollect', 'LedgerValidate'
    public int Version { get; set; }
    public string? Data { get; set; }  // JSON 字典数据
    public DateTime UpdatedAt { get; set; }
}

public class UserStateDto
{
    public string? UserId { get; set; }
    public string? TeamId { get; set; }
    public string? ProjectId { get; set; }
    public string? TreeNodeId { get; set; }
    public string? TableCellId { get; set; }
    public string? DocParagraphId { get; set; }
    public string? TicketNavTreeNodePath { get; set; }
    public string? ConnectionId { get; set; }
}

// 与客户端 FileTransferModel.FileSection 序列化字段对齐（SignalR Hub PushFileSectionToUser 用）
// 客户端 FileSection 类字段：Id (string)、Index (int)、Value (byte[])
public class FileSectionDto
{
    public string? Id { get; set; }
    public int Index { get; set; }
    public byte[]? Value { get; set; }
}

public class DataSourceVersionDto
{
    public int Version { get; set; }
    public string? Data { get; set; }
}

// ============= 阶段 3 Task 10：License 相关 DTO =============

public class LicenseDto
{
    public int Id { get; set; }
    public string? LicenseKey { get; set; }
    public int ProductEdition { get; set; }   // 2=Pro（默认）
    public int PlanType { get; set; }          // 0=Trial,1=Pro,2=Enterprise
    public int Seats { get; set; }
    public int MaxProjects { get; set; }
    public int MaxTemplates { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? OwnerType { get; set; }    // 'Team' / 'User'
    public string? OwnerId { get; set; }      // TeamId GUID 字符串
    public DateTime CreateTime { get; set; }
}

public class ActivationDto
{
    public int Id { get; set; }
    public int LicenseId { get; set; }
    public string? MachineCode { get; set; }
    public DateTime ActivatedAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public bool IsActive { get; set; }
}

public class LicenseStatusDto
{
    public int Id { get; set; }
    public int PlanType { get; set; }
    public int Seats { get; set; }
    public DateTime EndDate { get; set; }
    public List<ActivationDto> Activations { get; set; } = new();
    // 阶段 4 Task 14.3：客户端状态展示用补充字段
    // 阶段 8 Task 23：强制 camelCase 序列化，与客户端 Program.cs 读取的 licenseStatus["isExpired"] 等键一致
    // （服务端 Program.cs 全局用 DefaultNamingStrategy 保持 PascalCase，此处局部覆盖，模仿 TaskStatusDto 做法）
    /// <summary>距到期剩余天数（&lt;=0 表示已过期）</summary>
    [JsonProperty("daysRemaining")]
    public int DaysRemaining { get; set; }
    /// <summary>License 是否已过期</summary>
    [JsonProperty("isExpired")]
    public bool IsExpired { get; set; }
    /// <summary>是否在 7 天宽限期内（已过期但未超 7 天）</summary>
    [JsonProperty("inGracePeriod")]
    public bool InGracePeriod { get; set; }
}

// ============= 管理面板 Admin DTO =============

public class AdminUserListItemDto
{
    public long Id { get; set; }
    public string? UserName { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int Role { get; set; }
    public string? TeamName { get; set; }
    public Guid TeamId { get; set; }
    public bool IsTeamAdmin { get; set; }
    public bool IsSystemAdmin { get; set; }
    public bool IsDataAdmin { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime LicenseDate { get; set; }
    public DateTime CreateTime { get; set; }
}

public class AdminLicenseListItemDto
{
    public long Id { get; set; }
    public string? LicenseKey { get; set; }
    public int PlanType { get; set; }
    public int Seats { get; set; }
    public int MaxProjects { get; set; }
    public int MaxTemplates { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? OwnerType { get; set; }
    public string? OwnerId { get; set; }
    public string? TeamName { get; set; }
    public int ActivationCount { get; set; }
    public int DaysRemaining { get; set; }
    public DateTime CreateTime { get; set; }
}

public class AdminTeamListItemDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public int Level { get; set; }
    public int PayStatus { get; set; }
    public DateTime LicenseDate { get; set; }
    public long OwnerUserId { get; set; }
    public string? OwnerName { get; set; }
    public int MemberCount { get; set; }
    public int ProjectCount { get; set; }
    public int MaxUsers { get; set; }
    public int MaxProjects { get; set; }
    public int MaxTemplates { get; set; }
    public int PlanType { get; set; }
    public int EnterpriseId { get; set; }
}

public class AdminStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalTeams { get; set; }
    public int TotalProjects { get; set; }
    public int TotalLicenses { get; set; }
    public int ExpiringLicenses { get; set; }
    public int PendingInvitations { get; set; }
    public int NewUsersLast7Days { get; set; }
}

public class AdminInvitationListItemDto
{
    public long Id { get; set; }
    public string? InviteToken { get; set; }
    public Guid TeamId { get; set; }
    public string? TeamName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int Role { get; set; }
    public long InvitedBy { get; set; }
    public string? InvitedByName { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? Status { get; set; }
    public DateTime CreateTime { get; set; }
}
