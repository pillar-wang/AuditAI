using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Auditai.Model;

namespace Auditai.UI.Platform;

internal static class TreeNodeEx
{
	public static string GetRelativePath(this TreeNodeBase treenode)
	{
		List<string> list = new List<string>();
		if (treenode is TreeDirectoryNode treeDirectoryNode)
		{
			list.Add(treeDirectoryNode.Name);
		}
		for (TreeDirectoryNode parent = treenode.Parent; parent != null; parent = parent.Parent)
		{
			list.Add(parent.Name);
		}
		if (treenode.Group != null)
		{
			list.Add(treenode.Group.Name);
		}
		list.Reverse();
		string path = string.Join("/", list);
		string fileName = treenode.GetFileName();
		return Path.Combine(path, fileName);
	}

	public static string GetFileName(this TreeNodeBase treeNode)
	{
		return RemoveInvalidChars(treeNode.Name) + treeNode.GetExtension();
	}

	private static string GetExtension(this TreeNodeBase treeNode)
	{
		if (!(treeNode is TreeDirectoryNode))
		{
			if (!(treeNode is TreeTableNode))
			{
				if (!(treeNode is TreeDocumentNode))
				{
					if (!(treeNode is TreeImageNode treeImageNode))
					{
						if (treeNode is TreePdfNode)
						{
							return ".pdf";
						}
						throw new ArgumentOutOfRangeException("未识别的节点类型");
					}
					try
					{
						return GetExtension(treeImageNode.Image.GetGraphicsImage());
					}
					catch (Exception)
					{
						return ".jpg";
					}
				}
				return ".docx";
			}
			return ".xlsx";
		}
		return string.Empty;
	}

	private static string GetExtension(System.Drawing.Image image)
	{
		if (image.RawFormat.Equals(ImageFormat.Bmp))
		{
			return ".bmp";
		}
		if (image.RawFormat.Equals(ImageFormat.Gif))
		{
			return ".gif";
		}
		if (image.RawFormat.Equals(ImageFormat.Jpeg))
		{
			return ".jpeg";
		}
		if (image.RawFormat.Equals(ImageFormat.Png))
		{
			return ".png";
		}
		if (image.RawFormat.Equals(ImageFormat.Tiff))
		{
			return ".tiff";
		}
		if (image.RawFormat.Equals(ImageFormat.Icon))
		{
			return ".icon";
		}
		if (image.RawFormat.Equals(ImageFormat.Emf))
		{
			return ".emf";
		}
		if (image.RawFormat.Equals(ImageFormat.Exif))
		{
			return ".exif";
		}
		if (image.RawFormat.Equals(ImageFormat.Wmf))
		{
			return ".wmf";
		}
		throw new ArgumentOutOfRangeException("无法识别文件类型");
	}

	private static string RemoveInvalidChars(string path)
	{
		char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
		foreach (char c in invalidFileNameChars)
		{
			path.Replace(c.ToString(), string.Empty);
		}
		return path.Replace("\r", "").Replace("\n", "").Replace("/", "_")
			.Replace("\\", "_")
			.Trim();
	}
}
