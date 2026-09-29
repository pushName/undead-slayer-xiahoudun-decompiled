using UnityEngine;

public class CmBillingAndroid
{
	public class BillingResult
	{
		public const string NONE = "0";

		public const string SUCCESS = "1";

		public const string FAILED = "2";

		public const string CANCELLED = "3";
	}

	private AndroidJavaClass klass = new AndroidJavaClass("com.nhncorp.skundeadck.MainActivity");

	private static CmBillingAndroid _instance;

	public static CmBillingAndroid Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new CmBillingAndroid();
			}
			return _instance;
		}
	}

	public void InitializeApp(string chanelID, string aliPayKey, string aliPaySecret)
	{
		using AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		using (androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
		{
			klass.CallStatic("InitHS", chanelID, aliPayKey, aliPaySecret);
		}
	}

	public bool GetActivateFlag(string index)
	{
		return klass.CallStatic<bool>("getActivateFlag", new object[1] { index });
	}

	public void SetActivateFlag(string index, bool flag)
	{
		klass.CallStatic("setActivateFlag", index, flag);
	}

	public bool IsMusicEnabled()
	{
		return klass.CallStatic<bool>("isMusicEnabled", new object[0]);
	}

	public void DoBilling(string _index, string _name, string _price, int _telecomPayCode, bool _alipay)
	{
		Debug.Log("DoBilling start!!");
		using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
		{
			using AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			androidJavaObject.Call("DobillingStart", _index, _name, _price, _telecomPayCode, _alipay);
		}
		Debug.Log("DoBilling finish!!");
	}

	public void GetCash(string _id, string _name, int _price)
	{
		using AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		using (androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
		{
			klass.CallStatic("BuyJade", SystemInfo.deviceUniqueIdentifier, _id, _name, _price);
		}
	}

	public void UseCash(string _id, string _name, int _price)
	{
		using AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		using (androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
		{
			klass.CallStatic("UseJade", SystemInfo.deviceUniqueIdentifier, _id, _name, _price);
		}
	}

	public void Redeem(string _code)
	{
		using AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		using (androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
		{
			klass.CallStatic("redeemClicked", _code);
		}
	}

	public void Log_LogIn(string _deviceID, int _chaLV)
	{
		using AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		using (androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
		{
			klass.CallStatic("Log_LogIn", _deviceID, _chaLV);
		}
	}

	public void Log_LogOut(string _deviceID, int _chaLV, long _playTime)
	{
		using AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		using (androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
		{
			klass.CallStatic("Log_LogOut", _deviceID, _chaLV, _playTime);
		}
	}

	public string GetBillingResult(string index)
	{
		return klass.CallStatic<string>("getBillingResult", new object[1] { index });
	}

	public void ExitWithUI()
	{
		using AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		using AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
		klass.CallStatic("exit", androidJavaObject);
	}

	public void Exit()
	{
		klass.CallStatic("exitApp");
	}
}
