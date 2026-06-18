﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Leqisoft.DTO;
using Leqisoft.LocalDataStore;

namespace Leqisoft.Model;

public class DataReferenceManager
{
	private static Regex _regexKey = new Regex("\\[([^\\[\\]]+)\\]");

	private Project _project;

	internal Dictionary<string, DataReference> _dic = new Dictionary<string, DataReference>();

	internal HashSet<Id64> _removed = new HashSet<Id64>();

	internal HashSet<Id64> _toDelete = new HashSet<Id64>();

	internal DataReferenceManager(Project project)
	{
		_project = project;
		InitDic();
	}

	public DataReference Add(string key, string value)
	{
		DataReference dataReference = new DataReference
		{
			Id = Project.Current.GetNextId(),
			Key = key,
			Value = value,
			Dirty = 0,
			Status = SyncStatus.New,
			Kind = DataReferenceKind.Text
		};
		_dic.Add(key, dataReference);
		return dataReference;
	}

	public DataReference Add(string key, Cell cell)
	{
		DataReference dataReference = new DataReference
		{
			Id = Project.Current.GetNextId(),
			Key = key,
			Value = GetCellRefString(cell),
			Dirty = 0,
			Status = SyncStatus.New,
			Kind = DataReferenceKind.CellRef
		};
		_dic.Add(key, dataReference);
		return dataReference;
	}

	public DataReference AddCrossProject(string key, Guid projectId, Id64 tableId, Id64 cellId)
	{
		DataReference dataReference = new DataReference
		{
			Id = Project.Current.GetNextId(),
			Key = key,
			Value = projectId.ToString() + "|" + tableId.ToBase64() + "." + cellId.ToBase64(),
			Dirty = 0,
			Status = SyncStatus.New,
			Kind = DataReferenceKind.CrossProjectCellRef
		};
		_dic.Add(key, dataReference);
		return dataReference;
	}

	public bool Exists(string key)
	{
		DataReference value;
		return _dic.TryGetValue(key, out value);
	}

	public DataReference Get(string key)
	{
		if (!_dic.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public void UpdateKey(string key, string newKey)
	{
		DataReference dataReference = _dic[key];
		dataReference.UpdateKey(newKey);
		_dic.Remove(key);
		_dic.Add(newKey, dataReference);
	}

	public void Remove(string key)
	{
		DataReference dataReference = _dic[key];
		_dic.Remove(key);
		_removed.Add(dataReference.Id);
	}

	public IEnumerable<DataReference> Enumerate()
	{
		return _dic.Values;
	}

	public string GetCellRefDisplay(string store, DataReferenceKind kind)
	{
		if (kind == DataReferenceKind.CrossProjectCellRef)
		{
			// Value 格式: {ProjectId}|{TableId}.{CellId}
			string[] parts = store.Split('|');
			if (parts.Length != 2) return "引用格式错误";
			if (!Guid.TryParse(parts[0], out Guid projectId)) return "项目ID格式错误";
			string[] cellParts = parts[1].Split('.');
			if (cellParts.Length != 2) return "引用格式错误";
			
			// 从缓存获取项目名称
			Project externalProject = GetExternalProjectById(projectId);
			string projectName = externalProject?.Name ?? "外部项目";
			
			Table tableById = externalProject?.GetTableById(Id64.ParseBase64(cellParts[0]));
			if (tableById == null) return "表格不存在";
			
			Cell cellById = tableById.LoadAndReturn().GetCellById(Id64.ParseBase64(cellParts[1]));
			if (cellById == null) return "单元格不存在";
			
			return $"{{{projectName}}}::{tableById.TreeNode.Name}[{cellById.Column.CaptionDisplay},{cellById.Row.Index + 1}]";
		}
		
		// 原有逻辑
		string[] array = store.Split('.');
		Table tableById2 = _project.GetTableById(Id64.ParseBase64(array[0]));
		if (tableById2 == null)
		{
			return "表格不存在";
		}
		Cell cellById2 = tableById2.LoadAndReturn().GetCellById(Id64.ParseBase64(array[1]));
		if (cellById2 == null)
		{
			return "单元格不存在";
		}
		return $"{{{tableById2.TreeNode.Name}}}[{cellById2.Column.CaptionDisplay},{cellById2.Row.Index + 1}]";
	}

	public string ReplaceString(string input, DataReferenceEvaluationContext context)
	{
		if (input == null)
		{
			return null;
		}
		return _regexKey.Replace(input, delegate(Match match)
		{
			string value = match.Groups[1].Value;
			return Exists(value) ? Get(value).GetValue(context) : match.Value;
		});
	}

	public Tuple<int, int, string> FindIn(string input, DataReferenceEvaluationContext context)
	{
		Match match = _regexKey.Match(input);
		if (!match.Success)
		{
			return null;
		}
		string value = match.Groups[1].Value;
		if (Exists(value))
		{
			return Tuple.Create(match.Index, match.Length, Get(value).GetValue(context));
		}
		return null;
	}

	internal void Reset()
	{
		_dic.Clear();
		InitDic();
		_removed.Clear();
		_toDelete.Clear();
	}

	private static string GetCellRefString(Cell cell)
	{
		return cell._Table.Id.ToBase64() + "." + cell.Id.ToBase64();
	}

	private void InitDic()
	{
		AddBuiltIn(StringConstBase.Current.Project + "编号");
		AddBuiltIn(StringConstBase.Current.Project + "名称");
		AddBuiltIn(StringConstBase.Current.Auditee);
		AddBuiltIn("当前用户账号");
		AddBuiltIn("当前用户姓名");
		AddBuiltIn("当前用户角色");
		AddBuiltIn("当前组织名称");
		AddBuiltIn("全体成员账号");
		AddBuiltIn("全体成员姓名");
		AddBuiltIn("当前日期");
		AddBuiltIn("索引号");
		AddBuiltIn("换行");
	}

	private void AddBuiltIn(string key)
	{
		_dic.Add(key, new DataReference
		{
			Key = key,
			Kind = DataReferenceKind.BuiltIn
		});
	}

	private Project GetExternalProjectById(Guid projectId)
	{
		// 尝试从 Project.Current 的上下文中获取
		// 本地模式下通过 ProjectDAL 打开外部项目
		try
		{
			if (Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
			{
				var project = new Project { Id = projectId };
				// 本地模式：项目 DB 文件路径为 data/{userId}/{projectId}.db
				long userId = User.Current?.Id ?? 1;
				string dbPath = System.IO.Path.Combine("data", userId.ToString(), projectId.ToString() + ".db");
				if (!System.IO.File.Exists(dbPath))
					return null;

				var dal = new Leqisoft.DTO.ProjectDAL(dbPath);
				var dto = dal.GetProject();
				if (dto != null)
				{
					project.Name = dto.Name;
					return project;
				}
			}
		}
		catch { }
		return null;
	}
}
