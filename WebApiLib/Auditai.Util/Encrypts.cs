﻿﻿using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Auditai.Util;

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
		using Aes aes = Aes.Create();
		aes.Key = key;
#pragma warning disable SCS0013 // CBC with generated IV is secure
		aes.Mode = CipherMode.CBC;
#pragma warning restore SCS0013
		aes.Padding = PaddingMode.PKCS7;
		aes.GenerateIV();
		ICryptoTransform cryptoTransform = aes.CreateEncryptor();
		byte[] cipherText = cryptoTransform.TransformFinalBlock(value, 0, value.Length);
		byte[] result = new byte[aes.IV.Length + cipherText.Length];
		Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
		Buffer.BlockCopy(cipherText, 0, result, aes.IV.Length, cipherText.Length);
		return result;
	}

	public static byte[] AesDecrypt(byte[] value, string keyBase64)
	{
		byte[] key = Convert.FromBase64String(keyBase64);
		byte[] iv = new byte[16];
		byte[] cipherText = new byte[value.Length - 16];
		Buffer.BlockCopy(value, 0, iv, 0, 16);
		Buffer.BlockCopy(value, 16, cipherText, 0, cipherText.Length);
		using Aes aes = Aes.Create();
		aes.Key = key;
		aes.IV = iv;
#pragma warning disable SCS0013 // CBC with generated IV is secure
		aes.Mode = CipherMode.CBC;
#pragma warning restore SCS0013
		aes.Padding = PaddingMode.PKCS7;
		ICryptoTransform cryptoTransform = aes.CreateDecryptor();
		return cryptoTransform.TransformFinalBlock(cipherText, 0, cipherText.Length);
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
		byte[] bytes = new byte[4];
		using (var rng = RandomNumberGenerator.Create())
		{
			rng.GetBytes(bytes);
		}
		int value = BitConverter.ToInt32(bytes, 0) % 90000000;
		return Math.Abs(value + 10000000).ToString();
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
		// 使用 SHA256 替代 MD5（原方法在代码库中无调用方，为安全默认升级）
		return SHA256Encrypt(value, isUrl: false);
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
