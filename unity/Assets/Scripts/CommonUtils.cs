using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class CommonUtils
{
	public const string STR_BLANK = "";

	private static System.Random random = new System.Random();

	public static bool IsNumeric(string str)
	{
		Regex regex = new Regex("^[-]?\\d+[.]?\\d*$");
		return regex.IsMatch(str);
	}

	public static byte[] GetFileData(string fileName)
	{
		FileStream fileStream = new FileStream(fileName, FileMode.Open);
		byte[] array = new byte[fileStream.Length];
		fileStream.Read(array, 0, array.Length);
		fileStream.Close();
		return array;
	}

	public static bool IsEmpty(string val)
	{
		try
		{
			return string.IsNullOrEmpty(val);
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.Message);
			return true;
		}
	}

	public static string ToString(int intVal, int toBase)
	{
		return Convert.ToString(intVal, toBase);
	}

	public static bool EqualsIgnoreCase(string src, string target)
	{
		if (src == null || target == null)
		{
			return false;
		}
		return src.ToLower().Equals(target.ToLower());
	}

	public static int ParseInt16(string val)
	{
		if (IsEmpty(val))
		{
			return 0;
		}
		int result = 0;
		try
		{
			result = int.Parse(val, NumberStyles.HexNumber);
		}
		catch
		{
		}
		return result;
	}

	public static int ParseInt(string val)
	{
		if (IsEmpty(val))
		{
			return 0;
		}
		return int.Parse(val);
	}

	public static bool ParseBool(string val)
	{
		return val == "1";
	}

	public static float ParseFloat(string val)
	{
		if (IsEmpty(val))
		{
			return 0f;
		}
		return float.Parse(val);
	}

	public static T GetVal<T>(Dictionary<string, object> dic, string key)
	{
		object obj = default(T);
		if (dic != null && dic.ContainsKey(key))
		{
			object obj2 = dic[key];
			if (obj2 != null)
			{
				string text = obj2.ToString();
				float num = 1f;
				string text2 = true.ToString();
				if (obj != null && !string.IsNullOrEmpty(text))
				{
					if (obj is int)
					{
						obj = ParseInt(text);
					}
					else if (obj is float)
					{
						obj = float.Parse(text);
					}
					else if (obj is bool)
					{
						obj = text == "True";
					}
					else if (obj is DateTime)
					{
						obj = DateTime.Parse(text);
					}
				}
				else
				{
					obj = obj2;
				}
			}
		}
		return (T)obj;
	}
}
