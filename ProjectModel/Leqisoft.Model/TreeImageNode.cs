using System;

namespace Leqisoft.Model;

public class TreeImageNode : TreeNodeBase
{
	public Image Image { get; } = new Image();


	public TreeImageNode()
	{
		Image.TreeNode = this;
	}

	protected internal override int GetCode()
	{
		return 4;
	}

	public override void Remove()
	{
		Image.LoadAndReturn();
		if (Image.LocalExists)
		{
			base.Project.SnapshotManager.SaveSnapshot(Image, isDeleting: true);
		}
		base.Remove();
	}

	public TreeImageNode DuplicateImage()
	{
		Image.LoadAndReturn();
		TreeImageNode treeImageNode = new TreeImageNode
		{
			Id = Project.Current.GetNextId(),
			Name = base.Name,
			IsEntityDirty = true,
			Visible = true
		};
		treeImageNode.Image._isLoaded = true;
		treeImageNode.Image.NeedSave = true;
		treeImageNode.Image.Center = Image.Center;
		treeImageNode.Image.PageSetup.Deserialize(Image.PageSetup.Serialize());
		treeImageNode.Image.RotateFlip = Image.RotateFlip;
		treeImageNode.Image.ZoomFactor = Image.ZoomFactor;
		treeImageNode.Image.FileId = Guid.NewGuid();
		if (base.Project.FileCacheManager.Exists(Image.FileId))
		{
			Project.Current.FileCacheManager.CopyFrom(base.Project.FileCacheManager.GetPath(Image.FileId), treeImageNode.Image.FileId);
			return treeImageNode;
		}
		return null;
	}
}
