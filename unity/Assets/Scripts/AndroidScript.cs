using UnityEngine;

public class AndroidScript : MonoBehaviour
{
	private static AndroidScript _i;

	private AndroidJavaClass jcBI;

	public static AndroidScript i => _i;

	// Every AndroidJavaClass in this file targets a class shipped inside the original APK, and the
	// bridge only exists in an Android player -- on desktop the constructor throws. Only the
	// startup path degrades through this one predicate (see Start/getDeviceId and ChannelMgr.init);
	// user-triggered calls (pay, LogEvent, showNotice, QuitGame) deliberately stay unguarded so a
	// missing SDK shows up as an error instead of silently doing nothing.
	public static bool HasJavaBridge => Application.platform == RuntimePlatform.Android;

	private void Awake()
	{
		_i = this;
		string text = PlayerPrefs.GetString("isFirst", "NONE");
		if (text.CompareTo("NONE") == 0)
		{
			PlayerPrefs.SetString("isFirst", "YES");
		}
	}

	private void Start()
	{
		Object.DontDestroyOnLoad(base.gameObject);
		if (!HasJavaBridge)
		{
			return;
		}
		jcBI = new AndroidJavaClass("com.bi.libs.BIWrapper");
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (Time.timeScale == 0f || Application.loadedLevel == 0)
			{
				return;
			}
			int num = PlayerPrefs.GetInt("IsInit");
			int num2 = 0;
			int num3 = 0;
			string eventId = "exitGameEvent";
			if (num == 1)
			{
				num3 = Crypto.Load_int_key("n24");
				num2 = Crypto.Load_int_key("n06");
			}
			if (num2 < 0)
			{
				num2 = 0;
			}
			string jsonValue = "{jade:" + num3 + ",stage:" + num2 + "}";
			i.LogEvent(eventId, jsonValue);
			string text = PlayerPrefs.GetString("isFirst");
			if (text.CompareTo("YES") == 0)
			{
				string eventId2 = "firstPlay";
				string jsonValue2 = "{stage:" + num2 + "}";
				i.LogEvent(eventId2, jsonValue2);
			}
			QuitGame(num3 + string.Empty, num2 + string.Empty, text);
		}
		int num4 = PlayerPrefs.GetInt("updateRank");
		int num5 = PlayerPrefs.GetInt("doRank");
		if (num4 == 1 && num5 != 1)
		{
		}
	}

	private void PayCallBack(string jsonValue)
	{
		string text = jsonValue.Substring(1, jsonValue.Length - 2);
		string[] array = text.Split(',');
		string text2 = array[0];
		string[] array2 = text2.Split(':');
		int num = int.Parse(array2[1].Substring(1, array2[1].Length - 2));
		string text3 = array[2];
		string[] array3 = text3.Split(':');
		int num2 = int.Parse(array3[1].Substring(1, array3[1].Length - 2));
		if (num == 1 && num2 == 100)
		{
			int[] array4 = new int[20];
			array4 = PlayerPrefsX.GetIntArray("n22");
			int num3 = array4[14];
			if (num3 >= 0)
			{
				Crypto.Property_change(10000, _jade: false);
			}
			else
			{
				array4[14] = 0;
				PlayerPrefsX.SetIntArray("n22", array4);
				PlayerPrefs.SetInt("updateRank", 1);
				Crypto.Property_change(8000, _jade: false);
			}
			PlayerPrefs.SetInt("haveDrawedGift", 1);
			PlayerPrefs.SetString("charge", "true");
		}
	}

	public void StartRedeemService()
	{
		using AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.bi.libs.Redeem");
		androidJavaClass.CallStatic("inputCode");
	}

	public void StartFeedBack()
	{
		using AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.common.libs.CommonActivity");
		androidJavaClass.CallStatic("FeedBack");
	}

	public void jumpSence(string type)
	{
		UI_cashshop uI_cashshop = (UI_cashshop)Object.FindObjectOfType(typeof(UI_cashshop));
		if ((bool)uI_cashshop)
		{
			uI_cashshop.closePage();
		}
		int type2 = int.Parse(type);
		int num = PlayerPrefs.GetInt("currentSence");
		PlayerPrefs.SetInt("forceJumpSence", 1);
		switch (num)
		{
		case 0:
		{
			UI_intro uI_intro = (UI_intro)Object.FindObjectOfType(typeof(UI_intro));
			if ((bool)uI_intro)
			{
				uI_intro.jumpSence(type2);
			}
			break;
		}
		case 1:
		{
			UI_map uI_map = (UI_map)Object.FindObjectOfType(typeof(UI_map));
			if ((bool)uI_map)
			{
				uI_map.jumpSence(type2);
			}
			break;
		}
		case 2:
		{
			UI_Extreme uI_Extreme = (UI_Extreme)Object.FindObjectOfType(typeof(UI_Extreme));
			if ((bool)uI_Extreme)
			{
				uI_Extreme.jumpSence(type2);
			}
			break;
		}
		case 3:
		{
			UI_skill uI_skill = (UI_skill)Object.FindObjectOfType(typeof(UI_skill));
			if ((bool)uI_skill)
			{
				uI_skill.jumpSence(type2);
			}
			break;
		}
		case 4:
		{
			UI_archive uI_archive = (UI_archive)Object.FindObjectOfType(typeof(UI_archive));
			if ((bool)uI_archive)
			{
				uI_archive.jumpSence(type2);
			}
			break;
		}
		case 5:
		{
			UI_status uI_status = (UI_status)Object.FindObjectOfType(typeof(UI_status));
			if ((bool)uI_status)
			{
				uI_status.jumpSence(type2);
			}
			break;
		}
		case 6:
		{
			UI_forge uI_forge = (UI_forge)Object.FindObjectOfType(typeof(UI_forge));
			if ((bool)uI_forge)
			{
				uI_forge.jumpSence(type2);
			}
			break;
		}
		case 7:
		{
			UI_pet uI_pet = (UI_pet)Object.FindObjectOfType(typeof(UI_pet));
			if ((bool)uI_pet)
			{
				uI_pet.jumpSence(type2);
			}
			break;
		}
		case 8:
		{
			UI_general uI_general = (UI_general)Object.FindObjectOfType(typeof(UI_general));
			if ((bool)uI_general)
			{
				uI_general.jumpSence(type2);
			}
			break;
		}
		}
	}

	public void RedeemFeedbackSuc(string id)
	{
		int num = int.Parse(id);
		int num2 = 0;
		int value = 0;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		switch (num)
		{
		case 10009:
			num2 = 9;
			break;
		case 10016:
			num2 = 16;
			break;
		case 10036:
			num2 = 36;
			break;
		case 10060:
			num2 = 60;
			break;
		case 10120:
			num2 = 120;
			break;
		case 10200:
			num2 = 200;
			break;
		case 10360:
			num2 = 360;
			break;
		case 10900:
			num2 = 900;
			break;
		case 10001:
			flag3 = true;
			value = 2100;
			break;
		case 10002:
			flag3 = true;
			value = 4300;
			break;
		case 10003:
			flag3 = true;
			value = 8700;
			break;
		case 10004:
			flag3 = true;
			value = 1000;
			flag = true;
			break;
		case 10005:
			flag3 = true;
			value = 1000;
			flag2 = true;
			break;
		case 10006:
		{
			int[] array = new int[20];
			array = PlayerPrefsX.GetIntArray("n22");
			int num3 = array[14];
			if (num3 >= 0)
			{
				Crypto.Property_change(10000, _jade: false);
			}
			else
			{
				array[14] = 0;
				PlayerPrefsX.SetIntArray("n22", array);
				PlayerPrefs.SetInt("updateRank", 1);
				Crypto.Property_change(8000, _jade: false);
			}
			PlayerPrefs.SetInt("haveDrawedGift", 1);
			break;
		}
		}
		if (num2 > 0)
		{
			Crypto.Property_change(num2, _jade: true);
			PlayerPrefs.SetInt("updateJAndG", 1);
		}
		if (flag3)
		{
			Crypto.Property_change(value, _jade: false);
			PlayerPrefs.SetInt("updateJAndG", 1);
		}
		if (flag)
		{
			int[] intArray = PlayerPrefsX.GetIntArray("n39");
			int num4 = Random.Range(0, 100);
			int num5 = 0;
			num5 = ((num4 > 96) ? 4 : ((num4 > 88) ? 3 : ((num4 > 68) ? 2 : ((num4 > 40) ? 1 : 0))));
			intArray[num5]++;
			PlayerPrefsX.SetIntArray("n39", intArray);
		}
		if (flag2)
		{
			int num6 = Crypto.Load_int_key("n10");
			num6++;
			Crypto.Save_int_key("n10", num6);
		}
		using AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.bi.libs.Redeem");
		androidJavaClass.CallStatic("showText", true);
	}

	public void RedeemFeedbackFail()
	{
		using AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.bi.libs.Redeem");
		androidJavaClass.CallStatic("showText", false);
	}

	public void LogEvent(string eventId)
	{
		jcBI.CallStatic("logEvent", eventId);
	}

	public void LogEvent(string eventId, string jsonValue)
	{
		jcBI.CallStatic("logEvent", eventId, jsonValue);
	}

	public void QuitGame(string coin, string stage, string isFirst)
	{
		if (ChannelMgr.GetInstance().IsCT())
		{
			using AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.nhncorp.skundeadct.Game");
			androidJavaClass.CallStatic("quitGame", coin, stage, isFirst);
		}
		else
		{
			using AndroidJavaClass androidJavaClass2 = new AndroidJavaClass("com.common.libs.CommonActivity");
			androidJavaClass2.CallStatic("quitGame", coin, stage, isFirst);
		}
		string text = PlayerPrefs.GetString("isFirst");
		if (text.CompareTo("YES") == 0)
		{
			PlayerPrefs.SetString("isFirst", "NO");
		}
	}

	public void MoreGames()
	{
		if (ChannelMgr.GetInstance().IsCM())
		{
			using AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.pay.cm.CMPay");
			androidJavaClass.CallStatic("moreGame");
		}
		if (ChannelMgr.GetInstance().IsCT())
		{
			using (AndroidJavaClass androidJavaClass2 = new AndroidJavaClass("com.nhncorp.skundeadct.Game"))
			{
				androidJavaClass2.CallStatic("moreGame");
			}
		}
	}

	public void changeLoginState()
	{
		PlayerPrefs.SetString("isFirst", "NO");
		string text = PlayerPrefs.GetString("isFirst");
	}

	public void loginQihoo()
	{
		using AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.pay.qihoo.QihooPay");
		androidJavaClass.CallStatic("doLoginQihoo");
	}

	public bool isScreenLocked()
	{
		using AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.common.libs.CommonActivity");
		return androidJavaClass.CallStatic<bool>("isScreenLocked", new object[0]);
	}

	public bool MusicEnabled()
	{
		if (ChannelMgr.GetInstance().IsCM())
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.pay.cm.CMPay"))
			{
				return androidJavaClass.CallStatic<bool>("musicEnabled", new object[0]);
			}
		}
		return true;
	}

	public void onPause()
	{
		using AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.common.libs.CommonActivity");
		androidJavaClass.CallStatic("onPause");
	}

	public void SetPushOnOff(bool isOpen)
	{
		using AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.common.push.LocalPush");
		androidJavaClass.CallStatic("setPushFlag", isOpen);
	}

	public void pay(string goodsId, string goodsName, float price, int num, string payType, string cmIap, string ctIap, string cuIap, string cmmIap, string middleIap)
	{
		string text = "{'payId':" + goodsId + ",'name':" + goodsName + ",'count':" + price + ",'num':" + num + ",'payType':" + payType + ",'cmIap':" + cmIap + ",'ctIap':" + ctIap + ",'cuIap':" + cuIap + ",'cmmIap':" + cmmIap + ",'middleIap':" + middleIap + "}";
		using AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.nhncorp.pay.outinterface.PayOutInterface");
		androidJavaClass.CallStatic("Pay", text);
	}

	public void loadWDJAds()
	{
		using AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.nhncorp.ads.wdj.WdjAds");
		androidJavaClass.CallStatic("showAppWidgetAds");
	}

	public void showNotice(string type)
	{
		using AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.common.libs.CommonActivity");
		androidJavaClass.CallStatic("showNotice", type);
	}

	public string getDeviceId()
	{
		if (!HasJavaBridge)
		{
			return SystemInfo.deviceUniqueIdentifier;
		}
		using AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.common.libs.CommonActivity");
		return androidJavaClass.CallStatic<string>("getDeviceId", new object[0]);
	}

	public void showCCPlay()
	{
		string channelId = ChannelMgr.GetInstance().getChannelId();
		if (channelId != "000266" && channelId != "000056" && channelId != "000013" && channelId != "000032")
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.common.libs.CommonActivity"))
			{
				androidJavaClass.CallStatic("showCCPlay");
			}
		}
	}

	public void showCCPlayAndForce()
	{
		string channelId = ChannelMgr.GetInstance().getChannelId();
		if (channelId != "000266" && channelId != "000056" && channelId != "000013" && channelId != "000032")
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.common.libs.CommonActivity"))
			{
				androidJavaClass.CallStatic("showCCPlayAndForce");
			}
		}
	}

	public void hideCCPlay()
	{
		string channelId = ChannelMgr.GetInstance().getChannelId();
		if (channelId != "000266" && channelId != "000056" && channelId != "000013" && channelId != "000032")
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.common.libs.CommonActivity"))
			{
				androidJavaClass.CallStatic("hideCCPlay");
			}
		}
	}

	public void getItemSwitch()
	{
		string channelId = ChannelMgr.GetInstance().getChannelId();
		if (channelId != "000266" && channelId != "000013" && channelId != "000032" && channelId != "000056" && channelId != "000023" && channelId != "000005")
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.nhncorp.skundeadck.Game"))
			{
				androidJavaClass.Call("getItemSwitch");
			}
		}
	}
}
