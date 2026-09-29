using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using UnityEngine;

public class JSONObject
{
	private Dictionary<string, object> dictionary;

	public string hashtableStr => Json.Serialize(dictionary);

	public JSONObject()
	{
		Init();
	}

	public JSONObject(string jsonString)
	{
		ParseString(jsonString);
	}

	private void Init()
	{
		dictionary = new Dictionary<string, object>();
	}

	private void ParseString(string jsonString)
	{
		object obj = Json.Deserialize(jsonString);
		if (obj == null)
		{
			Init();
		}
		if (obj is Dictionary<string, object> || obj is ArrayList)
		{
		}
		dictionary = obj as Dictionary<string, object>;
	}

	public void Add(string key, object val)
	{
		if (key == null)
		{
			Debug.LogError("[JSONOjbect.Add] : the key cannot be null !");
		}
		else
		{
			dictionary[key] = val;
		}
	}

	public T Get<T>(string key)
	{
		return CommonUtils.GetVal<T>(dictionary, key);
	}

	public bool ContainKey(string key)
	{
		if (key == null)
		{
			return false;
		}
		return dictionary.ContainsKey(key);
	}

	public override string ToString()
	{
		return hashtableStr;
	}

	public static JSONObject Parse(string jsonValue)
	{
		return new JSONObject(jsonValue);
	}
}
