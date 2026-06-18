namespace Leqisoft.Model;

public class TreeDocumentNode : TreeNodeBase
{
	public Document Document { get; } = new Document();


	public TreeDocumentNode()
	{
		Document.TreeNode = this;
	}

	public TreeDocumentNode DuplicateDocument()
	{
		Document.LoadAndReturn();
		TreeDocumentNode treeDocumentNode = new TreeDocumentNode
		{
			Id = Project.Current.GetNextId(),
			Name = base.Name,
			IsEntityDirty = true,
			Visible = true
		};
		foreach (Paragraph paragraph2 in Document.Paragraphs)
		{
			Paragraph paragraph = paragraph2.Duplicate();
			paragraph.Document = treeDocumentNode.Document;
			treeDocumentNode.Document.Paragraphs.Add(paragraph);
		}
		treeDocumentNode.Document.SectPr = Document.SectPr;
		treeDocumentNode.Document.MergeTable = Document.MergeTable;
		treeDocumentNode.Document.FromDuplicationButNotSaved = true;
		treeDocumentNode.Document._isLoaded = true;
		return treeDocumentNode;
	}

	protected internal override int GetCode()
	{
		return 2;
	}

	public override void Remove()
	{
		Document.LoadAndReturn();
		if (Document.LocalExists)
		{
			base.Project.SnapshotManager.SaveSnapshot(Document, isDeleting: true);
		}
		base.Remove();
	}
}
