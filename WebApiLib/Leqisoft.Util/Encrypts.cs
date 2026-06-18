﻿using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Leqisoft.Util;

public static class Encrypts
{
	private static readonly RNGCryptoServiceProvider Rng = new RNGCryptoServiceProvider();

	public static string NewAESKeyBase64()
	{
		byte[] array = new byte[16];
		Rng.GetBytes(array);
		return Convert.ToBase64String(array);
	}

	public static string CreateBase64Key(string str)
	{
		byte[] bytes = Encoding.Default.GetBytes(str);
		byte[] array = new byte[16];
		for (int i = 0; i < array.Length && i < bytes.Length; i++)
		{
			array[i] = bytes[i];
		}
		return Convert.ToBase64String(array);
	}

	public static void NewRSAKeyBase64(out string publicKeyBase64, out string privateKeyBase64)
	{
		RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
		publicKeyBase64 = Convert.ToBase64String(rSACryptoServiceProvider.ExportCspBlob(includePrivateParameters: false));
		privateKeyBase64 = Convert.ToBase64String(rSACryptoServiceProvider.ExportCspBlob(includePrivateParameters: true));
	}

	public static byte[] AesEncrypt(byte[] value, string keyBase64)
	{
		byte[] key = Convert.FromBase64String(keyBase64);
		RijndaelManaged rijndaelManaged = new RijndaelManaged
		{
			Key = key,
			Mode = CipherMode.ECB,
			Padding = PaddingMode.PKCS7
		};
		ICryptoTransform cryptoTransform = rijndaelManaged.CreateEncryptor();
		return cryptoTransform.TransformFinalBlock(value, 0, value.Length);
	}

	public static byte[] AesDecrypt(byte[] value, string keyBase64)
	{
		byte[] key = Convert.FromBase64String(keyBase64);
		RijndaelManaged rijndaelManaged = new RijndaelManaged
		{
			Key = key,
			Mode = CipherMode.ECB,
			Padding = PaddingMode.PKCS7
		};
		ICryptoTransform cryptoTransform = rijndaelManaged.CreateDecryptor();
		return cryptoTransform.TransformFinalBlock(value, 0, value.Length);
	}

	public static byte[] RSAEncrypt(byte[] value, string publicKeyBase64)
	{
		RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
		rSACryptoServiceProvider.ImportCspBlob(Convert.FromBase64String(publicKeyBase64));
		return rSACryptoServiceProvider.Encrypt(value, fOAEP: false);
	}

	public static byte[] RSADecrypt(byte[] value, string privateKeyBase64)
	{
		RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
		rSACryptoServiceProvider.ImportCspBlob(Convert.FromBase64String(privateKeyBase64));
		return rSACryptoServiceProvider.Decrypt(value, fOAEP: false);
	}

	public static string CreateSalt()
	{
		Random random = new Random(DateTime.Now.Millisecond);
		return random.Next(10000000, 99999999).ToString();
	}

	public static string SHA256Encrypt(string str, bool isUrl)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(str);
		using SHA256Cng sHA256Cng = new SHA256Cng();
		byte[] inArray = sHA256Cng.ComputeHash(bytes);
		string text = Convert.ToBase64String(inArray);
		if (!isUrl)
		{
			return text;
		}
		return HttpUtility.UrlEncode(text);
	}

	public static string MD5Encrypt(string value)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(value);
		using MD5Cng mD5Cng = new MD5Cng();
		byte[] inArray = mD5Cng.ComputeHash(bytes);
		return Convert.ToBase64String(inArray);
	}

	public static string AESEncryptDeal(this string value, string keyBase64)
	{
		return Convert.ToBase64String(AesEncrypt(CompressHelper.Compress(Encoding.UTF8.GetBytes(value)), keyBase64));
	}

	public static string AESDecryptDeal(this string value, string keyBase64)
	{
		return Encoding.UTF8.GetString(CompressHelper.Decompress(AesDecrypt(Convert.FromBase64String(value.Replace(" ", "+")), keyBase64)));
	}

	public static string RSAEncriptDeal(this string str, string publicKeyBase64)
	{
		byte[] inArray = RSAEncrypt(new UnicodeEncoding().GetBytes(str), publicKeyBase64);
		return Convert.ToBase64String(inArray);
	}

	public static string RSADecryptDeal(this string base64Str, string privateKeyBase64)
	{
		byte[] bytes = RSADecrypt(Convert.FromBase64String(base64Str), privateKeyBase64);
		return new UnicodeEncoding().GetString(bytes);
	}

	public static string ToBase64(this string Str)
	{
		byte[] bytes = Encoding.Default.GetBytes(Str);
		return Convert.ToBase64String(bytes);
	}

	public static string ToStr(this string Base64)
	{
		byte[] bytes = Convert.FromBase64String(Base64);
		return Encoding.Default.GetString(bytes);
	}
}
