using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

public class UI_intro : MonoBehaviour
{
	private const int MAXPET = 2;

	private const int MAXSKILL = 20;

	private const int MAXWORK = 2;

	private const int MAXGENERAL = 12;

	private const int MAXGENERALPOOL = 30;

	private const int MAXTREASURE = 24;

	private const int MAXSTAGE = 90;

	private int iscontinue;

	private float vol_bgm;

	private float vol_master;

	private int language;

	private bool guion;

	private bool option;

	private bool slideon;

	private bool slideon2;

	private bool redeemOn;

	private bool classmark;

	private bool drawed;

	public Texture2D bg_pause;

	public Texture2D bg_option;

	public Texture2D ico_slide;

	public Texture2D bi;

	public Texture2D pop_blank;

	public Texture2D cl_over15;

	public Texture2D cl_vioence;

	public Texture2D txt_story;

	public Texture2D txt_extreme;

	public Texture2D img_start;

	public Texture2D black;

	public Texture2D img_reward_bg;

	public Texture2D img_reward_jade;

	public Texture2D img_reward_gold;

	public Texture2D img_reward_num_0;

	public Texture2D img_reward_num_1;

	public Texture2D img_reward_num_2;

	public Texture2D img_reward_num_3;

	public Texture2D img_reward_num_4;

	public Texture2D img_reward_num_5;

	public Texture2D img_reward_num_6;

	public Texture2D img_reward_num_7;

	public Texture2D img_reward_num_8;

	public Texture2D img_reward_num_9;

	public Texture2D img_reward_ranking;

	public Texture2D img_reward_x;

	public Texture2D img_reward_tel_bg;

	public Texture2D[] img_mode = new Texture2D[2];

	public GUISkin basicSkin;

	public GUIStyle bt_empty;

	public GUIStyle bt_redeem;

	public GUIStyle bt_new;

	public GUIStyle bt_yesno;

	public GUIStyle bar_slide;

	public GUIStyle sel_lang;

	public GUIStyle bt_menu;

	public GUIStyle bt_cgp;

	public GUIStyle img_reward_tel_x;

	public GUIStyle bt_ownRanking;

	private bool showAds;

	private bool warning;

	private bool freecash;

	private bool promotion_exist;

	private Color transalpha;

	private int[] negative = new int[20];

	private string userdata;

	private string s_version = string.Empty;

	private short confirm;

	private bool isSendGetReward = true;

	private bool can_through = true;

	public Font font;

	private string telNumber = string.Empty;

	private int txt_getJade;

	private void Start()
	{
		Input.multiTouchEnabled = false;
		Application.targetFrameRate = 60;
		iscontinue = PlayerPrefs.GetInt("IsContinue");
		if (iscontinue > 0)
		{
			vol_bgm = PlayerPrefs.GetFloat("vol_bgm");
			vol_master = PlayerPrefs.GetFloat("vol_master");
			base.GetComponent<AudioSource>().volume = vol_bgm;
			AudioListener.volume = vol_master;
			language = PlayerPrefs.GetInt("language");
			try
			{
				string text = Language.intxt[language, 1];
			}
			catch (IndexOutOfRangeException)
			{
				language = 0;
				PlayerPrefs.SetInt("language", language);
			}
			string channelId = ChannelMgr.GetInstance().getChannelId();
			if (channelId == "000023")
			{
				AndroidScript.i.loginQihoo();
			}
		}
		else
		{
			string channelId2 = ChannelMgr.GetInstance().getChannelId();
			if (channelId2 == "000023")
			{
				AndroidScript.i.loginQihoo();
			}
			language = 1;
			PlayerPrefs.SetInt("language", language);
			if (channelId2 == "000266")
			{
				base.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("vol_bgm");
			}
		}
		TextAsset textAsset = Resources.Load("hsp_version") as TextAsset;
		s_version = textAsset.text;
		try
		{
			int num = Crypto.Load_int_key("selequip_hp");
		}
		catch (Exception)
		{
			Crypto.Save_int_key("selequip_hp", 0);
		}
		AndroidScript.i.showCCPlay();
		PlayerPrefs.SetInt("currentSence", 0);
	}

	private void OnApplicationPause(bool pocus)
	{
		if (pocus)
		{
		}
	}

	public void CGPButtonLoadStart(string _bturl)
	{
		StartCoroutine(ImageLoading(_bturl));
	}

	public void GetCGPReward(string _rewardkind, int _rewardamount)
	{
		if (_rewardkind == "UDJR")
		{
			Crypto.Property_change(_rewardamount, _jade: false);
		}
		else if (_rewardkind == "UDGR")
		{
			Crypto.Property_change(_rewardamount, _jade: true);
		}
	}

	private IEnumerator ImageLoading(string _url)
	{
		WWW www = new WWW(_url);
		yield return www;
		bt_cgp.normal.background = www.texture;
		promotion_exist = true;
	}

	private void Update()
	{
		if (guion)
		{
			transalpha = Color.Lerp(transalpha, Color.white, Time.deltaTime * 3f);
		}
		if (Input.GetKeyDown("a"))
		{
			ItemDelivery("10360");
		}
	}

	public void jumpSence(int type)
	{
		switch (type)
		{
		case 0:
			if (iscontinue == 0)
			{
				InitStat();
				Crypto.Save_int_key("gamemode", 0);
				Application.LoadLevel("Story");
			}
			else if (SetVersionStart())
			{
				Crypto.Save_int_key("gamemode", 0);
				Application.LoadLevel("Map");
			}
			break;
		case 1:
			if (iscontinue == 0)
			{
				InitStat();
				Crypto.Save_int_key("gamemode", 1);
				Application.LoadLevel("Story");
			}
			else if (SetVersionStart())
			{
				Crypto.Save_int_key("gamemode", 1);
				Application.LoadLevel("Extreme");
			}
			break;
		}
	}

	public void InitStat()
	{
		iscontinue = 0;
		for (int i = 0; i < 20; i++)
		{
			negative[i] = -2;
		}
		PlayerPrefs.SetInt("IsInit", 1);
		PlayerPrefs.SetString("notice", "If key and value are modified, application data will be crashed or initialized.");
		PlayerPrefs.SetFloat("vol_master", 1f);
		PlayerPrefs.SetFloat("vol_bgm", 1f);
		PlayerPrefs.SetString("charge", "false");
		PlayerPrefs.SetInt("doRank", 0);
		Crypto.Save_int_key("camfov", 0);
		Crypto.Save_int_key("cashshopkind", 0);
		PlayerPrefsX.SetVector3("uiCampos", new Vector3(0f, 0.6f, 1f));
		PlayerPrefsX.SetVector3("uiCampos2", Vector3.zero);
		PlayerPrefs.SetInt("IsContinue", 0);
		Crypto.Save_int_key("n47", 1);
		Crypto.Save_int_key("n11", 0);
		Crypto.Save_int_key("n17", 1200);
		Crypto.Save_int_key("n24", 5);
		Crypto.Save_int_key("n09", 4);
		Crypto.Save_int_key("n03", 3);
		Crypto.Save_int_key("n42", 3);
		Crypto.Save_int_key("n06", -1);
		Crypto.Save_int_key("n10", 5);
		Crypto.Save_int_key("n12", 1);
		Crypto.Save_int_key("n16", 3);
		Crypto.Save_int_key("n14", 0);
		PlayerPrefsX.SetIntArray("n15", default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int));
		PlayerPrefsX.SetIntArray("n18", default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int));
		PlayerPrefsX.SetIntArray("n40", default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int));
		PlayerPrefsX.SetIntArray("n20", default(int), default(int), default(int), default(int), default(int), default(int));
		PlayerPrefsX.SetIntArray("n22", negative);
		PlayerPrefsX.SetIntArray("n23", default(int), default(int));
		PlayerPrefsX.SetIntArray("n27", default(int), default(int));
		PlayerPrefsX.SetIntArray("n25", default(int), default(int));
		Crypto.Save_int_key("n08", 1);
		Crypto.Save_int_key("n21", 1);
		Crypto.Save_int_key("n28", 1);
		Crypto.Save_int_key("n26", 100);
		Crypto.Save_int_key("n30", 1);
		Crypto.Save_int_key("n32", 5);
		Crypto.Save_int_key("n05", 0);
		Crypto.Save_int_key("n44", 0);
		Crypto.Save_int_key("n43", 0);
		PlayerPrefsX.SetIntArray("n13", 12707979, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		PlayerPrefsX.SetIntArray("n33", 100, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		PlayerPrefsX.SetIntArray("n29", default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int));
		PlayerPrefsX.SetIntArray("n36", default(int), default(int), default(int), default(int), default(int), default(int));
		Crypto.Save_int_key("cur_general", 0);
		Crypto.Save_int_key("n34", 0);
		Crypto.Save_int_key("n31", 10);
		Crypto.Save_int_key("n04", 8);
		Crypto.Save_int_key("n35", 2);
		Crypto.Save_int_key("n38", 0);
		Crypto.Save_int_key("cur_stage_index", 0);
		Crypto.Save_int_key("play_kind", 0);
		Crypto.Save_int_key("n37", -1);
		Crypto.Save_int_key("n46", 3);
		Crypto.Save_int_key("n48", 1);
		Crypto.Save_int_key("n49", 1);
		Crypto.Save_int_key("selequip_hp", 0);
		PlayerPrefsX.SetIntArray("n50", default(int), default(int), default(int));
		PlayerPrefsX.SetIntArray("n51", default(int), default(int));
		PlayerPrefsX.SetIntArray("n52", default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int));
		PlayerPrefsX.SetIntArray("n53", default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int));
		Crypto.Save_int_key("n54", 0);
		Crypto.Save_int_key("n55", 0);
		Crypto.Save_int_key("n56", 0);
		Crypto.Save_int_key("n58", 0);
		Crypto.Save_int_key("n59", 0);
		Crypto.Save_int_key("n61", 0);
		Crypto.Save_int_key("generalsearch", 0);
		Crypto.Save_int_key("caveplay", 0);
		Crypto.Save_int_key("perfectplay", 0);
		PlayerPrefsX.SetIntArray("skill_slot", -1, -1, -1, -1, -1);
		PlayerPrefsX.SetIntArray("n41", 10540, 0, 0, 0, 0, 0, 16040599, 17042599, 18041599, 19041599, 20040599, 21043599, 22042599, 23043599, 24040599, 25046599, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		PlayerPrefsX.SetIntArray("n45", 10000, 0, 0, 0, 0, 0, 151407, 202408, 253409, 301410, 352411, 403412, 501413, 552414, 603415, 704416, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		PlayerPrefsX.SetIntArray("n19", default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int));
		PlayerPrefsX.SetIntArray("n39", 1, 0, 0, 0, 0);
		PlayerPrefsX.SetIntArray("treasure", default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int));
		int[] array = new int[30];
		array[27] = 1;
		PlayerPrefsX.SetIntArray("staff", array);
		PlayerPrefsX.SetIntArray("archive", default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int));
		PlayerPrefsX.SetIntArray("bosskill", default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int));
		PlayerPrefsX.SetIntArray("skill_use", default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int));
		PlayerPrefsX.SetIntArray("pet_skill_use", default(int), default(int));
		Crypto.Save_int_key("enemykill", 0);
		Crypto.Save_int_key("grappling", 0);
		Crypto.Save_int_key("exattack", 0);
		Crypto.Save_int_key("death", 0);
		Crypto.Save_int_key("resurrection", 0);
		Crypto.Save_int_key("changelevel", 0);
		Crypto.Save_int_key("storycutin", 2);
		Crypto.Save_int_key("tutorial", 0);
		Crypto.Save_int_key("cashing", 0);
		TimeControl.EraseAll();
		TimeControl.InitStart();
		PurchaseLog.FileCreat();
		PlayerPrefs.SetInt("doRank", 0);
		Debug.Log(PlayerPrefs.GetInt("doRank"));
		PlayerPrefs.SetString("username", string.Empty);
		Crypto.Save_string_key("lastSatge", "0");
		Crypto.Save_string_key("highScore", "0");
		Crypto.Save_string_key("useTime", "0:00");
		PlayerPrefs.SetInt("updateRank", 0);
		warning = false;
	}

	public void StartGUI()
	{
		guion = true;
	}

	public bool SetVersionStart()
	{
		if (iscontinue < 3)
		{
			iscontinue = 3;
			Crypto.Save_int_key("n46", 3);
			PlayerPrefs.SetInt("IsContinue", 3);
			int num = Crypto.Load_int_key("n07");
			if (num > 99)
			{
				num = 99 + (num - 99) / 100;
				num = Mathf.Min(num, 199);
			}
			Crypto.Save_int_key("n47", num);
			PlayerPrefs.SetInt("updateRank", 1);
		}
		if (iscontinue < 4)
		{
			iscontinue = 4;
			Crypto.Save_int_key("n48", 1);
			Crypto.Save_int_key("n49", 1);
			PlayerPrefsX.SetIntArray("n50", default(int), default(int), default(int));
			PlayerPrefsX.SetIntArray("n51", default(int), default(int));
			PlayerPrefsX.SetVector3("uiCampos2", Vector3.zero);
			PlayerPrefsX.SetIntArray("n52", default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int));
			PlayerPrefsX.SetIntArray("n53", default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int));
			Crypto.Save_int_key("n54", 0);
			Crypto.Save_int_key("n55", 0);
			Crypto.Save_int_key("n56", 0);
			Crypto.Save_int_key("n58", 0);
			Crypto.Save_int_key("n59", 0);
			TimeControl.EraseAll();
			PlayerPrefs.SetInt("IsContinue", 4);
			PlayerPrefs.SetInt("IsContinue", 4);
		}
		if (iscontinue < 6)
		{
			Crypto.Save_int_key("n48", 1);
			Crypto.Save_int_key("n49", 1);
			PlayerPrefsX.SetIntArray("n50", default(int), default(int), default(int));
			PlayerPrefsX.SetIntArray("n51", default(int), default(int));
			PlayerPrefsX.SetVector3("uiCampos2", Vector3.zero);
			PlayerPrefsX.SetIntArray("n52", default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int));
			PlayerPrefsX.SetIntArray("n53", default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int), default(int));
			Crypto.Save_int_key("n54", 0);
			Crypto.Save_int_key("n55", 0);
			Crypto.Save_int_key("n56", 0);
			Crypto.Save_int_key("n58", 0);
			Crypto.Save_int_key("n59", 0);
			Crypto.Save_int_key("n61", 0);
			Crypto.Save_int_key("n16", 3);
			TimeControl.EraseAll();
			PlayerPrefs.SetInt("IsContinue", 6);
		}
		TimeControl.InitStart();
		return true;
	}

	public void ItemDelivery(string _itemindex)
	{
		string[] array = new string[7] { "10009", "10016", "10036", "10060", "10120", "10360", "10900" };
		int[] array2 = new int[7] { 9, 16, 36, 60, 120, 360, 900 };
		for (int i = 0; i < 6; i++)
		{
			if (array[i] == _itemindex)
			{
				txt_getJade = array2[i];
				Crypto.Property_change(array2[i], _jade: true);
				confirm = 3;
				Invoke("Delay", 1f);
				break;
			}
		}
	}

	private void Delay()
	{
		confirm = 0;
	}

	private void OnGUI()
	{
		GUI.skin = basicSkin;
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		if (!guion)
		{
			return;
		}
		if (confirm > 0)
		{
			GUI.enabled = false;
		}
		GUI.color = transalpha;
		GUI.DrawTexture(Crypto.Rect2(108f, 6f, 256f, 128f), bi);
		if (classmark)
		{
			GUI.DrawTexture(Crypto.Rect2(330f, 0f, 64f, 64f), cl_over15);
			GUI.DrawTexture(Crypto.Rect2(384f, 0f, 64f, 64f), cl_vioence);
		}
		if (transalpha.a < 0.9f)
		{
			return;
		}
		GUI.color = Color.white;
		if (warning || option)
		{
			GUI.enabled = false;
		}
		GUI.Box(Crypto.Rect2(66f, 136f, 128f, 128f), string.Empty, "blank_box");
		GUI.Box(Crypto.Rect2(286f, 136f, 128f, 128f), string.Empty, "blank_box");
		if (can_through)
		{
			if (GUI.Button(Crypto.Rect2(66f, 136f, 128f, 128f), Language.intxt[language, 417], bt_empty))
			{
				if (iscontinue == 0)
				{
					InitStat();
					Crypto.Save_int_key("gamemode", 0);
					Application.LoadLevel("Story");
				}
				else if (SetVersionStart())
				{
					Crypto.Save_int_key("gamemode", 0);
					Application.LoadLevel("Map");
				}
			}
			else if (GUI.Button(Crypto.Rect2(286f, 136f, 128f, 128f), Language.intxt[language, 418], bt_empty))
			{
				if (iscontinue == 0)
				{
					InitStat();
					Crypto.Save_int_key("gamemode", 1);
					Application.LoadLevel("Story");
				}
				else if (SetVersionStart())
				{
					Crypto.Save_int_key("gamemode", 1);
					Application.LoadLevel("Extreme");
				}
			}
		}
		GUI.DrawTexture(Crypto.Rect2(84f, 136f, 90f, 32f), txt_story);
		GUI.DrawTexture(Crypto.Rect2(304f, 136f, 90f, 32f), txt_extreme);
		GUI.DrawTexture(Crypto.Rect2(70f, 236f, 120f, 32f), img_start);
		GUI.DrawTexture(Crypto.Rect2(290f, 236f, 120f, 32f), img_start);
		GUI.DrawTexture(Crypto.Rect2(74f, 190f, 112f, 56f), img_mode[0]);
		GUI.DrawTexture(Crypto.Rect2(294f, 190f, 112f, 56f), img_mode[1]);
		if (iscontinue == 0)
		{
			GUI.Label(Crypto.Rect2(66f, 230f, 128f, 32f), Language.intxt[language, 57], "txt12_0");
			GUI.Label(Crypto.Rect2(286f, 230f, 128f, 32f), Language.intxt[language, 57], "txt12_0");
		}
		else
		{
			GUI.Label(Crypto.Rect2(66f, 230f, 128f, 32f), Language.intxt[language, 58], "txt12_0");
			GUI.Label(Crypto.Rect2(286f, 230f, 128f, 32f), Language.intxt[language, 58], "txt12_0");
		}
		if (option)
		{
			GUI.enabled = true;
			if (slideon)
			{
				vol_bgm = (Input.mousePosition.x * (480f / (float)Screen.width) - 214f) / 126f;
				vol_bgm = Mathf.Clamp(vol_bgm, 0f, 1f);
			}
			else if (slideon2)
			{
				vol_master = (Input.mousePosition.x * (480f / (float)Screen.width) - 214f) / 126f;
				vol_master = Mathf.Clamp(vol_master, 0f, 1f);
			}
			GUI.DrawTexture(Crypto.Rect2(0f, 0f, 480f, 320f), bg_pause);
			GUI.DrawTexture(Crypto.Rect2(112f, 85f, 256f, 256f), bg_option);
			GUI.Label(Crypto.Rect2(128f, 108f, 70f, 16f), Language.intxt[language, 302], "txt12_w");
			GUI.Label(Crypto.Rect2(128f, 143f, 70f, 16f), Language.intxt[language, 244], "txt12_w");
			GUI.Label(Crypto.Rect2(128f, 180f, 70f, 16f), Language.intxt[language, 245], "txt12_w");
			if (GUI.Button(Crypto.Rect2(212f, 172f, 128f, 32f), Language.intxt[language, 246], sel_lang))
			{
				short num = 2;
				language = (language + 1) % num;
				PlayerPrefs.SetInt("language", language);
			}
			if (GUI.RepeatButton(Crypto.Rect2(200f, 101f, 152f, 32f), string.Empty, bar_slide))
			{
				slideon2 = true;
			}
			else if (slideon2)
			{
				AudioListener.volume = vol_master;
				slideon2 = false;
			}
			GUI.DrawTexture(Crypto.Rect2(vol_master * 126f + 206f, 109f, 16f, 16f), ico_slide);
			if (GUI.RepeatButton(Crypto.Rect2(200f, 136f, 152f, 32f), string.Empty, bar_slide))
			{
				slideon = true;
			}
			else if (slideon)
			{
				base.GetComponent<AudioSource>().volume = vol_bgm;
				slideon = false;
			}
			GUI.DrawTexture(Crypto.Rect2(vol_bgm * 126f + 206f, 144f, 16f, 16f), ico_slide);
			if (GUI.Button(Crypto.Rect2(208f, 210f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
			{
				option = false;
				PlayerPrefs.SetFloat("vol_bgm", vol_bgm);
				PlayerPrefs.SetFloat("vol_master", vol_master);
			}
			if (iscontinue != 0)
			{
				GUI.Box(Crypto.Rect2(120f, 48f, 240f, 32f), string.Empty, "blank_box");
				GUI.Label(Crypto.Rect2(160f, 48f, 190f, 32f), Language.intxt[language, 419], "txt12_r");
				if (GUI.Button(Crypto.Rect2(122f, 48f, 32f, 32f), string.Empty, bt_new))
				{
					warning = true;
				}
			}
			GUI.enabled = false;
		}
		if (GUI.Button(Crypto.Rect2(442f, 0f, 32f, 32f), string.Empty, bt_menu) && can_through)
		{
			option = true;
			string channelId = ChannelMgr.GetInstance().getChannelId();
			if (channelId == "000116")
			{
				AndroidScript.i.loadWDJAds();
				showAds = false;
			}
		}
		if (warning)
		{
			GUI.enabled = true;
			GUI.DrawTexture(Crypto.Rect2(112f, 110f, 256f, 128f), pop_blank);
			GUI.Label(Crypto.Rect2(120f, 140f, 240f, 40f), Language.intxt[language, 48], "txt12_0");
			if (GUI.Button(Crypto.Rect2(160f, 192f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
			{
				InitStat();
				PlayerPrefs.SetInt("haveDrawedGift", 0);
			}
			else if (GUI.Button(Crypto.Rect2(256f, 192f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
			{
				warning = false;
			}
		}
		GUI.Box(Crypto.Rect2(70f, 4f, 64f, 24f), Language.intxt[language, 458]);
		GUI.Box(Crypto.Rect2(4f, 4f, 64f, 24f), Language.intxt[language, 428]);
		string channelId2 = ChannelMgr.GetInstance().getChannelId();
		if (ChannelMgr.GetInstance().IsCT() || ChannelMgr.GetInstance().IsCM())
		{
			GUI.Box(Crypto.Rect2(294f, 4f, 80f, 24f), Language.intxt[language, 462]);
			GUI.Box(Crypto.Rect2(376f, 4f, 64f, 24f), Language.intxt[language, 463]);
		}
		if (GUI.Button(Crypto.Rect2(70f, 4f, 64f, 24f), string.Empty, bt_empty) && can_through)
		{
			confirm = 6;
		}
		if (GUI.Button(Crypto.Rect2(4f, 4f, 64f, 24f), string.Empty, bt_empty) && can_through)
		{
			confirm = 2;
		}
		if (ChannelMgr.GetInstance().IsCT() || ChannelMgr.GetInstance().IsCM())
		{
			if (GUI.Button(Crypto.Rect2(294f, 4f, 80f, 24f), string.Empty, bt_empty))
			{
				confirm = 4;
			}
			if (GUI.Button(Crypto.Rect2(376f, 4f, 64f, 24f), string.Empty, bt_empty))
			{
				confirm = 5;
			}
		}
		GUI.Box(Crypto.Rect2(20f, 294f, 440f, 20f), Language.intxt[language, 380]);
		if (confirm > 0)
		{
			GUI.enabled = true;
			switch (confirm)
			{
			case 2:
				GUI.skin.font = font;
				if (ChannelMgr.GetInstance().IsCT())
				{
					GUI.DrawTexture(Crypto.Rect2(112f, 110f, 256f, 170f), pop_blank);
					GUI.Label(Crypto.Rect2(140f, 165f, 200f, 30f), Language.intxt[language, 430], "txt2");
					if (GUI.Button(Crypto.Rect2(208f, 235f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
					{
						confirm = 0;
					}
				}
				else if (ChannelMgr.GetInstance().IsCM())
				{
					GUI.DrawTexture(Crypto.Rect2(112f, 110f, 256f, 170f), pop_blank);
					GUI.Label(Crypto.Rect2(120f, 150f, 240f, 40f), Language.intxt[language, 485], "txt2");
					if (GUI.Button(Crypto.Rect2(208f, 222f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
					{
						confirm = 0;
					}
				}
				else
				{
					GUI.DrawTexture(Crypto.Rect2(112f, 110f, 256f, 170f), pop_blank);
					GUI.Label(Crypto.Rect2(120f, 150f, 240f, 40f), Language.intxt[language, 464], "txt2");
					if (GUI.Button(Crypto.Rect2(208f, 222f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
					{
						confirm = 0;
					}
				}
				break;
			case 3:
				GUI.Box(Crypto.Rect2(112f, 160f, 256f, 38f), Language.intxt[language, 233] + " " + txt_getJade);
				break;
			case 4:
				try
				{
					AndroidScript.i.MoreGames();
				}
				catch (Exception)
				{
					confirm = 0;
				}
				confirm = 0;
				break;
			case 5:
			{
				if (Time.timeScale == 0f || Application.loadedLevel == 0)
				{
					return;
				}
				int num2 = PlayerPrefs.GetInt("IsInit");
				int num3 = 0;
				int num4 = 0;
				if (num2 == 1)
				{
					num4 = Crypto.Load_int_key("n24");
					num3 = Crypto.Load_int_key("n06");
				}
				if (num3 < 0)
				{
					num3 = 0;
				}
				string isFirst = PlayerPrefs.GetString("isFirst");
				AndroidScript.i.QuitGame(num4 + string.Empty, num3 + string.Empty, isFirst);
				confirm = 0;
				break;
			}
			case 6:
				AndroidScript.i.StartRedeemService();
				confirm = 0;
				break;
			}
		}
		int num5 = PlayerPrefs.GetInt("doRank");
		int num6 = PlayerPrefs.GetInt("finalRanking");
		if (num5 == 1)
		{
			HttpMsgManager httpMsgManager = (HttpMsgManager)UnityEngine.Object.FindObjectOfType(typeof(HttpMsgManager));
			if (isSendGetReward)
			{
				if ((bool)httpMsgManager)
				{
					httpMsgManager.getReward();
				}
				isSendGetReward = false;
			}
			int num7 = PlayerPrefs.GetInt("haveReward");
			if (num7 == 1)
			{
				can_through = false;
				GUI.DrawTexture(Crypto.Rect2(60f, 65f, 360f, 200f), img_reward_bg);
				int num8 = PlayerPrefs.GetInt("rewardGold");
				int num9 = PlayerPrefs.GetInt("rewardJade");
				GUI.DrawTexture(Crypto.Rect2(75f, 130f, 200f, 28f), img_reward_ranking);
				GUI.Label(Crypto.Rect2(195f, 134f, 64f, -16f), ": " + num6, bt_ownRanking);
				if (num8 > 0)
				{
					GUI.DrawTexture(Crypto.Rect2(305f, 145f, 64f, 64f), img_reward_gold);
					char[] array = num8.ToString().ToCharArray();
					if (array.Length == 3)
					{
						GUI.DrawTexture(Crypto.Rect2(300f, 200f, 16f, 8f), img_reward_x);
						GUI.DrawTexture(Crypto.Rect2(315f, 200f, 16f, 8f), getTexture(array[0]));
						GUI.DrawTexture(Crypto.Rect2(330f, 200f, 16f, 8f), getTexture(array[1]));
						GUI.DrawTexture(Crypto.Rect2(345f, 200f, 16f, 8f), getTexture(array[2]));
					}
					else if (array.Length == 4)
					{
						GUI.DrawTexture(Crypto.Rect2(300f, 200f, 16f, 8f), img_reward_x);
						GUI.DrawTexture(Crypto.Rect2(315f, 200f, 16f, 8f), getTexture(array[0]));
						GUI.DrawTexture(Crypto.Rect2(330f, 200f, 16f, 8f), getTexture(array[1]));
						GUI.DrawTexture(Crypto.Rect2(345f, 200f, 16f, 8f), getTexture(array[2]));
						GUI.DrawTexture(Crypto.Rect2(360f, 200f, 16f, 8f), getTexture(array[3]));
					}
				}
				if (num9 > 0)
				{
					GUI.DrawTexture(Crypto.Rect2(305f, 135f, 64f, 64f), img_reward_jade);
					char[] array2 = num9.ToString().ToCharArray();
					if (array2.Length == 1)
					{
						GUI.DrawTexture(Crypto.Rect2(320f, 185f, 16f, 8f), img_reward_x);
						GUI.DrawTexture(Crypto.Rect2(335f, 185f, 16f, 8f), getTexture(array2[0]));
					}
					else if (array2.Length == 2)
					{
						GUI.DrawTexture(Crypto.Rect2(315f, 185f, 16f, 8f), img_reward_x);
						GUI.DrawTexture(Crypto.Rect2(330f, 185f, 16f, 8f), getTexture(array2[0]));
						GUI.DrawTexture(Crypto.Rect2(345f, 185f, 16f, 8f), getTexture(array2[1]));
					}
				}
				if (GUI.Button(Crypto.Rect2(140f, 200f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
				{
					if (num8 > 0)
					{
						Crypto.Property_change(num8, _jade: false);
					}
					if (num9 > 0)
					{
						Crypto.Property_change(num9, _jade: true);
					}
					if ((bool)httpMsgManager)
					{
						httpMsgManager.bearThePalm();
					}
					PlayerPrefs.SetInt("haveReward", 0);
					drawed = true;
					can_through = true;
				}
			}
		}
		if (!drawed || num6 <= 0 || num6 > 50)
		{
			return;
		}
		can_through = false;
		GUI.DrawTexture(Crypto.Rect2(80f, 30f, 300f, 250f), img_reward_tel_bg);
		telNumber = GUI.TextField(new Rect(375f, 363f, 290f, 40f), telNumber);
		bool flag = ValidateMobile(telNumber);
		if (GUI.Button(Crypto.Rect2(200f, 230f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
		{
			HttpMsgManager httpMsgManager2 = (HttpMsgManager)UnityEngine.Object.FindObjectOfType(typeof(HttpMsgManager));
			if ((bool)httpMsgManager2)
			{
				if (flag)
				{
					httpMsgManager2.saveTelNumber(telNumber, PlayerPrefs.GetString("rankName"));
					can_through = true;
					drawed = false;
				}
				telNumber = string.Empty;
			}
		}
		if (GUI.Button(Crypto.Rect2(315f, 45f, 32f, 32f), Language.intxt[language, 0], img_reward_tel_x))
		{
			can_through = true;
			drawed = false;
		}
	}

	private Texture2D getTexture(char c)
	{
		return c switch
		{
			'1' => img_reward_num_1, 
			'2' => img_reward_num_2, 
			'3' => img_reward_num_3, 
			'4' => img_reward_num_4, 
			'5' => img_reward_num_5, 
			'6' => img_reward_num_6, 
			'7' => img_reward_num_7, 
			'8' => img_reward_num_8, 
			'9' => img_reward_num_9, 
			_ => img_reward_num_0, 
		};
	}

	private static bool ValidateMobile(string mobile)
	{
		if (string.IsNullOrEmpty(mobile))
		{
			return false;
		}
		return Regex.IsMatch(mobile, "^(13|14|15|16|18|19)\\d{9}$");
	}
}
