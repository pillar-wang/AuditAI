using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using Auditai.EmojiResource.Properties;
using Newtonsoft.Json;

namespace Auditai.EmojiResource;

public class EmojiLib
{
	private class EmojiInfo
	{
		public string Version { get; set; } = string.Empty;
	}

	private static Dictionary<string, Image> _images;

	public static Dictionary<string, Image> GetImages()
	{
		if (_images != null)
		{
			return _images;
		}
		string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		directoryName = Path.Combine(directoryName, "emojis");
		string path = Path.Combine(directoryName, "emoji.json");
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		Version version = Assembly.GetExecutingAssembly().GetName().Version;
		bool flag = true;
		if (File.Exists(path))
		{
			EmojiInfo emojiInfo = JsonConvert.DeserializeObject<EmojiInfo>(File.ReadAllText(path));
			flag = emojiInfo.Version != version.ToString();
		}
		_images = new Dictionary<string, Image>();
		Resource obj = new Resource();
		PropertyInfo[] properties = typeof(Resource).GetProperties(BindingFlags.Static | BindingFlags.NonPublic);
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (!(propertyInfo.GetValue(obj, null) is Bitmap bitmap))
			{
				continue;
			}
			if (flag)
			{
				string text = Path.Combine(directoryName, propertyInfo.Name + ".gif");
				if (File.Exists(text))
				{
					File.Delete(text);
				}
				bitmap.Save(text, ImageFormat.Gif);
			}
			_images.Add(propertyInfo.Name, bitmap);
		}
		string contents = JsonConvert.SerializeObject(new EmojiInfo
		{
			Version = version.ToString()
		});
		File.WriteAllText(path, contents);
		return _images;
	}
}
