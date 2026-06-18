﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class DataReference
{
	private static readonly int REFDIRTY_KEY;

	private static readonly int REFDIRTY_VALUE;

	private BitVector32 _dirty;

	public string Key { get; internal set; }

	public string Value { get; internal set; }

	public Id64 Id { get; set; }

	public int Dirty
	{
		get
		{
			return _dirty.Data;
		}
		set
		{
			_dirty = new BitVector32(value);
		}
	}

	public DataReferenceKind Kind { get; internal set; }

	internal bool IsKeyDirty
	{
		get
		{
			return _dirty[REFDIRTY_KEY];
		}
		set
		{
			_dirty[REFDIRTY_KEY] = value;
		}
	}

	internal bool IsValueDirty
	{
		get
		{
			return _dirty[REFDIRTY_VALUE];
		}
		set
		{
			_dirty[REFDIRTY_VALUE] = value;
		}
	}

	public SyncStatus Status { get; set; }

	static DataReference()
	{
		REFDIRTY_KEY = BitVector32.CreateMask();
		REFDIRTY_VALUE = BitVector32.CreateMask(REFDIRTY_KEY);
	}

	public Cell GetCell(DataReferenceEvaluationContext context)
	{
		if (Kind == DataReferenceKind.CellRef)
		{
			string[] array = Value.Split('.');
			return context.Project.GetTableById(Id64.ParseBase64(array[0]))?.LoadAndReturn().GetCellById(Id64.ParseBase64(array[1]));
		}
		if (Kind == DataReferenceKind.CrossProjectCellRef)
		{
			// Value 格式: {ProjectId}|{TableId}.{CellId}
			string[] parts = Value.Split('|');
			if (parts.Length != 2) return null;
			if (!Guid.TryParse(parts[0], out Guid projectId)) return null;
			string[] cellParts = parts[1].Split('.');
			if (cellParts.Length != 2) return null;
			
			// 从缓存获取或通过 ProjectResolver 加载外部项目
			Project externalProject = null;
			if (context.ExternalProjectCache != null && context.ExternalProjectCache.TryGetValue(projectId, out externalProject))
			{
				// 缓存命中
			}
			else if (context.ProjectResolver != null)
			{
				externalProject = context.ProjectResolver(projectId);
				if (externalProject != null && context.ExternalProjectCache != null)
				{
					context.ExternalProjectCache[projectId] = externalProject;
				}
			}
			
			if (externalProject == null) return null;
			return externalProject.GetTableById(Id64.ParseBase64(cellParts[0]))?.LoadAndReturn()?.GetCellById(Id64.ParseBase64(cellParts[1]));
		}
		return null;
	}

	public string GetValue(DataReferenceEvaluationContext context)
	{
		if (Kind == DataReferenceKind.Text)
		{
			return Value;
		}
		if (Kind == DataReferenceKind.BuiltIn)
		{
			return EvaluateBuiltIn(context);
		}
		if (Kind == DataReferenceKind.CellRef)
		{
			return GetCell(context)?.GetDisplayValue() ?? "单元格不存在";
		}
		if (Kind == DataReferenceKind.CrossProjectCellRef)
		{
			return GetCell(context)?.GetDisplayValue() ?? "引用数据不可用";
		}
		throw new ArgumentOutOfRangeException();
	}

	public void SetSynced()
	{
		Status = SyncStatus.Synced;
		_dirty = default(BitVector32);
	}

	public void UpdateKey(string key)
	{
		Key = key;
		if (Status == SyncStatus.Synced)
		{
			IsKeyDirty = true;
		}
	}

	public void UpdateValue(string value)
	{
		Value = value;
		if (Status == SyncStatus.Synced)
		{
			IsValueDirty = true;
		}
	}

	internal Leqisoft.DTO.DataReference ToDto()
	{
		return new Leqisoft.DTO.DataReference
		{
			Dirty = Dirty,
			Id = Id,
			Key = Key,
			Status = (int)Status,
			Value = Value,
			Kind = (int)Kind
		};
	}

	private string EvaluateBuiltIn(DataReferenceEvaluationContext context)
	{
		switch (Key)
		{
		case "当前用户账号":
			return User.Current?.UserName ?? string.Empty;
		case "当前用户姓名":
			return User.Current?.Name ?? string.Empty;
		case "全体成员姓名":
			return context.Project?.Users != null
				? string.Join("|", context.Project.Users.Select((KeyValuePair<Leqisoft.DTO.User, UserRole> u) => u.Key.Name))
				: string.Empty;
		case "当前日期":
			return DateTime.Now.ToString("yyyy年MM月dd日");
		case "索引号":
			return context.CurrentTreeNode?.Number ?? string.Empty;
		case "当前用户角色":
			if (context.Project?.Users == null || User.Current == null)
				return string.Empty;
			var roleKv = context.Project.Users.FirstOrDefault((KeyValuePair<Leqisoft.DTO.User, UserRole> kv) => kv.Key.Id == User.Current.Id);
			return roleKv.Equals(default(KeyValuePair<Leqisoft.DTO.User, UserRole>)) ? string.Empty : GetUserRoleString(roleKv.Value);
		case "换行":
			return "\n";
		case "当前组织名称":
			return UserTeam.Current?.Name ?? string.Empty;
		case "全体成员账号":
			return context.Project?.Users != null
				? string.Join("|", context.Project.Users.Select((KeyValuePair<Leqisoft.DTO.User, UserRole> u) => u.Key.UserName))
				: string.Empty;
		default:
			if (Key == StringConstBase.Current.Project + "编号")
			{
				return context.Project?.Number ?? string.Empty;
			}
			if (Key == StringConstBase.Current.Project + "名称")
			{
				return context.Project?.Name ?? string.Empty;
			}
			if (Key == StringConstBase.Current.Auditee)
			{
				return context.Project?.Auditee ?? string.Empty;
			}
			return string.Empty;
		}
	}

	private static string GetUserRoleString(UserRole userRole)
	{
		return userRole switch
		{
			UserRole.Manager => StringConstBase.Current.Manager, 
			UserRole.Assistant => StringConstBase.Current.Assistant, 
			UserRole.Checker => "复核人", 
			UserRole.Editor => "可编辑的用户", 
			UserRole.User => "可使用的用户", 
			_ => string.Empty, 
		};
	}
}
