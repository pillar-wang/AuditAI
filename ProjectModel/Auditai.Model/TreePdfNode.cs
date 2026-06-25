using System;

namespace Auditai.Model;

public class TreePdfNode : TreeNodeBase
{
	public Pdf Pdf { get; } = new Pdf();


	public TreePdfNode()
	{
		Pdf.TreeNode = this;
	}

	protected internal override int GetCode()
	{
		return 3;
	}

	public override void Remove()
	{
		Pdf.LoadAndReturn();
		if (Pdf.LocalExists)
		{
			base.Project.SnapshotManager.SaveSnapshot(Pdf, isDeleting: true);
		}
		base.Remove();
	}

	public TreePdfNode DuplicatePdf()
	{
		Pdf.LoadAndReturn();
		TreePdfNode treePdfNode = new TreePdfNode
		{
			Id = Project.Current.GetNextId(),
			Name = base.Name,
			IsEntityDirty = true,
			Visible = true
		};
		treePdfNode.Pdf._isLoaded = true;
		treePdfNode.Pdf._isFirstOpened = true;
		treePdfNode.Pdf.FileId = Guid.NewGuid();
		if (base.Project.FileCacheManager.Exists(Pdf.FileId))
		{
			Project.Current.FileCacheManager.CopyFrom(base.Project.FileCacheManager.GetPath(Pdf.FileId), treePdfNode.Pdf.FileId);
			return treePdfNode;
		}
		return null;
	}
}
