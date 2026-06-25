using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Auditai.DTO;
using Auditai.Util;

namespace Auditai.Model;

public class FormulaManagerTransitional
{
	private readonly Project _project;

	private readonly List<FormulaHost> _hosts = new List<FormulaHost>();

	private readonly List<FormulaHostValidation> _validationHosts = new List<FormulaHostValidation>();

	public FormulaReferenceModelResolver Resolver { get; }

	public void EvalHostSet(IEnumerable<FormulaHost> hosts, Table onlyThisTable)
	{
		HashSet<FormulaHost> evaling = new HashSet<FormulaHost>();
		HashSet<FormulaHost> evaled = new HashSet<FormulaHost>();
		foreach (FormulaHost host in hosts)
		{
			Eval(host);
		}
		void Eval(FormulaHost host)
		{
			if (!evaling.Contains(host) && !evaled.Contains(host))
			{
				evaling.Add(host);
				foreach (FormulaHost item in _hosts.Where(ShouldEval))
				{
					Eval(item);
				}
				host.Eval();
				evaling.Remove(host);
				evaled.Add(host);
			}
			bool ShouldEval(FormulaHost d)
			{
				if (onlyThisTable != null && d.HostInfo.TableId != onlyThisTable.Id)
				{
					return false;
				}
				try
				{
					bool flag = d.ReferredBy(host);
					if (onlyThisTable != null)
					{
						flag = flag && d.HostInfo.TableId == onlyThisTable.Id;
					}
					return flag;
				}
				catch (FormulaException)
				{
					return false;
				}
			}
		}
	}

	public async Task EvalHostSetAsync(TaskProgressValueUpdater progressValueUpdater)
	{
		HashSet<FormulaHost> evaling = new HashSet<FormulaHost>();
		HashSet<FormulaHost> evaled = new HashSet<FormulaHost>();
		int i = 0;
		int totalCount = _hosts.Count();
		HashSet<Id64> hasEvalTableNameSet = new HashSet<Id64>();
		foreach (FormulaHost host in _hosts)
		{
			await Eval(host);
		}
		async Task Eval(FormulaHost host)
		{
			if (!evaling.Contains(host) && !evaled.Contains(host))
			{
				evaling.Add(host);
				foreach (FormulaHost item in _hosts.Where(ShouldEval))
				{
					await Eval(item);
				}
				string evalHostTableName = GetEvalHostTableName(host);
				if (evalHostTableName != null)
				{
					progressValueUpdater.UpdateMessage("正在运算 " + evalHostTableName);
				}
				host.Eval();
				evaling.Remove(host);
				evaled.Add(host);
				i++;
				progressValueUpdater.UpdateProgress(i, totalCount);
				if (i % 100 == 0)
				{
					await Task.Delay(1);
				}
			}
			bool ShouldEval(FormulaHost d)
			{
				try
				{
					return d.ReferredBy(host);
				}
				catch (FormulaException)
				{
					return false;
				}
			}
		}
		string GetEvalHostTableName(FormulaHost target)
		{
			try
			{
				if (target == null)
				{
					return null;
				}
				if (target.HostInfo == null)
				{
					return null;
				}
				if (hasEvalTableNameSet.Contains(target.HostInfo.TableId))
				{
					return null;
				}
				hasEvalTableNameSet.Add(target.HostInfo.TableId);
				return _project.GetTableById(target.HostInfo.TableId)?.TreeNode.Name;
			}
			catch (Exception)
			{
				return null;
			}
		}
	}

	public void CalculateTableRecursive(Table table)
	{
		List<Tuple<Table, bool>> list = new List<Tuple<Table, bool>>();
		List<Id64> tableIds = Project.Current.FormulaManager.GetTableReferredRecursive(table.Id);
		tableIds.Add(table.Id);
		foreach (Id64 item in tableIds)
		{
			Table tableById = _project.GetTableById(item);
			if (tableById != null)
			{
				tableById.LoadAndReturn();
				ReloadTable(tableById);
				tableById.TryApplyTitleFootFormula();
				list.Add(Tuple.Create(tableById, tableById.EnableFormulaTrigger));
			}
		}
		foreach (Tuple<Table, bool> item2 in list)
		{
			item2.Item1.EnableFormulaTrigger = false;
		}
		EvalHostSet(_hosts.Where((FormulaHost h) => h.HostInfo.TableId == table.Id), null);
		foreach (Tuple<Table, bool> item3 in list)
		{
			item3.Item1.EnableFormulaTrigger = item3.Item2;
		}
	}

	public async Task CalculateAllTables(TaskProgressValueUpdater progressValueUpdater)
	{
		HashSet<Id64> hashSet = new HashSet<Id64>(from n in _project.GetAllTableNodes()
			where n.Visible
			select n.Id);
		foreach (FormulaHost host in _hosts)
		{
			hashSet.Add(host.HostInfo.TableId);
			foreach (FormulaRefInfo refInfo in host.RefInfos)
			{
				hashSet.Add(refInfo.TableId);
			}
		}
		new TaskProgressValueUpdater(0f, 0.5f, progressValueUpdater.UpdateProgress, progressValueUpdater.UpdateMessage);
		TaskProgressValueUpdater evalProgressUpdater = new TaskProgressValueUpdater(0.5f, 0.5f, progressValueUpdater.UpdateProgress, progressValueUpdater.UpdateMessage);
		progressValueUpdater.UpdateMessage("准备开始加载表格...");
		List<Id64> list = hashSet.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			progressValueUpdater.UpdateProgress(i, list.Count * 2);
			Table table = _project.GetTableById(list[i]);
			if (table != null)
			{
				if (!table._loaded)
				{
					await Task.Delay(1);
					progressValueUpdater.UpdateMessage("正在加载 " + table.TreeNode?.Name);
					table.LoadAndReturn();
				}
				ReloadTable(table);
				table.TryApplyTitleFootFormula();
			}
		}
		progressValueUpdater.UpdateMessage("准备开始运算表格...");
		progressValueUpdater.UpdateProgress(0.9f);
		await EvalHostSetAsync(evalProgressUpdater);
	}

	public void CalculateTable(ICollection<Id64> tableIds)
	{
		foreach (FormulaHost host in _hosts)
		{
			tableIds.Add(host.HostInfo.TableId);
			foreach (FormulaRefInfo refInfo in host.RefInfos)
			{
				tableIds.Add(refInfo.TableId);
			}
		}
		List<Id64> list = tableIds.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			Table table = _project.GetTableById(list[i]);
			if (table != null)
			{
				if (!table._loaded)
				{
					table.LoadAndReturn();
				}
				ReloadTable(table);
				table.TryApplyTitleFootFormula();
				EvalHostSet(_hosts.Where((FormulaHost h) => h.HostInfo.TableId == table.Id), table);
			}
		}
	}

	public void CalculateTable(Table table)
	{
		EvalHostSet(_hosts.Where((FormulaHost h) => h.HostInfo.TableId == table.Id), table);
	}

	public void CalculateCells(IEnumerable<Cell> cells)
	{
		HashSet<FormulaHost> marking = new HashSet<FormulaHost>();
		HashSet<FormulaHost> marked = new HashSet<FormulaHost>();
		Table table = cells.First()._Table;
		foreach (Cell cell in cells)
		{
			Mark(new FormulaHostCell
			{
				Host = cell,
				HostInfo = new FormulaRefInfo
				{
					Kind = FormulaHostKind.Cell,
					TableId = table.Id,
					Id1 = cell.Id
				},
				RefInfos = new List<FormulaRefInfo>()
			});
		}
		EvalHostSet(marked, table);
		void Mark(FormulaHost h)
		{
			if (!marking.Contains(h))
			{
				marking.Add(h);
				foreach (FormulaHost item in _hosts.Where((FormulaHost parent) => parent.HostInfo.TableId == table.Id && h.ReferredBy(parent)))
				{
					Mark(item);
				}
				marking.Remove(h);
				marked.Add(h);
			}
		}
	}

	public FormulaManagerTransitional(Project project)
	{
		_project = project;
		Resolver = new FormulaReferenceModelResolver(_project);
	}

	public void Load()
	{
		_hosts.Clear();
		_validationHosts.Clear();
		List<FormulaRecord> list = _project.Dal.GetColumnFormulas().ToList();
		foreach (FormulaRecord item in list)
		{
			AddColumn(item);
		}
		list = _project.Dal.GetHeaderCellFormulas().ToList();
		foreach (FormulaRecord item2 in list)
		{
			AddHeaderCell(item2);
		}
		list = _project.Dal.GetCellFormulas().ToList();
		foreach (FormulaRecord item3 in list)
		{
			AddCell(item3);
		}
		foreach (Auditai.DTO.ValidationFormula validationFormula in _project.Dal.GetValidationFormulas())
		{
			AddVf(validationFormula);
		}
	}

	public FormulaHostCell AddCell(FormulaRecord fr)
	{
		if (string.IsNullOrWhiteSpace(fr.Formula))
		{
			return null;
		}
		try
		{
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(fr.Formula);
			List<FormulaRefInfo> refEntries = formulaEvaluator.GetRefEntries();
			FormulaHostCell formulaHostCell = new FormulaHostCell
			{
				HostInfo = new FormulaRefInfo
				{
					Kind = FormulaHostKind.Cell,
					TableId = fr.TableId,
					Id1 = fr.ObjectId
				},
				RefInfos = refEntries
			};
			_hosts.Add(formulaHostCell);
			return formulaHostCell;
		}
		catch (FormulaException)
		{
			return null;
		}
	}

	public FormulaHostHeaderCell AddHeaderCell(FormulaRecord fr)
	{
		if (string.IsNullOrWhiteSpace(fr.Formula))
		{
			return null;
		}
		try
		{
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(fr.Formula);
			List<FormulaRefInfo> refEntries = formulaEvaluator.GetRefEntries();
			FormulaHostHeaderCell formulaHostHeaderCell = new FormulaHostHeaderCell
			{
				HostInfo = new FormulaRefInfo
				{
					Kind = FormulaHostKind.HeaderCell,
					TableId = fr.TableId,
					Id1 = fr.ObjectId
				},
				RefInfos = refEntries
			};
			_hosts.Add(formulaHostHeaderCell);
			return formulaHostHeaderCell;
		}
		catch (FormulaException)
		{
			return null;
		}
	}

	public FormulaHostColumn AddColumn(FormulaRecord fr)
	{
		if (string.IsNullOrWhiteSpace(fr.Formula))
		{
			return null;
		}
		try
		{
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(fr.Formula);
			List<FormulaRefInfo> refEntries = formulaEvaluator.GetRefEntries();
			FormulaHostColumn formulaHostColumn = new FormulaHostColumn
			{
				HostInfo = new FormulaRefInfo
				{
					Kind = FormulaHostKind.Column,
					TableId = fr.TableId,
					Id1 = fr.ObjectId
				},
				RefInfos = refEntries
			};
			_hosts.Add(formulaHostColumn);
			return formulaHostColumn;
		}
		catch (FormulaException)
		{
			return null;
		}
	}

	public void AddVf(Auditai.DTO.ValidationFormula vf)
	{
		List<FormulaRefInfo> vfRefs = GetVfRefs(vf, vf.LeftExpr);
		vfRefs.AddRange(GetVfRefs(vf, vf.RightExpr));
		FormulaHostValidation formulaHostValidation = new FormulaHostValidation
		{
			HostInfo = new FormulaRefInfo
			{
				Kind = FormulaHostKind.Validation,
				TableId = vf.TableId,
				Id1 = vf.Id
			},
			RefInfos = vfRefs
		};
	}

	private List<FormulaRefInfo> GetVfRefs(Auditai.DTO.ValidationFormula vf, string leftOrRight)
	{
		if (string.IsNullOrWhiteSpace(leftOrRight))
		{
			return new List<FormulaRefInfo>();
		}
		try
		{
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(leftOrRight);
			return formulaEvaluator.GetRefEntries();
		}
		catch (FormulaException)
		{
			return new List<FormulaRefInfo>();
		}
	}

	public void RemoveHostObject(Id64 table, Id64 obj)
	{
		_hosts.RemoveAll((FormulaHost h) => h.HostInfo.TableId == table && h.HostInfo.Id1 == obj);
	}

	public void RemoveHostTable(Id64 table)
	{
		_hosts.RemoveAll((FormulaHost h) => h.HostInfo.TableId == table);
	}

	public void ReplaceHostCell(FormulaRecord fr, Table table)
	{
		RemoveHostObject(fr.TableId, fr.ObjectId);
		FormulaHostCell formulaHostCell = AddCell(fr);
	}

	public void ReplaceHostHeaderCell(FormulaRecord fr, Table table)
	{
		RemoveHostObject(fr.TableId, fr.ObjectId);
		FormulaHostHeaderCell formulaHostHeaderCell = AddHeaderCell(fr);
	}

	public void ReplaceHostColumn(FormulaRecord fr, Table table)
	{
		RemoveHostObject(fr.TableId, fr.ObjectId);
		FormulaHostColumn formulaHostColumn = AddColumn(fr);
	}

	public void ReplaceHostValidation(ValidationFormula vf)
	{
		RemoveHostObject(vf.TableId, vf.Id);
		AddVf(vf.ToDto());
	}

	public void SetHostTitleCell(FormulaRefInfo hostInfo, string formula, Table table)
	{
		_hosts.RemoveAll((FormulaHost h) => h.HostInfo.Kind == FormulaHostKind.TitleCell && h.HostInfo.TableId == hostInfo.TableId && h.HostInfo.Int1 == hostInfo.Int1 && h.HostInfo.Int2 == hostInfo.Int2);
		if (!string.IsNullOrEmpty(formula))
		{
			try
			{
				FormulaEvaluator formulaEvaluator = new FormulaEvaluator(formula);
				List<FormulaRefInfo> refEntries = formulaEvaluator.GetRefEntries();
				FormulaHostTitleCell item = new FormulaHostTitleCell
				{
					HostInfo = hostInfo,
					RefInfos = refEntries
				};
				_hosts.Add(item);
			}
			catch (FormulaException)
			{
			}
		}
	}

	public void ReloadTable(Table table)
	{
		if (table == null)
		{
			return;
		}
		RemoveHostTable(table.Id);
		foreach (Column column in table.Columns)
		{
			if (column.HasFormula)
			{
				AddColumn(new FormulaRecord
				{
					Formula = column.Formula,
					TableId = table.Id,
					ObjectId = column.Id
				});
			}
		}
		foreach (Cell cell in table.Cells)
		{
			if (cell.HasHeaderFormula)
			{
				AddHeaderCell(new FormulaRecord
				{
					Formula = cell.HeaderFormula,
					TableId = table.Id,
					ObjectId = cell.Id
				});
			}
			if (cell.HasFormula)
			{
				AddCell(new FormulaRecord
				{
					Formula = cell.Formula,
					TableId = table.Id,
					ObjectId = cell.Id
				});
			}
		}
	}

	public IEnumerable<FormulaHost> GetRowsReferrer(IEnumerable<Row> rows)
	{
		return from h in _hosts.Concat(_validationHosts)
			where rows.Any((Row r) => h.DependsOnRow(r))
			select h;
	}

	public IEnumerable<FormulaHost> GetColumnsReferrer(IEnumerable<Column> columns)
	{
		return from h in _hosts.Concat(_validationHosts)
			where columns.Any((Column c) => h.DependsOnColumn(c))
			select h;
	}

	public IEnumerable<FormulaHost> GetTableReferrer(Table table)
	{
		return from h in _hosts.Concat(_validationHosts)
			where h.DependsOnTable(table)
			select h;
	}

	public FormulaGraph ToGraph(Color themeColor, Color dark, bool showNumber)
	{
		Dictionary<TreeTableNode, GraphNode> _dicToGraph = new Dictionary<TreeTableNode, GraphNode>();
		FormulaGraph graph = new FormulaGraph
		{
			ThemeColor = themeColor,
			DarkColor = dark,
			ShowNumber = showNumber
		};
		foreach (TreeGroup treeGroup in _project.TreeGroups)
		{
			GraphGroup graphGroup = new GraphGroup
			{
				TreeGroup = treeGroup
			};
			foreach (TreeNodeBase rootNode in treeGroup.RootNodes)
			{
				if (rootNode is TreeDirectoryNode d2)
				{
					GraphDirectory graphDirectory = AddGraphDirectory(d2);
					graphDirectory.Parent = null;
					graphGroup.Children.Add(graphDirectory);
					continue;
				}
				GraphNode graphNode = graph.CreateNode();
				graphNode.Parent = null;
				graphNode.GraphGroup = graphGroup;
				graphNode.ModelNode = rootNode;
				graphGroup.Children.Add(graphNode);
				if (rootNode is TreeTableNode key)
				{
					_dicToGraph.Add(key, graphNode);
				}
			}
			graph.Groups.Add(graphGroup);
		}
		HashSet<FormulaHost> hashSet = new HashSet<FormulaHost>(_hosts.Where((FormulaHost d) => d.RefInfos.Any((FormulaRefInfo r) => r.TableId != d.HostInfo.TableId)));
		foreach (FormulaHost dep in hashSet)
		{
			Table tableById = _project.GetTableById(dep.HostInfo.TableId);
			foreach (FormulaRefInfo item in dep.RefInfos.Where((FormulaRefInfo r) => r.TableId != dep.HostInfo.TableId))
			{
				Table tableById2 = _project.GetTableById(item.TableId);
				if (tableById != null && tableById2 != null && tableById != tableById2)
				{
					graph.Edges.Add(Tuple.Create(_dicToGraph[tableById.TreeNode], _dicToGraph[tableById2.TreeNode]));
				}
			}
		}
		return graph;
		GraphDirectory AddGraphDirectory(TreeDirectoryNode d)
		{
			GraphDirectory graphDirectory2 = graph.CreateDirectoryNode();
			graphDirectory2.IsExpanded = false;
			graphDirectory2.ModelNode = d;
			foreach (TreeNodeBase child in d.Children)
			{
				if (child is TreeDirectoryNode d3)
				{
					GraphDirectory graphDirectory3 = AddGraphDirectory(d3);
					graphDirectory3.Parent = graphDirectory2;
					graphDirectory2.Children.Add(graphDirectory3);
				}
				else
				{
					GraphNode graphNode2 = graph.CreateNode();
					graphNode2.Parent = graphDirectory2;
					graphNode2.ModelNode = child;
					graphDirectory2.Children.Add(graphNode2);
					if (child is TreeTableNode key2)
					{
						_dicToGraph.Add(key2, graphNode2);
					}
				}
			}
			return graphDirectory2;
		}
	}
}
