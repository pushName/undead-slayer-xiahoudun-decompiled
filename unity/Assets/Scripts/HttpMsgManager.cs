using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

public class HttpMsgManager : MonoBehaviour
{
	private const int MSG_GETRANK = 10001;

	private const int MSG_REGIST = 10002;

	private const int MSG_CHECK = 10003;

	private const int MSG_OWNRANKING = 10004;

	private const int MSG_GETREWARD = 10005;

	private const int MSG_BEARTHEPALM = 10006;

	private const int MSG_FINALRANKING = 10007;

	private const int MSG_SAVETELNUMBER = 10008;

	private string httpHost = "http://sg.wlss.appget.cn:22000";

	private bool showErrorWindow;

	private string errorString = string.Empty;

	private string userName = string.Empty;

	private string password = string.Empty;

	public Texture2D pop_blank;

	private void Start()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Update()
	{
	}

	public void check(string username)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("msgId", 10003.ToString());
		dictionary.Add("username", username);
		StartCoroutine(POST(httpHost + "/check", dictionary));
	}

	public void regist(string username)
	{
		int num = Crypto.Load_int_key("n24");
		if (num >= 4000)
		{
			AndroidScript.i.showNotice("7");
			return;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("msgId", 10002.ToString());
		dictionary.Add("uid", SystemInfo.deviceUniqueIdentifier);
		string empty = string.Empty;
		empty = ((!(username == string.Empty)) ? username : "夏侯惇");
		dictionary.Add("username", empty);
		dictionary.Add("level", Crypto.Load_int_key("n47").ToString());
		dictionary.Add("fight", getFight().ToString());
		dictionary.Add("lastSatge", Crypto.Load_string_key("lastSatge"));
		dictionary.Add("useTime", Crypto.Load_string_key("useTime"));
		dictionary.Add("highScore", Crypto.Load_string_key("highScore"));
		StartCoroutine(POST(httpHost + "/regist", dictionary));
	}

	public void getRankList()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("msgId", 10001.ToString());
		StartCoroutine(POST(httpHost + "/rank", dictionary));
	}

	public void getOwnRanking()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("msgId", 10004.ToString());
		dictionary.Add("uid", SystemInfo.deviceUniqueIdentifier);
		StartCoroutine(POST(httpHost + "/ownRanking", dictionary));
	}

	private int getFight()
	{
		int num = 0;
		int num2 = Crypto.Load_int_key("n47");
		int num3 = Crypto.Load_int_key("n08");
		int num4 = Crypto.Load_int_key("n28");
		int num5 = Crypto.Load_int_key("n32");
		int num6 = Crypto.Load_int_key("n30");
		int num7 = Crypto.Load_int_key("n21");
		int num8 = Crypto.Load_int_key("n26");
		int num9 = Crypto.Load_int_key("n31");
		int num10 = Crypto.Load_int_key("n04");
		int num11 = Crypto.Load_int_key("n35");
		int num12 = Crypto.Load_int_key("selequip_hp");
		int num13 = 0;
		int[] array = new int[6] { 50, 50, 15, 20, 30, 20 };
		int[] intArray = PlayerPrefsX.GetIntArray("n36");
		for (int i = 0; i < 6; i++)
		{
			if (intArray[i] == 1)
			{
				num13 += array[i];
			}
		}
		int[] array2 = new int[20];
		array2 = PlayerPrefsX.GetIntArray("n22");
		int num14 = 0;
		for (int j = 0; j < array2.Length; j++)
		{
			if (array2[j] >= 0)
			{
				array2[j]++;
				num14 += array2[j];
			}
		}
		return num2 * 25 + (num3 + num4 + num5 + num6 + num7 + num8) * 75 + (num9 + num10) / 2 * 50 + num11 * 100 * 15 + num12 * 35 + num13 * 15 + num14 * 70;
	}

	public void getReward()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("msgId", 10005.ToString());
		dictionary.Add("uid", SystemInfo.deviceUniqueIdentifier);
		StartCoroutine(POST(httpHost + "/reward", dictionary));
	}

	public void getFinalRanking()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("msgId", 10007.ToString());
		dictionary.Add("uid", SystemInfo.deviceUniqueIdentifier);
		StartCoroutine(POST(httpHost + "/finalRanking", dictionary));
	}

	public void bearThePalm()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("msgId", 10006.ToString());
		dictionary.Add("uid", SystemInfo.deviceUniqueIdentifier);
		StartCoroutine(POST(httpHost + "/bearThePalm", dictionary));
	}

	public void saveTelNumber(string telNumber, string username)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("msgId", 10008.ToString());
		dictionary.Add("uid", SystemInfo.deviceUniqueIdentifier);
		dictionary.Add("username", username);
		dictionary.Add("telNumber", telNumber);
		StartCoroutine(POST(httpHost + "/saveTelNumber", dictionary));
	}

	private IEnumerator POST(string url, Dictionary<string, string> post)
	{
		WWWForm form = new WWWForm();
		foreach (KeyValuePair<string, string> post_arg in post)
		{
			form.AddField(post_arg.Key, post_arg.Value);
		}
		WWW www = new WWW(url, form);
		yield return www;
		if (www.error != null)
		{
			Debug.Log("error is :" + www.error);
			AndroidScript.i.showNotice("6");
			yield break;
		}
		JSONNode data = JSON.Parse(www.text);
		int querySuccess = int.Parse(data["querySuccess"].ToString());
		int code = int.Parse(data["code"].ToString());
		if (querySuccess > 0)
		{
			switch (int.Parse(data["msgId"].ToString()))
			{
			case 10001:
			{
				JSONArray arr = data["rankList"].AsArray;
				string[] rankArray = new string[arr.Count];
				for (int i = 0; i < arr.Count; i++)
				{
					rankArray[i] = arr[i].ToString();
				}
				PlayerPrefsX.SetStringArray("n_r_list", rankArray);
				break;
			}
			case 10002:
				PlayerPrefs.SetInt("doRank", 1);
				getRankList();
				getOwnRanking();
				break;
			case 10003:
			{
				int haveUser = data["haveUserName"].AsInt;
				if (haveUser == 1)
				{
					AndroidScript.i.showNotice("0");
					break;
				}
				string username = data["username"].ToString();
				if (string.Empty != username)
				{
					PlayerPrefs.SetString("rankName", username);
					regist(username);
				}
				break;
			}
			case 10004:
			{
				int ownRanking = data["len"].AsInt;
				PlayerPrefs.SetInt("ownRanking", ownRanking);
				break;
			}
			case 10005:
			{
				int gold = data["gold"].AsInt;
				PlayerPrefs.SetInt("rewardGold", gold);
				int jade = data["jade"].AsInt;
				PlayerPrefs.SetInt("rewardJade", jade);
				int haveReward = data["haveReward"].AsInt;
				PlayerPrefs.SetInt("haveReward", haveReward);
				int len = data["len"].AsInt;
				PlayerPrefs.SetInt("finalRanking", len);
				break;
			}
			case 10006:
				Debug.Log(data["result"]);
				break;
			case 10008:
				Debug.Log("Save telnumber success!");
				break;
			}
		}
		else
		{
			errorParse(code);
		}
	}

	private void errorParse(int code)
	{
	}
}
