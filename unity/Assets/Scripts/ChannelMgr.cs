using UnityEngine;

public class ChannelMgr
{
	private string channelId = "000000";

	private static ChannelMgr instance;

	private ChannelMgr()
	{
	}

	public static ChannelMgr GetInstance()
	{
		if (instance == null)
		{
			instance = new ChannelMgr();
		}
		return instance;
	}

	public void init()
	{
		if (!AndroidScript.HasJavaBridge)
		{
			return;
		}
		using AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.common.libs.Config");
		string jsonValue = androidJavaClass.CallStatic<string>("getConfigInfo", new object[0]);
		JSONObject jSONObject = JSONObject.Parse(jsonValue);
		channelId = jSONObject.Get<string>("channel");
	}

	public bool IsCM()
	{
		if (channelId == "000266")
		{
			return true;
		}
		return false;
	}

	public bool IsCT()
	{
		if (channelId == "000032")
		{
			return true;
		}
		return false;
	}

	public bool IsCU()
	{
		if (channelId == "000056")
		{
			return true;
		}
		return false;
	}

	public bool IsCMM()
	{
		if (channelId == "000013")
		{
			return true;
		}
		return false;
	}

	public string getChannelId()
	{
		return channelId;
	}
}
