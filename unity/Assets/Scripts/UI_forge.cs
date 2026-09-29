using UnityEngine;

public class UI_forge : MonoBehaviour
{
	public Texture2D bg_status;

	public Texture2D txt_name;

	public Texture2D bg_black;

	public Texture2D icon_lock;

	public Texture2D icon_unlock;

	public Texture2D titlebase;

	public Texture2D titlebase_w;

	public Texture2D pop_blank;

	public Texture2D pop_blank2;

	public Texture2D star_grade;

	public Texture2D slot_circle;

	public Texture2D bg_upgrade;

	public Texture2D upgrade_emptyslot;

	public Texture2D upgrade_slot;

	public Texture2D c_equip;

	public Texture2D bg_asset;

	public Texture2D icon_jade;

	public Texture2D icon_coin;

	public Texture2D stat_arrow;

	public Texture2D bg_hitgauge;

	public Texture2D gauge;

	public Texture2D[] weapon_kindicon = new Texture2D[3];

	public Texture2D[] gem_tex = new Texture2D[5];

	public Texture2D arrow_upgrade;

	public Texture2D emptyslot;

	public Texture2D[] gem_color = new Texture2D[4];

	public Texture2D bg_hammerhit;

	public Texture2D please_touch;

	public Texture2D tab_1;

	public Texture2D tab_2;

	public Texture2D tab_over;

	public Texture2D fillbox_unique;

	public Texture2D fillbox_normal;

	public Texture2D[] equipicon = new Texture2D[17];

	public Texture2D[] weaponicon = new Texture2D[35];

	private int[] bottom_weaponicon = new int[6];

	private int[] bottom_equipicon = new int[6];

	private short[] weapon_grade = new short[6];

	public Texture2D[] invenkind = new Texture2D[2];

	public GUIStyle bt_equip;

	public GUIStyle bt_yesno;

	public GUIStyle bt_back;

	public GUIStyle erase;

	public GUIStyle bt_plus;

	public GUIStyle sub_menu;

	public GUIStyle bt_upgrade;

	public GUIStyle bt_empty;

	public GUIStyle bt_init;

	private Transform cha1;

	public Transform bg_cha1;

	public Transform weapon_dummy;

	public Transform ef_hammer;

	public Transform ef_result;

	public Transform pt_plus;

	public GUISkin basicSkin;

	private Color empty = new Color(0f, 0f, 0f, 0.5f);

	private Cha_Costume script_costume;

	private Cha_Weapon script_weapon;

	private Ef_forge_hammer script_hammer;

	private GameObject cashshop;

	private Transform sound_UI;

	public Transform sound_dummy;

	private SoundEf_UI script_soundUI;

	private Language_Costume script_name;

	private int bg_posX_l;

	private int bg_posX_r;

	private int icon_posY;

	private short pop_upgrade;

	private bool maxupgrade;

	private int selectequip;

	private int cur_equip;

	private int selectweapon;

	private int cur_weapon;

	private int chalv;

	private int menu_kind;

	private int subkind;

	private int coin;

	private int jade;

	private int confirm;

	private bool dragOn;

	private bool scrollOn;

	private float dragposX;

	private float prevposX;

	private float currentX;

	private bool imagemovefinish;

	private Vector2 cur_iconpos;

	private Vector2 cur_iconpos2;

	private Vector2 targetpos = new Vector2(294f, 82f);

	private Vector2 targetpos2 = new Vector2(310f, 96f);

	private int language;

	private int plusfactor;

	private int oldplusfactor;

	private int slot_weapon;

	private int slot_costume;

	private bool b_delay;

	private float f_delay;

	private float dragrange;

	private int selweapon_index;

	private int selweapon_cost;

	private int selweapon_meshkind;

	private int selweapon_kind;

	private int selweapon_grade;

	private int selweapon_name;

	private int selweapon_maxatk;

	private int selweapon_minatk;

	private float selweapon_spd;

	private int selweapon_special;

	private int selweapon_special_txt;

	private int selweapon_upgrade;

	private int upgradelimit;

	private int selequip_cost;

	private int selequip_index;

	private int selequip_special;

	private int selequip_hp;

	private int oldweapon_meshkind;

	private int oldweapon_kind;

	private int[] unlock_costume = new int[20];

	private int[] unlock_weapon = new int[20];

	private int[] cost_armor = new int[16]
	{
		1, 1, 1, 1, 1, 1, 40, 80, 80, 70,
		150, 150, 100, 290, 290, 150
	};

	private int[] reqLvFactor_weapon = new int[10] { 0, 10, 10, 30, 30, 30, 40, 40, 40, 50 };

	private int[] reqLvFactor_equip = new int[10] { 0, 10, 10, 20, 20, 30, 30, 40, 40, 50 };

	private int[] weapon_seed = new int[26];

	private int[] costume_seed = new int[26];

	private int[] weapon_plusup = new int[26];

	private int[] gem_inven = new int[5];

	private int upgradecost;

	private WeaponStat script_weaponstat;

	private int select_gem;

	private int hitcount;

	private float hitgaugeX;

	private float targetgaugeX;

	private int tutorial;

	private int icon_size;

	private void Awake()
	{
		cha1 = GameObject.FindWithTag("Player").transform;
		weapon_seed = PlayerPrefsX.GetIntArray("n41");
		costume_seed = PlayerPrefsX.GetIntArray("n45");
		chalv = Crypto.Load_int_key("n47");
		script_hammer = ef_hammer.GetComponent<Ef_forge_hammer>();
		script_weaponstat = GetComponent<WeaponStat>();
		script_name = GetComponent<Language_Costume>();
		script_costume = cha1.GetComponent<Cha_Costume>();
		script_weapon = weapon_dummy.GetComponent<Cha_Weapon>();
		slot_weapon = Crypto.Load_int_key("n03");
		slot_costume = Crypto.Load_int_key("n42");
		language = PlayerPrefs.GetInt("language");
		weapon_plusup = PlayerPrefsX.GetIntArray("n19");
	}

	private void OnEnable()
	{
		coin = Crypto.Load_int_key("n17");
		jade = Crypto.Load_int_key("n24");
		gem_inven = PlayerPrefsX.GetIntArray("n39");
		unlock_costume = PlayerPrefsX.GetIntArray("n18");
		unlock_weapon = PlayerPrefsX.GetIntArray("n40");
	}

	private void Start()
	{
		tutorial = Crypto.Load_int_key("tutorial");
		if (tutorial == 3)
		{
			InvokeRepeating("Warning_iconsize", 0.1f, 0.34f);
		}
		selectequip = (cur_equip = Crypto.Load_int_key("n05"));
		selectweapon = (cur_weapon = Crypto.Load_int_key("n44"));
		SetUpWeapon(weapon_seed[selectweapon]);
		SetUpArmor(costume_seed[cur_equip]);
		oldweapon_meshkind = selweapon_meshkind;
		oldweapon_kind = selweapon_kind;
		script_weapon.SetWeapon(selweapon_meshkind, selweapon_kind);
		dragrange = (float)Screen.height * 0.375f;
		bg_posX_l = -380;
		bg_posX_r = 480;
		icon_posY = 320;
		cha1.GetComponent<Animation>().Play("costume");
		if (GameObject.FindWithTag("sound") == null)
		{
			sound_UI = (Transform)Object.Instantiate(sound_dummy, Vector3.zero, Quaternion.identity);
		}
		else
		{
			sound_UI = GameObject.FindWithTag("sound").transform;
		}
		if (sound_UI != null)
		{
			script_soundUI = sound_UI.GetComponent<SoundEf_UI>();
		}
		for (int i = 0; i < slot_weapon; i++)
		{
			bottom_weaponicon[i] = script_weaponstat.IconImage_Only(weapon_seed[i]);
			weapon_grade[i] = (short)(weapon_seed[i] % 100000 / 10000);
		}
		for (int j = 0; j < slot_costume; j++)
		{
			bottom_equipicon[j] = costume_seed[j] % 100;
		}
		bg_cha1.localScale = Vector3.zero;
		PlayerPrefs.SetInt("currentSence", 6);
	}

	public void Warning_iconsize()
	{
		icon_size = (icon_size + 1) % 2;
	}

	public void Cost_upgrade()
	{
		if (tutorial != 3)
		{
			upgradecost = (int)((float)(selweapon_index * 12 + 7) * 0.5f * (0.4f * (float)(select_gem + 1) + 0.6f) * 13f);
		}
		else
		{
			upgradecost = 0;
		}
	}

	public void Delay(float t)
	{
		b_delay = true;
		f_delay = t;
	}

	public void SetUpWeapon(int _index)
	{
		script_weaponstat.SetStat(_index);
		selweapon_index = script_weaponstat.weapon_index;
		selweapon_cost = script_weaponstat.weapon_cost;
		selweapon_meshkind = script_weaponstat.weapon_meshkind;
		selweapon_kind = script_weaponstat.weapon_kind;
		selweapon_grade = script_weaponstat.weapon_grade;
		selweapon_name = script_weaponstat.weapon_name;
		selweapon_maxatk = script_weaponstat.weapon_maxatk + weapon_plusup[selectweapon];
		selweapon_minatk = script_weaponstat.weapon_minatk + weapon_plusup[selectweapon];
		selweapon_spd = script_weaponstat.weapon_spd;
		selweapon_special = script_weaponstat.weapon_special;
		selweapon_special_txt = script_weaponstat.weapon_special_txt;
		selweapon_upgrade = script_weaponstat.weapon_upgrade;
		upgradelimit = Mathf.Min(selweapon_grade + 2, 6);
	}

	public void SetUpArmor(int _index)
	{
		selequip_index = _index % 100;
		selequip_cost = cost_armor[selectequip];
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

	private void Update()
	{
		if (!imagemovefinish)
		{
			if (bg_posX_l < -64)
			{
				bg_posX_l = (int)((float)bg_posX_l + Mathf.Min(-bg_posX_l, Time.deltaTime * 600f));
			}
			else
			{
				bg_posX_l = -64;
				bg_posX_r = (int)((float)bg_posX_r - Mathf.Min(bg_posX_r, Time.deltaTime * 1500f));
				if (bg_posX_r <= 0)
				{
					bg_posX_r = 0;
					icon_posY = (int)((float)icon_posY - Mathf.Min(icon_posY, Time.deltaTime * 500f));
					if (icon_posY <= 240)
					{
						icon_posY = 240;
						imagemovefinish = true;
					}
				}
			}
		}
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
			if (Mathf.Abs(Input.mousePosition.x - prevposX) > 8f)
			{
				scrollOn = true;
			}
			dragposX = (Input.mousePosition.x - prevposX) * (480f / (float)Screen.width) + currentX;
			dragposX = Mathf.Min(dragposX, 0f);
			dragposX = Mathf.Max(dragposX, -300f);
		}
		if (b_delay)
		{
			f_delay -= Time.deltaTime;
			if (f_delay <= 0f)
			{
				b_delay = false;
				f_delay = 0f;
			}
		}
		if (pop_upgrade == 1)
		{
			cur_iconpos = Vector2.MoveTowards(cur_iconpos, targetpos, Time.deltaTime * 1000f);
		}
		else if (pop_upgrade == 2)
		{
			cur_iconpos2 = Vector2.MoveTowards(cur_iconpos2, targetpos2, Time.deltaTime * 600f);
		}
		else if (pop_upgrade == 3)
		{
			if (f_delay < 0f)
			{
				if (hitcount < select_gem + 4)
				{
					script_hammer.HammerHit();
					hitcount++;
					f_delay = 1.2f;
				}
				else
				{
					ef_result.gameObject.active = true;
					ef_result.localScale = Vector3.zero;
					pop_upgrade = 4;
					hitcount = 0;
					f_delay = 0f;
					Delay(1f);
					weapon_plusup[selectweapon] += plusfactor;
					weapon_seed[selectweapon] += 100000;
					SetUpWeapon(weapon_seed[selectweapon]);
					PlayerPrefsX.SetIntArray("n19", weapon_plusup);
					PlayerPrefsX.SetIntArray("n41", weapon_seed);
					if (selectweapon == cur_weapon)
					{
						Crypto.Save_int_key("n31", selweapon_maxatk);
						PlayerPrefs.SetInt("updateRank", 1);
						Crypto.Save_int_key("n04", selweapon_minatk);
					}
				}
			}
			else
			{
				f_delay -= Time.deltaTime;
			}
			hitgaugeX = Mathf.Lerp(hitgaugeX, targetgaugeX, Time.deltaTime * 6f);
		}
		else if (pop_upgrade >= 4)
		{
			ef_result.localScale = Vector3.MoveTowards(ef_result.localScale, Vector3.one * 2.1f, Time.deltaTime * 15f);
		}
		if (menu_kind == 1)
		{
			cha1.rotation = Quaternion.Lerp(cha1.rotation, Quaternion.LookRotation(-Vector3.forward), Time.deltaTime * 4f);
		}
		else
		{
			cha1.rotation = Quaternion.Lerp(cha1.rotation, Quaternion.LookRotation(Vector3.forward), Time.deltaTime * 4f);
		}
		int num = PlayerPrefs.GetInt("updateJAndG");
		if (num == 1)
		{
			coin = Crypto.Load_int_key("n17");
			jade = Crypto.Load_int_key("n24");
			PlayerPrefs.SetInt("updateJAndG", 0);
		}
	}

	public void jumpSence(int type)
	{
		switch (type)
		{
		case 0:
			Application.LoadLevel("Map");
			break;
		case 1:
			Application.LoadLevel("Extreme");
			break;
		}
	}

	public void Impact()
	{
		base.GetComponent<AudioSource>().Play();
		targetgaugeX = (float)Random.Range(6, 32) + hitgaugeX;
		targetgaugeX = Mathf.Min(targetgaugeX, 208f);
	}

	public void HideCha(bool _hide)
	{
		if (_hide)
		{
			cha1.position = new Vector3(-0.03f, -0.28f, 2f);
			bg_cha1.localScale = Vector3.zero;
		}
		else
		{
			cha1.position = new Vector3(-0.03f, -0.28f, -1f);
			bg_cha1.localScale = new Vector3(1.7f, 2.4f, 1f);
		}
	}

	public void CashshopOpen()
	{
		if (cashshop == null)
		{
			cashshop = Resources.Load("CashShop") as GameObject;
		}
		Object.Instantiate(cashshop.transform, Vector3.zero, Quaternion.identity);
	}

	private void OnGUI()
	{
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		GUI.skin = basicSkin;
		GUI.DrawTexture(Crypto.Rect2(bg_posX_l, 0f, 320f, 320f), bg_status);
		GUI.DrawTexture(Crypto.Rect2(120f, 30f, 100f, 50f), txt_name);
		GUI.Box(Crypto.Rect2(212f, 35f, 80f, 24f), Language.intxt[language, 73]);
		GUI.DrawTexture(Crypto.Rect2(112f, 0f, 256f, 32f), bg_asset);
		GUI.Label(Crypto.Rect2(146f, 6f, 80f, 14f), string.Empty + coin, "txt12_w");
		GUI.Label(Crypto.Rect2(280f, 6f, 64f, 14f), string.Empty + jade, "txt12_w");
		if (GUI.Button(Crypto.Rect2(144f, 2f, 100f, 24f), string.Empty, bt_empty))
		{
			if (sound_UI != null)
			{
				script_soundUI.SoundOn(0);
			}
			Crypto.Save_int_key("cashshopkind", 2);
			CashshopOpen();
		}
		if (GUI.Button(Crypto.Rect2(272f, 2f, 88f, 24f), string.Empty, bt_empty))
		{
			if (sound_UI != null)
			{
				script_soundUI.SoundOn(0);
			}
			Crypto.Save_int_key("cashshopkind", 1);
			CashshopOpen();
		}
		if (confirm > 0)
		{
			GUI.enabled = false;
		}
		else
		{
			GUI.enabled = true;
		}
		if (menu_kind == 0)
		{
			GUI.Label(Crypto.Rect2(204f, 66f, 240f, 32f), Language.intxt[language, 19], "txt12_0");
			for (int i = 0; i < 2; i++)
			{
				GUI.DrawTexture(Crypto.Rect2(243 + i * 100, 100f, 64f, 64f), invenkind[i]);
				if (GUI.Button(Crypto.Rect2(243 + i * 100, 100f, 64f, 64f), string.Empty, sub_menu))
				{
					HideCha(_hide: false);
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					menu_kind = i + 1;
					if (menu_kind == 1)
					{
						selectweapon = cur_weapon;
						SetUpWeapon(weapon_seed[selectweapon]);
						script_weapon.SetWeapon(selweapon_meshkind, selweapon_kind);
					}
					bg_posX_r = 480;
					icon_posY = 320;
					imagemovefinish = false;
				}
			}
			for (int j = 0; j < 5; j++)
			{
				if (gem_inven[j] > 0)
				{
					GUI.color = Color.white;
				}
				else
				{
					GUI.color = empty;
				}
				GUI.DrawTexture(Crypto.Rect2(216 + j * 46, 177f, 32f, 32f), gem_tex[j]);
				GUI.color = Color.white;
				GUI.Box(Crypto.Rect2(216 + j * 46, 208f, 32f, 16f), string.Empty + gem_inven[j]);
			}
			if (GUI.Button(Crypto.Rect2(416f, 0f, 64f, 64f), string.Empty, bt_back))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
				if (Crypto.Load_int_key("gamemode") == 0)
				{
					Application.LoadLevel("Map");
				}
				else
				{
					Application.LoadLevel("Extreme");
				}
			}
			if (tutorial == 3)
			{
				GUI.DrawTexture(Crypto.Rect2(247f, 102f, icon_size * 64, icon_size * 64), please_touch);
			}
		}
		else
		{
			if (pop_upgrade <= 0)
			{
				GUI.DrawTexture(Crypto.Rect2(90f, icon_posY - 30, 64f, 32f), tab_1);
				GUI.DrawTexture(Crypto.Rect2(20f, icon_posY - 30, 64f, 32f), tab_2);
				GUI.DrawTexture(Crypto.Rect2(bg_posX_r, 242f, 480f, 100f), bg_black);
				if (subkind == 0)
				{
					GUI.DrawTexture(Crypto.Rect2(bg_posX_r, 242f, 480f, 4f), fillbox_normal);
				}
				else
				{
					GUI.DrawTexture(Crypto.Rect2(bg_posX_r, 242f, 480f, 4f), fillbox_unique);
				}
				if (GUI.Button(Crypto.Rect2(20f, icon_posY - 30, 64f, 32f), string.Empty, bt_empty))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					subkind = 0;
				}
				else if (GUI.Button(Crypto.Rect2(90f, icon_posY - 30, 64f, 32f), string.Empty, bt_empty))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					subkind = 1;
				}
			}
			if (menu_kind == 1)
			{
				if (pop_upgrade == 1)
				{
					GUI.DrawTexture(Crypto.Rect2(294f, 82f, 64f, 64f), bg_upgrade);
					GUI.DrawTexture(Crypto.Rect2(230f, 78f, 192f, 72f), bg_black);
					GUI.DrawTexture(Crypto.Rect2(294f, 82f, 64f, 64f), emptyslot);
					if (selweapon_upgrade > 0)
					{
						GUI.Label(Crypto.Rect2(363f, 80f, 64f, 32f), selweapon_upgrade + " / " + upgradelimit + "\r\n" + Language.intxt[language, 384], "txt12_w");
						if (GUI.Button(Crypto.Rect2(378f, 110f, 32f, 32f), string.Empty, bt_init))
						{
							if (sound_UI != null)
							{
								script_soundUI.SoundOn(0);
							}
							confirm = 11;
						}
					}
					if (!maxupgrade)
					{
						GUI.Label(Crypto.Rect2(197f, 164f, 256f, 14f), Language.intxt[language, 29], "txt12_0");
						GUI.DrawTexture(Crypto.Rect2(197f, 232f, 256f, 64f), pop_blank);
						for (int k = 0; k < 5; k++)
						{
							if (gem_inven[k] > 0)
							{
								GUI.color = Color.white;
								if (GUI.Button(Crypto.Rect2(216 + k * 46, 240f, 32f, 32f), string.Empty, sub_menu))
								{
									if (sound_UI != null)
									{
										script_soundUI.SoundOn(0);
									}
									if (tutorial == 3 && k != 0)
									{
										Delay(1f);
										confirm = 8;
										return;
									}
									cur_iconpos2 = new Vector2(216 + k * 46, 230f);
									select_gem = k;
									Cost_upgrade();
									pop_upgrade = 2;
								}
							}
							else
							{
								GUI.color = empty;
							}
							GUI.DrawTexture(Crypto.Rect2(216 + k * 46, 240f, 32f, 32f), gem_tex[k]);
							GUI.color = Color.white;
							GUI.Box(Crypto.Rect2(216 + k * 46, 270f, 32f, 16f), string.Empty + gem_inven[k]);
						}
					}
					else
					{
						GUI.Label(Crypto.Rect2(197f, 164f, 256f, 14f), Language.intxt[language, 377], "txt12_0");
					}
					if (GUI.Button(Crypto.Rect2(378f, 198f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
					{
						if (sound_UI != null)
						{
							script_soundUI.SoundOn(1);
						}
						HideCha(_hide: false);
						pop_upgrade = 0;
					}
					GUI.DrawTexture(Crypto.Rect2(cur_iconpos.x, cur_iconpos.y, 64f, 64f), weaponicon[selweapon_meshkind]);
				}
				else if (pop_upgrade == 2)
				{
					GUI.DrawTexture(Crypto.Rect2(294f, 82f, 64f, 64f), bg_upgrade);
					GUI.DrawTexture(Crypto.Rect2(294f, 82f, 64f, 64f), weaponicon[selweapon_meshkind]);
					GUI.DrawTexture(Crypto.Rect2(294f, 82f, 64f, 64f), emptyslot);
					GUI.DrawTexture(Crypto.Rect2(340f, 158f, 16f, 16f), icon_coin);
					GUI.DrawTexture(Crypto.Rect2(212f, 130f, 50f, 50f), bg_hammerhit);
					GUI.Label(Crypto.Rect2(232f, 150f, 16f, 16f), string.Empty + (select_gem + 4 - hitcount), "txt12_0");
					GUI.Label(Crypto.Rect2(270f, 158f, 64f, 14f), Language.intxt[language, 63], "txt12_0");
					GUI.Label(Crypto.Rect2(340f, 158f, 64f, 14f), string.Empty + upgradecost, "txt12_0");
					if (GUI.Button(Crypto.Rect2(308f, 198f, 64f, 32f), Language.intxt[language, 205], bt_yesno))
					{
						if (sound_UI != null)
						{
							script_soundUI.SoundOn(0);
						}
						if (coin >= upgradecost)
						{
							if (tutorial == 3)
							{
								Crypto.Save_int_key("tutorial", -1);
							}
							if (Crypto.Property_change(-upgradecost, _jade: false))
							{
								coin -= upgradecost;
								gem_inven[select_gem]--;
								PlayerPrefsX.SetIntArray("n39", gem_inven);
								f_delay = 1f;
								plusfactor = 0;
								pop_upgrade = 3;
								ef_hammer.gameObject.active = true;
							}
						}
						else
						{
							confirm = 3;
							Delay(1f);
						}
					}
					else if (GUI.Button(Crypto.Rect2(378f, 198f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
					{
						if (sound_UI != null)
						{
							script_soundUI.SoundOn(1);
						}
						pop_upgrade = 1;
					}
					GUI.DrawTexture(Crypto.Rect2(cur_iconpos2.x, cur_iconpos2.y, 32f, 32f), gem_tex[select_gem]);
				}
				else if (pop_upgrade == 3)
				{
					plusfactor = (int)(hitgaugeX / 50f);
					if (plusfactor != oldplusfactor)
					{
						Object.Instantiate(pt_plus, new Vector3(-0.3f * (float)plusfactor + 0.1f, -0.22f, -0.4f), Quaternion.identity);
						oldplusfactor = plusfactor;
					}
					GUI.DrawTexture(Crypto.Rect2(200f, 168f, 256f, 64f), bg_hitgauge);
					GUI.DrawTexture(Crypto.Rect2(218f, 196f, hitgaugeX, 13f), gauge);
					GUI.DrawTexture(Crypto.Rect2(212f, 130f, 50f, 50f), bg_hammerhit);
					GUI.Label(Crypto.Rect2(232f, 150f, 16f, 16f), string.Empty + (select_gem + 4 - hitcount), "txt12_0");
					for (int l = 0; l < 4; l++)
					{
						if (hitgaugeX >= (float)(50 * l + 50))
						{
							GUI.Label(Crypto.Rect2(50 * l + 250, 163f, 40f, 40f), "+ " + (weapon_plusup[selectweapon] + 1 + l), "txt12_w");
						}
						else
						{
							GUI.Label(Crypto.Rect2(50 * l + 250, 163f, 40f, 40f), "+ " + (weapon_plusup[selectweapon] + 1 + l), "txt12_0");
						}
					}
				}
				else if (pop_upgrade == 4)
				{
					GUI.DrawTexture(Crypto.Rect2(294f, 82f, 64f, 64f), weaponicon[selweapon_meshkind]);
					GUI.DrawTexture(Crypto.Rect2(200f, 168f, 256f, 64f), bg_hitgauge);
					GUI.DrawTexture(Crypto.Rect2(218f, 196f, hitgaugeX, 13f), gauge);
					if (f_delay == 0f)
					{
						pop_upgrade = 5;
					}
					for (int m = 0; m < 4; m++)
					{
						if (hitgaugeX >= (float)(50 * m + 50))
						{
							GUI.Label(Crypto.Rect2(50 * m + 250, 163f, 40f, 40f), "+ " + (weapon_plusup[selectweapon] + 1 - plusfactor + m), "txt12_w");
						}
						else
						{
							GUI.Label(Crypto.Rect2(50 * m + 250, 163f, 40f, 40f), "+ " + (weapon_plusup[selectweapon] + 1 - plusfactor + m), "txt12_0");
						}
					}
				}
				else if (pop_upgrade == 5)
				{
					GUI.DrawTexture(Crypto.Rect2(294f, 82f, 64f, 64f), weaponicon[selweapon_meshkind]);
					GUI.DrawTexture(Crypto.Rect2(222f, 187f, 200f, 16f), titlebase);
					GUI.DrawTexture(Crypto.Rect2(340f, 187f, 16f, 16f), stat_arrow);
					GUI.DrawTexture(Crypto.Rect2(276f, 150f, 98f, 20f), bg_black);
					if (weapon_plusup[selectweapon] > 0)
					{
						GUI.Label(Crypto.Rect2(276f, 150f, 98f, 20f), script_name.txt_cos[language, selweapon_name] + " + " + weapon_plusup[selectweapon], "txt12_w");
					}
					else
					{
						GUI.Label(Crypto.Rect2(276f, 150f, 98f, 20f), script_name.txt_cos[language, selweapon_name], "txt12_w");
					}
					GUI.Label(Crypto.Rect2(224f, 188f, 64f, 14f), Language.intxt[language, 141], "txt12_w");
					GUI.Label(Crypto.Rect2(244f, 188f, 128f, 14f), selweapon_minatk - plusfactor + " - " + (selweapon_maxatk - plusfactor), "txt12_w");
					GUI.Label(Crypto.Rect2(324f, 188f, 128f, 14f), selweapon_minatk + " - " + selweapon_maxatk, "txt12_w");
					if (GUI.Button(Crypto.Rect2(378f, 144f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
					{
						if (sound_UI != null)
						{
							script_soundUI.SoundOn(0);
						}
						HideCha(_hide: false);
						pop_upgrade = 0;
						ef_result.gameObject.active = false;
						ef_hammer.gameObject.active = false;
						if (tutorial == 3)
						{
							tutorial = -1;
							Delay(1f);
							confirm = 9;
						}
					}
				}
				else
				{
					GUI.DrawTexture(Crypto.Rect2(256f, 69f, 146f, 16f), titlebase);
					if (weapon_plusup[selectweapon] > 0)
					{
						GUI.Label(Crypto.Rect2(266f, 70f, 128f, 14f), script_name.txt_cos[language, selweapon_name] + " + " + weapon_plusup[selectweapon], "txt12_w");
					}
					else
					{
						GUI.Label(Crypto.Rect2(266f, 70f, 128f, 14f), script_name.txt_cos[language, selweapon_name], "txt12_w");
					}
					GUI.DrawTexture(Crypto.Rect2(410f, 64f, 26f, 26f), weapon_kindicon[selweapon_kind]);
					if (GUI.Button(Crypto.Rect2(378f, 198f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
					{
						if (sound_UI != null)
						{
							script_soundUI.SoundOn(1);
						}
						selectweapon = -1;
						script_weapon.SetWeapon(oldweapon_meshkind, oldweapon_kind);
						selectweapon = cur_weapon;
						menu_kind = 0;
						subkind = 0;
						HideCha(_hide: true);
					}
					if (selweapon_upgrade < upgradelimit)
					{
						if (GUI.Button(Crypto.Rect2(306f, 88f, 128f, 32f), Language.intxt[language, 205], bt_upgrade))
						{
							if (subkind > 0 && unlock_weapon[selectweapon] == 0)
							{
								return;
							}
							if (sound_UI != null)
							{
								script_soundUI.SoundOn(0);
							}
							maxupgrade = false;
							if (tutorial == 3)
							{
								if (cur_weapon != 0)
								{
									confirm = 8;
									Delay(1f);
								}
								else
								{
									cur_iconpos = Vector2.right * (selectweapon * 76 + 18) + Vector2.up * (icon_posY + 8);
									hitgaugeX = 0f;
									targetgaugeX = 0f;
									pop_upgrade = 1;
									HideCha(_hide: true);
								}
							}
							else
							{
								int num = selectweapon;
								if (num >= 6)
								{
									num -= 6;
								}
								cur_iconpos = Vector2.right * ((float)(num * 76 + 18) + dragposX) + Vector2.up * (icon_posY + 8);
								hitgaugeX = 0f;
								targetgaugeX = 0f;
								pop_upgrade = 1;
								HideCha(_hide: true);
							}
						}
					}
					else if (GUI.Button(Crypto.Rect2(306f, 88f, 128f, 32f), "MAX", bt_upgrade))
					{
						if (sound_UI != null)
						{
							script_soundUI.SoundOn(1);
						}
						maxupgrade = true;
						cur_iconpos = Vector2.right * (selectweapon * 76 + 18) + Vector2.up * (icon_posY + 8);
						hitgaugeX = 0f;
						targetgaugeX = 0f;
						pop_upgrade = 1;
						HideCha(_hide: true);
					}
					for (int n = 0; n < upgradelimit; n++)
					{
						if (n < selweapon_upgrade)
						{
							GUI.DrawTexture(Crypto.Rect2(412 - upgradelimit * 3 + n * 6, 98f, 3f, 12f), upgrade_slot);
						}
						else
						{
							GUI.DrawTexture(Crypto.Rect2(412 - upgradelimit * 3 + n * 6, 98f, 3f, 12f), upgrade_emptyslot);
						}
					}
					if (selectweapon >= 6)
					{
						for (int num2 = 0; num2 < 4; num2++)
						{
							GUI.DrawTexture(Crypto.Rect2(302f, 120 + num2 * 20, 140f, 16f), titlebase_w);
						}
						GUI.Label(Crypto.Rect2(300f, 120f, 128f, 16f), Language.intxt[language, 141] + "   " + selweapon_minatk + " - " + selweapon_maxatk, "txt12_0");
						GUI.Label(Crypto.Rect2(306f, 140f, 128f, 16f), Language.intxt[language, 153] + "   " + (selweapon_spd * 100f).ToString("F1"), "txt12_0");
						GUI.Label(Crypto.Rect2(306f, 160f, 128f, 16f), script_name.txt_cos[language, 120], "txt12_b");
						GUI.Label(Crypto.Rect2(306f, 180f, 128f, 16f), script_name.txt_cos[language, selweapon_special_txt], "txt12_b");
						if (unlock_weapon[selectweapon - 6] == 0)
						{
							GUI.Box(Crypto.Rect2(214f, 190f, 64f, 20f), "    " + selweapon_cost);
							GUI.DrawTexture(Crypto.Rect2(218f, 192f, 16f, 16f), icon_jade);
							if (chalv >= reqLvFactor_weapon[selectweapon - 6])
							{
								if (GUI.Button(Crypto.Rect2(308f, 198f, 64f, 32f), Language.intxt[language, 6], bt_yesno))
								{
									if (sound_UI != null)
									{
										script_soundUI.SoundOn(0);
									}
									if (selweapon_cost < 1000)
									{
										if (jade >= selweapon_cost)
										{
											confirm = 10;
										}
										else
										{
											confirm = 2;
											Delay(1f);
										}
									}
									else if (coin >= selweapon_cost)
									{
										confirm = 10;
									}
									else
									{
										confirm = 3;
										Delay(1f);
									}
								}
							}
							else
							{
								GUI.Label(Crypto.Rect2(308f, 198f, 64f, 32f), Language.intxt[language, 266] + reqLvFactor_weapon[selectweapon - 6], "txt12_r");
							}
						}
						else if (cur_weapon != selectweapon && GUI.Button(Crypto.Rect2(308f, 198f, 64f, 32f), Language.intxt[language, 10], bt_yesno))
						{
							if (sound_UI != null)
							{
								script_soundUI.SoundOn(0);
							}
							Crypto.Save_int_key("n44", selectweapon);
							Crypto.Save_int_key("n34", selweapon_meshkind + selweapon_kind * 100);
							PlayerPrefs.SetInt("updateRank", 1);
							Crypto.Save_int_key("n31", selweapon_maxatk);
							Crypto.Save_int_key("n04", selweapon_minatk);
							Crypto.Save_int_key("n35", (int)(selweapon_spd * 100f));
							Crypto.Save_int_key("n38", selweapon_grade * 10 + selweapon_special);
							cur_weapon = selectweapon;
							oldweapon_meshkind = selweapon_meshkind;
							oldweapon_kind = selweapon_kind;
						}
					}
					else
					{
						for (int num3 = 0; num3 < 3; num3++)
						{
							GUI.DrawTexture(Crypto.Rect2(302f, 124 + num3 * 20, 140f, 16f), titlebase_w);
						}
						GUI.Label(Crypto.Rect2(300f, 124f, 128f, 16f), Language.intxt[language, 141] + "   " + selweapon_minatk + " - " + selweapon_maxatk, "txt12_0");
						GUI.Label(Crypto.Rect2(306f, 144f, 128f, 16f), Language.intxt[language, 153] + "   " + (selweapon_spd * 100f).ToString("F1"), "txt12_0");
						if (selweapon_grade > 1)
						{
							GUI.Label(Crypto.Rect2(306f, 164f, 128f, 16f), script_name.txt_cos[language, selweapon_special_txt], "txt12_b");
						}
						else
						{
							GUI.Label(Crypto.Rect2(306f, 164f, 128f, 16f), script_name.txt_cos[language, selweapon_special_txt], "txt12_0");
						}
						for (int num4 = 0; num4 < selweapon_grade; num4++)
						{
							GUI.DrawTexture(Crypto.Rect2(234 - selweapon_grade * 7 + num4 * 14, 70f, 16f, 16f), star_grade);
						}
						if (cur_weapon != selectweapon && GUI.Button(Crypto.Rect2(308f, 198f, 64f, 32f), Language.intxt[language, 10], bt_yesno))
						{
							if (sound_UI != null)
							{
								script_soundUI.SoundOn(0);
							}
							Crypto.Save_int_key("n44", selectweapon);
							PlayerPrefs.SetInt("updateRank", 1);
							Crypto.Save_int_key("n34", selweapon_meshkind + selweapon_kind * 100);
							Crypto.Save_int_key("n31", selweapon_maxatk);
							Crypto.Save_int_key("n04", selweapon_minatk);
							Crypto.Save_int_key("n35", (int)(selweapon_spd * 100f));
							int num5 = Mathf.Min(selweapon_grade, 3);
							Crypto.Save_int_key("n38", num5 * 10 + selweapon_special);
							cur_weapon = selectweapon;
							oldweapon_meshkind = selweapon_meshkind;
							oldweapon_kind = selweapon_kind;
						}
						if (GUI.Button(Crypto.Rect2(260f, 198f, 32f, 32f), string.Empty, erase))
						{
							if (sound_UI != null)
							{
								script_soundUI.SoundOn(0);
							}
							if (tutorial == 3)
							{
								confirm = 8;
								Delay(1f);
							}
							else if (selectweapon != cur_weapon)
							{
								confirm = 4;
							}
							else
							{
								confirm = 6;
								Delay(1f);
							}
						}
					}
					if (subkind == 0)
					{
						for (int num6 = 0; num6 < 6; num6++)
						{
							if (GUI.Button(Crypto.Rect2(num6 * 76 + 18, icon_posY + 8, 64f, 64f), string.Empty, bt_equip))
							{
								if (sound_UI != null)
								{
									script_soundUI.SoundOn(0);
								}
								if (num6 >= slot_weapon)
								{
									confirm = 7;
								}
								else if (weapon_seed[num6] > 0 && selectweapon != num6)
								{
									selectweapon = num6;
									SetUpWeapon(weapon_seed[selectweapon]);
									script_weapon.SetWeapon(selweapon_meshkind, selweapon_kind);
								}
							}
							if (num6 == cur_weapon)
							{
								GUI.DrawTexture(Crypto.Rect2(num6 * 76 + 18, icon_posY + 8, 64f, 64f), c_equip);
							}
							if (weapon_seed[num6] > 0)
							{
								GUI.DrawTexture(Crypto.Rect2(num6 * 76 + 18, icon_posY + 8, 64f, 64f), weaponicon[bottom_weaponicon[num6]]);
								for (int num7 = 0; num7 < weapon_grade[num6]; num7++)
								{
									GUI.DrawTexture(Crypto.Rect2(num6 * 76 + 49 - weapon_grade[num6] * 5 + num7 * 10, icon_posY + 10, 12f, 12f), star_grade);
								}
								GUI.Box(Crypto.Rect2(num6 * 76 + 28, icon_posY + 58, 44f, 18f), "+ " + weapon_plusup[num6]);
							}
							else if (num6 >= slot_weapon)
							{
								GUI.DrawTexture(Crypto.Rect2(num6 * 76 + 18, icon_posY + 8, 64f, 64f), icon_lock);
							}
						}
					}
					else if (subkind > 0)
					{
						for (int num8 = 0; num8 < 10; num8++)
						{
							if (GUI.Button(Crypto.Rect2((float)(num8 * 76 + 18) + dragposX, icon_posY + 8, 64f, 64f), string.Empty, bt_equip))
							{
								if (sound_UI != null)
								{
									script_soundUI.SoundOn(0);
								}
								if (selectweapon != num8 + 6 && !scrollOn)
								{
									selectweapon = num8 + 6;
									SetUpWeapon(weapon_seed[selectweapon]);
									script_weapon.SetWeapon(selweapon_meshkind, selweapon_kind);
								}
							}
							if (num8 + 6 == cur_weapon)
							{
								GUI.DrawTexture(Crypto.Rect2((float)(num8 * 76 + 18) + dragposX, icon_posY + 8, 64f, 64f), c_equip);
							}
							GUI.DrawTexture(Crypto.Rect2((float)(num8 * 76 + 18) + dragposX, icon_posY + 8, 64f, 64f), weaponicon[num8 + 16]);
							if (unlock_weapon[num8] == 0)
							{
								GUI.DrawTexture(Crypto.Rect2((float)(num8 * 76 + 18) + dragposX, icon_posY + 8, 64f, 64f), icon_lock);
							}
						}
					}
				}
				if (tutorial == 3)
				{
					if (cur_weapon == 0)
					{
						if (selectweapon != 0)
						{
							if (subkind != 0)
							{
								GUI.DrawTexture(Crypto.Rect2(22f, icon_posY - 50, icon_size * 64, icon_size * 64), please_touch);
							}
							else
							{
								GUI.DrawTexture(Crypto.Rect2(22f, icon_posY + 10, icon_size * 64, icon_size * 64), please_touch);
							}
						}
						else if (pop_upgrade == 5)
						{
							GUI.DrawTexture(Crypto.Rect2(382f, 126f, icon_size * 64, icon_size * 64), please_touch);
						}
						else if (pop_upgrade == 2)
						{
							GUI.DrawTexture(Crypto.Rect2(312f, 164f, icon_size * 64, icon_size * 64), please_touch);
						}
						else if (pop_upgrade == 1)
						{
							GUI.DrawTexture(Crypto.Rect2(204f, 214f, icon_size * 64, icon_size * 64), please_touch);
						}
						else if (pop_upgrade == 0)
						{
							GUI.DrawTexture(Crypto.Rect2(330f, 76f, icon_size * 64, icon_size * 64), please_touch);
						}
					}
					else if (subkind != 0)
					{
						GUI.DrawTexture(Crypto.Rect2(22f, icon_posY - 50, icon_size * 64, icon_size * 64), please_touch);
					}
					else if (selectweapon != 0)
					{
						GUI.DrawTexture(Crypto.Rect2(22f, icon_posY + 10, icon_size * 64, icon_size * 64), please_touch);
					}
					else if (cur_weapon != 0)
					{
						GUI.DrawTexture(Crypto.Rect2(312f, 166f, icon_size * 64, icon_size * 64), please_touch);
					}
					else
					{
						GUI.DrawTexture(Crypto.Rect2(22f, icon_posY + 10, icon_size * 64, icon_size * 64), please_touch);
					}
				}
			}
			else if (menu_kind == 2)
			{
				GUI.DrawTexture(Crypto.Rect2(256f, 69f, 146f, 16f), titlebase);
				GUI.Label(Crypto.Rect2(266f, 70f, 128f, 14f), string.Empty + script_name.txt_cos[language, selequip_index + 1], "txt12_w");
				if (GUI.Button(Crypto.Rect2(378f, 198f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(1);
					}
					selectequip = cur_equip;
					SetUpArmor(costume_seed[selectequip]);
					script_costume.Costume(selequip_index);
					menu_kind = 0;
					subkind = 0;
					HideCha(_hide: true);
				}
				if (selectequip >= 6)
				{
					for (int num9 = 0; num9 < 3; num9++)
					{
						GUI.DrawTexture(Crypto.Rect2(302f, 104 + num9 * 20, 140f, 16f), titlebase_w);
					}
					GUI.Label(Crypto.Rect2(292f, 80f, 150f, 64f), Language.intxt[language, 142] + " + " + selequip_hp, "txt12_0");
					GUI.Label(Crypto.Rect2(292f, 100f, 150f, 64f), string.Empty + script_name.txt_cos[language, 35], "txt12_b");
					GUI.Label(Crypto.Rect2(292f, 120f, 150f, 64f), string.Empty + script_name.txt_cos[language, selequip_special + 21], "txt12_b");
					if (unlock_costume[selectequip - 6] > 0)
					{
						if (cur_equip != selectequip && GUI.Button(Crypto.Rect2(308f, 198f, 64f, 32f), Language.intxt[language, 10], bt_yesno))
						{
							if (sound_UI != null)
							{
								script_soundUI.SoundOn(0);
							}
							Crypto.Save_int_key("n05", selectequip);
							Crypto.Save_int_key("n43", selequip_index);
							Crypto.Save_int_key("selequip_hp", selequip_hp);
							PlayerPrefs.SetInt("updateRank", 1);
							cur_equip = selectequip;
						}
					}
					else
					{
						GUI.Box(Crypto.Rect2(214f, 190f, 64f, 20f), "    " + selequip_cost);
						GUI.DrawTexture(Crypto.Rect2(218f, 192f, 16f, 16f), icon_jade);
						if (chalv >= reqLvFactor_equip[selectequip - 6])
						{
							if (GUI.Button(Crypto.Rect2(308f, 198f, 64f, 32f), Language.intxt[language, 6], bt_yesno))
							{
								if (sound_UI != null)
								{
									script_soundUI.SoundOn(0);
								}
								if (jade >= selequip_cost)
								{
									confirm = 1;
								}
								else
								{
									confirm = 2;
									Delay(1f);
								}
							}
						}
						else
						{
							GUI.Label(Crypto.Rect2(308f, 198f, 64f, 32f), Language.intxt[language, 266] + reqLvFactor_equip[selectequip - 6], "txt12_r");
						}
					}
				}
				else
				{
					for (int num10 = 0; num10 < 2; num10++)
					{
						GUI.DrawTexture(Crypto.Rect2(302f, 104 + num10 * 20, 140f, 16f), titlebase_w);
					}
					GUI.Label(Crypto.Rect2(292f, 80f, 150f, 64f), Language.intxt[language, 142] + " + " + selequip_hp, "txt12_0");
					if (selequip_special == 0)
					{
						GUI.Label(Crypto.Rect2(292f, 100f, 150f, 64f), string.Empty + script_name.txt_cos[language, selequip_special + 21], "txt12_0");
					}
					else
					{
						GUI.Label(Crypto.Rect2(292f, 100f, 150f, 64f), string.Empty + script_name.txt_cos[language, selequip_special + 21], "txt12_b");
					}
					if (cur_equip != selectequip && GUI.Button(Crypto.Rect2(308f, 198f, 64f, 32f), Language.intxt[language, 10], bt_yesno))
					{
						if (sound_UI != null)
						{
							script_soundUI.SoundOn(0);
						}
						Crypto.Save_int_key("n05", selectequip);
						Crypto.Save_int_key("n43", selequip_index);
						Crypto.Save_int_key("selequip_hp", selequip_hp);
						PlayerPrefs.SetInt("updateRank", 1);
						cur_equip = selectequip;
					}
					if (GUI.Button(Crypto.Rect2(260f, 198f, 32f, 32f), string.Empty, erase))
					{
						if (sound_UI != null)
						{
							script_soundUI.SoundOn(0);
						}
						if (selectequip != cur_equip)
						{
							confirm = 12;
						}
						else
						{
							confirm = 6;
							Delay(1f);
						}
					}
				}
				if (subkind == 0)
				{
					for (int num11 = 0; num11 < 6; num11++)
					{
						if (GUI.Button(Crypto.Rect2(num11 * 76 + 18, icon_posY + 8, 64f, 64f), string.Empty, bt_equip))
						{
							if (sound_UI != null)
							{
								script_soundUI.SoundOn(0);
							}
							if (num11 >= slot_costume)
							{
								confirm = 7;
							}
							else if (costume_seed[num11] > 0 && selectequip != num11 && !scrollOn)
							{
								selectequip = num11;
								SetUpArmor(costume_seed[selectequip]);
								script_costume.Costume(selequip_index);
							}
						}
						if (costume_seed[num11] > 0)
						{
							if (num11 == cur_equip)
							{
								GUI.DrawTexture(Crypto.Rect2(num11 * 76 + 18, icon_posY + 8, 64f, 64f), c_equip);
							}
							GUI.DrawTexture(Crypto.Rect2(num11 * 76 + 18, icon_posY + 8, 64f, 64f), equipicon[bottom_equipicon[num11]]);
						}
						else if (num11 >= slot_costume)
						{
							GUI.DrawTexture(Crypto.Rect2(num11 * 76 + 18, icon_posY + 8, 64f, 64f), icon_lock);
						}
					}
				}
				else if (subkind > 0)
				{
					for (int num12 = 0; num12 < 10; num12++)
					{
						if (GUI.Button(Crypto.Rect2((float)(num12 * 76 + 18) + dragposX, icon_posY + 8, 64f, 64f), string.Empty, bt_equip))
						{
							if (sound_UI != null)
							{
								script_soundUI.SoundOn(0);
							}
							if (selectequip != num12 + 6 && !scrollOn)
							{
								selectequip = num12 + 6;
								SetUpArmor(costume_seed[selectequip]);
								script_costume.Costume(selequip_index);
							}
						}
						if (num12 + 6 == cur_equip)
						{
							GUI.DrawTexture(Crypto.Rect2((float)(num12 * 76 + 18) + dragposX, icon_posY + 8, 64f, 64f), c_equip);
						}
						GUI.DrawTexture(Crypto.Rect2((float)(num12 * 76 + 18) + dragposX, icon_posY + 8, 64f, 64f), equipicon[num12 + 7]);
						if (unlock_costume[num12] == 0)
						{
							GUI.DrawTexture(Crypto.Rect2((float)(num12 * 76 + 18) + dragposX, icon_posY + 8, 64f, 64f), icon_lock);
						}
					}
				}
			}
		}
		if (confirm <= 0)
		{
			return;
		}
		GUI.enabled = true;
		if (confirm == 1)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			GUI.Label(Crypto.Rect2(112f, 120f, 256f, 14f), Language.intxt[language, 310], "txt12_0");
			GUI.DrawTexture(Crypto.Rect2(200f, 148f, 16f, 16f), icon_jade);
			GUI.Label(Crypto.Rect2(224f, 148f, 48f, 16f), string.Empty + selequip_cost, "txt12_b");
			if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
			{
				if (Crypto.Property_change(-selequip_cost, _jade: true))
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					unlock_costume[selectequip - 6] = 1;
					jade -= selequip_cost;
					PlayerPrefsX.SetIntArray("n18", unlock_costume);
					PurchaseLog.LogOn(Language.intxt[language, 234] + " (" + selequip_cost + ")\t" + script_name.txt_cos[language, selequip_index + 1]);
				}
				confirm = 0;
			}
			else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
				confirm = 0;
			}
		}
		else if (confirm == 2)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			GUI.Label(Crypto.Rect2(120f, 130f, 230f, 16f), Language.intxt[language, 21], "txt12_0");
			if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				Crypto.Save_int_key("cashshopkind", 1);
				confirm = 0;
				if (cashshop == null)
				{
					cashshop = Resources.Load("CashShop") as GameObject;
				}
				Object.Instantiate(cashshop.transform, Vector3.zero, Quaternion.identity);
			}
			else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
				confirm = 0;
			}
		}
		else if (confirm == 3)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			GUI.Label(Crypto.Rect2(120f, 130f, 240f, 16f), Language.intxt[language, 22], "txt12_0");
			if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				Crypto.Save_int_key("cashshopkind", 2);
				confirm = 0;
				if (cashshop == null)
				{
					cashshop = Resources.Load("CashShop") as GameObject;
				}
				Object.Instantiate(cashshop.transform, Vector3.zero, Quaternion.identity);
			}
			else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
				confirm = 0;
			}
		}
		else if (confirm == 4)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			GUI.Label(Crypto.Rect2(112f, 120f, 256f, 14f), Language.intxt[language, 12], "txt12_0");
			GUI.DrawTexture(Crypto.Rect2(200f, 148f, 16f, 16f), icon_coin);
			GUI.Label(Crypto.Rect2(224f, 148f, 48f, 16f), string.Empty + ((float)selweapon_index + (float)selweapon_grade * 0.1f) * 10f, "txt12_b");
			if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				confirm = 0;
				Crypto.Property_change((int)(((float)selweapon_index + (float)selweapon_grade * 0.1f) * 10f), _jade: false);
				coin += (int)(((float)selweapon_index + (float)selweapon_grade * 0.1f) * 10f);
				weapon_seed[selectweapon] = 0;
				weapon_plusup[selectweapon] = 0;
				PlayerPrefsX.SetIntArray("n41", weapon_seed);
				PlayerPrefsX.SetIntArray("n19", weapon_plusup);
				script_weapon.SetWeapon(oldweapon_meshkind, oldweapon_kind);
				selectweapon = cur_weapon;
				SetUpWeapon(weapon_seed[selectweapon]);
			}
			else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
				confirm = 0;
			}
		}
		else if (confirm == 5)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 110f, 256f, 64f), pop_blank);
			GUI.Label(Crypto.Rect2(112f, 110f, 256f, 64f), Language.intxt[language, 267], "txt12_0");
			if (f_delay == 0f)
			{
				confirm = 0;
			}
		}
		else if (confirm == 6)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 110f, 256f, 64f), pop_blank);
			GUI.Label(Crypto.Rect2(112f, 110f, 256f, 64f), Language.intxt[language, 206], "txt12_0");
			if (f_delay == 0f)
			{
				confirm = 0;
			}
		}
		else if (confirm == 7)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			GUI.DrawTexture(Crypto.Rect2(160f, 98f, 160f, 18f), titlebase);
			GUI.Label(Crypto.Rect2(120f, 120f, 230f, 16f), Language.intxt[language, 300], "txt12_0");
			GUI.DrawTexture(Crypto.Rect2(186f, 134f, 32f, 32f), icon_unlock);
			GUI.DrawTexture(Crypto.Rect2(244f, 142f, 16f, 16f), icon_jade);
			GUI.Label(Crypto.Rect2(264f, 142f, 16f, 16f), "3", "txt12_b");
			GUI.Label(Crypto.Rect2(112f, 162f, 256f, 16f), Language.intxt[language, 350], "txt12_b");
			if (menu_kind == 1)
			{
				GUI.Label(Crypto.Rect2(120f, 98f, 230f, 16f), Language.intxt[language, 235] + " +1", "txt12_w");
			}
			else
			{
				GUI.Label(Crypto.Rect2(120f, 98f, 230f, 16f), Language.intxt[language, 198] + " +1", "txt12_w");
			}
			if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				confirm = 0;
				if (jade >= 3)
				{
					if (Crypto.Property_change(-3, _jade: true))
					{
						jade -= 3;
						if (menu_kind == 1)
						{
							slot_weapon = Mathf.Min(6, slot_weapon + 1);
							Crypto.Save_int_key("n03", slot_weapon);
							PurchaseLog.LogOn(Language.intxt[language, 234] + " (3)\t" + Language.intxt[language, 235]);
						}
						else
						{
							slot_costume = Mathf.Min(6, slot_costume + 1);
							Crypto.Save_int_key("n42", slot_costume);
							PurchaseLog.LogOn(Language.intxt[language, 234] + " (3)\t" + Language.intxt[language, 198]);
						}
					}
				}
				else
				{
					confirm = 2;
				}
			}
			else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
				confirm = 0;
			}
		}
		else if (confirm == 8)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 110f, 256f, 64f), pop_blank);
			GUI.Label(Crypto.Rect2(112f, 110f, 256f, 64f), Language.intxt[language, 98], "txt12_0");
			if (f_delay == 0f)
			{
				confirm = 0;
			}
		}
		else if (confirm == 9)
		{
			GUI.Box(Crypto.Rect2(112f, 140f, 256f, 32f), Language.intxt[language, 258]);
			if (f_delay == 0f)
			{
				confirm = 0;
			}
		}
		if (confirm == 10)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			GUI.Label(Crypto.Rect2(112f, 120f, 256f, 14f), Language.intxt[language, 310], "txt12_0");
			GUI.DrawTexture(Crypto.Rect2(200f, 148f, 16f, 16f), icon_jade);
			GUI.Label(Crypto.Rect2(224f, 148f, 48f, 16f), string.Empty + selweapon_cost, "txt12_b");
			if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 0], bt_yesno))
			{
				if (selweapon_cost < 1000)
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					confirm = 0;
					if (Crypto.Property_change(-selweapon_cost, _jade: true))
					{
						unlock_weapon[selectweapon - 6] = 1;
						jade -= selweapon_cost;
						PurchaseLog.LogOn(Language.intxt[language, 234] + " (" + selweapon_cost + ")\t" + script_name.txt_cos[language, selweapon_name]);
					}
					confirm = 0;
				}
				else
				{
					if (sound_UI != null)
					{
						script_soundUI.SoundOn(0);
					}
					confirm = 0;
					if (Crypto.Property_change(-selectweapon, _jade: false))
					{
						unlock_weapon[selectweapon - 6] = 1;
						coin -= selweapon_cost;
					}
				}
				PlayerPrefsX.SetIntArray("n40", unlock_weapon);
			}
			else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 3], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
				confirm = 0;
			}
		}
		else if (confirm == 11)
		{
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			GUI.Label(Crypto.Rect2(125f, 100f, 230f, 16f), Language.intxt[language, 385], "txt12_0");
			GUI.Label(Crypto.Rect2(125f, 120f, 230f, 22f), Language.intxt[language, 386], "txt12_r");
			GUI.DrawTexture(Crypto.Rect2(200f, 146f, 16f, 16f), icon_jade);
			GUI.Label(Crypto.Rect2(224f, 146f, 48f, 16f), string.Empty + 10, "txt12_b");
			GUI.Label(Crypto.Rect2(125f, 164f, 230f, 16f), Language.intxt[language, 350], "txt12_b");
			if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
			{
				if (jade >= 10)
				{
					if (Crypto.Property_change(-10, _jade: true))
					{
						if (sound_UI != null)
						{
							script_soundUI.SoundOn(0);
						}
						jade -= 10;
						weapon_seed[selectweapon] -= selweapon_upgrade * 100000;
						weapon_plusup[selectweapon] = 0;
						selweapon_upgrade = 0;
						PlayerPrefsX.SetIntArray("n41", weapon_seed);
						PlayerPrefsX.SetIntArray("n19", weapon_plusup);
						SetUpWeapon(weapon_seed[selectweapon]);
						if (selectweapon == cur_weapon)
						{
							Crypto.Save_int_key("n31", selweapon_maxatk);
							PlayerPrefs.SetInt("updateRank", 1);
							Crypto.Save_int_key("n04", selweapon_minatk);
						}
						maxupgrade = false;
						PurchaseLog.LogOn(Language.intxt[language, 234] + " (10)\t" + Language.intxt[language, 384]);
					}
					confirm = 0;
				}
				else
				{
					confirm = 2;
				}
			}
			else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				confirm = 0;
			}
		}
		else
		{
			if (confirm != 12)
			{
				return;
			}
			GUI.DrawTexture(Crypto.Rect2(112f, 90f, 256f, 128f), pop_blank2);
			GUI.Label(Crypto.Rect2(112f, 120f, 256f, 14f), Language.intxt[language, 12], "txt12_0");
			GUI.DrawTexture(Crypto.Rect2(200f, 148f, 16f, 16f), icon_coin);
			GUI.Label(Crypto.Rect2(224f, 148f, 48f, 16f), string.Empty + selequip_hp * 5, "txt12_b");
			if (GUI.Button(Crypto.Rect2(170f, 180f, 64f, 32f), Language.intxt[language, 1], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(0);
				}
				confirm = 0;
				Crypto.Property_change(selequip_hp * 5, _jade: false);
				coin += selequip_hp * 5;
				costume_seed[selectequip] = 0;
				PlayerPrefsX.SetIntArray("n45", costume_seed);
				selectequip = cur_equip;
				SetUpArmor(costume_seed[cur_equip]);
				script_costume.Costume(selequip_index);
			}
			else if (GUI.Button(Crypto.Rect2(240f, 180f, 64f, 32f), Language.intxt[language, 2], bt_yesno))
			{
				if (sound_UI != null)
				{
					script_soundUI.SoundOn(1);
				}
				confirm = 0;
			}
		}
	}
}
