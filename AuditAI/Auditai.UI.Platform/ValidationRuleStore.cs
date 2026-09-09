using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Auditai.DTO;
using Auditai.Util;

namespace Auditai.UI.Platform;

/// <summary>
/// 稽核检查页数据访问层：统一封装本地/云端双模式的项目级校验规则读写。
/// 本地模式直接读写项目 SQLite 库（ValidationFormula 表）；
/// 云端模式走 GetProjectValidations / SaveProjectValidations / DeleteProjectValidations API。
/// 规则新增/导入所需的新 Id 与执行校验所需的公式引擎，均通过 EnsureSessionAsync
/// 加载的项目会话获得（与编辑器打开项目共用同一加载通道）。
/// </summary>
public class ValidationRuleStore
{
	/// <summary>规则清单 + 节点名映射</summary>
	public class RuleSet
	{
		public List<ValidationFormula> Formulas { get; } = new List<ValidationFormula>();

		public Dictionary<long, string> NodeNames { get; } = new Dictionary<long, string>();

		/// <summary>把 TableId 解析为归属名称；文档域规则（DocumentFieldId 非 0）显示"文档域"</summary>
		public string ResolveOwner(ValidationFormula vf)
		{
			if (!vf.DocumentFieldId.IsZero())
			{
				return "文档域";
			}
			if (!vf.TableId.IsZero() && NodeNames.TryGetValue(vf.TableId.Value, out var value))
			{
				return value;
			}
			return vf.TableId.IsZero() ? "未指定" : "表格 " + vf.TableId.Value;
		}
	}

	/// <summary>单条规则的校验结果（一键全项目校验产物）。存字段快照以隔离 Model/DTO 类型差异</summary>
	public class ValidationOutcome
	{
		public Id64 RuleId { get; set; }

		public string LeftExpr { get; set; }

		public int OperatorCode { get; set; }

		public string RightExpr { get; set; }

		public string Note { get; set; }

		public string Owner { get; set; }

		public bool Passed { get; set; }

		public string LeftValue { get; set; }

		public string RightValue { get; set; }

		public int RowIndex { get; set; }

		/// <summary>公式求值失败等异常信息；null 表示正常完成比较</summary>
		public string Error { get; set; }

		public static string OperatorSymbol(int code)
		{
			return code switch
			{
				1 => ">",
				2 => ">=",
				3 => "<",
				4 => "<=",
				5 => "<>",
				_ => "="
			};
		}
	}

	private readonly Auditai.DTO.Project _projectDto;

	private Auditai.Model.Project _session;

	private readonly object _sessionLock = new object();

	public ValidationRuleStore(Auditai.DTO.Project dto)
	{
		_projectDto = dto;
	}

	public Guid ProjectId => _projectDto.Id;

	private static bool IsLocal => Auditai.LocalDataStore.StorageRouter.IsLocalMode;

	/// <summary>读取规则清单（云端 API 或本地项目库直读）</summary>
	public async Task<RuleSet> LoadAsync()
	{
		if (IsLocal)
		{
			return await Task.Run(delegate
			{
				RuleSet ruleSet = new RuleSet();
				string path = MainForm.GetDbPathByGuid(_projectDto.Id);
				if (!File.Exists(path))
				{
					return ruleSet;
				}
				try
				{
					using ProjectDAL dal = new ProjectDAL(path);
					ruleSet.Formulas.AddRange(dal.GetValidationFormulas());
					foreach (TreeNode n in dal.GetTreeNodes())
					{
						if (n.Name != null)
						{
							ruleSet.NodeNames[n.Id.Value] = n.Name;
						}
					}
				}
				catch (Exception)
				{
				}
				return ruleSet;
			});
		}
		RuleSet ruleSet2 = new RuleSet();
		ValidationRuleSetDto rsp = await WebApiClient.GetProjectValidations(_projectDto.Id);
		if (rsp != null)
		{
			ruleSet2.Formulas.AddRange(rsp.Formulas ?? new List<ValidationFormula>());
			foreach (TreeNodeNameDto t in rsp.Tables ?? new List<TreeNodeNameDto>())
			{
				ruleSet2.NodeNames[t.Id] = t.Name;
			}
		}
		return ruleSet2;
	}

	/// <summary>保存规则（新增/编辑/导入，UPSERT）</summary>
	public async Task SaveAsync(IEnumerable<ValidationFormula> rules)
	{
		if (IsLocal)
		{
			await Task.Run(delegate
			{
				string path = MainForm.GetDbPathByGuid(_projectDto.Id);
				using ProjectDAL dal = new ProjectDAL(path);
				dal.SaveValidationFormulas(rules);
			});
		}
		else
		{
			await WebApiClient.SaveProjectValidations(_projectDto.Id, rules);
		}
	}

	/// <summary>删除规则（本地物理删除；云端 API 删除并记录变更历史）</summary>
	public async Task DeleteAsync(IEnumerable<Id64> ids)
	{
		Id64[] array = ids.ToArray();
		if (IsLocal)
		{
			await Task.Run(delegate
			{
				string path = MainForm.GetDbPathByGuid(_projectDto.Id);
				using ProjectDAL dal = new ProjectDAL(path);
				dal.DeleteValidationFormulas(array);
			});
		}
		else
		{
			await WebApiClient.DeleteProjectValidations(_projectDto.Id, array.Select((Id64 i) => i.Value));
		}
	}

	/// <summary>
	/// 获取（或复用）已加载的项目会话。执行校验与生成新 Id 都依赖会话：
	/// 新 Id 的高 32 位基址必须大于存量 Id（含已删除公式），否则 INSERT OR REPLACE 会顶掉旧记录。
	/// 若编辑器（MainForm）已打开同一项目，直接复用编辑器会话——云端模式重新下载会
	/// File.Delete 编辑器正在使用的 db，本地模式双会话也会读到不一致的数据。
	/// </summary>
	public async Task<Auditai.Model.Project> EnsureSessionAsync(Action<string> progress = null)
	{
		lock (_sessionLock)
		{
			if (_session != null)
			{
				return _session;
			}
		}
		Auditai.Model.Project editorSession = Program.MainForm?.CurrentProject;
		if (editorSession != null && editorSession.Id == _projectDto.Id)
		{
			lock (_sessionLock)
			{
				_session = editorSession;
				return _session;
			}
		}
		progress?.Invoke("正在加载" + _projectDto.Name + "数据，请稍候...");
		Auditai.Model.Project project = await Program.MainForm.OpenProjectDb_DownloadIfNotExist(_projectDto);
		if (project == null)
		{
			return null;
		}
		try
		{
			Tuple<int, int> openResult = await Auditai.LocalDataStore.StorageRouter.OpenProject(_projectDto.Id);
			int maxBase = openResult?.Item1 ?? 0;
			foreach (var node in project.GetAllTreeNodes())
			{
				int nodeBase = (int)((ulong)node.Id.Value >> 32);
				if (nodeBase >= maxBase)
				{
					maxBase = nodeBase + 1;
				}
			}
			foreach (var vf in project.ValidationManager.Formulas)
			{
				int vfBase = (int)((ulong)vf.Id.Value >> 32);
				if (vfBase >= maxBase)
				{
					maxBase = vfBase + 1;
				}
			}
			try
			{
				foreach (Id64 removedId in project.Dal.GetLocalRemovedValidationFormulas())
				{
					int removedBase = (int)((ulong)removedId.Value >> 32);
					if (removedBase >= maxBase)
					{
						maxBase = removedBase + 1;
					}
				}
			}
			catch
			{
			}
			project.SetIdBase(maxBase);
		}
		catch
		{
		}
		lock (_sessionLock)
		{
			_session = project;
			return _session;
		}
	}

	/// <summary>
	/// 新增/编辑/导入规则后调用：内存会话的 ValidationManager 不含刚写入的规则，
	/// 不失效会导致一键校验遗漏新规则。下次执行校验时自动重新加载。
	/// </summary>
	public void InvalidateSession()
	{
		Release();
	}

	/// <summary>
	/// 把稽核页的规则变更同步进已解析的会话内存（典型为复用的编辑器会话）。
	/// 复用场景下若不同步，一键校验仍用编辑器内存中的旧 ValidationManager，
	/// 会遗漏新增规则、仍校验已删规则。仅内存操作：持久化已由 SaveAsync/DeleteAsync 完成。
	/// </summary>
	public void SyncChangesToSession(IEnumerable<ValidationFormula> upserted = null, IEnumerable<Id64> removed = null)
	{
		Auditai.Model.Project session;
		lock (_sessionLock)
		{
			session = _session;
		}
		if (session == null)
		{
			return;
		}
		if (removed != null)
		{
			HashSet<long> removeSet = new HashSet<long>(removed.Select((Id64 i) => i.Value));
			for (int i = session.ValidationManager.Formulas.Count - 1; i >= 0; i--)
			{
				if (removeSet.Contains(session.ValidationManager.Formulas[i].Id.Value))
				{
					session.ValidationManager.Formulas.RemoveAt(i);
				}
			}
		}
		if (upserted != null)
		{
			foreach (ValidationFormula dto in upserted)
			{
				Auditai.Model.ValidationFormula existing = session.ValidationManager.Formulas.FirstOrDefault((Auditai.Model.ValidationFormula f) => f.Id.Value == dto.Id.Value);
				if (existing != null)
				{
					existing.LeftExpr = dto.LeftExpr;
					existing.Operator = Auditai.Model.ValidationOperator.FromCode(dto.Operator);
					existing.RightExpr = dto.RightExpr;
					existing.Note = dto.Note;
					existing.TableId = dto.TableId;
					existing.DocumentFieldId = dto.DocumentFieldId;
					continue;
				}
				session.ValidationManager.Formulas.Add(new Auditai.Model.ValidationFormula
				{
					Id = dto.Id,
					LeftExpr = dto.LeftExpr,
					Operator = Auditai.Model.ValidationOperator.FromCode(dto.Operator),
					RightExpr = dto.RightExpr,
					Note = dto.Note,
					TableId = dto.TableId,
					DocumentFieldId = dto.DocumentFieldId,
					Status = Auditai.Model.SyncStatus.Synced
				});
			}
		}
	}

	/// <summary>空值安全的值转字符串：ValidationManager 对语法错误/不完整规则返回 LeftValue/RightValue=null 的占位结果，
	/// 直接调 ValidationResult.ValueToString 会 NRE。</summary>
	private static string SafeValueToString(object value)
	{
		return value == null ? "" : Auditai.Model.ValidationResult.ValueToString(value);
	}

	/// <summary>执行一键全项目校验：对全部表格校验公式求值（文档域规则跳过，需在编辑器中打开文档校验）。</summary>
	public async Task<List<ValidationOutcome>> RunValidationAsync(Action<string> progress = null)
	{
		Auditai.Model.Project session = await EnsureSessionAsync(progress);
		List<ValidationOutcome> outcomes = new List<ValidationOutcome>();
		if (session == null)
		{
			return outcomes;
		}
		RuleSet rules = await LoadAsync();
		await Task.Run(delegate
		{
			List<Auditai.Model.ValidationFormula> formulas = session.ValidationManager.Formulas
				.Where((Auditai.Model.ValidationFormula f) => f.DocumentFieldId.IsZero())
				.ToList();
			int total = formulas.Count;
			int done = 0;
			foreach (Auditai.Model.ValidationFormula vf in formulas)
			{
				done++;
				progress?.Invoke($"正在校验 {done}/{total}：{vf.LeftExpr}");
				string owner = vf.TableId.IsZero() ? "未指定" : (rules.NodeNames.TryGetValue(vf.TableId.Value, out var name) ? name : ("表格 " + vf.TableId.Value));
				try
				{
					List<Auditai.Model.ValidationResult> list = session.ValidationManager.Validate(vf);
					if (list == null)
					{
						continue;
					}
					foreach (Auditai.Model.ValidationResult r in list)
					{
						// FormulaException 时 ValidationManager 返回 Passed=false 且左右值均为 null 的占位结果，
						// 归类为求值错误，避免与真实的"未通过"（有实际比对值）混淆
						bool isPlaceholder = !r.Passed && r.LeftValue == null && r.RightValue == null;
						outcomes.Add(new ValidationOutcome
						{
							RuleId = vf.Id,
							LeftExpr = vf.LeftExpr,
							OperatorCode = vf.Operator?.Code ?? 0,
							RightExpr = vf.RightExpr,
							Note = vf.Note,
							Owner = owner,
							Passed = r.Passed,
							LeftValue = SafeValueToString(r.LeftValue),
							RightValue = SafeValueToString(r.RightValue),
							RowIndex = r.RowIndex,
							Error = isPlaceholder ? "公式无法求值，请检查表达式语法或规则是否完整" : null
						});
					}
				}
				catch (Exception ex)
				{
					outcomes.Add(new ValidationOutcome
					{
						RuleId = vf.Id,
						LeftExpr = vf.LeftExpr,
						OperatorCode = vf.Operator?.Code ?? 0,
						RightExpr = vf.RightExpr,
						Note = vf.Note,
						Owner = owner,
						Error = ex.Message
					});
				}
			}
		});
		return outcomes;
	}

	/// <summary>生成新规则 Id（依赖会话的 Id 基址，防冲突）</summary>
	public async Task<Id64> NewIdAsync()
	{
		Auditai.Model.Project session = await EnsureSessionAsync();
		if (session == null)
		{
			throw new InvalidOperationException("加载项目数据失败，无法生成规则编号，请检查项目数据或网络后重试");
		}
		return session.GetNextId();
	}

	/// <summary>FormProjectManage 关闭时释放会话引用</summary>
	public void Release()
	{
		lock (_sessionLock)
		{
			_session = null;
		}
	}
}
