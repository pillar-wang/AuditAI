﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class DocumentValidationHealthChecker
{
	/// <summary>
	/// 健康检查结果
	/// </summary>
	public class HealthCheckResult
	{
		public int OrphanCount { get; set; }           // 失效规则数（域丢失）
		public int DuplicateCount { get; set; }        // 重复域数
		public List<Id64> OrphanRuleIds { get; set; } = new List<Id64>();    // 失效规则 Id
		public List<string> Messages { get; set; } = new List<string>();     // 提示信息
		public bool HasIssues => OrphanCount > 0 || DuplicateCount > 0;
	}

	/// <summary>
	/// 执行健康检查：检查所有文档关联的校验规则对应的域是否仍存在于文档中
	/// </summary>
	/// <param name="formulas">ValidationManager 实例，包含所有公式</param>
	/// <param name="documentFieldIds">文档中当前存在的所有域 Id 集合</param>
	/// <returns>健康检查结果</returns>
	public static HealthCheckResult Check(IEnumerable<ValidationFormula> formulas, HashSet<Id64> documentFieldIds)
	{
		var result = new HealthCheckResult();

		if (formulas == null || documentFieldIds == null)
			return result;

		foreach (var formula in formulas)
		{
			// 只检查文档域绑定的公式（DocumentFieldId > 0）
			if (formula.DocumentFieldId.Value <= 0) continue;

			if (!documentFieldIds.Contains(formula.DocumentFieldId))
			{
				result.OrphanCount++;
				result.OrphanRuleIds.Add(formula.Id);
				result.Messages.Add($"校验规则 '{formula.Note ?? formula.LeftExpr}'（Id={formula.Id}）关联的域已丢失。");
			}
		}

		return result;
	}

	/// <summary>
	/// 检测重复域 Id（粘贴导致的重复）
	/// </summary>
	/// <param name="documentFieldIds">文档中所有域 Id 集合</param>
	/// <param name="getFieldById">通过域 Id 获取域对象的委托</param>
	/// <param name="setFieldId">设置域对象新 Id 的委托</param>
	/// <returns>修复后的文档域 Id 集合</returns>
	public static HashSet<Id64> FixDuplicateIds(
		IEnumerable<Id64> documentFieldIds,
		Func<Id64, object> getFieldById,
		Action<object, Id64> setFieldId)
	{
		var result = new HashSet<Id64>();
		var seen = new HashSet<Id64>();

		foreach (var id in documentFieldIds)
		{
			if (id.Value <= 0) continue;

			if (seen.Contains(id))
			{
				// 重复 Id：分配新 Id
				var newId = Project.Current.GetNextId();
				var field = getFieldById(id);
				if (field != null)
				{
					setFieldId(field, newId);
					result.Add(newId);
				}
			}
			else
			{
				seen.Add(id);
				result.Add(id);
			}
		}

		return result;
	}
}
