using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Auditai.DTO;

namespace Auditai.Model;

public class FormulaManager
{
	private readonly Project _project;

	private readonly List<FormulaDependency> _dependencies = new List<FormulaDependency>();

	public FormulaReferenceModelResolver Resolver { get; }

	public FormulaManager(Project project)
	{
		_project = project;
		Resolver = new FormulaReferenceModelResolver(_project);
	}

	public void Load()
	{
		_dependencies.Clear();
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

	public void AddCell(FormulaRecord fr)
	{
		AddFormulaDependency(fr.Formula, FormulaDependencyObjectKind.Cell, fr.TableId, fr.ObjectId);
	}

	public void AddHeaderCell(FormulaRecord fr)
	{
		AddFormulaDependency(fr.Formula, FormulaDependencyObjectKind.HeaderCell, fr.TableId, fr.ObjectId);
	}

	public void AddColumn(FormulaRecord fr)
	{
		AddFormulaDependency(fr.Formula, FormulaDependencyObjectKind.Column, fr.TableId, fr.ObjectId);
	}

	public void AddVf(Auditai.DTO.ValidationFormula vf)
	{
		AddVfImpl(vf, vf.LeftExpr);
		AddVfImpl(vf, vf.RightExpr);
	}

	private void AddVfImpl(Auditai.DTO.ValidationFormula vf, string leftOrRight)
	{
		AddFormulaDependency(leftOrRight, FormulaDependencyObjectKind.ValidationFormula, vf.TableId, vf.Id);
	}

	private void AddFormulaDependency(string formula, FormulaDependencyObjectKind hostKind, Id64 hostTable, Id64 hostObject)
	{
		if (string.IsNullOrWhiteSpace(formula))
		{
			return;
		}
		try
		{
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(formula);
			List<Tuple<Id64, Id64, FormulaDependencyObjectKind>> refIds = formulaEvaluator.GetRefIds();
			foreach (Tuple<Id64, Id64, FormulaDependencyObjectKind> item in refIds)
			{
				_dependencies.Add(new FormulaDependency
				{
					HostKind = hostKind,
					HostTable = hostTable,
					HostObject = hostObject,
					ReferredKind = item.Item3,
					ReferredTable = item.Item1,
					ReferredObject = item.Item2
				});
			}
		}
		catch (FormulaException)
		{
		}
	}

	public void RemoveHostObject(Id64 table, Id64 obj)
	{
		_dependencies.RemoveAll((FormulaDependency d) => d.HostTable == table && d.HostObject == obj);
	}

	public void RemoveHostTable(Id64 table)
	{
		_dependencies.RemoveAll((FormulaDependency d) => d.HostTable == table);
	}

	public void ReplaceHostCell(FormulaRecord fr)
	{
		RemoveHostObject(fr.TableId, fr.ObjectId);
		AddCell(fr);
	}

	public void ReplaceHostHeaderCell(FormulaRecord fr)
	{
		RemoveHostObject(fr.TableId, fr.ObjectId);
		AddHeaderCell(fr);
	}

	public void ReplaceHostColumn(FormulaRecord fr)
	{
		RemoveHostObject(fr.TableId, fr.ObjectId);
		AddColumn(fr);
	}

	public void ReplaceHostValidation(ValidationFormula vf)
	{
		RemoveHostObject(vf.TableId, vf.Id);
		AddVf(vf.ToDto());
	}

	public IEnumerable<FormulaDependency> GetCellsReferrer(Id64 table, IEnumerable<Id64> cells)
	{
		return _dependencies.Where((FormulaDependency d) => d.ReferredTable == table && d.ReferredKind == FormulaDependencyObjectKind.Cell && cells.Contains(d.ReferredObject));
	}

	public IEnumerable<FormulaDependency> GetColumnsReferrer(Id64 table, IEnumerable<Id64> columns)
	{
		return _dependencies.Where((FormulaDependency d) => d.ReferredTable == table && d.ReferredKind == FormulaDependencyObjectKind.Column && columns.Contains(d.ReferredObject));
	}

	public IEnumerable<Id64> GetTableReferredDirect(Id64 table)
	{
		return from d in _dependencies
			where (d.HostKind == FormulaDependencyObjectKind.Cell || d.HostKind == FormulaDependencyObjectKind.Column) && d.HostTable == table && d.ReferredTable != table
			select d.ReferredTable;
	}

	public List<Id64> GetTableReferredRecursive(Id64 table)
	{
		List<Id64> walked = new List<Id64>();
		GetTableReferredRecursiveImpl(table);
		walked.Reverse();
		return walked;
		void GetTableReferredRecursiveImpl(Id64 t)
		{
			IEnumerable<Id64> tableReferredDirect = GetTableReferredDirect(t);
			foreach (Id64 item in tableReferredDirect)
			{
				if (!walked.Contains(item))
				{
					walked.Add(item);
					GetTableReferredRecursiveImpl(item);
				}
			}
		}
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
		HashSet<FormulaDependency> hashSet = new HashSet<FormulaDependency>(_dependencies.Where((FormulaDependency d) => d.HostKind != FormulaDependencyObjectKind.ValidationFormula && d.HostTable != d.ReferredTable), FormulaDependencyComparer.Instance);
		foreach (FormulaDependency item in hashSet)
		{
			Table tableById = _project.GetTableById(item.HostTable);
			Table tableById2 = _project.GetTableById(item.ReferredTable);
			if (tableById != null && tableById2 != null && tableById != tableById2)
			{
				graph.Edges.Add(Tuple.Create(_dicToGraph[tableById.TreeNode], _dicToGraph[tableById2.TreeNode]));
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
