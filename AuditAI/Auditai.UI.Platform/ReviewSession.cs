﻿using System;

namespace Auditai.UI.Platform;

/// <summary>审批只读会话：审批人以只读方式打开待审项目期间设置。
/// 权威标志存储在模型层（Auditai.Model.Project.ReadonlyOpenProjectId），
/// 由 TreeNodeBase.HasRead/HasWrite/HasSchemaPermission 联动实现全链路只读。</summary>
public static class ReviewSession
{
	public static Guid? ReadOnlyProjectId
	{
		get => Auditai.Model.Project.ReadonlyOpenProjectId;
		set => Auditai.Model.Project.ReadonlyOpenProjectId = value;
	}

	public static bool IsReadOnly(Guid projectId) => ReadOnlyProjectId.HasValue && ReadOnlyProjectId.Value == projectId;

	public static void Begin(Guid projectId) => ReadOnlyProjectId = projectId;

	public static void End()
	{
		if (ReadOnlyProjectId != null)
		{
			ReadOnlyProjectId = null;
		}
	}
}
