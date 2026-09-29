using System.IO;
using UnityEngine;

public class UI_cashshop : MonoBehaviour
{
	public GUIStyle bt_empty;

	public GUIStyle bt_yesno;

	public GUIStyle bt_back;

	public GUIStyle bt_history;

	public GUISkin basicSkin;

	public Texture2D prt_shop;

	public Texture2D[] img_jade = new Texture2D[8];

	public Texture2D[] img_coin = new Texture2D[6];

	public Texture2D[] img_pr = new Texture2D[6];

	public Texture2D[] img_weapon = new Texture2D[10];

	public Texture2D[] img_armor = new Texture2D[10];

	public Texture2D[] img_acce = new Texture2D[7];

	public Texture2D[] img_etc = new Texture2D[6];

	public Texture2D[] img_menu = new Texture2D[6];

	private Texture2D icon_gem;

	public Texture2D bg_asset;

	public Texture2D bg_black;

	public Texture2D titlebase;

	public Texture2D icon_jade;

	public Texture2D icon_coin;

	public Texture2D pop_blank2;

	public Texture2D pop_blank3;

	public Texture2D bg_cashlist;

	public Texture2D icon_doller;

	public Texture2D icon_won;

	public Texture2D icon_yen;

	public Texture2D soldout;

	public Texture2D freejade;

	public Texture2D limitedSale;

	private Texture2D icon_cashcost;

	public Texture2D bg_item;

	public Texture2D bg_item_0;

	public Texture2D percent_image;

	public Texture2D[] weapon_kindicon = new Texture2D[3];

	public Texture2D titlebase_w;

	public Texture2D[] txt_amount = new Texture2D[8];

	public Transform weapon_dummy;

	public Transform spine_dummy;

	private short confirm;

	private bool dragOn;

	private bool scrollOn;

	private float dragposX;

	private float prevposX;

	private float currentX;

	private float dragrange;

	private GameObject ui;

	private GameObject ui2;

	private int language;

	private int select_itemidx;

	private int coin;

	private int jade;

	private int linecount = 10;

	private string eventName = "consumeJade";

	private float starttime;

	private float bg_posX_l = -380f;

	private float bg_posX_r = 480f;

	private float icon_posY = 340f;

	private int[] get_jade_amount = new int[9] { 9, 16, 36, 60, 120, 200, 360, 900, 0 };

	private float[] cost_jade = new float[9] { 3f, 5f, 10f, 15f, 20f, 30f, 60f, 120f, 0f };

	private int[] CT_ID = new int[8] { 101489, 101488, 101490, 101491, 101492, 5039416, 0, 0 };

	private int[] cost_acce = new int[6] { 50, 50, 15, 20, 30, 20 };

	private int[] cost_coin = new int[6] { 10, 20, 40, 80, 160, 320 };

	private int[] goods_coin = new int[6] { 1000, 2100, 4300, 8700, 17700, 36000 };

	private int[] cost_weapon = new int[10];

	private int[] cost_armor = new int[10] { 40, 80, 80, 70, 150, 150, 100, 290, 290, 150 };

	private int[] cost_etc = new int[6] { 3, 1, 5, 9, 0, 0 };

	private int[] reqLvFactor_weapon = new int[10] { 0, 10, 10, 30, 30, 30, 40, 40, 40, 50 };

	private int[] reqLvFactor_equip = new int[10] { 0, 10, 10, 20, 20, 30, 30, 40, 40, 50 };

	private string[] itemindex = new string[9];

	private int[] unlock_costume = new int[20];

	private int[] unlock_weapon = new int[20];

	private int[] gem_inven = new int[5];

	private int[] accessory = new int[6];

	private int cashing;

	private Vector2 scrollPosition = Vector2.zero;

	private Vector2 prev_scrollPosition = Vector2.zero;

	private float startMousePosY;

	private float curMousePosY;

	private bool popupOn;

	private bool freecash;

	private bool webshop;

	private string log_string;

	private short kind_cash;

	private int review;

	private int kakao;

	private string reviewURL;

	private int cur_weapon = -1;

	private int cur_equip = -1;

	private int selectweapon = -1;

	private int selectequip = -1;

	private int chalv;

	private int selweapon_meshkind;

	private int selweapon_kind;

	private int selweapon_grade;

	private int selweapon_name;

	private int selweapon_maxatk;

	private int selweapon_minatk;

	private float selweapon_spd;

	private int selweapon_special;

	private int selweapon_special_txt;

	private int selequip_index;

	private int selequip_special;

	private int selequip_hp;

	private short margin;

	private Language_Costume script_name;

	private WeaponStat script_weaponstat;

	private Cha_Costume_shop script_costume;

	private Cha_Weapon script_weapon;

	private int[] weapon_seed = new int[26];

	private int[] costume_seed = new int[26];

	private short scene_kind;

	private string st_cashkind = "$";

	private bool getItem = true;

	private void Awake()
	{
		script_weaponstat = GetComponent<WeaponStat>();
		script_name = GetComponent<Language_Costume>();
		script_costume = base.transform.GetChild(0).GetChild(0).GetComponent<Cha_Costume_shop>();
		script_weapon = weapon_dummy.GetComponent<Cha_Weapon>();
		cur_equip = Crypto.Load_int_key("n05");
		cur_weapon = Crypto.Load_int_key("n44");
		chalv = Crypto.Load_int_key("n47");
	}

	private void OnEnable()
	{
		starttime = Time.realtimeSinceStartup;
		base.transform.GetChild(0).GetChild(0).position = new Vector3(0f, 2f, 2f);
		base.transform.GetChild(0).GetChild(1).position = new Vector3(0f, 2f, 2f);
		unlock_costume = PlayerPrefsX.GetIntArray("n18");
		unlock_weapon = PlayerPrefsX.GetIntArray("n40");
		weapon_seed = PlayerPrefsX.GetIntArray("n41");
		costume_seed = PlayerPrefsX.GetIntArray("n45");
	}

	private void Start()
	{
		dragrange = (float)Screen.height * 0.375f;
		kind_cash = (short)Crypto.Load_int_key("cashshopkind");
		accessory = PlayerPrefsX.GetIntArray("n36");
		language = PlayerPrefs.GetInt("language");
		cashing = Crypto.Load_int_key("cashing");
		st_cashkind = Language.intxt[language, 62];
		if (GameObject.Find("UI_Ingame_GUI") != null)
		{
			ui = GameObject.Find("UI_Ingame_GUI");
			scene_kind = 2;
		}
		else if (Application.loadedLevelName == "Result")
		{
			ui = GameObject.FindWithTag("ui");
			ui2 = GameObject.FindWithTag("dummy");
			scene_kind = 2;
		}
		else
		{
			ui = GameObject.FindWithTag("ui");
		}
		ui.active = false;
		if (ui2 != null)
		{
			ui2.active = false;
		}
		coin = Crypto.Load_int_key("n17");
		jade = Crypto.Load_int_key("n24");
		Time.timeScale = 0f;
		review = Crypto.Load_int_key("n12");
		icon_cashcost = icon_doller;
		reviewURL = "https://play.google.com/store/apps/details?id=com.nhncorp.skundeadgr";
		itemindex = new string[9]
		{
			"1",
			"2",
			"3",
			"4",
			"5",
			"7",
			"60",
			"120",
			string.Empty
		};
		for (int i = 0; i < 10; i++)
		{
			cost_weapon[i] = script_weaponstat.Cost_only(weapon_seed[i + 6]);
		}
		SetUpWeapon(weapon_seed[cur_weapon]);
		SetUpArmor(costume_seed[cur_equip]);
		selectequip = cur_equip - 6;
		script_weapon.SetWeapon(selweapon_meshkind, selweapon_kind);
		for (int j = 0; j < spine_dummy.childCount; j++)
		{
			spine_dummy.GetChild(j).gameObject.layer = 2;
		}
	}

	private void OnApplicationPause(bool pocus)
	{
		if (pocus)
		{
			coin = Crypto.Load_int_key("n17");
			jade = Crypto.Load_int_key("n24");
		}
	}

	public void SetUpWeapon(int _index)
	{
		script_weaponstat.SetStat(_index);
		selweapon_meshkind = script_weaponstat.weapon_meshkind;
		selweapon_kind = script_weaponstat.weapon_kind;
		selweapon_grade = script_weaponstat.weapon_grade;
		selweapon_name = script_weaponstat.weapon_name;
		selweapon_maxatk = script_weaponstat.weapon_maxatk;
		selweapon_minatk = script_weaponstat.weapon_minatk;
		selweapon_spd = script_weaponstat.weapon_spd;
		selweapon_special = script_weaponstat.weapon_special;
		selweapon_special_txt = script_weaponstat.weapon_special_txt;
	}

	public void SetUpArmor(int _index)
	{
		selequip_index = _index % 100;
		_index /= 100;
		int[] array = new int[2];
		for (int i = 0; i < 2; i++)
		{
			array[i] = _index % 10;
			_index /= 10;
		}
		selequip_special = array[1];
		selequip_hp = _index;
	}

	public void OnBillingResult(string _itemindex)
	{
		ItemDelivery(_itemindex);
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
		if (num != 1)
		{
			return;
		}
		if (num2 == 100)
		{
			int[] array4 = new int[20];
			array4 = PlayerPrefsX.GetIntArray("n22");
			int num3 = array4[14];
			if (num3 > 0)
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
		}
		if (num2 > 0)
		{
			int num4 = int.Parse(Crypto.Load_string_key(string.Empty + num2));
			if (num4 > 0)
			{
				Crypto.Property_change(num4, _jade: true);
				PlayerPrefs.SetString("charge", "true");
			}
		}
	}

	public void ItemDelivery(string _itemindex)
	{
		for (int i = 0; i < 6; i++)
		{
			if (itemindex[i] == _itemindex)
			{
				cashing += (int)(cost_jade[i] + 0.1f);
				Crypto.Save_int_key("cashing", cashing);
				jade += get_jade_amount[i];
				Crypto.Property_change(get_jade_amount[i], _jade: true);
				PurchaseLog.LogOn(Language.intxt[language, 233] + " (" + get_jade_amount[i] + ")\t" + cost_jade[i] + st_cashkind);
				break;
			}
		}
	}

	public void closePage()
	{
		Object.Destroy(base.gameObject);
		ui.active = true;
		if (ui2 != null)
		{
			ui2.active = true;
		}
		Time.timeScale = 1f;
	}

	private void Update()
	{
		bg_posX_l = Mathf.MoveTowards(bg_posX_l, -64f, (Time.realtimeSinceStartup - starttime) * 500f);
		bg_posX_r = Mathf.MoveTowards(480f, 0f, (Time.realtimeSinceStartup - starttime) * 1500f);
		icon_posY = Mathf.MoveTowards(340f, 230f, (Time.realtimeSinceStartup - starttime) * 500f);
		if (Input.GetMouseButtonDown(0))
		{
			if (Input.mousePosition.y < dragrange)
			{
				dragOn = true;
				prevposX = Input.mousePosition.x;
				currentX = dragposX;
			}
		}
		else if (Input.GetMouseButtonUp(0))
		{
			dragOn = false;
			scrollOn = false;
		}
		if (dragOn)
		{
			if (Mathf.Abs(Input.mousePosition.x - prevposX) > 200f)
			{
				scrollOn = true;
			}
			dragposX = (Input.mousePosition.x - prevposX) * (480f / (float)Screen.width) + currentX;
			dragposX = Mathf.Min(dragposX, 0f);
			dragposX = Mathf.Max(dragposX, -300f);
		}
		string text = PlayerPrefs.GetString("charge");
		if (text.CompareTo("true") == 0)
		{
			coin = Crypto.Load_int_key("n17");
			jade = Crypto.Load_int_key("n24");
			PlayerPrefs.SetString("charge", "false");
		}
		int num = PlayerPrefs.GetInt("updateJAndG");
		if (num == 1)
		{
			coin = Crypto.Load_int_key("n17");
			jade = Crypto.Load_int_key("n24");
			PlayerPrefs.SetInt("updateJAndG", 0);
		}
	}

	private void OnGUI()
	{
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		GUI.skin = basicSkin;
		GUI.depth = -10;
		GUI.DrawTexture(Crypto.Rect2(112f, 0f, 256f, 32f), bg_asset);
		GUI.Label(Crypto.Rect2(146f, 6f, 80f, 14f), string.Empty + coin, "txt12_w");
		GUI.Label(Crypto.Rect2(280f, 6f, 64f, 14f), string.Empty + jade, "txt12_w");
		if (GUI.Button(Crypto.Rect2(144f, 2f, 100f, 24f), string.Empty, bt_empty))
		{
			kind_cash = 2;
			popupOn = false;
			base.transform.GetChild(0).GetChild(0).position = new Vector3(0f, 2f, 2f);
			base.transform.GetChild(0).GetChild(1).position = new Vector3(0f, 2f, 2f);
		}
		if (GUI.Button(Crypto.Rect2(272f, 2f, 88f, 24f), string.Empty, bt_empty))
		{
			kind_cash = 1;
			popupOn = false;
			base.transform.GetChild(0).GetChild(0).position = new Vector3(0f, 2f, 2f);
			base.transform.GetChild(0).GetChild(1).position = new Vector3(0f, 2f, 2f);
		}
		GUI.DrawTexture(Crypto.Rect2(bg_posX_l, 0f, 320f, 320f), prt_shop);
		if (confirm > 0)
		{
			GUI.enabled = false;
		}
		if (kind_cash == 0)
		{
			GUI.DrawTexture(Crypto.Rect2(200f, 84f, 256f, 256f), pop_blank3);
			for (short num = 0; num < 6; num++)
			{
				GUI.DrawTexture(Crypto.Rect2(220 + 76 * (num % 3), 116 + 90 * (num / 3), 64f, 64f), img_menu[num]);
				if (GUI.Button(Crypto.Rect2(220 + 76 * (num % 3), 116 + 90 * (num / 3), 64f, 64f), string.Empty, bt_empty))
				{
					starttime = Time.realtimeSinceStartup;
					kind_cash = (short)(num + 1);
					if (kind_cash == 4)
					{
						popupOn = true;
						if (cur_weapon >= 6)
						{
							selectweapon = cur_weapon - 6;
						}
						base.transform.GetChild(0).GetChild(0).position = new Vector3(0f, -0.1f, -2f);
						base.transform.GetChild(0).GetChild(1).position = new Vector3(-0.6f, 0.3f, -3f);
						base.transform.GetChild(0).GetChild(0).forward = -Vector3.forward;
					}
					else if (kind_cash == 5)
					{
						popupOn = true;
						base.transform.GetChild(0).GetChild(0).position = new Vector3(0f, -0.1f, -2f);
						base.transform.GetChild(0).GetChild(1).position = new Vector3(-0.6f, 0.3f, -3f);
						base.transform.GetChild(0).GetChild(0).forward = Vector3.forward;
					}
				}
			}
			GUI.Box(Crypto.Rect2(220f, 98f, 64f, 20f), Language.intxt[language, 233]);
			GUI.Box(Crypto.Rect2(296f, 98f, 64f, 20f), Language.intxt[language, 237]);
			GUI.Box(Crypto.Rect2(372f, 98f, 64f, 20f), Language.intxt[language, 370]);
			GUI.Box(Crypto.Rect2(220f, 188f, 64f, 20f), Language.intxt[language, 369]);
			GUI.Box(Crypto.Rect2(296f, 188f, 64f, 20f), Language.intxt[language, 199]);
			GUI.Box(Crypto.Rect2(372f, 188f, 64f, 20f), Language.intxt[language, 264]);
			if (GUI.Button(Crypto.Rect2(416f, 0f, 64f, 64f), string.Empty, bt_back))
			{
				Object.Destroy(base.gameObject);
				ui.active = true;
				if (ui2 != null)
				{
					ui2.active = true;
				}
				Time.timeScale = 1f;
			}
			if (GUI.Button(Crypto.Rect2(188f, 32f, 128f, 32f), Language.intxt[language, 358], bt_history))
			{
				linecount = 0;
				log_string = string.Empty;
				StreamReader streamReader = new StreamReader(Application.persistentDataPath + "/purchaselog.txt");
				string text;
				while ((text = streamReader.ReadLine()) != null)
				{
					log_string = log_string + "\r\n" + text;
					linecount++;
				}
				kind_cash = 10;
			}
		}
		else
		{
			if (!popupOn && GUI.Button(Crypto.Rect2(416f, 0f, 64f, 64f), string.Empty, bt_back))
			{
				kind_cash = 0;
				base.transform.GetChild(0).GetChild(0).position = new Vector3(0f, 2f, 2f);
				base.transform.GetChild(0).GetChild(1).position = new Vector3(0f, 2f, 2f);
			}
			if (kind_cash == 1)
			{
				GUI.DrawTexture(Crypto.Rect2(bg_posX_r, 200f, 480f, 120f), bg_black);
				if (freecash && GUI.Button(Crypto.Rect2(250f, 120f, 128f, 32f), Language.intxt[language, 371], bt_history))
				{
					kind_cash = 11;
				}
				bool flag = true;
				int num2 = 8;
				if (flag)
				{
					num2 = 6;
				}
				for (int i = 0; i < num2; i++)
				{
					GUI.DrawTexture(Crypto.Rect2((float)(i * 76) + dragposX + 18f, icon_posY, 64f, 84f), bg_item);
					if (GUI.Button(Crypto.Rect2((float)(i * 76) + dragposX + 18f, icon_posY, 64f, 84f), string.Empty, bt_empty) && !scrollOn)
					{
						select_itemidx = i;
						bool flag2 = false;
						if (Crypto.usimsIndex != "NONE" && select_itemidx >= 5)
						{
							flag2 = true;
						}
						int num3 = 0;
						num3 = ((select_itemidx < 5) ? (select_itemidx + 1) : (select_itemidx + 2));
						AndroidScript.i.pay(string.Empty + itemindex[select_itemidx], get_jade_amount[select_itemidx] + string.Empty, cost_jade[select_itemidx], get_jade_amount[select_itemidx], "pay_offline", "00" + num3, CT_ID[select_itemidx] + string.Empty, "00" + num3, "3000074462250" + num3, string.Empty + num3);
						Crypto.Save_string_key(string.Empty + itemindex[select_itemidx], string.Empty + get_jade_amount[select_itemidx]);
					}
					GUI.DrawTexture(Crypto.Rect2((float)(i * 76) + dragposX + 18f, icon_posY, 64f, 64f), img_jade[i]);
					GUI.Label(Crypto.Rect2((float)(i * 76) + dragposX + 8f, icon_posY + 62f, 64f, 20f), string.Empty + cost_jade[i], "txt12_w");
					GUI.DrawTexture(Crypto.Rect2((float)(i * 76) + dragposX + 62f, icon_posY + 64f, 16f, 16f), icon_cashcost);
					GUI.DrawTexture(Crypto.Rect2((float)(i * 76) + dragposX + 18f, icon_posY - 30f, 64f, 64f), txt_amount[i]);
				}
			}
			else if (kind_cash == 2)
			{
				GUI.DrawTexture(Crypto.Rect2(bg_posX_r, 200f, 480f, 120f), bg_black);
				for (int j = 0; j < 6; j++)
				{
					GUI.DrawTexture(Crypto.Rect2(j * 76 + 18, icon_posY, 64f, 84f), bg_item);
					if (GUI.Button(Crypto.Rect2(j * 76 + 18, icon_posY, 64f, 84f), string.Empty, bt_empty))
					{
						if (jade >= cost_coin[j])
						{
							select_itemidx = j;
							confirm = 2;
						}
						else
						{
							confirm = 3;
						}
					}
					GUI.DrawTexture(Crypto.Rect2(j * 76 + 18, icon_posY, 64f, 64f), img_coin[j]);
					GUI.DrawTexture(Crypto.Rect2(j * 76 + 20, icon_posY - 17f, 16f, 16f), icon_coin);
					GUI.DrawTexture(Crypto.Rect2(j * 76 + 21, icon_posY + 64f, 16f, 16f), icon_jade);
					GUI.Label(Crypto.Rect2(j * 76 + 26, icon_posY + 62f, 64f, 20f), string.Empty + cost_coin[j], "txt12_w");
					GUI.Label(Crypto.Rect2(j * 76 + 23, icon_posY - 20f, 64f, 20f), string.Empty + goods_coin[j], "txt12_w");
				}
			}
			else if (kind_cash == 11)
			{
				if (freecash)
				{
					if (kakao > 0)
					{
						if (GUI.Button(Crypto.Rect2(200f, icon_posY - 150f, 256f, 64f), string.Empty, bt_empty))
						{
							Application.OpenURL("kakaolink://sendurl?msg=%5B%EC%96%B8%EB%8D%B0%EB%93%9C%EC%8A%AC%EB%A0%88%EC%9D%B4%EC%96%B4%5D%20%0A%EC%9D%B4%EC%A0%9C%EA%BB%8F%20%EB%8A%90%EA%BB%B4%20%EB%B3%B8%20%EC%A0%81%20%EC%97%86%EB%8A%94%20%EC%83%88%EB%A1%9C%EC%9A%B4%20%EC%95%A1%EC%85%98%20%EC%BE%8C%EA%B0%90%EC%9D%84%20%EB%A7%9B%EB%B3%B4%EA%B3%A0%20%EC%8B%B6%EB%8B%A4%EB%A9%B4%20%0A%EC%A7%80%EA%B8%88%20%EB%AC%B4%EB%A3%8C%20%EB%8B%A4%EC%9A%B4%EB%A1%9C%EB%93%9C%20%ED%95%98%EC%84%B8%EC%9A%94.%20%0Ahttp%3A%2F%2Fhgurl.me%2F9as&url=http://hgurl.me/9as&appid=com.nhncorp.skundead&appver=2.0");
							kakao *= -1;
							Crypto.Save_int_key("n16", kakao);
						}
						GUI.DrawTexture(Crypto.Rect2(200f, icon_posY - 150f, 256f, 64f), img_pr[0]);
					}
					if (review > 0)
					{
						if (GUI.Button(Crypto.Rect2(200f, icon_posY - 70f, 256f, 64f), string.Empty, bt_empty))
						{
							Application.OpenURL(reviewURL);
							review = -1;
							Crypto.Save_int_key("n12", -1);
							Crypto.Property_change(2, _jade: true);
						}
						GUI.DrawTexture(Crypto.Rect2(200f, icon_posY - 70f, 256f, 64f), img_pr[1]);
						GUI.Label(Crypto.Rect2(200f, icon_posY - 70f, 256f, 64f), Language.intxt[language, 376], "txt12_0");
					}
				}
			}
			else if (kind_cash == 4)
			{
				GUI.DrawTexture(Crypto.Rect2(bg_posX_r, 200f, 480f, 120f), bg_black);
				for (int k = 0; k < 10; k++)
				{
					if (unlock_weapon[k] == 0)
					{
						GUI.DrawTexture(Crypto.Rect2((float)(k * 76) + dragposX + 18f, icon_posY, 64f, 84f), bg_item);
						GUI.DrawTexture(Crypto.Rect2((float)(k * 76) + dragposX + 18f, icon_posY, 64f, 64f), img_weapon[k]);
						GUI.Box(Crypto.Rect2((float)(k * 76) + dragposX + 18f, icon_posY - 30f, 64f, 28f), script_name.txt_cos[language, 71 + k]);
						GUI.DrawTexture(Crypto.Rect2((float)(k * 76) + dragposX + 21f, icon_posY + 64f, 16f, 16f), icon_jade);
						GUI.Label(Crypto.Rect2((float)(k * 76) + dragposX + 26f, icon_posY + 62f, 64f, 20f), string.Empty + cost_weapon[k], "txt12_w");
					}
					else
					{
						GUI.DrawTexture(Crypto.Rect2((float)(k * 76) + dragposX + 18f, icon_posY, 64f, 84f), bg_item_0);
						GUI.DrawTexture(Crypto.Rect2((float)(k * 76) + dragposX + 18f, icon_posY, 64f, 64f), img_weapon[k]);
						GUI.DrawTexture(Crypto.Rect2((float)(k * 76) + dragposX + 18f, icon_posY, 64f, 64f), soldout);
					}
					if (GUI.Button(Crypto.Rect2((float)(k * 76) + dragposX + 18f, icon_posY, 64f, 84f), string.Empty, bt_empty) && !scrollOn)
					{
						popupOn = true;
						base.transform.GetChild(0).GetChild(0).position = new Vector3(0f, -0.1f, -2f);
						base.transform.GetChild(0).GetChild(1).position = new Vector3(-0.6f, 0.3f, -3f);
						selectweapon = k;
						SetUpWeapon(weapon_seed[selectweapon + 6]);
						script_weapon.SetWeapon(selweapon_meshkind, selweapon_kind);
						for (int l = 0; l < spine_dummy.childCount; l++)
						{
							spine_dummy.GetChild(l).gameObject.layer = 2;
						}
					}
					if (k == cur_weapon - 6)
					{
						GUI.Box(Crypto.Rect2((float)(k * 76) + dragposX + 18f, icon_posY - 30f, 64f, 20f), Language.intxt[language, 89]);
					}
					if (k % 3 == 0)
					{
						GUI.DrawTexture(Crypto.Rect2((float)(k * 76) + dragposX + 6f, icon_posY - 12f, 48f, 48f), limitedSale);
					}
				}
				if (popupOn)
				{
					GUI.Label(Crypto.Rect2(266f, 46f, 128f, 14f), script_name.txt_cos[language, selweapon_name], "txt12_w");
					GUI.DrawTexture(Crypto.Rect2(410f, 40f, 26f, 26f), weapon_kindicon[selweapon_kind]);
					for (int m = 0; m < 3; m++)
					{
						GUI.DrawTexture(Crypto.Rect2(302f, 76 + m * 20, 140f, 16f), titlebase_w);
					}
					if (selectweapon < 0)
					{
						GUI.Label(Crypto.Rect2(306f, 116f, 128f, 16f), script_name.txt_cos[language, selweapon_special_txt], "txt12_b");
					}
					else
					{
						GUI.DrawTexture(Crypto.Rect2(302f, 136f, 140f, 16f), titlebase_w);
						GUI.Label(Crypto.Rect2(306f, 116f, 128f, 16f), script_name.txt_cos[language, 120], "txt12_b");
						GUI.Label(Crypto.Rect2(306f, 136f, 128f, 16f), script_name.txt_cos[language, selweapon_special_txt], "txt12_b");
					}
					GUI.Label(Crypto.Rect2(300f, 76f, 128f, 16f), Language.intxt[language, 141] + "   " + selweapon_minatk + " - " + selweapon_maxatk, "txt12_0");
					GUI.Label(Crypto.Rect2(306f, 96f, 128f, 16f), Language.intxt[language, 153] + "   " + (selweapon_spd * 100f).ToString("F1"), "txt12_0");
					if (selectweapon >= 0)
					{
						if (unlock_weapon[selectweapon] == 0)
						{
							if (chalv >= reqLvFactor_weapon[selectweapon])
							{
								if (GUI.Button(Crypto.Rect2(308f, 156f, 64f, 32f), Language.intxt[language, 6], bt_yesno))
								{
									select_itemidx = selectweapon;
									confirm = 5;
								}
							}
							else
							{
								GUI.Label(Crypto.Rect2(308f, 156f, 64f, 32f), Language.intxt[language, 266] + reqLvFactor_weapon[selectweapon], "txt12_r");
							}
						}
						else if (cur_weapon != selectweapon + 6 && scene_kind <= 0 && GUI.Button(Crypto.Rect2(308f, 156f, 64f, 32f), Language.intxt[language, 10], bt_yesno))
						{
							cur_weapon = selectweapon + 6;
							Crypto.Save_int_key("n44", selectweapon + 6);
							Crypto.Save_int_key("n34", selweapon_meshkind + selweapon_kind * 100);
							Crypto.Save_int_key("n31", selweapon_maxatk);
							PlayerPrefs.SetInt("updateRank", 1);
							Crypto.Save_int_key("n04", selweapon_minatk);
							Crypto.Save_int_key("n35", (int)(selweapon_spd * 100f));
							Crypto.Save_int_key("n38", selweapon_grade * 10 + selweapon_special);
						}
					}
					if (GUI.Button(Crypto.Rect2(378f, 156f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
					{
						popupOn = false;
						base.transform.GetChild(0).GetChild(0).position = new Vector3(0f, 2f, 2f);
						base.transform.GetChild(0).GetChild(1).position = new Vector3(0f, 2f, 2f);
					}
				}
			}
			else if (kind_cash == 5)
			{
				GUI.DrawTexture(Crypto.Rect2(bg_posX_r, 200f, 480f, 120f), bg_black);
				for (int n = 0; n < 10; n++)
				{
					if (unlock_costume[n] == 0)
					{
						GUI.DrawTexture(Crypto.Rect2((float)(n * 76) + dragposX + 18f, icon_posY, 64f, 84f), bg_item);
						GUI.DrawTexture(Crypto.Rect2((float)(n * 76) + dragposX + 18f, icon_posY, 64f, 64f), img_armor[n]);
						GUI.Box(Crypto.Rect2((float)(n * 76) + dragposX + 18f, icon_posY - 30f, 64f, 28f), script_name.txt_cos[language, 8 + n]);
						GUI.DrawTexture(Crypto.Rect2((float)(n * 76) + dragposX + 21f, icon_posY + 64f, 16f, 16f), icon_jade);
						GUI.Label(Crypto.Rect2((float)(n * 76) + dragposX + 26f, icon_posY + 62f, 64f, 20f), string.Empty + cost_armor[n], "txt12_w");
					}
					else
					{
						GUI.DrawTexture(Crypto.Rect2((float)(n * 76) + dragposX + 18f, icon_posY, 64f, 84f), bg_item_0);
						GUI.DrawTexture(Crypto.Rect2((float)(n * 76) + dragposX + 18f, icon_posY, 64f, 64f), img_armor[n]);
						GUI.DrawTexture(Crypto.Rect2((float)(n * 76) + dragposX + 18f, icon_posY, 64f, 64f), soldout);
					}
					if (GUI.Button(Crypto.Rect2((float)(n * 76) + dragposX + 18f, icon_posY, 64f, 84f), string.Empty, bt_empty) && !scrollOn)
					{
						popupOn = true;
						base.transform.GetChild(0).GetChild(0).position = new Vector3(0f, -0.1f, -2f);
						base.transform.GetChild(0).GetChild(1).position = new Vector3(-0.6f, 0.3f, -3f);
						selectequip = n;
						SetUpArmor(costume_seed[selectequip + 6]);
						script_costume.Costume(selequip_index);
					}
					if (n == cur_equip - 6)
					{
						GUI.Box(Crypto.Rect2((float)(n * 76) + dragposX + 18f, icon_posY - 30f, 64f, 20f), Language.intxt[language, 89]);
					}
					if (n % 3 == 0)
					{
						GUI.DrawTexture(Crypto.Rect2((float)(n * 76) + dragposX + 6f, icon_posY - 12f, 48f, 48f), limitedSale);
					}
				}
				if (popupOn)
				{
					for (int num4 = 0; num4 < 2; num4++)
					{
						GUI.DrawTexture(Crypto.Rect2(302f, 80 + num4 * 20, 140f, 16f), titlebase_w);
					}
					GUI.Label(Crypto.Rect2(266f, 46f, 128f, 16f), script_name.txt_cos[language, selequip_index + 1], "txt12_w");
					GUI.Label(Crypto.Rect2(292f, 80f, 150f, 16f), Language.intxt[language, 142] + " + " + selequip_hp, "txt12_0");
					if (selectequip < 0)
					{
						GUI.Label(Crypto.Rect2(292f, 100f, 150f, 16f), string.Empty + script_name.txt_cos[language, selequip_special + 21], "txt12_b");
					}
					else
					{
						GUI.DrawTexture(Crypto.Rect2(302f, 120f, 140f, 16f), titlebase_w);
						GUI.Label(Crypto.Rect2(292f, 100f, 150f, 16f), string.Empty + script_name.txt_cos[language, 35], "txt12_b");
						GUI.Label(Crypto.Rect2(292f, 120f, 150f, 16f), string.Empty + script_name.txt_cos[language, selequip_special + 21], "txt12_b");
					}
					if (selectequip >= 0)
					{
						if (unlock_costume[selectequip] == 0)
						{
							if (chalv >= reqLvFactor_equip[selectequip])
							{
								if (GUI.Button(Crypto.Rect2(308f, 156f, 64f, 32f), Language.intxt[language, 6], bt_yesno))
								{
									select_itemidx = selectweapon;
									confirm = 8;
								}
							}
							else
							{
								GUI.Label(Crypto.Rect2(308f, 156f, 64f, 32f), Language.intxt[language, 266] + reqLvFactor_equip[selectequip], "txt12_r");
							}
						}
						else if (cur_equip != selectequip + 6 && scene_kind <= 0 && GUI.Button(Crypto.Rect2(308f, 156f, 64f, 32f), Language.intxt[language, 10], bt_yesno))
						{
							cur_equip = selectequip + 6;
							Crypto.Save_int_key("n05", selectequip + 6);
							Crypto.Save_int_key("n43", selequip_index);
							Crypto.Save_int_key("selequip_hp", selequip_hp);
							PlayerPrefs.SetInt("updateRank", 1);
						}
					}
					if (GUI.Button(Crypto.Rect2(378f, 156f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
					{
						popupOn = false;
						base.transform.GetChild(0).GetChild(0).position = new Vector3(0f, 2f, 2f);
						base.transform.GetChild(0).GetChild(1).position = new Vector3(0f, 2f, 2f);
					}
				}
			}
			else if (kind_cash == 6)
			{
				GUI.DrawTexture(Crypto.Rect2(bg_posX_r, 200f, 480f, 120f), bg_black);
				dragposX = Mathf.Max(dragposX, -100f);
				GUI.DrawTexture(Crypto.Rect2(dragposX + 18f, icon_posY, 64f, 84f), bg_item);
				GUI.DrawTexture(Crypto.Rect2(dragposX + 18f, icon_posY, 64f, 64f), img_acce[0]);
				GUI.DrawTexture(Crypto.Rect2(dragposX + 50f, icon_posY, 32f, 32f), percent_image);
				if (GUI.Button(Crypto.Rect2(dragposX + 18f, icon_posY, 64f, 84f), string.Empty, bt_empty) && !scrollOn)
				{
					int[] intArray = PlayerPrefsX.GetIntArray("n36");
					bool flag3 = false;
					for (int num5 = 0; num5 < 6; num5++)
					{
						if (intArray[num5] == 0)
						{
							flag3 = true;
							break;
						}
					}
					if (flag3)
					{
						confirm = 15;
					}
				}
				GUI.DrawTexture(Crypto.Rect2(dragposX + 21f, icon_posY + 64f, 16f, 16f), icon_jade);
				GUI.Label(Crypto.Rect2(dragposX + 26f, icon_posY + 62f, 64f, 20f), "150", "txt12_w");
				GUI.Box(Crypto.Rect2(dragposX + 18f, icon_posY - 30f, 64f, 28f), string.Empty + Language.intxt[language, 460]);
				for (int num6 = 0; num6 < 6; num6++)
				{
					if (accessory[num6] > 0)
					{
						GUI.DrawTexture(Crypto.Rect2((float)((num6 + 1) * 76) + dragposX + 18f, icon_posY, 64f, 84f), bg_item_0);
						GUI.DrawTexture(Crypto.Rect2((float)((num6 + 1) * 76) + dragposX + 18f, icon_posY, 64f, 64f), img_acce[num6 + 1]);
						GUI.DrawTexture(Crypto.Rect2((float)((num6 + 1) * 76) + dragposX + 18f, icon_posY, 64f, 64f), soldout);
					}
					else
					{
						GUI.DrawTexture(Crypto.Rect2((float)((num6 + 1) * 76) + dragposX + 18f, icon_posY, 64f, 84f), bg_item);
						GUI.DrawTexture(Crypto.Rect2((float)((num6 + 1) * 76) + dragposX + 18f, icon_posY, 64f, 64f), img_acce[num6 + 1]);
						if (GUI.Button(Crypto.Rect2((float)((num6 + 1) * 76) + dragposX + 18f, icon_posY, 64f, 84f), string.Empty, bt_empty) && !scrollOn)
						{
							select_itemidx = num6;
							confirm = 4;
						}
						GUI.DrawTexture(Crypto.Rect2((float)((num6 + 1) * 76) + dragposX + 21f, icon_posY + 64f, 16f, 16f), icon_jade);
						GUI.Label(Crypto.Rect2((float)((num6 + 1) * 76) + dragposX + 26f, icon_posY + 62f, 64f, 20f), string.Empty + cost_acce[num6], "txt12_w");
					}
					GUI.Box(Crypto.Rect2((float)((num6 + 1) * 76) + dragposX + 18f, icon_posY - 30f, 64f, 28f), string.Empty + Language.intxt[language, 331 + num6]);
				}
			}
			else if (kind_cash == 3)
			{
				GUI.DrawTexture(Crypto.Rect2(bg_posX_r, 200f, 480f, 120f), bg_black);
				for (int num7 = 0; num7 < 4; num7++)
				{
					GUI.DrawTexture(Crypto.Rect2(num7 * 76 + 18, icon_posY, 64f, 84f), bg_item);
					if (img_etc[num7] != null)
					{
						GUI.DrawTexture(Crypto.Rect2(num7 * 76 + 18, icon_posY, 64f, 64f), img_etc[num7]);
						if (GUI.Button(Crypto.Rect2(num7 * 76 + 18, icon_posY, 64f, 84f), string.Empty, bt_empty))
						{
							select_itemidx = num7;
							if (select_itemidx == 0)
							{
								confirm = 6;
							}
							else
							{
								confirm = 7;
							}
						}
						GUI.DrawTexture(Crypto.Rect2(num7 * 76 + 21, icon_posY + 64f, 16f, 16f), icon_jade);
						GUI.Label(Crypto.Rect2(num7 * 76 + 26, icon_posY + 62f, 64f, 20f), string.Empty + cost_etc[num7], "txt12_w");
					}
					else
					{
						GUI.DrawTexture(Crypto.Rect2(num7 * 76 + 18, icon_posY, 64f, 84f), bg_item_0);
					}
				}
				GUI.Box(Crypto.Rect2(18f, icon_posY - 30f, 64f, 28f), string.Empty + Language.intxt[language, 253]);
				GUI.Box(Crypto.Rect2(94f, icon_posY - 30f, 64f, 28f), Language.intxt[language, 255] + "\r\nx 1");
				GUI.Box(Crypto.Rect2(170f, icon_posY - 30f, 64f, 28f), Language.intxt[language, 255] + "\r\nx 5");
				GUI.Box(Crypto.Rect2(246f, icon_posY - 30f, 64f, 28f), Language.intxt[language, 255] + "\r\nx 10");
			}
			else if (kind_cash == 10)
			{
				GUI.DrawTexture(Crypto.Rect2(210f, 60f, 256f, 256f), bg_cashlist);
				if (Input.GetMouseButtonDown(0))
				{
					startMousePosY = Input.mousePosition.y;
					prev_scrollPosition = scrollPosition;
				}
				if (Input.GetMouseButton(0) && Crypto.Rect2(230f, 70f, 220f, 224f).Contains(Event.current.mousePosition))
				{
					curMousePosY = Input.mousePosition.y;
					scrollPosition = prev_scrollPosition + Vector2.up * (curMousePosY - startMousePosY) * (320f / (float)Screen.height);
				}
				scrollPosition = GUI.BeginScrollView(Crypto.Rect2(230f, 70f, 240f, 224f), scrollPosition, Crypto.Rect2(0f, 0f, 220f, (float)linecount * 14.5f + 10f));
				if (linecount == 0)
				{
					GUI.Label(Crypto.Rect2(0f, 0f, 220f, 64f), Language.intxt[language, 159], "txt12_0");
				}
				else
				{
					GUI.Label(Crypto.Rect2(0f, 0f, 220f, (float)linecount * 14.5f + 10f), log_string);
				}
				GUI.EndScrollView();
			}
		}
		GUI.enabled = true;
		if (confirm == 2)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			GUI.Box(Crypto.Rect2(176f, 96f, 128f, 20f), Language.intxt[language, 237] + "  " + goods_coin[select_itemidx]);
			GUI.DrawTexture(Crypto.Rect2(204f, 120f, 16f, 16f), icon_jade);
			GUI.Label(Crypto.Rect2(208f, 120f, 64f, 16f), string.Empty + cost_coin[select_itemidx], "txt12_b");
			GUI.Label(Crypto.Rect2(120f, 140f, 230f, 16f), Language.intxt[language, 350], "txt12_b");
			GUI.Label(Crypto.Rect2(120f, 160f, 230f, 16f), Language.intxt[language, 310], "txt12_0");
			if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
			{
				if (Crypto.Property_change(-cost_coin[select_itemidx], _jade: true))
				{
					jade -= cost_coin[select_itemidx];
					coin += goods_coin[select_itemidx];
					Crypto.Property_change(goods_coin[select_itemidx], _jade: false);
					PurchaseLog.LogOn(Language.intxt[language, 234] + " (" + cost_coin[select_itemidx] + ")\t" + Language.intxt[language, 237]);
					string jsonValue = "{jade:" + cost_coin[select_itemidx] + ",type: buycoin}";
					AndroidScript.i.LogEvent(eventName, jsonValue);
				}
				confirm = 0;
			}
			else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
			{
				confirm = 0;
			}
		}
		else if (confirm == 3)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			GUI.Label(Crypto.Rect2(120f, 130f, 230f, 16f), Language.intxt[language, 21], "txt12_0");
			if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
			{
				starttime = Time.realtimeSinceStartup;
				popupOn = false;
				base.transform.GetChild(0).GetChild(0).position = new Vector3(0f, 2f, 2f);
				base.transform.GetChild(0).GetChild(1).position = new Vector3(0f, 2f, 2f);
				kind_cash = 1;
				confirm = 0;
			}
			else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
			{
				confirm = 0;
			}
		}
		else if (confirm == 4)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			GUI.Box(Crypto.Rect2(176f, 96f, 128f, 20f), Language.intxt[language, 331 + select_itemidx]);
			GUI.Label(Crypto.Rect2(120f, 122f, 240f, 16f), Language.intxt[language, 351 + select_itemidx], "txt12_0");
			GUI.DrawTexture(Crypto.Rect2(164 + margin, 144f, 16f, 16f), icon_jade);
			GUI.Label(Crypto.Rect2(120f, 144f, 240f, 16f), string.Empty + cost_acce[select_itemidx] + "   " + Language.intxt[language, 350], "txt12_b");
			GUI.Label(Crypto.Rect2(120f, 162f, 240f, 16f), Language.intxt[language, 310], "txt12_0");
			if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
			{
				if (jade >= cost_acce[select_itemidx])
				{
					if (Crypto.Property_change(-cost_acce[select_itemidx], _jade: true))
					{
						accessory[select_itemidx] = 1;
						jade -= cost_acce[select_itemidx];
						PlayerPrefsX.SetIntArray("n36", accessory);
						PlayerPrefs.SetInt("updateRank", 1);
						confirm = 0;
						PurchaseLog.LogOn(Language.intxt[language, 234] + " (" + cost_acce[select_itemidx] + ")\t" + Language.intxt[language, 331 + select_itemidx]);
						string jsonValue2 = "{jade:" + cost_acce[select_itemidx] + ",type: buyAcce}";
						AndroidScript.i.LogEvent(eventName, jsonValue2);
					}
				}
				else
				{
					confirm = 3;
				}
			}
			else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
			{
				confirm = 0;
			}
		}
		else if (confirm == 5)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			GUI.Box(Crypto.Rect2(176f, 96f, 128f, 20f), script_name.txt_cos[language, selweapon_name]);
			GUI.DrawTexture(Crypto.Rect2(200f, 124f, 16f, 16f), icon_jade);
			GUI.Label(Crypto.Rect2(120f, 124f, 230f, 16f), string.Empty + cost_weapon[select_itemidx], "txt12_b");
			GUI.Label(Crypto.Rect2(120f, 144f, 230f, 16f), string.Empty + Language.intxt[language, 350], "txt12_b");
			GUI.Label(Crypto.Rect2(120f, 162f, 230f, 16f), Language.intxt[language, 310], "txt12_0");
			if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
			{
				if (jade >= cost_weapon[selectweapon])
				{
					if (Crypto.Property_change(-cost_weapon[selectweapon], _jade: true))
					{
						jade -= cost_weapon[selectweapon];
						unlock_weapon[selectweapon] = 1;
						PlayerPrefsX.SetIntArray("n40", unlock_weapon);
						confirm = 0;
						PurchaseLog.LogOn(Language.intxt[language, 234] + " (" + cost_weapon[selectweapon] + ")\t" + script_name.txt_cos[language, selweapon_name]);
						string jsonValue3 = "{jade:" + cost_weapon[selectweapon] + ",type: buyWeapon}";
						AndroidScript.i.LogEvent(eventName, jsonValue3);
					}
				}
				else
				{
					confirm = 3;
				}
			}
			else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
			{
				confirm = 0;
			}
		}
		else if (confirm == 6)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			if (select_itemidx == 0)
			{
				GUI.Box(Crypto.Rect2(160f, 96f, 160f, 22f), Language.intxt[language, 253] + " (???) x 1");
			}
			GUI.DrawTexture(Crypto.Rect2(204f, 120f, 16f, 16f), icon_jade);
			GUI.Label(Crypto.Rect2(208f, 120f, 64f, 16f), string.Empty + cost_etc[select_itemidx], "txt12_b");
			GUI.Label(Crypto.Rect2(120f, 140f, 230f, 16f), Language.intxt[language, 350], "txt12_b");
			GUI.Label(Crypto.Rect2(120f, 162f, 230f, 16f), Language.intxt[language, 310], "txt12_0");
			if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
			{
				if (jade >= cost_etc[select_itemidx])
				{
					if (Crypto.Property_change(-cost_etc[select_itemidx], _jade: true))
					{
						gem_inven = PlayerPrefsX.GetIntArray("n39");
						if (select_itemidx == 0)
						{
							int num8 = Random.Range(0, 100);
							int num9 = 0;
							num9 = ((num8 > 96) ? 4 : ((num8 > 88) ? 3 : ((num8 > 68) ? 2 : ((num8 > 40) ? 1 : 0))));
							gem_inven[num9]++;
							icon_gem = Resources.Load("jewel0" + (num9 + 1) + "a") as Texture2D;
							confirm = 9;
						}
						else if (select_itemidx == 1)
						{
							gem_inven[4]++;
							confirm = 0;
						}
						else
						{
							gem_inven[4] += 5;
							confirm = 0;
						}
						PlayerPrefsX.SetIntArray("n39", gem_inven);
						jade -= cost_etc[select_itemidx];
						PurchaseLog.LogOn(Language.intxt[language, 234] + " (" + cost_etc[select_itemidx] + ")\t" + Language.intxt[language, 253]);
						string jsonValue4 = "{jade:" + cost_etc[select_itemidx] + ",type: buyGem}";
						AndroidScript.i.LogEvent(eventName, jsonValue4);
					}
				}
				else
				{
					confirm = 3;
				}
			}
			else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
			{
				confirm = 0;
			}
		}
		else if (confirm == 7)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			if (select_itemidx == 1)
			{
				GUI.Box(Crypto.Rect2(160f, 96f, 160f, 22f), Language.intxt[language, 255] + " x 1");
			}
			else if (select_itemidx == 2)
			{
				GUI.Box(Crypto.Rect2(160f, 96f, 160f, 22f), Language.intxt[language, 255] + " x 5");
			}
			else
			{
				GUI.Box(Crypto.Rect2(160f, 96f, 160f, 22f), Language.intxt[language, 255] + " x 10");
			}
			GUI.DrawTexture(Crypto.Rect2(204f, 120f, 16f, 16f), icon_jade);
			GUI.Label(Crypto.Rect2(208f, 120f, 64f, 16f), string.Empty + cost_etc[select_itemidx], "txt12_b");
			GUI.Label(Crypto.Rect2(120f, 140f, 230f, 16f), Language.intxt[language, 350], "txt12_b");
			GUI.Label(Crypto.Rect2(120f, 162f, 230f, 16f), Language.intxt[language, 310], "txt12_0");
			if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
			{
				if (jade >= cost_etc[select_itemidx])
				{
					if (Crypto.Property_change(-cost_etc[select_itemidx], _jade: true))
					{
						int num10 = Crypto.Load_int_key("n10");
						num10 = ((select_itemidx == 1) ? (num10 + 1) : ((select_itemidx != 2) ? (num10 + 10) : (num10 + 5)));
						Crypto.Save_int_key("n10", num10);
						jade -= cost_etc[select_itemidx];
						PurchaseLog.LogOn(Language.intxt[language, 234] + " (" + cost_etc[select_itemidx] + ")\t" + Language.intxt[language, 255]);
						string jsonValue5 = "{jade:" + cost_etc[select_itemidx] + ",type: buyKey}";
						AndroidScript.i.LogEvent(eventName, jsonValue5);
					}
					confirm = 0;
				}
				else
				{
					confirm = 3;
				}
			}
			else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
			{
				confirm = 0;
			}
		}
		else if (confirm == 8)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			GUI.Box(Crypto.Rect2(176f, 96f, 128f, 20f), script_name.txt_cos[language, selectequip + 8]);
			GUI.DrawTexture(Crypto.Rect2(200f, 124f, 16f, 16f), icon_jade);
			GUI.Label(Crypto.Rect2(120f, 124f, 230f, 16f), string.Empty + cost_armor[selectequip], "txt12_b");
			GUI.Label(Crypto.Rect2(120f, 144f, 230f, 16f), string.Empty + Language.intxt[language, 350], "txt12_b");
			GUI.Label(Crypto.Rect2(120f, 162f, 230f, 16f), Language.intxt[language, 310], "txt12_0");
			if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
			{
				if (jade >= cost_armor[selectequip])
				{
					if (Crypto.Property_change(-cost_armor[selectequip], _jade: true))
					{
						jade -= cost_armor[selectequip];
						unlock_costume[selectequip] = 1;
						PlayerPrefsX.SetIntArray("n18", unlock_costume);
						PurchaseLog.LogOn(Language.intxt[language, 234] + " (" + cost_armor[selectequip] + ")\t" + script_name.txt_cos[language, selectequip + 8]);
						string jsonValue6 = "{jade:" + cost_armor[selectequip] + ",type: buyCostume}";
						AndroidScript.i.LogEvent(eventName, jsonValue6);
					}
					confirm = 0;
				}
				else
				{
					confirm = 3;
				}
			}
			else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
			{
				confirm = 0;
			}
		}
		else if (confirm == 9)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			GUI.Label(Crypto.Rect2(120f, 110f, 230f, 16f), Language.intxt[language, 375], "txt12_0");
			if (icon_gem != null)
			{
				GUI.DrawTexture(Crypto.Rect2(224f, 130f, 32f, 32f), icon_gem);
			}
			if (GUI.Button(Crypto.Rect2(208f, 180f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
			{
				confirm = 0;
			}
		}
		else
		{
			if (confirm != 15)
			{
				return;
			}
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			GUI.Box(Crypto.Rect2(176f, 96f, 128f, 20f), Language.intxt[language, 460]);
			GUI.Label(Crypto.Rect2(120f, 122f, 240f, 16f), Language.intxt[language, 461], "txt12_0");
			GUI.DrawTexture(Crypto.Rect2(164 + margin, 144f, 16f, 16f), icon_jade);
			GUI.Label(Crypto.Rect2(120f, 144f, 240f, 16f), string.Empty + 150 + "   " + Language.intxt[language, 350], "txt12_b");
			GUI.Label(Crypto.Rect2(120f, 162f, 240f, 16f), Language.intxt[language, 310], "txt12_0");
			if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
			{
				if (jade >= 150)
				{
					if (Crypto.Property_change(-150, _jade: true))
					{
						for (int num11 = 0; num11 < 6; num11++)
						{
							accessory[num11] = 1;
						}
						jade -= 150;
						PlayerPrefsX.SetIntArray("n36", accessory);
						PlayerPrefs.SetInt("updateRank", 1);
						confirm = 0;
						PurchaseLog.LogOn(Language.intxt[language, 234] + " (" + 150 + ")\t" + Language.intxt[language, 460]);
						string jsonValue7 = "{jade:" + 150 + ",type: buyAllAcce}";
						AndroidScript.i.LogEvent(eventName, jsonValue7);
					}
				}
				else
				{
					confirm = 3;
				}
			}
			else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
			{
				confirm = 0;
			}
		}
	}
}
