using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class Crypto
{
	private static string KEY = "anjueolh";

	private static string IV = "45287112549354892144548565456541";

	private static string deviceid = "abcdefgh";

	public static long memberNo;

	public static string usimsIndex = "CT";

	public static int telecomPayCode;

	private static string KEY2 = string.Empty;

	public static void Init()
	{
		deviceid = SystemInfo.deviceUniqueIdentifier;
		deviceid += "somissu2";
		KEY = deviceid.Substring(0, 8);
		string deviceId = AndroidScript.i.getDeviceId();
		deviceid = deviceId;
		deviceid += "somissu2";
		KEY2 = deviceid.Substring(0, 8);
	}

	public static void SetUsim(string index)
	{
		usimsIndex = index;
	}

	public static void SetTelecomPayCode(int _code)
	{
		telecomPayCode = _code;
	}

	public static Rect Rect2(float _posx, float _posy, float _width, float _height)
	{
		return new Rect(_posx * 2f, _posy * 2f, _width * 2f, _height * 2f);
	}

	public static bool Property_change(int _value, bool _jade)
	{
		int num = 0;
		num = ((!_jade) ? Load_int_key("n17") : Load_int_key("n24"));
		if (_value < 0 && num < _value * -1)
		{
			return false;
		}
		if (_jade)
		{
			Save_int_key("n24", num + _value);
		}
		else
		{
			Save_int_key("n17", num + _value);
		}
		return true;
	}

	public static void Save_int_key(string _kind, int _key_int)
	{
		string text = _kind + "hideahidea";
		string value = EncryptString(_key_int.ToString(), KEY + text.Substring(0, 8), IV);
		PlayerPrefs.SetString(_kind, value);
	}

	public static int Load_int_key(string _kind)
	{
		string text = _kind + "hideahidea";
		string str = PlayerPrefs.GetString(_kind);
		string empty = string.Empty;
		try
		{
			empty = DecryptRJ256(Decode(str), KEY + text.Substring(0, 8), IV);
			char[] array = empty.ToCharArray();
			bool flag = false;
			for (int i = 0; i < array.Length; i++)
			{
				char c = char.Parse(array[i].ToString());
				if (c < '!' || c > '~')
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				empty = DecryptRJ256(Decode(str), KEY2 + text.Substring(0, 8), IV);
			}
		}
		catch (Exception)
		{
			empty = DecryptRJ256(Decode(str), KEY2 + text.Substring(0, 8), IV);
		}
		return Convert.ToInt32(empty);
	}

	public static void Save_string_key(string _kind, string _key_string)
	{
		string text = _kind + "hideahidea";
		string value = EncryptString(_key_string, KEY + text.Substring(0, 8), IV);
		PlayerPrefs.SetString(_kind, value);
	}

	public static string Load_string_key(string _kind)
	{
		string text = _kind + "hideahidea";
		string str = PlayerPrefs.GetString(_kind);
		string empty = string.Empty;
		try
		{
			string text2 = DecryptRJ256(Decode(str), KEY + text.Substring(0, 8), IV);
			string s = text2.Substring(0, 1);
			char c = char.Parse(s);
			if (c >= '!' && c <= '~')
			{
				return text2;
			}
			return DecryptRJ256(Decode(str), KEY2 + text.Substring(0, 8), IV);
		}
		catch (Exception)
		{
			return DecryptRJ256(Decode(str), KEY2 + text.Substring(0, 8), IV);
		}
	}

	public static byte[] Decode(string str)
	{
		return Convert.FromBase64String(str);
	}

	public static string DecryptRJ256(byte[] cypher, string KeyString, string IVString)
	{
		UTF8Encoding uTF8Encoding = new UTF8Encoding();
		byte[] bytes = uTF8Encoding.GetBytes(KeyString);
		byte[] bytes2 = uTF8Encoding.GetBytes(IVString);
		string result = string.Empty;
		RijndaelManaged rijndaelManaged = new RijndaelManaged();
		rijndaelManaged.Mode = CipherMode.CBC;
		rijndaelManaged.KeySize = 256;
		rijndaelManaged.BlockSize = 256;
		rijndaelManaged.Key = bytes;
		rijndaelManaged.IV = bytes2;
		try
		{
			MemoryStream stream = new MemoryStream(cypher);
			using CryptoStream cryptoStream = new CryptoStream(stream, rijndaelManaged.CreateDecryptor(bytes, bytes2), CryptoStreamMode.Read);
			using (StreamReader streamReader = new StreamReader(cryptoStream))
			{
				result = streamReader.ReadLine();
			}
			cryptoStream.Close();
			return result;
		}
		finally
		{
			rijndaelManaged.Clear();
		}
	}

	public static string EncryptString(string message, string KeyString, string IVString)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(KeyString);
		byte[] bytes2 = Encoding.UTF8.GetBytes(IVString);
		string result = null;
		RijndaelManaged rijndaelManaged = new RijndaelManaged();
		rijndaelManaged.BlockSize = 256;
		rijndaelManaged.Key = bytes;
		rijndaelManaged.IV = bytes2;
		rijndaelManaged.Mode = CipherMode.CBC;
		try
		{
			MemoryStream memoryStream = new MemoryStream();
			using (CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndaelManaged.CreateEncryptor(bytes, bytes2), CryptoStreamMode.Write))
			{
				using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
				{
					streamWriter.Write(message);
					streamWriter.Close();
				}
				cryptoStream.Close();
			}
			byte[] inArray = memoryStream.ToArray();
			result = Convert.ToBase64String(inArray);
			memoryStream.Close();
		}
		catch (CryptographicException ex)
		{
			Console.WriteLine("A Cryptographic error occurred: {0}", ex.Message);
			return null;
		}
		catch (UnauthorizedAccessException ex2)
		{
			Console.WriteLine("A file error occurred: {0}", ex2.Message);
			return null;
		}
		catch (Exception ex3)
		{
			Console.WriteLine("An error occurred: {0}", ex3.Message);
		}
		finally
		{
			rijndaelManaged.Clear();
		}
		return result;
	}
}
