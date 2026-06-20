﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Leqisoft.DTO;
using Leqisoft.Util;

namespace Leqisoft.Model;

public class Project
{
	private const int MAX_TABLE_DOC_COUNT = 10000;

	private bool isLoaded;

	internal bool _treeNodeFormulaNamesExpired = true;

	private int _idOffset;

	private int _idBase;

	internal Dictionary<Id64, TreeTableNode> _dicTableNodes = new Dictionary<Id64, TreeTableNode>();

	internal HashSet<Id64> RemovedTreeGroups { get; } = new HashSet<Id64>();


	internal HashSet<Id64> TreeGroupsToDelete { get; } = new HashSet<Id64>();


	internal HashSet<Id64> RemovedTreeNodes { get; } = new HashSet<Id64>();


	internal HashSet<Id64> TreeNodesToDelete { get; } = new HashSet<Id64>();

	internal HashSet<Id64> _removedCrossDocumentRules = new HashSet<Id64>();

	internal HashSet<Id64> _toDeleteCrossDocumentRules = new HashSet<Id64>();


	public static Project Current { get; set; }

	public ProjectDAL Dal { get; set; }

	public Guid Id { get; set; }

	public string Number { get; set; }

	public string Category { get; set; }

	public string Name { get; set; }

	public string Auditee { get; set; }

	public int Version { get; internal set; }

	public bool SystemBuild { get; set; }

	public ChargeType ProjectChargeType { get; set; }

	public DateTime ProjectLicenseDate { get; set; } = DateTime.Now;


	public int ServerVersionOnOpen { get; set; }

	public Dictionary<Leqisoft.DTO.User, UserRole> Users { get; set; }

	public Leqisoft.DTO.User Creator { get; set; }

	public DateTime CreateTime { get; set; }

	public string Note { get; set; }

	public int DemoId { get; set; }

	public List<TreeGroup> TreeGroups { get; } = new List<TreeGroup>();


	public Guid? ParentId { get; set; }

	public ProjectType Kind { get; set; }

	public ChargeType ChargeType { get; set; }

	public bool NeedSave { get; set; }

	public DataReferenceManager DataReferenceManager { get; private set; }

	public CrossProjectFormulaStore FormulaStore { get; private set; }

	public ValidationManager ValidationManager { get; }

	public FormatComplianceChecker FormatComplianceChecker { get; }

	public System.Collections.ObjectModel.ObservableCollection<CrossDocumentValidationRule> CrossDocumentValidationRules { get; } = new System.Collections.ObjectModel.ObservableCollection<CrossDocumentValidationRule>();

	public TableManager TableManager { get; private set; }

	public FileCacheManager FileCacheManager { get; private set; }

	public SnapshotManager SnapshotManager { get; private set; }

	public FormulaManager FormulaManager { get; }

	public bool FormulaMapDirty { get; set; } = true;


	public bool IsNeedSyncDataOnOpen { get; set; } = true;


	public TreeNodeBase LastNode { get; set; }

	public bool AddAllowed => GetAllTreeNodes().Count((TreeNodeBase n) => n is TreeTableNode || n is TreeDocumentNode) < 10000;

	public Project()
	{
		Users = new Dictionary<Leqisoft.DTO.User, UserRole>();
		DataReferenceManager = new DataReferenceManager(this);
		FormulaStore = new CrossProjectFormulaStore(this);
		TableManager = new TableManager(this);
		ValidationManager = new ValidationManager(this);
		FormatComplianceChecker = new FormatComplianceChecker(this);
		FileCacheManager = new FileCacheManager(this);
		SnapshotManager = new SnapshotManager(this);
		FormulaManager = new FormulaManager(this);
	}

	public void SetIdBase(int idBase)
	{
		_idBase = idBase;
		_idOffset = 0;
	}

	public Id64 GetNextId()
	{
		Interlocked.Increment(ref _idOffset);
		return new Id64(_idBase, _idOffset);
	}

	public void PopulateFieldsFromDto(Leqisoft.DTO.Project dto)
	{
		if (dto == null)
		{
			return;
		}
		Id = dto.Id;
		Category = dto.Category;
		Name = dto.Name;
		Note = dto.Note;
		Number = dto.Number;
		Auditee = dto.Auditee;
		Kind = dto.Type;
		ChargeType = dto.ChargeType;
		DemoId = dto.DemoId;
		Users.Clear();
		Creator = dto.Creator;
		CreateTime = dto.CreateTime;
		if (dto.Users != null)
		{
			Users = dto.Users.ToDictionary((Leqisoft.DTO.User u) => u, (Leqisoft.DTO.User u) => u.Role);
		}
		SystemBuild = dto.SystemBuild;
		ProjectChargeType = dto.ProjectChargeType;
		ProjectLicenseDate = dto.ProjectLicenseDate;
	}

	public IEnumerable<TreeNodeBase> GetAllTreeNodes()
	{
		return TreeGroups.SelectMany((TreeGroup group) => group.GetAllNodes());
	}

	public IEnumerable<TreeTableNode> GetAllTableNodes()
	{
		return _dicTableNodes.Values;
	}

	public IEnumerable<TreeDocumentNode> GetAllDocumentNodes()
	{
		return GetAllTreeNodes().OfType<TreeDocumentNode>();
	}

	public IEnumerable<TreeImageNode> GetAllImageNodes()
	{
		return GetAllTreeNodes().OfType<TreeImageNode>();
	}

	public IEnumerable<TreePdfNode> GetAllPdfNodes()
	{
		return GetAllTreeNodes().OfType<TreePdfNode>();
	}

	public TreeGroup AppendTreeGroup()
	{
		NeedSave = true;
		TreeGroup treeGroup = new TreeGroup
		{
			Id = Current.GetNextId(),
			Name = GetNewGroupName(),
			Project = this,
			Status = SyncStatus.New
		};
		TreeGroups.Add(treeGroup);
		FormulaMapDirty = true;
		return treeGroup;
	}

	private void NothingToDoCallback(float progress)
	{
	}

	public void Load(TaskProgressValueReportCallback progressReportCallback = null)
	{
		if (progressReportCallback == null)
		{
			progressReportCallback = NothingToDoCallback;
		}
		if (isLoaded)
		{
			return;
		}
		isLoaded = true;
		Version = Dal.GetProject().Version;
		TreeGroups.Clear();
		RemovedTreeGroups.Clear();
		RemovedTreeNodes.Clear();
		TreeGroupsToDelete.Clear();
		TreeNodesToDelete.Clear();
		foreach (TreeGroup item in Dal.GetTreeGroups().Select(TreeGroup.FromDto))
		{
			item.Project = this;
			TreeGroups.Add(item);
		}
		progressReportCallback(0.2f);
		IEnumerable<TreeNode> treeNodes = Dal.GetTreeNodes();
		var list = treeNodes.Select((TreeNode dto) => new
		{
			dto = dto,
			model = TreeNodeBase.FromDto(dto)
		}).ToList();
		foreach (var item2 in list.Join(TreeGroups, outer => outer.dto.GroupId, inner => inner.Id, (tuple, group) => new { tuple, group }))
		{
			item2.tuple.model.Group = item2.group;
			if (!item2.tuple.dto.ParentId.HasValue)
			{
				item2.group.RootNodes.Add(item2.tuple.model);
			}
			if (item2.tuple.model is TreeTableNode value)
			{
				_dicTableNodes.Add(item2.tuple.model.Id, value);
			}
		}
		progressReportCallback(0.4f);
		foreach (var item3 in list.Join(list, outer => outer.dto.Id, inner => inner.dto.ParentId, (parent, child) => new { parent, child }))
		{
			((TreeDirectoryNode)item3.parent.model).Children.Add(item3.child.model);
			item3.child.model.Parent = (TreeDirectoryNode)item3.parent.model;
		}
		foreach (Id64 localRemovedTreeGroup in Dal.GetLocalRemovedTreeGroups())
		{
			RemovedTreeGroups.Add(localRemovedTreeGroup);
		}
		foreach (Id64 localRemovedTreeNode in Dal.GetLocalRemovedTreeNodes())
		{
			RemovedTreeNodes.Add(localRemovedTreeNode);
		}
		progressReportCallback(0.6f);
		DataReferenceManager.Reset();
		FormulaStore.Load().Wait();
		foreach (Leqisoft.DTO.DataReference dataReference in Dal.GetDataReferences())
		{
			DataReferenceManager._dic.Add(dataReference.Key, new DataReference
			{
				Id = dataReference.Id,
				Key = dataReference.Key,
				Dirty = dataReference.Dirty,
				Status = (SyncStatus)dataReference.Status,
				Value = dataReference.Value,
				Kind = (DataReferenceKind)dataReference.Kind
			});
		}
		foreach (Id64 localRemovedDataReference in Dal.GetLocalRemovedDataReferences())
		{
			DataReferenceManager._removed.Add(localRemovedDataReference);
		}
		ValidationManager.Reset();
		foreach (Leqisoft.DTO.ValidationFormula validationFormula in Dal.GetValidationFormulas())
		{
			ValidationManager.Formulas.Add(ValidationFormula.FromDto(validationFormula));
		}
		foreach (Id64 localRemovedValidationFormula in Dal.GetLocalRemovedValidationFormulas())
		{
			ValidationManager._removed.Add(localRemovedValidationFormula);
		}
		FormatComplianceChecker.Reset();
		foreach (Leqisoft.DTO.FormatComplianceRule formatComplianceRule in Dal.GetFormatComplianceRules())
		{
			FormatComplianceChecker.Rules.Add(FormatComplianceRule.FromDto(formatComplianceRule));
		}
		foreach (Id64 localRemovedFormatComplianceRule in Dal.GetLocalRemovedFormatComplianceRules())
		{
			FormatComplianceChecker._removed.Add(localRemovedFormatComplianceRule);
		}
		CrossDocumentValidationRules.Clear();
		foreach (Leqisoft.DTO.CrossDocumentValidationRule crossDto in Dal.GetCrossDocumentValidationRules())
		{
			CrossDocumentValidationRules.Add(CrossDocumentValidationRule.FromDto(crossDto));
		}
		_removedCrossDocumentRules.Clear();
		foreach (Id64 rid in Dal.GetLocalRemovedCrossDocumentValidationRules())
		{
			_removedCrossDocumentRules.Add(rid);
		}
		_treeNodeFormulaNamesExpired = true;
		progressReportCallback(0.8f);
		FormulaManager.Load();
		foreach (var item4 in list)
		{
			if (item4.model is TreePdfNode treePdfNode)
			{
				treePdfNode.Pdf._isFirstOpened = true;
			}
		}
	}

	public void Save()
	{
		Dal.BeginTransaction();
		Dal.SaveProject(ToDto());
		Dal.SaveTreeGroups(TreeGroups.Select((TreeGroup g) => g.ToDto()));
		Dal.RemoveTreeGroups(RemovedTreeGroups);
		Dal.DeleteTreeGroups(TreeGroupsToDelete);
		TreeGroupsToDelete.Clear();
		Dal.SaveTreeNodes(from n in GetAllTreeNodes()
			select n.ToDto());
		Dal.RemoveTreeNodes(RemovedTreeNodes);
		Dal.DeleteTreeNodes(TreeNodesToDelete);
		TreeNodesToDelete.Clear();
		Dal.SaveDataReferences(from dr in DataReferenceManager.Enumerate()
			where dr.Kind == DataReferenceKind.Text || dr.Kind == DataReferenceKind.CellRef || dr.Kind == DataReferenceKind.CrossProjectCellRef
			select dr.ToDto());
		Dal.RemoveDataReferences(DataReferenceManager._removed);
		Dal.DeleteDataReferences(DataReferenceManager._toDelete);
		DataReferenceManager._toDelete.Clear();
		foreach (var formula in FormulaStore.GetAll())
			FormulaStore.Save(formula).Wait();
		Dal.SaveValidationFormulas(ValidationManager.Formulas.Select((ValidationFormula f) => f.ToDto()));
		Dal.RemoveValidationFormulas(ValidationManager._removed);
		Dal.DeleteValidationFormulas(ValidationManager._toDelete);
		Dal.SaveFormatComplianceRules(FormatComplianceChecker.Rules.Select((FormatComplianceRule r) => r.ToDto()));
		Dal.RemoveFormatComplianceRules(FormatComplianceChecker._removed);
		Dal.DeleteFormatComplianceRules(FormatComplianceChecker._toDelete);
		Dal.SaveCrossDocumentValidationRules(CrossDocumentValidationRules.Select((CrossDocumentValidationRule r) => r.ToDto()));
		Dal.RemoveCrossDocumentValidationRules(_removedCrossDocumentRules);
		Dal.DeleteCrossDocumentValidationRules(_toDeleteCrossDocumentRules);
		Dal.Commit();
		ValidationManager._toDelete.Clear();
		FormatComplianceChecker._toDelete.Clear();
		_toDeleteCrossDocumentRules.Clear();
		NeedSave = false;
	}

	public void ThrowIfMaxExceeded()
	{
		if (!AddAllowed)
		{
			throw new ProjectModelException("表格和文档数量已达上限");
		}
	}

	public Table GetTableById(Id64 id)
	{
		if (!_dicTableNodes.TryGetValue(id, out var value))
		{
			return null;
		}
		return value.Table;
	}

	public TreeNodeBase GetTreeNodeByCanonicalName(string n)
	{
		return GetAllTreeNodes().FirstOrDefault((TreeNodeBase t) => t.FormulaUniqueName == n);
	}

	public Table GetTableByCanonicalName(string n)
	{
		return GetAllTableNodes().FirstOrDefault((TreeTableNode t) => t.FormulaUniqueName == n)?.Table;
	}

	public TreeNodeBase GetNodeById(Id64 id)
	{
		return GetAllTreeNodes().FirstOrDefault((TreeNodeBase n) => n.Id == id);
	}

	public bool IsCurrentUserManager()
	{
		if (!Current.Users.Any((KeyValuePair<Leqisoft.DTO.User, UserRole> kv) => kv.Key.Id == User.Current.Id))
		{
			return false;
		}
		if (Current.Users.First((KeyValuePair<Leqisoft.DTO.User, UserRole> kv) => kv.Key.Id == User.Current.Id).Value == UserRole.Manager || Current.Users.First((KeyValuePair<Leqisoft.DTO.User, UserRole> kv) => kv.Key.Id == User.Current.Id).Value == UserRole.Editor)
		{
			return true;
		}
		return false;
	}

	internal Leqisoft.DTO.Project ToDto()
	{
		return new Leqisoft.DTO.Project
		{
			Id = Id,
			Name = Name,
			ParentId = ParentId,
			Version = Version,
			Number = Number,
			Category = Category,
			Note = Note,
			Auditee = Auditee,
			DemoId = DemoId,
			CreateTime = CreateTime
		};
	}

	internal void SetSynced()
	{
		foreach (TreeGroup treeGroup in TreeGroups)
		{
			treeGroup.SetSynced();
		}
		foreach (Id64 removedTreeGroup in RemovedTreeGroups)
		{
			TreeGroupsToDelete.Add(removedTreeGroup);
		}
		RemovedTreeGroups.Clear();
		foreach (TreeNodeBase allTreeNode in GetAllTreeNodes())
		{
			allTreeNode.SetSynced();
		}
		foreach (Id64 removedTreeNode in RemovedTreeNodes)
		{
			TreeNodesToDelete.Add(removedTreeNode);
		}
		RemovedTreeNodes.Clear();
		foreach (DataReference item in DataReferenceManager.Enumerate())
		{
			item.SetSynced();
		}
		DataReferenceManager._removed.Clear();
		foreach (ValidationFormula formula in ValidationManager.Formulas)
		{
			formula.SetSynced();
		}
		ValidationManager._removed.Clear();
		foreach (FormatComplianceRule rule in FormatComplianceChecker.Rules)
		{
			rule.SetSynced();
		}
		FormatComplianceChecker._removed.Clear();
		foreach (CrossDocumentValidationRule rule in CrossDocumentValidationRules)
		{
			rule.SetSynced();
		}
		_removedCrossDocumentRules.Clear();
	}

	private string GetNewGroupName()
	{
		HashSet<string> hashSet = new HashSet<string>(TreeGroups.Select((TreeGroup g) => g.Name));
		int num = 0;
		do
		{
			num++;
		}
		while (hashSet.Contains($"新建分组 {num}"));
		return $"新建分组 {num}";
	}

	internal void RefreshTreeNodeFormulaNamesIfExpired()
	{
		if (!_treeNodeFormulaNamesExpired)
		{
			return;
		}
		foreach (IGrouping<string, TreeNodeBase> item in from n in GetAllTreeNodes()
			group n by n.GetValidFormulaName())
		{
			List<TreeNodeBase> list = item.ToList();
			if (list.Count == 1)
			{
				list[0].FormulaUniqueName = item.Key;
				continue;
			}
			for (int i = 0; i < list.Count; i++)
			{
				list[i].FormulaUniqueName = $"{item.Key}_{i + 1}";
			}
		}
		_treeNodeFormulaNamesExpired = false;
	}
}
