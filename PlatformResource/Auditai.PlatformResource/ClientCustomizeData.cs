using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Auditai.PlatformResource;

public class ClientCustomizeData
{
	protected class BlockData
	{
		public string BlockName { get; private set; }

		public int BlockOffset { get; private set; }

		public int BlockLength { get; private set; }

		public byte EncryptType { get; private set; }

		public BlockData(string blockName, int offset, int length, byte encryptType)
		{
			BlockName = blockName;
			BlockOffset = offset;
			BlockLength = length;
			EncryptType = encryptType;
		}
	}

	public class Color
	{
		public byte Red = 0;

		public byte Green = 0;

		public byte Blue = 0;

		public byte Alpha = 0;

		public Color()
		{
		}

		public Color(byte red, byte green, byte blue)
		{
			Red = red;
			Green = green;
			Blue = blue;
			Alpha = byte.MaxValue;
		}

		public Color(byte red, byte green, byte blue, byte alpha)
		{
			Red = red;
			Green = green;
			Blue = blue;
			Alpha = alpha;
		}
	}

	private Dictionary<string, BlockData> _blockDataDic = new Dictionary<string, BlockData>(StringComparer.OrdinalIgnoreCase);

	private Dictionary<string, string> _settingIniDic = null;

	private string _settingFilePath;

	private static ClientCustomizeData g_instance;

	public Guid ClientTypeId { get; private set; }

	public string Version { get; private set; }

	public string FileMD5 { get; private set; }

	public int TeamType { get; private set; }

	public static ClientCustomizeData Current => g_instance;

	private ClientCustomizeData()
	{
	}

	public byte[] GetFileData(string fileName)
	{
		try
		{
			if (!_blockDataDic.TryGetValue(fileName, out var value))
			{
				return null;
			}
			using FileStream fileStream = new FileStream(_settingFilePath, FileMode.Open, FileAccess.Read);
			fileStream.Seek(value.BlockOffset, SeekOrigin.Begin);
			byte[] array = new byte[value.BlockLength];
			int num = fileStream.Read(array, 0, array.Length);
			if (num != array.Length)
			{
				return null;
			}
			byte encryptType = value.EncryptType;
			byte b = encryptType;
			if (b == 1)
			{
				array = GetDecryptData_InvertByte(array);
			}
			return array;
		}
		catch
		{
			return null;
		}
	}

	public List<string> GetAllFileName()
	{
		return _blockDataDic.Keys.ToList();
	}

	protected string GetSettingOptionValue(string optionId)
	{
		if (_settingIniDic == null)
		{
			byte[] fileData = GetFileData("setting.ini");
			if (fileData == null)
			{
				return null;
			}
			_settingIniDic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			string text = Encoding.UTF8.GetString(fileData);
			string[] array = text.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				string text3 = text2.Trim();
				if (text3.Length == 0)
				{
					continue;
				}
				char c = text3[0];
				if (c == '#' || c == '/' || c == '\\' || c == '[')
				{
					continue;
				}
				int num = text3.IndexOf('=');
				if (num != -1)
				{
					int num2 = text3.Length - num - 1;
					if (num2 != 0)
					{
						string key = text3.Substring(0, num).Trim();
						string value = text3.Substring(num + 1, num2).Trim();
						_settingIniDic[key] = value;
					}
				}
			}
		}
		if (!_settingIniDic.TryGetValue(optionId, out var value2))
		{
			return null;
		}
		return value2;
	}

	public bool IsOptionExistInSettingIniFile(string optionId)
	{
		return GetSettingOptionValue(optionId) != null;
	}

	public bool GetOptionValueInSettingIniFile_Bool(string optionId, bool defaultValue)
	{
		string settingOptionValue = GetSettingOptionValue(optionId);
		if (settingOptionValue == null)
		{
			return defaultValue;
		}
		if (!bool.TryParse(settingOptionValue, out var result))
		{
			return defaultValue;
		}
		return result;
	}

	public int GetOptionValueInSettingIniFile_Int(string optionId, int defaultValue)
	{
		string settingOptionValue = GetSettingOptionValue(optionId);
		if (settingOptionValue == null)
		{
			return defaultValue;
		}
		if (!int.TryParse(settingOptionValue, out var result))
		{
			return defaultValue;
		}
		return result;
	}

	public float GetOptionValueInSettingIniFile_Float(string optionId, float defaultValue)
	{
		string settingOptionValue = GetSettingOptionValue(optionId);
		if (settingOptionValue == null)
		{
			return defaultValue;
		}
		if (!float.TryParse(settingOptionValue, out var result))
		{
			return defaultValue;
		}
		return result;
	}

	public string GetOptionValueInSettingIniFile_String(string optionId, string defaultValue)
	{
		string settingOptionValue = GetSettingOptionValue(optionId);
		if (settingOptionValue == null)
		{
			return defaultValue;
		}
		return settingOptionValue;
	}

	public Color GetOptionValueInSettingIniFile_Color(string optionId, Color defaultValue)
	{
		string settingOptionValue = GetSettingOptionValue(optionId);
		if (settingOptionValue == null)
		{
			return defaultValue;
		}
		string[] array = settingOptionValue.Split(',');
		byte red = byte.MaxValue;
		byte green = byte.MaxValue;
		byte blue = byte.MaxValue;
		byte alpha = byte.MaxValue;
		if (array.Length >= 1 && int.TryParse(array[0].Trim(), out var result))
		{
			red = (byte)result;
		}
		if (array.Length >= 2 && int.TryParse(array[1].Trim(), out var result2))
		{
			green = (byte)result2;
		}
		if (array.Length >= 3 && int.TryParse(array[2].Trim(), out var result3))
		{
			blue = (byte)result3;
		}
		if (array.Length >= 4 && int.TryParse(array[3].Trim(), out var result4))
		{
			alpha = (byte)result4;
		}
		return new Color(red, green, blue, alpha);
	}

	public static string GetSettingFileFullPath()
	{
		string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
		return Path.Combine(baseDirectory, "ClientCustomize.dat");
	}

	public static ClientCustomizeData LoadSettingFile()
	{
		return LoadSettingFile(GetSettingFileFullPath());
	}

	public static ClientCustomizeData LoadSettingFile(string settingFilePath)
	{
		try
		{
			ClientCustomizeData clientCustomizeData = new ClientCustomizeData();
			clientCustomizeData._settingFilePath = settingFilePath;
			using (FileStream stream = new FileStream(settingFilePath, FileMode.Open, FileAccess.Read))
			{
				byte[] outBuffer = new byte[4];
				ReadRawData(stream, outBuffer);
				byte[] array = new byte[16];
				ReadEncryptData(stream, array);
				clientCustomizeData.ClientTypeId = new Guid(array);
				int num = 0;
				byte[] array2 = new byte[4];
				ReadEncryptData(stream, array2);
				num = BitConverter.ToInt32(array2, 0);
				for (int i = 0; i < num; i++)
				{
					int num2 = 0;
					int num3 = 0;
					byte b = 0;
					byte[] array3 = new byte[4];
					ReadEncryptData(stream, array3);
					int num4 = BitConverter.ToInt32(array3, 0);
					array3 = new byte[num4];
					ReadEncryptData(stream, array3);
					string text = Encoding.UTF8.GetString(array3, 0, array3.Length);
					byte[] array4 = new byte[4];
					ReadEncryptData(stream, array4);
					num2 = BitConverter.ToInt32(array4, 0);
					byte[] array5 = new byte[4];
					ReadEncryptData(stream, array5);
					num3 = BitConverter.ToInt32(array5, 0);
					byte[] array6 = new byte[1];
					ReadEncryptData(stream, array6);
					b = array6[0];
					clientCustomizeData._blockDataDic[text] = new BlockData(text, num2, num3, b);
				}
			}
			byte[] fileData = clientCustomizeData.GetFileData("version.txt");
			if (fileData == null)
			{
				clientCustomizeData.Version = string.Empty;
			}
			else
			{
				clientCustomizeData.Version = Encoding.UTF8.GetString(fileData);
			}
			byte[] fileData2 = clientCustomizeData.GetFileData("team_type.txt");
			if (fileData2 == null)
			{
				return null;
			}
			string s = Encoding.UTF8.GetString(fileData2);
			clientCustomizeData.TeamType = int.Parse(s);
			clientCustomizeData.FileMD5 = GetFileMD5(settingFilePath);
			g_instance = clientCustomizeData;
			return clientCustomizeData;
		}
		catch
		{
			return null;
		}
	}

	private static string GetFileMD5(string path)
	{
		MD5 mD = MD5.Create();
		using FileStream inputStream = new FileStream(path, FileMode.Open);
		return BitConverter.ToString(mD.ComputeHash(inputStream)).Replace("-", "").ToUpper();
	}

	private static void ReadEncryptData(FileStream stream, byte[] outBuffer)
	{
		int num = stream.Read(outBuffer, 0, outBuffer.Length);
		if (num != outBuffer.Length)
		{
			throw new Exception();
		}
		for (int i = 0; i < outBuffer.Length; i++)
		{
			outBuffer[i] = (byte)(~outBuffer[i]);
		}
	}

	private static byte[] GetDecryptData_InvertByte(byte[] dataBuffer)
	{
		for (int i = 0; i < dataBuffer.Length; i++)
		{
			dataBuffer[i] = (byte)(~dataBuffer[i]);
		}
		return dataBuffer;
	}

	private static void ReadRawData(FileStream stream, byte[] outBuffer)
	{
		int num = stream.Read(outBuffer, 0, outBuffer.Length);
		if (num != outBuffer.Length)
		{
			throw new Exception();
		}
	}
}
